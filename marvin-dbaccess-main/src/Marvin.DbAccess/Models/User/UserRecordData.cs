using System.Text.Json.Serialization;
using DotNetBungieAPI.Models.Destiny;

namespace Marvin.DbAccess.Models.User;

/// <summary>
///     Data that can be used to get user current records
/// </summary>
public class UserRecordData
{
    /// <summary>
    ///     State of this record
    /// </summary>
    public DestinyRecordState State { get; set; }

    /// <summary>
    ///     States of this record objectives
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<uint, UserObjectiveState>? Objectives { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? CompletedCount { get; set; }
}