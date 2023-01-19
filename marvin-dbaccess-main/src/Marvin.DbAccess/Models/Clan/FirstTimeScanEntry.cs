using Marvin.DbAccess.Attributes;

namespace Marvin.DbAccess.Models.Clan;

[MapDapperProperties]
public class FirstTimeScanEntry
{
    [DapperColumn("clan_id")] public long ClanId { get; set; }

    [DapperColumn("guild_id")] public ulong GuildId { get; set; }

    [DapperColumn("channel_id")] public ulong ChannelId { get; set; }
}