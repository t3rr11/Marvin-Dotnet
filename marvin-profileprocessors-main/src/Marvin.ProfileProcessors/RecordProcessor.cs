using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using DotNetBungieAPI.Models;
using DotNetBungieAPI.Models.Destiny;
using DotNetBungieAPI.Models.Destiny.Components;
using DotNetBungieAPI.Models.Destiny.Definitions.Records;
using DotNetBungieAPI.Models.Destiny.Quests;
using DotNetBungieAPI.Models.Destiny.Responses;
using DotNetBungieAPI.Service.Abstractions;
using Marvin.DbAccess.Models.Broadcasting;
using Marvin.DbAccess.Models.Guild;
using Marvin.DbAccess.Models.User;
using Marvin.DbAccess.Services.Interfaces;
using Marvin.ProfileProcessors.Interfaces;

namespace Marvin.ProfileProcessors;

public class RecordProcessor : IRecordProcessor
{
    private readonly IBroadcastsDbAccess _broadcastsDbAccess;
    private readonly IBungieClient _bungieClient;

    public RecordProcessor(
        IBungieClient bungieClient,
        IBroadcastsDbAccess broadcastsDbAccess)
    {
        _bungieClient = bungieClient;
        _broadcastsDbAccess = broadcastsDbAccess;
    }

