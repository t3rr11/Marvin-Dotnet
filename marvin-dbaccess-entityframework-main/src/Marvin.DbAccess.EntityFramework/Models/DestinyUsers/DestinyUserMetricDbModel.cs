using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Marvin.DbAccess.EntityFramework.Models.DestinyUsers;

#if DEBUG
[DebuggerDisplay("Progress = {Progress}")]
#endif
public class DestinyUserMetricDbModel
{
    [JsonPropertyName("progress")]
    public int Progress { get; set; }

}