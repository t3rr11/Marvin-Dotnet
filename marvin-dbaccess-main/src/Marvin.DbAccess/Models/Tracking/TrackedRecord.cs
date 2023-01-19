using Marvin.DbAccess.Attributes;

namespace Marvin.DbAccess.Models.Tracking;

[MapDapperProperties]
public class TrackedRecord
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

    /// <summary>
    ///     Whether this record should be checked on a character
    /// </summary>
    [DapperColumn("character_scoped")]
    public bool IsCharacterScoped { get; set; }

    /// <summary>
    ///     Whether this entry completion should be reported to frontend
    /// </summary>
    [DapperColumn("is_reported")]
    public bool IsReported { get; set; }
}