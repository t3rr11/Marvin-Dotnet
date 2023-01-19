using Marvin.DbAccess.Models.Broadcasting;

namespace Marvin.DbAccess.Services.Interfaces;

public interface IBroadcastsDbAccess
{
    Task SendClanBroadcast(
        ClanBroadcast clanBroadcast,
        CancellationToken cancellationToken);

    Task SendDestinyUserBroadcast(
        DestinyUserBroadcast destinyUserBroadcast,
        CancellationToken cancellationToken);

    Task<IEnumerable<DestinyUserBroadcast>> GetAllDestinyUserBroadcastsAsync(
        CancellationToken cancellationToken);

    Task<IEnumerable<ClanBroadcast>> GetAllClanBroadcastsAsync(
        CancellationToken cancellationToken);
}