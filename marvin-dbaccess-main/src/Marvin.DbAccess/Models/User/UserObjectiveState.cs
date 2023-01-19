using System.Text.Json.Serialization;

namespace Marvin.DbAccess.Models.User;

public class UserObjectiveState
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Progress { get; set; }

    public bool IsComplete { get; set; }
}