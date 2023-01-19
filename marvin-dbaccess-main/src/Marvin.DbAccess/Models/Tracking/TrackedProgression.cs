using Marvin.DbAccess.Attributes;

namespace Marvin.DbAccess.Models.Tracking;

[MapDapperProperties]
public class TrackedProgression
{
    /// <summary>
    ///     Definition hash
    /// </summary>
    [DapperColumn("hash")]
    public long Hash { get; set; }

    /// <summary>
    ///     Display name
    /// </summary>
    [DapperColumn("display_name")]
    public string DisplayName { get; set; }

    /// <summary>
    ///     Whether this progression should be checked
    /// </summary>
    [DapperColumn("is_tracking")]
    public bool IsTracking { get; set; }
}