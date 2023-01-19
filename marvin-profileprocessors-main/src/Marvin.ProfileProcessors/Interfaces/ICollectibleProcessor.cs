using DotNetBungieAPI.Models.Destiny.Responses;
using Marvin.DbAccess.Models.Guild;
using Marvin.DbAccess.Models.User;

namespace Marvin.ProfileProcessors.Interfaces;

public interface ICollectibleProcessor
{
    /// <summary>
    ///     Updates and reports all collectible for specified user within all guilds
    /// </summary>
    /// <param name="broadcastsConfigs"></param>
    /// <param name="destinyProfile"></param>
    /// <param name="destinyProfileResponse"></param>
    /// <param name="curatedCollectibles"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    void UpdateAndReportCollectibles(
        Dictionary<ulong, GuildBroadcastsConfig> broadcastsConfigs,
        DestinyProfile destinyProfile,
        DestinyProfileResponse destinyProfileResponse,
        List<uint> curatedCollectibles,
        CancellationToken cancellationToken);

    /// <summary>
    ///     Updates user collectibles
    /// </summary>
    /// <param name="broadcastsConfigs"></param>
    /// <param name="destinyProfile"></param>
    /// <param name="destinyProfileResponse"></param>
    /// <param name="curatedCollectibles"></param>
    void UpdateCollectibles(
        DestinyProfile destinyProfile,
        DestinyProfileResponse destinyProfileResponse,
        List<uint> curatedCollectibles);
}