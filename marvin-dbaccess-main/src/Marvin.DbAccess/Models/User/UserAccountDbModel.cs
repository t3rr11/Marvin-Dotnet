using DotNetBungieAPI.Models;
using Marvin.DbAccess.Attributes;

namespace Marvin.DbAccess.Models.User;

/// <summary>
///     Basic data for registered user account
/// </summary>
[MapDapperProperties]
public class UserAccountDbModel
{
    /// <summary>
    ///     Registered user discord Id
    /// </summary>
    [DapperColumn("user_id")]
    public string DiscordId { get; set; }

    /// <summary>
    ///     Discord nickname
    /// </summary>
    [DapperColumn("username")]
    public string Username { get; set; }

    /// <summary>
    ///     Destiny membership Id
    /// </summary>
    [DapperColumn("membership_id")]
    public long MembershipId { get; set; }

    /// <summary>
    ///     Destiny platform type
    /// </summary>
    [DapperColumn("platform")]
    public BungieMembershipType Platform { get; set; }

    /// <summary>
    ///     Account creation date
    /// </summary>
    [DapperColumn("created_at")]
    public DateTime CreatedAt { get; set; }
}