using System.Text.Json.Serialization;

namespace Marvin.DbAccess.EntityFramework.Models.Guilds;

public class GuildAnnouncementsConfigDbModel
{
    [JsonPropertyName("xur")] public TrackingMode Xur { get; set; }
    [JsonPropertyName("adas")] public TrackingMode Ada { get; set; }
    [JsonPropertyName("gunsmiths")] public TrackingMode Gunsmith { get; set; }
    [JsonPropertyName("channel_id")] public ulong? ChannelId { get; set; }
    [JsonPropertyName("wellspring")] public TrackingMode Wellspring { get; set; }
    [JsonPropertyName("lost_sectors")] public TrackingMode LostSectors { get; set; }
    [JsonPropertyName("is_announcing")] public bool IsAnnouncing { get; set; }
}