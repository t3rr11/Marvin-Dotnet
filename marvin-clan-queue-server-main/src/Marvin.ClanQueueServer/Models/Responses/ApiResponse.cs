using System.Text.Json.Serialization;

namespace Marvin.ClanQueueServer.Models.Responses;

public class ApiResponse<TData>
{
    [JsonPropertyName("status")]
    public string Status { get; set; }
    
    [JsonPropertyName("code")]
    public ApiResponseCodes StatusCode { get; set; }
    
    [JsonPropertyName("message")]
    public string Message { get; set; }
    
    [JsonPropertyName("data")]
    public TData Data { get; set; }
}