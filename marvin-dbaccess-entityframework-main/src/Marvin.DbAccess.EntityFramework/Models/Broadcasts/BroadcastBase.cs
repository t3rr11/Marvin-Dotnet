using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marvin.DbAccess.EntityFramework.Models.Broadcasts;

public abstract class BroadcastBase
{
    /// <summary>
    ///     Discord guild Id
    /// </summary>
    public ulong GuildId { get; set; }

    /// <summary>
    ///     Destiny clan Id
    /// </summary>
    public long ClanId { get; set; }

    /// <summary>
    ///     What type this broadcast is
    /// </summary>
    public BroadcastType Type { get; set; }

    /// <summary>
    ///     Whether this broadcast has already been announced
    /// </summary>
    public bool WasAnnounced { get; set; }

    /// <summary>
    ///     Date when this broadcast was created
    /// </summary>
    public DateTime Date { get; set; }
}