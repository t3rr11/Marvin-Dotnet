using Marvin.DbAccess.Attributes;

namespace Marvin.DbAccess.Models.Broadcasting;

/// <summary>
///     Destiny user broadcast with essential data
/// </summary>
[MapDapperProperties]
public class DestinyUserBroadcast : BroadcastBase
{
    /// <summary>
    ///     Destiny membership
    /// </summary>
    [DapperColumn("membership_id")]
    public long MembershipId { get; set; }

    /// <summary>
    ///     Destiny definition hash to be reported
    /// </summary>
    [DapperColumn("hash")]
    public long DefinitionHash { get; set; }

    /// <summary>
    ///     This can be used to store any generic data you might ever need in this life, just don't abuse this too much
    ///     <para />
    ///     Example: store completions of activity while reporting raid exotic acquisition ("raidCompletions": 10)
    /// </summary>
    [DapperColumn("additional_data")]
    public Dictionary<string, string> AdditionalData { get; set; }
}