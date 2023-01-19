using System.Text.Json.Serialization;
using Marvin.DbAccess.Attributes;

namespace Marvin.DbAccess.Models.Guild;

public class GuildAnnouncementConfig
{
    [JsonPropertyName("xur")]
    public EnabledMode Xur { get; set; }
    
    [JsonPropertyName("adas")]
    public EnabledMode Ada { get; set; }
    
    [JsonPropertyName("gunsmiths")]
    public EnabledMode Gunsmith { get; set; }
    
    [JsonPropertyName("channel_id")]
    public ulong? ChannelId { get; set; }
    
    [JsonPropertyName("wellspring")]
    public EnabledMode Wellspring { get; set; }
    
    [JsonPropertyName("lost_sectors")]
    public EnabledMode LostSectors { get; set; }
    
    [JsonPropertyName("is_announcing")]
    public bool IsAnnouncing { get; set; }
}