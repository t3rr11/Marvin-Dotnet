using DotNetBungieAPI.Models.Destiny.Definitions.Metrics;
using Marvin.DbAccess.Attributes;

namespace Marvin.DbAccess.Models.Tracking;

/// <summary>
///     Metadata for <see cref="DestinyMetricDefinition" /> reference
/// </summary>
[MapDapperProperties]
public class TrackedMetric
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
    ///     Whether this metric should be checked
    /// </summary>
    [DapperColumn("is_tracking")]
    public bool IsTracking { get; set; }
}