using Marvin.DbAccess.Models.Guild;

namespace Marvin.ClanScannerServer.Models;

public class ClanUpdateContext
{
    public long ClanId { get; set; }
    public int MembersOnline { get; set; }
    public int MembersScanned { get; set; }
    public int MembersTotal { get; set; }
    public Dictionary<ulong, GuildBroadcastsConfig> BroadcastsConfigs { get; set; }
}