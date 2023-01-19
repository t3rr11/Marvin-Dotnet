using DotNetBungieAPI.Models.GroupsV2;
using Marvin.DbAccess.Models.Clan;
using Marvin.DbAccess.Models.Guild;

namespace Marvin.ProfileProcessors.Interfaces;

public interface IClanProcessor
{
    /// <summary>
    ///     Process and report any significant changes within clan (name, call sign, level)
    /// </summary>
    /// <param name="clanDbModel"></param>
    /// <param name="broadcastingSettings"></param>
    /// <param name="groupResponse"></param>
    /// <param name="cancellationToken"></param>
    void ProcessClanChanges(
        ClanDbModel clanDbModel,
        Dictionary<ulong, GuildBroadcastsConfig> broadcastingSettings,
        GroupResponse groupResponse,
        CancellationToken cancellationToken);
}