using DotNetBungieAPI.Models.GroupsV2;
using Marvin.DbAccess.Models.Clan;
using Marvin.DbAccess.Models.Guild;

namespace Marvin.DbAccess.Services.Interfaces;

public interface IClansDbAccess
{
    ValueTask<IEnumerable<ClanDbModel>> GetClan(
        long groupId,
        CancellationToken cancellationToken);

    ValueTask<IEnumerable<ClanDbModel>> UpdateClan(
        GroupResponse groupResponse,
        int onlineUsers,
        CancellationToken cancellationToken);

    ValueTask<IEnumerable<FirstTimeScanEntry>> GetClansForFirstTimeScanning(
        CancellationToken cancellationToken);

    Task DeleteClanFromFirstScanList(
        long clanId,
        CancellationToken cancellationToken);

    ValueTask<IEnumerable<long>> GetGeneralClanIdsForScanning(
        CancellationToken cancellationToken);

    ValueTask<IEnumerable<long>> GetPatreonClanIdsForScanning(
        CancellationToken cancellationToken);

    Task SetClanForcedFlagToFalse(
        long clanId,
        CancellationToken cancellationToken);

    ValueTask<Dictionary<ulong, GuildBroadcastsConfig>> GetAllLinkedDiscordGuilds(
        long groupId,
        CancellationToken cancellationToken);
    
    Task<Dictionary<long, string>> GetClanNamesAsync(
        List<long> clanIds,
        CancellationToken cancellationToken);

    Task<Dictionary<long, string>> GetClanUserNamesAsync(
        long clanId,
        CancellationToken cancellationToken);

    Task UpsertClanAsync(
        ClanDbModel clanDbModel,
        CancellationToken cancellationToken);

    Task MarkClanAsDeletedAsync(
        ClanDbModel clanDbModel,
        CancellationToken cancellationToken);
}