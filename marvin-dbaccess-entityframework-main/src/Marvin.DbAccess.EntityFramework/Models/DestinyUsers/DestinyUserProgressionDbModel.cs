using System.Text.Json.Serialization;

namespace Marvin.DbAccess.EntityFramework.Models.DestinyUsers;

public class DestinyUserProgressionDbModel
{
    /// <summary>
    ///     Daily progress, if any exists
    /// </summary>
    [JsonPropertyName("dailyProgress")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? DailyProgress { get; set; }

    /// <summary>
    ///     Weekly progress, if any exists
    /// </summary>
    [JsonPropertyName("weeklyProgress")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? WeeklyProgress { get; set; }

    /// <summary>
    ///     Current progress
    /// </summary>
    [JsonPropertyName("currentProgress")]
    public int CurrentProgress { get; set; }

    /// <summary>
    ///     Current reset count
    /// </summary>
    [JsonPropertyName("currentResetCount")]
    public int? CurrentResetCount { get; set; }

    /// <summary>
    ///     Progression level
    /// </summary>
    [JsonPropertyName("level")]
    public int Level { get; set; }

}