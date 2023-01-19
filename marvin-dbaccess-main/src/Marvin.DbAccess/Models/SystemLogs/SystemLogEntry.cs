using Marvin.DbAccess.Attributes;

namespace Marvin.DbAccess.Models.SystemLogs;

public class SystemLogEntry<TPayload>
{
    /// <summary>
    ///     Time of this entry creation
    /// </summary>
    [DapperColumn("date")]
    public DateTime Date { get; set; }

    /// <summary>
    ///     Type for this entry
    /// </summary>
    [DapperColumn("log_type")]
    public SystemLogType LogType { get; set; }

    /// <summary>
    ///     Date payload of this entry, type should be determined by log type
    /// </summary>
    [DapperColumn("payload")]
    public TPayload Payload { get; set; }

    /// <summary>
    ///     Where this entry was sourced from
    /// </summary>
    [DapperColumn("source")]
    public string Source { get; set; }

    public static SystemLogEntry<TPayload> Create(TPayload payload, SystemLogType type, string source)
    {
        return new SystemLogEntry<TPayload>()
        {
            Payload = payload,
            LogType = type,
            Source = source,
            Date = DateTime.UtcNow
        };
    }
}