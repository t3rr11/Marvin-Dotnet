using DotNetBungieAPI.HashReferences;
using DotNetBungieAPI.Models;
using DotNetBungieAPI.Models.GroupsV2;
using DotNetBungieAPI.Models.Queries;
using DotNetBungieAPI.Service.Abstractions;
using Marvin.ClanScannerServer.Extensions;
using Marvin.ClanScannerServer.Models;
using Marvin.ClanScannerServer.Services.Hosted.Interfaces;
using Marvin.ClanScannerServer.Services.ProfileProcessors.Interfaces;
using Marvin.ClanScannerServer.Services.Scanning.Attributes;
using Marvin.DbAccess.Models.Clan;
using Marvin.DbAccess.Models.Guild;
using Marvin.DbAccess.Models.User;
using Marvin.DbAccess.Services.Interfaces;
using Polly;

namespace Marvin.ClanScannerServer.Services.Scanning.Scanners;

public class ClanScanner : EntityScannerBase<ClanUpdateContext, object>
{
    private const string ClanDbModelKey = "ClanDbModel";
    private const string GroupKey = "Group";
    private const string GroupMembersKey = "GroupMembers";
    private const string GuildBroadcastSettingsKey = "GuildBroadcastSettings";
    private const string CurrentClanMemberReferencesKey = "CurrentClanMemberReferences";

    private const int MaxUsersLoadParallelism = 30;

    private AsyncPolicy _apiCallPolicy;

    private readonly IClansDbAccess _clansDbAccess;
    private readonly IBungieClient _bungieClient;
    private readonly BungieNetApiCallLogger _bungieNetApiCallLogger;
    private readonly IDestinyProfileDbAccess _destinyProfileDbAccess;
    private readonly IClanProcessor _clanProcessor;
    private readonly ScanMetricsService _scanMetricsService;
    private readonly IUserQueue _userQueue;

    public ClanScanner(
        ILogger<ClanScanner> logger,
        IClansDbAccess clansDbAccess,
        IBungieClient bungieClient,
        BungieNetApiCallLogger bungieNetApiCallLogger,
        IDestinyProfileDbAccess destinyProfileDbAccess,
        IClanProcessor clanProcessor,
        ScanMetricsService scanMetricsService,
        IUserQueue userQueue) : base(logger)
    {
        Initialize();
        _clansDbAccess = clansDbAccess;
        _bungieClient = bungieClient;
        _bungieNetApiCallLogger = bungieNetApiCallLogger;
        _destinyProfileDbAccess = destinyProfileDbAccess;
        _clanProcessor = clanProcessor;
        _scanMetricsService = scanMetricsService;
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

    [ScanStep(nameof(GetClanFromDb), 1)]
    public async ValueTask<bool> GetClanFromDb(
        ClanUpdateContext context,
        object empty,
        ScanContext scanContext,
        CancellationToken cancellationToken)
    {
        var clanData = (await _clansDbAccess.GetClan(context.ClanId, cancellationToken))
            .FirstOrDefault();

        scanContext[ClanDbModelKey] = clanData;

        return clanData is not null;
    }

    [ScanStep(nameof(GetGroupData), 2)]
    public async ValueTask<bool> GetGroupData(
        ClanUpdateContext context,
        object empty,
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
            try
            {
                if (groupResponse.ErrorCode == PlatformErrorCodes.ClanNotFound)
                {
                    var clan = scanContext.Get<ClanDbModel>(ClanDbModelKey);
                    await _clansDbAccess.MarkClanAsDeletedAsync(clan, cancellationToken);
                }
            }
            catch (Exception exception)
            {
                Logger.LogError(
                    exception,
                    "Failed to set clan as deleted, clan id = {ClanId}",
                    context.ClanId);
            }

            return false;
        }

        scanContext[GroupKey] = groupResponse.Response;
        return true;
    }

