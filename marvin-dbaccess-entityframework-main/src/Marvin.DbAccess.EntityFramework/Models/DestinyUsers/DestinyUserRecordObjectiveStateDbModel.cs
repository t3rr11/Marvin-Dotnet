using System.Text.Json.Serialization;

namespace Marvin.DbAccess.EntityFramework.Models.DestinyUsers;

public class DestinyUserRecordObjectiveStateDbModel
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Progress { get; set; }

    public bool IsComplete { get; set; }

}