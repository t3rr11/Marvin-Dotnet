using System.Text.Json.Serialization;

namespace Marvin.DbAccess.Models.User;

public class DestinyProfileComputedData
{
    [JsonPropertyName("lightLevel")] public int LightLevel { get; set; }

    [JsonPropertyName("artifactLevel")] public int ArtifactLevel { get; set; }

    [JsonPropertyName("totalLightLevel")] public int TotalLightLevel { get; set; }

    [JsonPropertyName("totalTitles")] public int TotalTitles { get; set; }

    [JsonPropertyName("titlesStatus")] public Dictionary<uint, int>? TitlesStatus { get; set; } = new();

    [JsonPropertyName("totalRaids")] public int TotalRaids { get; set; }

    [JsonPropertyName("raidCompletions")] public Dictionary<uint, int>? RaidCompletions { get; set; } = new();

    [JsonPropertyName("drystreaks")] public Dictionary<uint, int>? ItemDrystreaks { get; set; } = new();
}