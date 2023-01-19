using System.Text.Json;
using DotNetBungieAPI.HashReferences;
using DotNetBungieAPI.Models;
using DotNetBungieAPI.Models.Destiny;
using DotNetBungieAPI.Models.Destiny.Responses;
using DotNetBungieAPI.Models.GroupsV2;
using DotNetBungieAPI.Service.Abstractions;
using Marvin.ClanScannerServer.Extensions;
using Marvin.ClanScannerServer.Models;
using Marvin.ClanScannerServer.Services.ProfileProcessors.Interfaces;
using Marvin.ClanScannerServer.Services.Scanning.Attributes;
using Marvin.DbAccess.Extensions;
using Marvin.DbAccess.Models.User;
using Marvin.DbAccess.Services.Interfaces;
using Marvin.Extensions;
using Marvin.MemoryCache;

namespace Marvin.ClanScannerServer.Services.Scanning.Scanners;

public class SilentMemberScanner : EntityScannerBase<GroupMember, ClanUpdateContext>
{
    private const string ProfileResponseKey = "ProfileResponse";
    private const string ProfileUpdateContextKey = "ProfileUpdateContext";
    private const string DestinyProfileKey = "DestinyProfile";
    
    private readonly IBungieClient _bungieClient;
    private readonly BungieNetApiCallLogger _bungieNetApiCallLogger;
    private readonly ITrackedEntitiesDbAccess _trackedEntitiesDbAccess;
    private readonly IDestinyProfileDbAccess _destinyProfileDbAccess;
    private readonly IProgressionProcessor _progressionProcessor;
    private readonly IMetricProcessor _metricProcessor;
    private readonly ICollectibleProcessor _collectibleProcessor;
    private readonly IRecordProcessor _recordProcessor;
    private readonly ScanMetricsService _scanMetricsService;
    private readonly ICacheProvider _cacheProvider;

    public SilentMemberScanner(
        ILogger<SilentMemberScanner> logger,
        IBungieClient bungieClient,
        BungieNetApiCallLogger bungieNetApiCallLogger,
        ITrackedEntitiesDbAccess trackedEntitiesDbAccess,
        IDestinyProfileDbAccess destinyProfileDbAccess,
        IProgressionProcessor progressionProcessor,
        IMetricProcessor metricProcessor,
        ICollectibleProcessor collectibleProcessor,
        IRecordProcessor recordProcessor,
        ScanMetricsService scanMetricsService,
        ICacheProvider cacheProvider) : base(logger)
    {
        _bungieClient = bungieClient;
        _bungieNetApiCallLogger = bungieNetApiCallLogger;
        _trackedEntitiesDbAccess = trackedEntitiesDbAccess;
        _destinyProfileDbAccess = destinyProfileDbAccess;
        _progressionProcessor = progressionProcessor;
        _metricProcessor = metricProcessor;
        _collectibleProcessor = collectibleProcessor;
        _recordProcessor = recordProcessor;
        _scanMetricsService = scanMetricsService;
        _cacheProvider = cacheProvider;
        Initialize();
    }
    
    [ScanStep(nameof(CheckIfMemberIsOnline), 1)]
    public ValueTask<bool> CheckIfMemberIsOnline(
        GroupMember groupMember,
        ClanUpdateContext context,
        ScanContext scanContext,
        CancellationToken cancellationToken)
    {
        if (!groupMember.IsOnline)
            return ValueTask.FromResult(true);

        context.MembersOnline++;
        return ValueTask.FromResult(true);
    }
    
