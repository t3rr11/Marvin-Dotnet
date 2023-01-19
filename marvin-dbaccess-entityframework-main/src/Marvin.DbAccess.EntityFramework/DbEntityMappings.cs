using Marvin.DbAccess.EntityFramework.Models;
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
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marvin.DbAccess.EntityFramework;

internal static class DbEntityMappings
{
    private static Dictionary<Type, object> _bindings;

    static DbEntityMappings()
    {
        _bindings = new Dictionary<Type, object>();

        RegisterBindings();
    }

    private static void RegisterBindings()
    {
        RegisterModel<ClanDbModel>();
        RegisterModel<ClanToScanDbModel>();
        RegisterModel<DestinyUserDbModel>();
        RegisterModel<GuildDbModel>();
        RegisterModel<RegisteredUserDbModel>();

        RegisterModel<TrackedCollectibleDbModel>();
        RegisterModel<TrackedMetricDbModel>();
        RegisterModel<TrackedProgressionDbModel>();
        RegisterModel<TrackedRecordDbModel>();
        
        RegisterModel<UserBroadcastDbModel>();
        RegisterModel<ClanBroadcastDbModel>();
    }
    
    private static void RegisterModel<T>() where T : class, IDbEntity<T>
    {
        _bindings.Add(typeof(T), T.GetBinder());
    }

    public static Action<EntityTypeBuilder<T>> GetBindAction<T>() where T : class
    {
        var type = typeof(T);

        var action = (Action<EntityTypeBuilder<T>>)_bindings[type];

        return action;
    }
}