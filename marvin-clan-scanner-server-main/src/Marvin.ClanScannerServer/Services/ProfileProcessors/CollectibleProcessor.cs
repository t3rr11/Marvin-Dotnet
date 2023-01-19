using DotNetBungieAPI.HashReferences;
using DotNetBungieAPI.Models.Destiny;
using DotNetBungieAPI.Models.Destiny.Responses;
using Marvin.ClanScannerServer.Services.ProfileProcessors.Interfaces;
using Marvin.DbAccess.Models.Broadcasting;
using Marvin.DbAccess.Models.Guild;
using Marvin.DbAccess.Models.User;
using Marvin.DbAccess.Services.Interfaces;

namespace Marvin.ClanScannerServer.Services.ProfileProcessors;

public class CollectibleProcessor : ICollectibleProcessor
{
    private readonly IBroadcastsDbAccess _broadcastsDbAccess;

    public CollectibleProcessor(
        IBroadcastsDbAccess broadcastsDbAccess)
    {
        _broadcastsDbAccess = broadcastsDbAccess;
    }

    public void UpdateAndReportCollectibles(
        Dictionary<ulong, GuildBroadcastsConfig> broadcastsConfigs,
        DestinyProfile destinyProfile,
        DestinyProfileResponse destinyProfileResponse,
        List<uint> curatedCollectibles,
        CancellationToken cancellationToken)
    {
        if (destinyProfileResponse.ProfileCollectibles?.Data is null) return;

        var acquiredCollectibleHashes = destinyProfileResponse
            .ProfileCollectibles
            .Data
            .Collectibles
            // faster enum check than Enum.HasFlag(Enum value)
            .Where(x => (x.Value.State & DestinyCollectibleState.NotAcquired) == 0)
            // fetch hash directly
            .Select(x => x.Key.Hash.GetValueOrDefault());

        foreach (var hash in acquiredCollectibleHashes)
        {
            if (destinyProfile.Items.Contains(hash))
                continue;

            destinyProfile.Items.Add(hash);
            foreach (var (guildId, guildBroadcastsConfig) in broadcastsConfigs)
                ProcessCollectibleAcquisition(
                    destinyProfile,
                    hash,
                    destinyProfile.ClanId.GetValueOrDefault(),
                    destinyProfile.MembershipId,
                    curatedCollectibles,
                    guildId,
                    guildBroadcastsConfig,
                    cancellationToken);
        }
        
        destinyProfile.RecentItems = destinyProfileResponse
            .ProfileCollectibles
            .Data
            .RecentCollectibles
            .Select(x => x.Hash.GetValueOrDefault())
            .ToList();

        var characterCollectiblesData = destinyProfileResponse
            .CharacterCollectibles
            .Data;

        if (characterCollectiblesData.Count == 0)
            return;

        var characterCollectibles = characterCollectiblesData.FirstOrDefault();

        foreach (var destinyCollectibleComponent in characterCollectibles.Value.Collectibles)
        {
            var collectibleHash = destinyCollectibleComponent.Key.Hash.GetValueOrDefault();

            if (!characterCollectiblesData.Values.Any(x =>
                {
                    if (x.Collectibles.TryGetValue(collectibleHash, out var collectibleComponent))
                    {
                        return (collectibleComponent.State & DestinyCollectibleState.NotAcquired) == 0;
                    }

                    return false;
                }))
                continue;

            if (destinyProfile.Items.Contains(collectibleHash))
                continue;

            destinyProfile.Items.Add(collectibleHash);
            
            foreach (var (guildId, guildBroadcastsConfig) in broadcastsConfigs)
                ProcessCollectibleAcquisition(
                    destinyProfile,
                    collectibleHash,
                    destinyProfile.ClanId.GetValueOrDefault(),
                    destinyProfile.MembershipId,
                    curatedCollectibles,
                    guildId,
                    guildBroadcastsConfig,
                    cancellationToken);
        }
    }

    public void UpdateCollectibles(
        DestinyProfile destinyProfile,
        DestinyProfileResponse destinyProfileResponse,
        List<uint> curatedCollectibles)
    {
        if (destinyProfileResponse.ProfileCollectibles?.Data is null) return;

        var acquiredCollectibleHashes = destinyProfileResponse
            .ProfileCollectibles
            .Data
            .Collectibles
            // faster enum check than Enum.HasFlag(Enum value)
            .Where(x => (x.Value.State & DestinyCollectibleState.NotAcquired) == 0)
            // fetch hash directly
            .Select(x => x.Key.Hash.GetValueOrDefault());

        foreach (var hash in acquiredCollectibleHashes)
        {
            if (destinyProfile.Items.Contains(hash))
                continue;

            destinyProfile.Items.Add(hash);
        }

        destinyProfile.RecentItems = destinyProfileResponse
            .ProfileCollectibles
            .Data
            .RecentCollectibles
            .Select(x => x.Hash.GetValueOrDefault())
            .ToList();

        var characterCollectiblesData = destinyProfileResponse
            .CharacterCollectibles
            .Data;

        if (characterCollectiblesData.Count == 0)
            return;

        var characterCollectibles = characterCollectiblesData.FirstOrDefault();

        foreach (var destinyCollectibleComponent in characterCollectibles.Value.Collectibles)
        {
            var collectibleHash = destinyCollectibleComponent.Key.Hash.GetValueOrDefault();

            if (!characterCollectiblesData.Values.Any(x =>
                {
                    if (x.Collectibles.TryGetValue(collectibleHash, out var collectibleComponent))
                    {
                        return (collectibleComponent.State & DestinyCollectibleState.NotAcquired) == 0;
                    }

                    return false;
                }))
                continue;

            if (destinyProfile.Items.Contains(collectibleHash))
                continue;

            destinyProfile.Items.Add(collectibleHash);
        }
    }

