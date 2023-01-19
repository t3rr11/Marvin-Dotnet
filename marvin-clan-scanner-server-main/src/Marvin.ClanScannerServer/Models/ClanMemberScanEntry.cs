using DotNetBungieAPI.Models.GroupsV2;

namespace Marvin.ClanScannerServer.Models;

public class ClanMemberScanEntry
{
    public ClanUpdateContext UpdateContext { get; init; }
    public GroupMember Member { get; init; }
    public CancellationToken CancellationToken { get; set; }
    public bool Silent { get; init; }
}