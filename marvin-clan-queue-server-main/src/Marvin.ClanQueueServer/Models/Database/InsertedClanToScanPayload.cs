using System.Text.Json.Serialization;

namespace Marvin.ClanQueueServer.Models.Database;

public class InsertedClanToScanPayload : IEquatable<InsertedClanToScanPayload>
{
    /// <summary>
    ///     Bungie.net clan id
    /// </summary>
    [JsonPropertyName("clan_id")]
    public long ClanId { get; init; }

    /// <summary>
    ///     Discord guild id
    /// </summary>
    [JsonPropertyName("guild_id")]
    public ulong GuildId { get; init; }

    /// <summary>
    ///     Discord channel id to report back to
    /// </summary>
    [JsonPropertyName("channel_id")]
    public ulong? ChannelId { get; init; }

    public override bool Equals(object? obj)
    {
        return obj is InsertedClanToScanPayload payload &&
               ClanId == payload.ClanId &&
               GuildId == payload.GuildId &&
               ChannelId == payload.ChannelId;
    }

    public bool Equals(InsertedClanToScanPayload? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return ClanId == other.ClanId && GuildId == other.GuildId && ChannelId == other.ChannelId;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ClanId, GuildId, ChannelId);
    }
}