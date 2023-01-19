using Marvin.DbAccess.Attributes;

namespace Marvin.DbAccess.Models.Guild;

/// <summary>
///     Representation of how discord guild data is stored in DB
/// </summary>
[MapDapperProperties]
public class GuildDbModel
{
    /// <summary>
    ///     Discord guild ID
    /// </summary>
    [DapperColumn("guild_id")]
    public string GuildId { get; set; }

    /// <summary>
    ///     Discord guild name
    /// </summary>
    [DapperColumn("guild_name")]
    public string GuildName { get; set; }

    /// <summary>
    ///     Person who added the bot first or current discord guild owner ID
    /// </summary>
    [DapperColumn("owner_id")]
    public ulong? OwnerId { get; set; }

    /// <summary>
    ///     Discord guild owner avatar hash
    /// </summary>
    [DapperColumn("owner_avatar")]
    public string? AvatarHash { get; set; }

    /// <summary>
    ///     Whether Marvin is actually tracking this clan
    /// </summary>
    [DapperColumn("is_tracking")]
    public bool IsTracking { get; set; }

    /// <summary>
    ///     Clans that are linked to this discord guild
    /// </summary>
    [DapperColumn("clans")]
    public List<long>? ClanIds { get; set; }

    /// <summary>
    ///     Date when Marvin joined this discord guild
    /// </summary>
    [DapperColumn("joined_on")]
    public DateTime JoinedOn { get; set; }

    /// <summary>
    ///     Configuration related to broadcasting new items, records etc...
    /// </summary>
    [DapperColumn("broadcasts_config")]
    public GuildBroadcastsConfig? BroadcastsConfig { get; set; }
    
    [DapperColumn("announcements_config")]
    public GuildAnnouncementConfig? AnnouncementsConfig { get; set; }
    
    
}