    [ScanStep(nameof(GetDestinyProfile), 2)]
    public async ValueTask<bool> GetDestinyProfile(
        GroupMember groupMember,
        ClanUpdateContext context,
        ScanContext scanContext,
        CancellationToken cancellationToken)
    {
        BungieResponse<DestinyProfileResponse> profileResponse;

        try
        {
            var timeoutTokenSource = new CancellationTokenSource();
            timeoutTokenSource.CancelAfter(TimeSpan.FromSeconds(20));
            using var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                timeoutTokenSource.Token);
            var combinedToken = tokenSource.Token;

            profileResponse = await _bungieClient.ApiAccess.Destiny2.GetProfile(
                groupMember.DestinyUserInfo.MembershipType,
                groupMember.DestinyUserInfo.MembershipId,
                Destiny2Metadata.GenericProfileComponents,
                cancellationToken: combinedToken);
        }
        catch (JsonException)
        {
            Logger.LogWarning(
                "Failed to read json response for Destiny2.GetProfile api call, Context = {@Context}",
                new
                {
                    groupMember.GroupId,
                    groupMember.DestinyUserInfo.MembershipId,
                    groupMember.DestinyUserInfo.MembershipType
                });
            _bungieNetApiCallLogger.LogRequest(new BungieResponse<int>()
            {
                ErrorCode = PlatformErrorCodes.JsonDeserializationError
            });
            return false;
        }
        catch (IOException)
        {
            Logger.LogWarning(
                "Failed to read web response for Destiny2.GetProfile api call, Context = {@Context}",
                new
                {
                    groupMember.GroupId,
                    groupMember.DestinyUserInfo.MembershipId,
                    groupMember.DestinyUserInfo.MembershipType
                });
            _bungieNetApiCallLogger.LogRequest(new BungieResponse<int>()
            {
                ErrorCode = PlatformErrorCodes.JsonDeserializationError
            });
            return false;
        }
        catch (OperationCanceledException)
        {
            Logger.LogWarning(
                "Failed to receive web response for Destiny2.GetProfile api call in 20s, Context = {@Context}",
                new
                {
                    groupMember.GroupId,
                    groupMember.DestinyUserInfo.MembershipId,
                    groupMember.DestinyUserInfo.MembershipType
                });
            _bungieNetApiCallLogger.LogTimeout();
            return false;
        }

        _bungieNetApiCallLogger.LogRequest(profileResponse);

        if (!profileResponse.IsSuccessfulResponseCode)
            return false;

