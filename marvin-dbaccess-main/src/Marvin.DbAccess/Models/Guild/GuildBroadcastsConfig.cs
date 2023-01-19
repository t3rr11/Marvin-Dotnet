using System.Text.Json.Serialization;
using Marvin.DbAccess.Attributes;
using Marvin.DbAccess.Models.Broadcasting;

namespace Marvin.DbAccess.Models.Guild;

/// <summary>
///     Config that holds settings for which categories should be broadcasted in discord guild
/// </summary>
public class GuildBroadcastsConfig
{
    /// <summary>
    ///     Discord guild channel Id
    /// </summary>
    [JsonPropertyName("channel_id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public ulong? ChannelId { get; set; }

    /// <summary>
    ///     Whether to use curated or manual item list
    /// </summary>
    [JsonPropertyName("item_track_mode")]
    public SettingsBroadcastMode ItemTrackMode { get; set; }

    /// <summary>
    ///     List of manually picked items to track
    /// </summary>
    [JsonPropertyName("tracked_items")]
    public List<uint> TrackedItems { get; set; }
    
    /// <summary>
    ///     List of manually picked triumphs to track
    /// </summary>
    [JsonPropertyName("tracked_triumphs")]
    public List<uint> TrackedTriumphs { get; set; }

    /// <summary>
    ///     Whether to use curated or manual title list
    /// </summary>
    [JsonPropertyName("title_track_mode")]
    public EnabledMode TitleTrackMode { get; set; }

    /// <summary>
    ///     Whether to send clan updates or not
    /// </summary>
    [JsonPropertyName("clan_track_mode")]
    public EnabledMode ClanTrackMode { get; set; }

    /// <summary>
    ///     Whether this guild should produce any broadcasts at all
    /// </summary>
    [JsonPropertyName("is_broadcasting")]
    public bool IsBroadcasting { get; set; }
    
    /// <summary>
    ///     Whether to use curated or manual title list
    /// </summary>
    [JsonPropertyName("triumph_track_mode")]
    public SettingsBroadcastMode TriumphTrackMode { get; set; }
}