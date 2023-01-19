using System.Text.Json.Serialization;

namespace Marvin.DbAccess.Models.User;

/// <summary>
///     Data that can be used to get user current value of the metric
/// </summary>
public class UserMetricData
{
    /// <summary>
    ///     Metric progress
    /// </summary>
    [JsonPropertyName("progress")]
    public int Progress { get; set; }
}