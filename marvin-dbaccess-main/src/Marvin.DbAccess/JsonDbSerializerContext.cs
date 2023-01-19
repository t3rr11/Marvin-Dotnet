using System.Text.Json.Serialization;
using Marvin.DbAccess.Models.BackendMetrics;
using Marvin.DbAccess.Models.Clan;
using Marvin.DbAccess.Models.Guild;
using Marvin.DbAccess.Models.User;

namespace Marvin.DbAccess;

[JsonSerializable(typeof(GuildBroadcastsConfig))]
[JsonSerializable(typeof(GuildAnnouncementConfig))]
[JsonSerializable(typeof(List<int>))]
[JsonSerializable(typeof(List<uint>))]
[JsonSerializable(typeof(List<long>))]
[JsonSerializable(typeof(List<ulong>))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(Dictionary<uint, bool>))]
[JsonSerializable(typeof(Dictionary<uint, int>))]
[JsonSerializable(typeof(Dictionary<uint, UserMetricData>))]
[JsonSerializable(typeof(Dictionary<uint, UserRecordData>))]
[JsonSerializable(typeof(Dictionary<uint, UserProgressionData>))]
[JsonSerializable(typeof(DestinyProfileComputedData))]
[JsonSerializable(typeof(BungieNetApiMetrics))]
[JsonSerializable(typeof(ClanBannerDataDbModel))]
internal partial class JsonDbSerializerContext : JsonSerializerContext
{
}