    public void UpdateRecords(
        DestinyProfile destinyProfile,
        IEnumerable<uint> trackedProfileRecords,
        DestinyProfileResponse destinyProfileResponse)
    {
        if (destinyProfileResponse.ProfileRecords.Data is null)
            return;

        foreach (var (recordHash, recordComponent) in destinyProfileResponse.ProfileRecords.Data.Records)
        {
            if (destinyProfile.Records.TryGetValue(recordHash, out var userRecordData))
            {
                userRecordData.State = recordComponent.State;
                userRecordData.CompletedCount = recordComponent.CompletedCount;

                if (recordComponent.Objectives.Count > 0 ||
                    recordComponent.IntervalObjectives.Count > 0)
                {
                    userRecordData.Objectives ??= new Dictionary<uint, UserObjectiveState>();
                    UpdateObjectives(userRecordData, recordComponent.Objectives);
                    UpdateObjectives(userRecordData, recordComponent.IntervalObjectives);
                }
                else
                {
                    userRecordData.Objectives = null;
                }
            }
            else
            {
                var newUserRecordData = new UserRecordData()
                {
                    State = recordComponent.State,
                    CompletedCount = recordComponent.CompletedCount,
                    Objectives = null
                };

                if (recordComponent.Objectives.Count > 0 ||
                    recordComponent.IntervalObjectives.Count > 0)
                {
                    newUserRecordData.Objectives = new Dictionary<uint, UserObjectiveState>();

                    InsertObjectives(newUserRecordData, recordComponent.Objectives);
                    InsertObjectives(newUserRecordData, recordComponent.IntervalObjectives);
                }

                destinyProfile.Records.Add(recordHash, newUserRecordData);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdateObjectives(
        UserRecordData userRecordData,
        ReadOnlyCollection<DestinyObjectiveProgress> objectives)
    {
        foreach (var intervalObjective in objectives)
        {
            var objectiveHash = intervalObjective.Objective.Hash.GetValueOrDefault();

            if (userRecordData.Objectives!.TryGetValue(
                    objectiveHash,
                    out var storedObjectiveState))
            {
                storedObjectiveState.Progress = intervalObjective.Progress;
                storedObjectiveState.IsComplete = intervalObjective.IsComplete;
            }
            else
            {
                userRecordData.Objectives.TryAdd(
                    objectiveHash,
                    new UserObjectiveState
                    {
                        Progress = intervalObjective.Progress,
                        IsComplete = intervalObjective.IsComplete
                    });
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void InsertObjectives(
        UserRecordData userRecordData,
        ReadOnlyCollection<DestinyObjectiveProgress> objectives)
    {
        foreach (var objectiveProgress in objectives)
        {
            userRecordData.Objectives!.Add(
                objectiveProgress.Objective.Hash.GetValueOrDefault(),
                new UserObjectiveState
                {
                    IsComplete = objectiveProgress.IsComplete,
                    Progress = objectiveProgress.Progress
                });
        }
    }

    public void UpdateAndReportRecords(
        Dictionary<ulong, GuildBroadcastsConfig> broadcastsConfigs,
        DestinyProfile destinyProfile,
        List<(uint titleHash, uint? gildingHash)> titlesHashes,
        IEnumerable<uint> trackedProfileRecords,
        DestinyProfileResponse destinyProfileResponse,
        CancellationToken cancellationToken)
    {
        if (destinyProfileResponse.ProfileRecords.Data is null)
            return;

        foreach (var (recordHash, recordComponent) in destinyProfileResponse.ProfileRecords.Data.Records)
        {
            if (destinyProfile.Records.TryGetValue(recordHash, out var userRecordData))
            {
                if (RecordGotCompleted(userRecordData, recordComponent))
                {
                    foreach (var (guildId, broadcastsConfig) in broadcastsConfigs)
                    {
                        ProcessTriumphCompleted(
                            titlesHashes,
                            trackedProfileRecords,
                            recordHash,
                            destinyProfile.ClanId.GetValueOrDefault(),
                            destinyProfile.MembershipId,
                            guildId,
                            broadcastsConfig,
                            recordComponent.CompletedCount,
                            cancellationToken);
                    }
                }

                userRecordData.State = recordComponent.State;
                userRecordData.CompletedCount = recordComponent.CompletedCount;

                if (recordComponent.Objectives.Count > 0 ||
                    recordComponent.IntervalObjectives.Count > 0)
                {
                    userRecordData.Objectives ??= new Dictionary<uint, UserObjectiveState>();
                    // for now let's just update objectives, since they don't get tracked
                    UpdateObjectives(userRecordData, recordComponent.Objectives);
                    UpdateObjectives(userRecordData, recordComponent.IntervalObjectives);
                }
                else
                {
                    userRecordData.Objectives = null;
                }
            }
            else
            {
                var newUserRecordData = new UserRecordData()
                {
                    State = recordComponent.State,
                    CompletedCount = recordComponent.CompletedCount,
                    Objectives = null
                };

                if (recordComponent.Objectives.Count > 0 ||
                    recordComponent.IntervalObjectives.Count > 0)
                {
                    newUserRecordData.Objectives = new Dictionary<uint, UserObjectiveState>();

                    InsertObjectives(newUserRecordData, recordComponent.Objectives);
                    InsertObjectives(newUserRecordData, recordComponent.IntervalObjectives);
                }

                destinyProfile.Records.Add(recordHash, newUserRecordData);
            }
        }
    }

    private bool RecordGotCompleted(
        UserRecordData userRecordData,
        DestinyRecordComponent recordComponent)
    {
        return (userRecordData.State & DestinyRecordState.ObjectiveNotCompleted) != 0 &&
               (recordComponent.State & DestinyRecordState.ObjectiveNotCompleted) == 0;
    }

    private void ProcessTriumphCompleted(
        List<(uint titleHash, uint? gildingHash)> titleHashes,
        IEnumerable<uint> trackedProfileRecords,
        uint recordHash,
        long clanId,
        long membershipId,
        ulong guildId,
        GuildBroadcastsConfig broadcastsConfig,
        int? completedCount,
        CancellationToken cancellationToken)
    {
        if (broadcastsConfig.TitleTrackMode == EnabledMode.Enabled)
        {
            if (titleHashes.Any(x => x.titleHash == recordHash))
            {
                _broadcastsDbAccess.SendDestinyUserBroadcast(
                    CreateTitleTriumphBroadcast(recordHash, clanId, membershipId, guildId),
                    cancellationToken);
            }
            else if (titleHashes.Any(x => x.gildingHash == recordHash))
            {
                var broadcast = CreateTitleGildTriumphBroadcast(
                    recordHash,
                    clanId,
                    membershipId,
                    completedCount.GetValueOrDefault(),
                    guildId);
                if (broadcast is not null)
                    _broadcastsDbAccess.SendDestinyUserBroadcast(broadcast, cancellationToken);
            }

            return;
        }

        switch (broadcastsConfig.TriumphTrackMode)
        {
            case SettingsBroadcastMode.Manual:
                if (trackedProfileRecords.Contains(recordHash) || broadcastsConfig.TrackedTriumphs.Contains(recordHash))
                {
                    _broadcastsDbAccess.SendDestinyUserBroadcast(
                        CreateTriumphBroadcast(recordHash, clanId, membershipId, guildId),
                        cancellationToken);
                }
                break;
            case SettingsBroadcastMode.Curated:
                if (trackedProfileRecords.Contains(recordHash))
                {
                    _broadcastsDbAccess.SendDestinyUserBroadcast(
                        CreateTriumphBroadcast(recordHash, clanId, membershipId, guildId),
                        cancellationToken);
                }
                break;
        }
    }

    private void ProcessTriumphStepCompleted()
    {
    }

    private DestinyUserBroadcast CreateTriumphBroadcast(
        uint recordHash,
        long clanId,
        long membershipId,
        ulong guildId)
    {
        var broadcast = new DestinyUserBroadcast
        {
            DefinitionHash = recordHash,
            ClanId = clanId,
            Date = DateTime.UtcNow,
            GuildId = guildId,
            MembershipId = membershipId,
            Type = BroadcastType.Triumph,
            WasAnnounced = false
        };

        return broadcast;
    }

    private DestinyUserBroadcast CreateTitleTriumphBroadcast(
        uint recordHash,
        long clanId,
        long membershipId,
        ulong guildId)
    {
        var broadcast = new DestinyUserBroadcast
        {
            DefinitionHash = recordHash,
            ClanId = clanId,
            Date = DateTime.UtcNow,
            GuildId = guildId,
            MembershipId = membershipId,
            Type = BroadcastType.Title,
            WasAnnounced = false
        };

        return broadcast;
    }

    private DestinyUserBroadcast? CreateTitleGildTriumphBroadcast(
        uint recordHash,
        long clanId,
        long membershipId,
        int gildedCount,
        ulong guildId)
    {
        var parentTitleRecord = _bungieClient
            .Repository
            .Search<DestinyRecordDefinition>(
                BungieLocales.EN,
                def => ((DestinyRecordDefinition)def).TitleInfo?.GildingTrackingRecord.Hash == recordHash)
            .FirstOrDefault();

        if (parentTitleRecord is null)
            return null;

        var broadcastGuildId = guildId;
        var parentTitleHash = parentTitleRecord.Hash.ToString();

        var broadcast = new DestinyUserBroadcast
        {
            DefinitionHash = recordHash,
            ClanId = clanId,
            Date = DateTime.UtcNow,
            GuildId = broadcastGuildId,
            MembershipId = membershipId,
            Type = BroadcastType.GildedTitle,
            WasAnnounced = false,
            AdditionalData = new Dictionary<string, string>
            {
                { "parentTitleHash", parentTitleHash },
                { "gildedCount", gildedCount.ToString() }
            }
        };

        return broadcast;
    }

    // private DestinyUserBroadcast CreateRecordObjectiveCompletedBroadcast(
    //     uint recordHash,
    //     long clanId,
    //     long membershipId,
    //     GuildBroadcastsConfig broadcastsConfig,
    //     uint objectiveHash,
    //     UserObjectiveState state)
    // {
    //     var broadcast = new DestinyUserBroadcast
    //     {
    //         DefinitionHash = recordHash,
    //         ClanId = clanId,
    //         Date = DateTime.UtcNow,
    //         GuildId = broadcastsConfig.GuildId,
    //         MembershipId = membershipId,
    //         Type = BroadcastType.RecordStepObjectiveCompleted,
    //         WasAnnounced = false,
    //         AdditionalData = new Dictionary<string, string>
    //         {
    //             { "objectiveHash", objectiveHash.ToString() },
    //             { "value", state.Progress.ToString() }
    //         }
    //     };
    //
    //     return broadcast;
    // }
}