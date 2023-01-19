using System.Text.Json.Serialization;
using Marvin.DbAccess.EntityFramework.Models.ClanBroadcasts;
using Marvin.DbAccess.EntityFramework.Models.Clans;
using Marvin.DbAccess.EntityFramework.Models.ClansToScan;
using Marvin.DbAccess.EntityFramework.Models.DestinyUsers;
using Marvin.DbAccess.EntityFramework.Models.Guilds;
using Marvin.DbAccess.EntityFramework.Models.RegisteredUsers;
using Marvin.DbAccess.EntityFramework.Models.TrackedCollectibles;
using Marvin.DbAccess.EntityFramework.Models.TrackedMetrics;
using Marvin.DbAccess.EntityFramework.Models.TrackedProgressions;
using Marvin.DbAccess.EntityFramework.Models.TrackedRecords;
using Marvin.DbAccess.EntityFramework.Models.UserBroadcasts;

namespace Marvin.DbAccess.EntityFramework;

[JsonSerializable(typeof(GuildAnnouncementsConfigDbModel))]
[JsonSerializable(typeof(GuildBroadcastsConfigDbModel))]

[JsonSerializable(typeof(DestinyUserDbModel))]
[JsonSerializable(typeof(RegisteredUserDbModel))]

[JsonSerializable(typeof(UserBroadcastDbModel))]
[JsonSerializable(typeof(ClanBroadcastDbModel))]

[JsonSerializable(typeof(GuildDbModel))]
[JsonSerializable(typeof(ClanDbModel))]

[JsonSerializable(typeof(TrackedRecordDbModel))]
[JsonSerializable(typeof(TrackedProgressionDbModel))]
[JsonSerializable(typeof(TrackedMetricDbModel))]
[JsonSerializable(typeof(TrackedCollectibleDbModel))]

[JsonSerializable(typeof(ClanToScanDbModel))]
public partial class DbJsonSerializationContext : JsonSerializerContext
{
    
}