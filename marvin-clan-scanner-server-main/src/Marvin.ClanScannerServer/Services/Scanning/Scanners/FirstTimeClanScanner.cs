using DotNetBungieAPI.Models;
using DotNetBungieAPI.Models.GroupsV2;
using DotNetBungieAPI.Models.Queries;
using DotNetBungieAPI.Service.Abstractions;
using Marvin.ClanScannerServer.Models;
using Marvin.ClanScannerServer.Services.Hosted.Interfaces;
using Marvin.ClanScannerServer.Services.Scanning.Attributes;
using Marvin.DbAccess.Models.Broadcasting;
using Marvin.DbAccess.Models.Clan;
using Marvin.DbAccess.Services.Interfaces;
using Polly;

namespace Marvin.ClanScannerServer.Services.Scanning.Scanners;

public class FirstTimeClanScanner : EntityScannerBase<ClanUpdateContext, FirstTimeScanEntry>
{
    private const string GroupKey = "Group";
    private const string GroupMembersKey = "GroupMembers";
    
    private const int MaxUsersLoadParallelism = 30;
    
    private readonly IBungieClient _bungieClient;
    private readonly BungieNetApiCallLogger _bungieNetApiCallLogger;
    private readonly SilentMemberScanner _silentMemberScanner;
    private readonly IClansDbAccess _clansDbAccess;
    private readonly ScanMetricsService _scanMetricsService;
    private readonly IBroadcastsDbAccess _broadcastsDbAccess;
    private readonly IUserQueue _userQueue;
    private AsyncPolicy _apiCallPolicy;
    
    public FirstTimeClanScanner(
        ILogger<FirstTimeClanScanner> logger,
        IBungieClient bungieClient,
        BungieNetApiCallLogger bungieNetApiCallLogger,
        SilentMemberScanner silentMemberScanner,
        IClansDbAccess clansDbAccess,
        ScanMetricsService scanMetricsService,
        IBroadcastsDbAccess broadcastsDbAccess,
        IUserQueue userQueue) : base(logger)
    {
        Initialize();
        _bungieClient = bungieClient;
        _bungieNetApiCallLogger = bungieNetApiCallLogger;
        _silentMemberScanner = silentMemberScanner;
        _clansDbAccess = clansDbAccess;
        _scanMetricsService = scanMetricsService;
        _broadcastsDbAccess = broadcastsDbAccess;
        _userQueue = userQueue;
        BuildApiCallPolicy();
    }
    
    private void BuildApiCallPolicy()
    {
        var timeoutPolicy = Policy
            .TimeoutAsync(TimeSpan.FromSeconds(20));

        var retryPolicy = Policy
            .Handle<Exception>()
            .RetryAsync(3);

        _apiCallPolicy = retryPolicy.WrapAsync(timeoutPolicy);
    }
    
    [ScanStep(nameof(GetGroupData), 1)]
    public async ValueTask<bool> GetGroupData(
        ClanUpdateContext context,
        FirstTimeScanEntry entry,
        ScanContext scanContext,
        CancellationToken cancellationToken)
    {
        var apiCallResult = await _apiCallPolicy.ExecuteAndCaptureAsync(async (ct) =>
                await _bungieClient
                    .ApiAccess
                    .GroupV2
                    .GetGroup(context.ClanId, ct),
            cancellationToken);

        if (apiCallResult.Outcome == OutcomeType.Failure)
        {
            _bungieNetApiCallLogger.LogRequest(new BungieResponse<bool>()
            {
                ErrorCode = PlatformErrorCodes.ExternalServiceTimeout
            });
            return false;
        }

        var groupResponse = apiCallResult.Result;

        _bungieNetApiCallLogger.LogRequest(groupResponse);

        if (!groupResponse.IsSuccessfulResponseCode)
        {
            return false;
        }

        scanContext[GroupKey] = groupResponse.Response;
        return true;
    }
    
    [ScanStep(nameof(GetMembersOfGroup), 2)]
    public async ValueTask<bool> GetMembersOfGroup(
        ClanUpdateContext context,
        FirstTimeScanEntry entry,
        ScanContext scanContext,
        CancellationToken cancellationToken)
    {
        var apiCallResult = await _apiCallPolicy.ExecuteAndCaptureAsync(async (ct) =>
                await _bungieClient
                    .ApiAccess
                    .GroupV2
                    .GetMembersOfGroup(
                        context.ClanId,
                        cancellationToken: ct),
            cancellationToken: cancellationToken);

        if (apiCallResult.Outcome == OutcomeType.Failure)
        {
            _bungieNetApiCallLogger.LogFailure(PlatformErrorCodes.ExternalServiceTimeout);
            return false;
        }

        var clanMembersResponse = apiCallResult.Result;

        _bungieNetApiCallLogger.LogRequest(clanMembersResponse);

        if (!clanMembersResponse.IsSuccessfulResponseCode) return false;

        scanContext[GroupMembersKey] = clanMembersResponse.Response;
        context.MembersTotal = clanMembersResponse.Response.Results.Count;
        return true;
    }

    [ScanStep(nameof(UpdateClanMembers), 3)]
    public async ValueTask<bool> UpdateClanMembers(
        ClanUpdateContext context,
        FirstTimeScanEntry entry,
        ScanContext scanContext,
        CancellationToken cancellationToken)
    {
        var groupMembers = scanContext.Get<SearchResultOfGroupMember>(GroupMembersKey);

        await _userQueue.EnqueueAndWaitForBroadcastedUserScans(context, groupMembers.Results.ToList(), cancellationToken);

        return true;
    }

    [ScanStep(nameof(UpdateClanDataInDb), 4, true)]
    public async ValueTask<bool> UpdateClanDataInDb(
        ClanUpdateContext context,
        FirstTimeScanEntry entry,
        ScanContext scanContext,
        CancellationToken cancellationToken)
    {
        var groupResponse = scanContext.Get<GroupResponse>(GroupKey);
        await _clansDbAccess.UpdateClan(groupResponse, context.MembersOnline, cancellationToken);
        await _clansDbAccess.DeleteClanFromFirstScanList(context.ClanId, cancellationToken);
        await _broadcastsDbAccess.SendClanBroadcast(new ClanBroadcast
        {
            ClanId = entry.ClanId,
            Date = DateTime.UtcNow,
            GuildId = entry.GuildId,
            Type = BroadcastType.ClanScanFinished,
            WasAnnounced = false,
            NewValue = entry.ChannelId.ToString()
        }, cancellationToken);
        _scanMetricsService.ClansScanned++;
        return true;
    }
    
    
}