        scanContext[ProfileResponseKey] = profileResponse.Response;
        return true;
    }
    
    [ScanStep(nameof(CheckIfProfileIsPublic), 3)]
    public async ValueTask<bool> CheckIfProfileIsPublic(
        GroupMember groupMember,
        ClanUpdateContext context,
        ScanContext scanContext,
        CancellationToken cancellationToken)
    {
        var profileResponse = (DestinyProfileResponse)scanContext[ProfileResponseKey];

        var isPublic = profileResponse!.HasPublicRecords();

        if (!isPublic)
        {
            await _destinyProfileDbAccess.RemoveDestinyUserFromDbAsync(
                groupMember.DestinyUserInfo.MembershipId,
                cancellationToken);
        }
        
        return isPublic;
    }
    
    [ScanStep(nameof(CreateProfileUpdateContext), 4)]
    public async ValueTask<bool> CreateProfileUpdateContext(
        GroupMember groupMember,
        ClanUpdateContext context,
        ScanContext scanContext,
        CancellationToken cancellationToken)
    {
        var updateContext = await ProfileUpdateContext.CreateContextAsync(
            _trackedEntitiesDbAccess,
            _bungieClient,
            _cacheProvider,
            cancellationToken);

        scanContext[ProfileUpdateContextKey] = updateContext;
        return true;
    }

    [ScanStep(nameof(LoadOrCreateProfile), 5)]
    public async ValueTask<bool> LoadOrCreateProfile(
        GroupMember groupMember,
        ClanUpdateContext context,
        ScanContext scanContext,
        CancellationToken cancellationToken)
    {
        var destinyProfile = await _destinyProfileDbAccess.GetDestinyProfileByMembershipId(
            groupMember.DestinyUserInfo.MembershipId,
            cancellationToken);

        var profileResponse = scanContext.Get<DestinyProfileResponse>(ProfileResponseKey);
        var updateContext = scanContext.Get<ProfileUpdateContext>(ProfileUpdateContextKey);
        if (destinyProfile is null || destinyProfile.ClanId is null)
        {
            destinyProfile = new DestinyProfile(groupMember, profileResponse);
            updateContext.ShouldScanSilently = true;
        }

        if (destinyProfile.Private && profileResponse!.HasPublicRecords())
        {
            updateContext.ShouldScanSilently = true;
        }

        scanContext[DestinyProfileKey] = destinyProfile;
        return true;
    }

    [ScanStep(nameof(PerformProfileUpdate), 6)]
    public async ValueTask<bool> PerformProfileUpdate(
        GroupMember groupMember,
        ClanUpdateContext context,
        ScanContext scanContext,
        CancellationToken cancellationToken)
    {
        var destinyProfile = scanContext.Get<DestinyProfile>(DestinyProfileKey);
        var updateContext = scanContext.Get<ProfileUpdateContext>(ProfileUpdateContextKey);
        var profileResponse = scanContext.Get<DestinyProfileResponse>(ProfileResponseKey);

        PerformSilentProfileUpdateAsync(
            destinyProfile,
            updateContext,
            profileResponse);

        destinyProfile.ClanId = groupMember.GroupId;
        destinyProfile.LastUpdated = DateTime.UtcNow;
        destinyProfile.DisplayName = groupMember.GetDisplayName();
        destinyProfile.Private = !profileResponse.HasPublicRecords();

        if (profileResponse.Characters.Data.Count > 0)
        {
            var lastPlayedCharacter = profileResponse.Characters.GetLastPlayedCharacter();
            if (lastPlayedCharacter is not null)
                destinyProfile.LastPlayed = lastPlayedCharacter.DateLastPlayed;

            destinyProfile.TimePlayed = profileResponse
                .Characters
                .Data
                .Sum(x => x.Value.MinutesPlayedTotal);

            if (profileResponse.CharacterActivities.Data.TryGetValue(
                    lastPlayedCharacter.CharacterId,
                    out var activitiesComponent))
            {
                destinyProfile.CurrentActivity = activitiesComponent.CurrentActivity.Hash;
                destinyProfile.DateActivityStarted = activitiesComponent.DateActivityStarted;
            }
        }

        return true;
    }

    [ScanStep(nameof(PerformProfileUpdate), 7)]
    public async ValueTask<bool> UpsertDestinyProfileData(
        GroupMember groupMember,
        ClanUpdateContext context,
        ScanContext scanContext,
        CancellationToken cancellationToken)
    {
        var destinyProfile = scanContext.Get<DestinyProfile>(DestinyProfileKey);
        context.MembersScanned++;
        _scanMetricsService.MembersScanned++;
        await _destinyProfileDbAccess.UpsertDestinyProfileData(destinyProfile, cancellationToken);
        return true;
    }

    private void PerformSilentProfileUpdateAsync(
        DestinyProfile destinyProfile,
        ProfileUpdateContext profileUpdateContext,
        DestinyProfileResponse destinyProfileResponse)
    {
        if (destinyProfileResponse.Characters.Data.Count > 0)
        {
            var character = destinyProfileResponse.Characters.GetMostPlayedCharacterSafe();

            _progressionProcessor.UpdateProgressions(
                profileUpdateContext.TrackedProgressionsHashes,
                destinyProfile,
                destinyProfileResponse,
                character);
        }

        _metricProcessor.UpdateMetrics(
            profileUpdateContext.TrackedMetricHashes,
            destinyProfileResponse,
            destinyProfile);

        _collectibleProcessor.UpdateCollectibles(
            destinyProfile,
            destinyProfileResponse,
            profileUpdateContext.TrackedProfileCollectibleHashes);

        _recordProcessor.UpdateRecords(
            destinyProfile,
            profileUpdateContext.TrackedProfileRecordHashes,
            destinyProfileResponse);

        UpdateComputedDataOnProfile(destinyProfile, profileUpdateContext.TitleHashes);
    }
    
    private void UpdateComputedDataOnProfile(
        DestinyProfile destinyProfile,
        List<(uint titleHash, uint? gildingHash)> titleHashes)
    {
        destinyProfile.ComputedData ??= new DestinyProfileComputedData();

        if (destinyProfile.Records.TryGetValue(DefinitionHashes.Records.PathtoPower, out var powerRecordData))
        {
            var objectiveValue = powerRecordData
                .Objectives
                .Select(x => (KeyValuePair<uint, UserObjectiveState>?)x)
                .LastOrDefault();

            if (objectiveValue?.Value.Progress is not null)
                destinyProfile.ComputedData.LightLevel = objectiveValue.Value.Value.Progress.Value;
        }

        if (destinyProfile.Records.TryGetValue(DefinitionHashes.Records.SynapticSurge, out var artifactRecordData))
        {
            var objectiveValue = artifactRecordData
                .Objectives
                .Select(x => (KeyValuePair<uint, UserObjectiveState>?)x)
                .LastOrDefault();

            if (objectiveValue?.Value.Progress is not null)
                destinyProfile.ComputedData.ArtifactLevel = objectiveValue.Value.Value.Progress.Value;
        }

        destinyProfile.ComputedData.TotalLightLevel = destinyProfile.ComputedData.LightLevel +
                                                      destinyProfile.ComputedData.ArtifactLevel;

        UpdatedComputedDataTitlesStatus(destinyProfile, titleHashes);

        UpdateComputedDataRaidCompletions(destinyProfile);

        UpdateDrystreaks(destinyProfile);
    }
    
    private void UpdatedComputedDataTitlesStatus(
        DestinyProfile destinyProfile,
        List<(uint titleHash, uint? gildingHash)> titleHashes)
    {
        var totalTitles = 0;
        destinyProfile.ComputedData!.TitlesStatus ??= new Dictionary<uint, int>();

        foreach (var titleHash in titleHashes)
            if (destinyProfile.Records.TryGetValue(titleHash.titleHash, out var titleRecordData))
            {
                if (!titleRecordData.State.HasFlag(DestinyRecordState.ObjectiveNotCompleted)) totalTitles += 1;

                if (titleHash.gildingHash.HasValue &&
                    destinyProfile.Records.TryGetValue(titleHash.gildingHash.Value, out var gildTitleRecordData))
                    destinyProfile.ComputedData.TitlesStatus[titleHash.titleHash] =
                        (!titleRecordData.State.HasFlag(DestinyRecordState.ObjectiveNotCompleted)).ToInt32() +
                        gildTitleRecordData.CompletedCount.GetValueOrDefault();
                else
                    destinyProfile.ComputedData.TitlesStatus[titleHash.titleHash] =
                        (!titleRecordData.State.HasFlag(DestinyRecordState.ObjectiveNotCompleted)).ToInt32();
            }

        if (destinyProfile.ComputedData.TotalTitles < totalTitles)
            destinyProfile.ComputedData.TotalTitles = totalTitles;
    }
    
    private void UpdateComputedDataRaidCompletions(
        DestinyProfile destinyProfile)
    {
        var totalRaidsDone = 0;

        destinyProfile.ComputedData!.RaidCompletions ??= new Dictionary<uint, int>();

        foreach (var raidMetricHash in Destiny2Metadata.RaidCompletionMetricHashes)
            if (destinyProfile.Metrics.TryGetValue(raidMetricHash, out var raidCompletionsMetric))
            {
                totalRaidsDone += raidCompletionsMetric.Progress;
                destinyProfile.ComputedData.RaidCompletions[raidMetricHash] = raidCompletionsMetric.Progress;
            }

        if (destinyProfile.ComputedData.TotalRaids < totalRaidsDone)
            destinyProfile.ComputedData.TotalRaids = totalRaidsDone;
    }
    
    private void UpdateDrystreaks(DestinyProfile destinyProfile)
    {
        destinyProfile.ComputedData!.ItemDrystreaks ??= new Dictionary<uint, int>();
        foreach (var (collectibleHash, activityCompletionsMetricHash) in Destiny2Metadata.DryStreakItemSettings)
        {
            if (destinyProfile.Items.Contains(collectibleHash))
            {
                continue;
            }

            if (!destinyProfile.Metrics.TryGetValue(activityCompletionsMetricHash, out var metricProgressData))
            {
                continue;
            }

            destinyProfile.ComputedData.ItemDrystreaks[collectibleHash] = metricProgressData.Progress;
        }
    }
}