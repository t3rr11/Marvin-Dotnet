using System.Text.Json.Serialization;

namespace Marvin.DbAccess.EntityFramework.Models.Guilds;

public class GuildBroadcastsConfigDbModel
{
    [JsonPropertyName("channel_id")] public string? ChannelId { get; set; }

    [JsonPropertyName("tracked_items")] public List<uint> TrackedItems { get; set; } = new();

    [JsonPropertyName("tracked_triumphs")] public List<uint> TrackedTriumphs { get; set; } = new();

    [JsonPropertyName("is_broadcasting")] public bool IsBroadcasting { get; set; }

    [JsonPropertyName("clan_track_mode")] public TrackingMode ClanTrackMode { get; set; }

    [JsonPropertyName("title_track_mode")] public TrackingMode TitleTrackMode { get; set; }

    [JsonPropertyName("item_track_mode")] public BroadcastMode ItemTrackMode { get; set; }

    [JsonPropertyName("triumph_track_mode")] public BroadcastMode TriumphTrackMode { get; set; }
}