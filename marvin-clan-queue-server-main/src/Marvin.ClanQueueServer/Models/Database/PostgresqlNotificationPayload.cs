using System.Text.Json.Serialization;

namespace Marvin.ClanQueueServer.Models.Database;

public class PostgresqlNotificationPayload<T>
{
    /// <summary>
    ///     Table from which this payload was received from
    /// </summary>
    [JsonPropertyName("table")]
    public string Table { get; set; }

    /// <summary>
    ///     Executed action
    /// </summary>
    [JsonPropertyName("action")]
    public string Action { get; set; }

    /// <summary>
    ///     Data of payload
    /// </summary>
    [JsonPropertyName("data")]
    public T Data { get; set; }
}