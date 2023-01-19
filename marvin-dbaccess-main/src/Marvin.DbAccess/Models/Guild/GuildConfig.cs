using Marvin.DbAccess.Attributes;

namespace Marvin.DbAccess.Models.Guild;

/// <summary>
///     Guild configs
/// </summary>
[MapDapperProperties]
public class GuildConfig
{
    /// <summary>
    ///     Discord guild Id
    /// </summary>
    [DapperColumn("guild_id")]
    public ulong GuildId { get; set; }

    /// <summary>
    ///     Discord guild name
    /// </summary>
    [DapperColumn("guild_name")]
    public string GuildName { get; set; }

    /// <summary>
    ///     Discord guild owner Id
    /// </summary>
    [DapperColumn("owner_id")]
    public ulong OwnerId { get; set; }

    /// <summary>
    ///     Discord guild owner avatar
    /// </summary>
    [DapperColumn("owner_avatar")]
    public string OwnerAvatar { get; set; }

    /// <summary>
    ///     Whether this guild is tracked
    /// </summary>
    [DapperColumn("is_tracking")]
    public bool IsTracking { get; set; }

    /// <summary>
    ///     Clans linked to this guild
    /// </summary>
    [DapperColumn("clans")]
    public List<long> Clans { get; set; }

    /// <summary>
    ///     When bot joined this discord guild
    /// </summary>
    [DapperColumn("joined_on")]
    public DateTime JoinedOn { get; set; }
}