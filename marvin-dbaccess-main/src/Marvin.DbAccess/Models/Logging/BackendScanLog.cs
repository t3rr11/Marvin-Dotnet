using Marvin.DbAccess.Attributes;

namespace Marvin.DbAccess.Models.Logging;

public class BackendScanLog
{
    /// <summary>
    ///     Type of scan log - Patreon, General... etc.
    /// </summary>
    [DapperColumn("type")]
    public string Type { get; set; }

    /// <summary>
    ///     Scan time taken
    /// </summary>
    [DapperColumn("elapsed_time")]
    public string ElapsedTime { get; set; }
}