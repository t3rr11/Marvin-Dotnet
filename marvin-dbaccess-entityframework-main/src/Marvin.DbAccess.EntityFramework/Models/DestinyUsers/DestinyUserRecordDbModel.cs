using System.Text.Json.Serialization;
using DotNetBungieAPI.Models.Destiny;

namespace Marvin.DbAccess.EntityFramework.Models.DestinyUsers;

public class DestinyUserRecordDbModel
{
    public DestinyRecordState State { get; set; }

    /// <summary>
    ///     States of this record objectives
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<uint, DestinyUserRecordObjectiveStateDbModel>? Objectives { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? CompletedCount { get; set; }

}