    private void ProcessCollectibleAcquisition(
        DestinyProfile destinyProfile,
        uint collectibleHash,
        long clanId,
        long membershipId,
        List<uint> curatedCollectibles,
        ulong guildId,
        GuildBroadcastsConfig broadcastsConfig,
        CancellationToken cancellationToken)
    {
        switch (broadcastsConfig.ItemTrackMode)
        {
            case SettingsBroadcastMode.Curated:
                if (curatedCollectibles.Contains(collectibleHash) ||
                    broadcastsConfig.TrackedItems.Contains(collectibleHash))
                    _broadcastsDbAccess.SendDestinyUserBroadcast(
                        CreateNewCollectibleBroadcast(
                            destinyProfile,
                            collectibleHash,
                            clanId,
                            membershipId,
                            guildId,
                            broadcastsConfig),
                        cancellationToken);

                break;
            case SettingsBroadcastMode.Manual:
                if (broadcastsConfig.TrackedItems.Contains(collectibleHash))
                    _broadcastsDbAccess.SendDestinyUserBroadcast(
                        CreateNewCollectibleBroadcast(
                            destinyProfile,
                            collectibleHash,
                            clanId,
                            membershipId,
                            guildId,
                            broadcastsConfig),
                        cancellationToken);

                break;
            case SettingsBroadcastMode.None:
            case SettingsBroadcastMode.Disabled:
            default: return;
        }
    }

    private DestinyUserBroadcast CreateNewCollectibleBroadcast(
        DestinyProfile destinyProfile,
        uint collectibleHash,
        long clanId,
        long membershipId,
        ulong guildId,
        GuildBroadcastsConfig broadcastsConfig)
    {
        var broadcast = new DestinyUserBroadcast
        {
            DefinitionHash = collectibleHash,
            ClanId = clanId,
            Date = DateTime.UtcNow,
            GuildId = guildId,
            MembershipId = membershipId,
            Type = BroadcastType.Collectible,
            WasAnnounced = false
        };

        switch (collectibleHash)
        {
            case DefinitionHashes.Collectibles.OneThousandVoices:
                broadcast.AdditionalData = new Dictionary<string, string>
                {
                    {
                        "completions",
                        (destinyProfile.Metrics.TryGetValue(
                            DefinitionHashes.Metrics.LastWishCompletions,
                            out var lastWishCompletions)
                            ? lastWishCompletions.Progress
                            : 0)
                        .ToString()
                    }
                };
                break;

            case DefinitionHashes.Collectibles.EyesofTomorrow:
                broadcast.AdditionalData = new Dictionary<string, string>
                {
                    {
                        "completions",
                        (destinyProfile.Metrics.TryGetValue(
                            DefinitionHashes.Metrics.DeepStoneCryptCompletions,
                            out var dscCompletions)
                            ? dscCompletions.Progress
                            : 0)
                        .ToString()
                    }
                };
                break;

            case DefinitionHashes.Collectibles.VexMythoclast:
                broadcast.AdditionalData = new Dictionary<string, string>
                {
                    {
                        "completions",
                        (destinyProfile.Metrics.TryGetValue(
                            DefinitionHashes.Metrics.VaultofGlassCompletions,
                            out var vogCompletions)
                            ? vogCompletions.Progress
                            : 0)
                        .ToString()
                    }
                };
                break;

            case DefinitionHashes.Collectibles.CollectiveObligation:
                broadcast.AdditionalData = new Dictionary<string, string>
                {
                    {
                        "completions",
                        (destinyProfile.Metrics.TryGetValue(
                            DefinitionHashes.Metrics.VowoftheDiscipleCompletions,
                            out var vowCompletions)
                            ? vowCompletions.Progress
                            : 0)
                        .ToString()
                    }
                };
                break;

            case DefinitionHashes.Collectibles.InMemoriamShell:
                broadcast.AdditionalData = new Dictionary<string, string>
                {
                    {
                        "completions",
                        (destinyProfile.Metrics.TryGetValue(
                            DefinitionHashes.Metrics.Wins_1365664208,
                            out var trialWins)
                            ? trialWins.Progress
                            : 0)
                        .ToString()
                    }
                };
                break;

            case DefinitionHashes.Collectibles.Heartshadow:
                broadcast.AdditionalData = new Dictionary<string, string>
                {
                    {
                        "completions",
                        (destinyProfile.Metrics.TryGetValue(
                            DefinitionHashes.Metrics.DualityCompletions,
                            out var dualityCompletions)
                            ? dualityCompletions.Progress
                            : 0)
                        .ToString()
                    }
                };
                break;
            
            case DefinitionHashes.Collectibles.TouchofMalice:
                broadcast.AdditionalData = new Dictionary<string, string>
                {
                    {
                        "completions",
                        (destinyProfile.Metrics.TryGetValue(
                            DefinitionHashes.Metrics.KingsFallCompletions,
                            out var kingsFallCompletions)
                            ? kingsFallCompletions.Progress
                            : 0)
                        .ToString()
                    }
                };
                break;
            case 3558330464: // Spire of the Watcher
                broadcast.AdditionalData = new Dictionary<string, string>
                {
                    {
                        "completions",
                        (destinyProfile.Metrics.TryGetValue(
                            3702217360,
                            out var sotwCompletions)
                            ? sotwCompletions.Progress
                            : 0)
                        .ToString()
                    }
                };
                break;
        }

        return broadcast;
    }

}