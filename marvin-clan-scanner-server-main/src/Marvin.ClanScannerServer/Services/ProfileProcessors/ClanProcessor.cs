using DotNetBungieAPI.HashReferences;
using DotNetBungieAPI.Models.GroupsV2;
using Marvin.ClanScannerServer.Services.ProfileProcessors.Interfaces;
using Marvin.DbAccess.Models.Broadcasting;
using Marvin.DbAccess.Models.Clan;
using Marvin.DbAccess.Models.Guild;
using Marvin.DbAccess.Services.Interfaces;

namespace Marvin.ClanScannerServer.Services.ProfileProcessors;

public class ClanProcessor : IClanProcessor
{
    private readonly IBroadcastsDbAccess _broadcastsDbAccess;

    public ClanProcessor(IBroadcastsDbAccess broadcastsDbAccess)
    {
        _broadcastsDbAccess = broadcastsDbAccess;
    }

    public void ProcessClanChanges(
        ClanDbModel clanDbModel,
        Dictionary<ulong, GuildBroadcastsConfig>? broadcastingSettings,
        GroupResponse groupResponse,
        CancellationToken cancellationToken)
    {
        if (broadcastingSettings is null)
            return;
        
        foreach (var (guildId, broadcastsConfig) in broadcastingSettings)
        {
            if (broadcastsConfig.ClanTrackMode != EnabledMode.Enabled)
                continue;

            var currentClanLevel = groupResponse
                .Detail
                .ClanInfo
                .D2ClanProgressions[DefinitionHashes.Progressions.ClanLevel]
                .Level;

            if (clanDbModel.ClanLevel < currentClanLevel && 
                (currentClanLevel - clanDbModel.ClanLevel) == 1)
            {
                _broadcastsDbAccess.SendClanBroadcast(
                    new ClanBroadcast
                    {
                        GuildId = guildId,
                        ClanId = clanDbModel.ClanId,
                        Type = BroadcastType.ClanLevel,
                        Date = DateTime.UtcNow,
                        WasAnnounced = false,
                        OldValue = clanDbModel.ClanLevel.ToString(),
                        NewValue = currentClanLevel.ToString()
                    },
                    cancellationToken);
            }

            if (clanDbModel.ClanName != groupResponse.Detail.Name)
                _broadcastsDbAccess.SendClanBroadcast(
                    new ClanBroadcast
                    {
                        GuildId = guildId,
                        ClanId = clanDbModel.ClanId,
                        Type = BroadcastType.ClanName,
                        Date = DateTime.UtcNow,
                        WasAnnounced = false,
                        OldValue = clanDbModel.ClanName,
                        NewValue = groupResponse.Detail.Name
                    },
                    cancellationToken);

            if (clanDbModel.ClanCallsign != groupResponse.Detail.ClanInfo.ClanCallSign)
                _broadcastsDbAccess.SendClanBroadcast(
                    new ClanBroadcast
                    {
                        GuildId = guildId,
                        ClanId = clanDbModel.ClanId,
                        Type = BroadcastType.ClanCallSign,
                        Date = DateTime.UtcNow,
                        WasAnnounced = false,
                        OldValue = clanDbModel.ClanCallsign,
                        NewValue = groupResponse.Detail.ClanInfo.ClanCallSign
                    },
                    cancellationToken);
        }
    }

}