    [ScanStep(nameof(GetMembersOfGroup), 3)]
    public async ValueTask<bool> GetMembersOfGroup(
        ClanUpdateContext context,
        object empty,
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

    [ScanStep(nameof(GetGuildBroadcastSettings), 4)]
    public async ValueTask<bool> GetGuildBroadcastSettings(
        ClanUpdateContext context,
        object empty,
        ScanContext scanContext,
        CancellationToken cancellationToken)
    {
        var discordLinkedGuilds = await _clansDbAccess
            .GetAllLinkedDiscordGuilds(context.ClanId, cancellationToken);

        scanContext[GuildBroadcastSettingsKey] = discordLinkedGuilds;
        context.BroadcastsConfigs = discordLinkedGuilds;
        return true;
    }

    [ScanStep(nameof(GetCurrentClanMemberReferences), 5)]
    public async ValueTask<bool> GetCurrentClanMemberReferences(
        ClanUpdateContext context,
        object empty,
        ScanContext scanContext,
        CancellationToken cancellationToken)
    {
        var currentClanMemberReferences = (await _destinyProfileDbAccess.GetClanMemberReferencesAsync(
                context.ClanId,
                cancellationToken))
            .ToList();

        scanContext[CurrentClanMemberReferencesKey] = currentClanMemberReferences;
        return true;
    }

    [ScanStep(nameof(RemoveMembers), 6)]
    public async ValueTask<bool> RemoveMembers(
        ClanUpdateContext context,
        object empty,
        ScanContext scanContext,
        CancellationToken cancellationToken)
    {
        var currentClanMemberReferences =
            scanContext.Get<List<DestinyProfileClanMemberReference>>(CurrentClanMemberReferencesKey);
        var clanMembers = scanContext.Get<SearchResultOfGroupMember>(GroupMembersKey);

        var membersToRemove = currentClanMemberReferences
            .Where(x => !clanMembers.Results.Any(m =>
                m.DestinyUserInfo.MembershipId == x.MembershipId));

        foreach (var memberToRemove in membersToRemove)
            await _destinyProfileDbAccess.UpdateDestinyProfileClanId(
                null,
                memberToRemove.MembershipId,
                cancellationToken);

        return true;
    }

    [ScanStep(nameof(UpdateClanUsers), 7)]
    public async ValueTask<bool> UpdateClanUsers(
        ClanUpdateContext context,
        object empty,
        ScanContext scanContext,
        CancellationToken cancellationToken)
    {
        var clanData = scanContext.Get<ClanDbModel>(ClanDbModelKey);
        var clanMembers = scanContext.Get<SearchResultOfGroupMember>(GroupMembersKey);

        if (clanMembers.Results.Count == 0)
            return true;
        
        if ((DateTime.UtcNow - clanData.LastScan).TotalMinutes > 120 || clanData.IsForcedScan)
        {
            await _userQueue.EnqueueAndWaitForSilentUserScans(
                context,
                clanMembers.Results.ToList(),
                cancellationToken);
        }
        else
        {
            var membersToScan = clanMembers
                .Results
                .Where(x => x.ShouldScanClanMember())
                .ToList();

            if (membersToScan.Count == 0)
                return true;

            await _userQueue.EnqueueAndWaitForBroadcastedUserScans(
                context,
                membersToScan,
                cancellationToken);
        }

        return true;
    }

    [ScanStep(nameof(ProcessAndUpdateClanChanges), 8, true)]
    public async ValueTask<bool> ProcessAndUpdateClanChanges(
        ClanUpdateContext context,
        object empty,
        ScanContext scanContext,
        CancellationToken cancellationToken)
    {
        var clanData = scanContext.Get<ClanDbModel>(ClanDbModelKey);
        if (clanData is null)
            return false;

        var groupResponse = scanContext.Get<GroupResponse>(GroupKey);
        if (groupResponse is null)
            return false;
        
        _clanProcessor.ProcessClanChanges(clanData, context.BroadcastsConfigs, groupResponse, cancellationToken);

        UpdateClanDbModel(clanData, groupResponse, context);
        await _clansDbAccess.UpsertClanAsync(clanData, cancellationToken);

        if (clanData.IsForcedScan)
            await _clansDbAccess.SetClanForcedFlagToFalse(context.ClanId, cancellationToken);
        _scanMetricsService.ClansScanned++;
        return true;
    }

    private void UpdateClanDbModel(
        ClanDbModel clanDbModel,
        GroupResponse groupResponse,
        ClanUpdateContext context)
    {
        clanDbModel.ClanName = groupResponse.Detail.Name;
        clanDbModel.ClanCallsign = groupResponse.Detail.ClanInfo.ClanCallSign;
        clanDbModel.ClanLevel = groupResponse.Detail.ClanInfo.D2ClanProgressions[DefinitionHashes.Progressions.ClanLevel].Level;
        clanDbModel.MemberCount = groupResponse.Detail.MemberCount;
        clanDbModel.MembersOnline = context.MembersOnline;
        clanDbModel.LastScan = DateTime.UtcNow;
        if (clanDbModel.BannerData is null)
        {
            clanDbModel.BannerData = new ClanBannerDataDbModel
            {
                DecalId = groupResponse.Detail.ClanInfo.ClanBannerData.DecalId,
                DecalColorId = groupResponse.Detail.ClanInfo.ClanBannerData.DecalColorId,
                DecalBackgroundColorId = groupResponse.Detail.ClanInfo.ClanBannerData.DecalBackgroundColorId,
                GonfalonId = groupResponse.Detail.ClanInfo.ClanBannerData.GonfalonId,
                GonfalonColorId = groupResponse.Detail.ClanInfo.ClanBannerData.GonfalonColorId,
                GonfalonDetailId = groupResponse.Detail.ClanInfo.ClanBannerData.GonfalonDetailId,
                GonfalonDetailColorId = groupResponse.Detail.ClanInfo.ClanBannerData.GonfalonDetailColorId
            };
        }
        else
        {
            clanDbModel.BannerData.DecalId = groupResponse.Detail.ClanInfo.ClanBannerData.DecalId;
            clanDbModel.BannerData.DecalColorId = groupResponse.Detail.ClanInfo.ClanBannerData.DecalColorId;
            clanDbModel.BannerData.DecalBackgroundColorId = groupResponse.Detail.ClanInfo.ClanBannerData.DecalBackgroundColorId;
            clanDbModel.BannerData.GonfalonId = groupResponse.Detail.ClanInfo.ClanBannerData.GonfalonId;
            clanDbModel.BannerData.GonfalonColorId = groupResponse.Detail.ClanInfo.ClanBannerData.GonfalonColorId;
            clanDbModel.BannerData.GonfalonDetailId = groupResponse.Detail.ClanInfo.ClanBannerData.GonfalonDetailId;
            clanDbModel.BannerData.GonfalonDetailColorId = groupResponse.Detail.ClanInfo.ClanBannerData.GonfalonDetailColorId;
        }
    }
}