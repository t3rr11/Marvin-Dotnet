using Marvin.DbAccess.Attributes;

namespace Marvin.DbAccess.Models.User;

[MapDapperProperties]
public class DestinyProfileClanMemberReference
{
    /// <summary>
    ///     Clan that this profile is linked to
    /// </summary>
    [DapperColumn("clan_id")]
    public long ClanId { get; set; }

    [DapperColumn("membership_id")] public long MembershipId { get; set; }
}