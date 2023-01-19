using Marvin.DbAccess.Attributes;

namespace Marvin.DbAccess.Models.Broadcasting;

/// <summary>
///     Basic properties for any guild broadcast
/// </summary>
public abstract class BroadcastBase
{
    /// <summary>
    ///     Discord guild Id
    /// </summary>
    [DapperColumn("guild_id")]
    public ulong GuildId { get; set; }

    /// <summary>
    ///     Destiny clan Id
    /// </summary>
    [DapperColumn("clan_id")]
    public long ClanId { get; set; }

    /// <summary>
    ///     What type this broadcast is
    /// </summary>
    [DapperColumn("type")]
    public BroadcastType Type { get; set; }

    /// <summary>
    ///     Whether this broadcast has already been announced
    /// </summary>
    [DapperColumn("was_announced")]
    public bool WasAnnounced { get; set; }

    /// <summary>
    ///     Date when this broadcast was created
    /// </summary>
    [DapperColumn("date")]
    public DateTime Date { get; set; }
}