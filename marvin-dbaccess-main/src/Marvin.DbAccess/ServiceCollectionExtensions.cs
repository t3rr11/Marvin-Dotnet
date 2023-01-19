using Marvin.DbAccess.Services;
using Marvin.DbAccess.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Marvin.DbAccess;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPostgresqlDbAccess(
        this IServiceCollection serviceCollection)
    {
        DapperSqlMapper.RegisterTypes();

        return serviceCollection
            .AddSingleton<IGuildDbAccess, GuildDbAccess>()
            .AddSingleton<IClansDbAccess, ClansDbAccess>()
            .AddSingleton<IDestinyProfileDbAccess, DestinyProfileDbAccess>()
            .AddSingleton<ITrackedMetricsDbAccess, TrackedMetricsDbAccess>()
            .AddSingleton<ITrackedRecordsDbAccess, TrackedRecordsDbAccess>()
            .AddSingleton<ITrackedProgressionsDbAccess, TrackedProgressionsDbAccess>()
            .AddSingleton<ITrackedCollectibleDbAccess, TrackedCollectibleDbAccess>()
            .AddSingleton<ITrackedEntitiesDbAccess, TrackedEntitiesDbAccess>()
            .AddSingleton<ISystemLogsDbAccess, SystemLogsDbAccess>()
            .AddSingleton<IBroadcastsDbAccess, BroadcastsDbAccess>()
            .AddSingleton<IUserAccountDbAccess, UserAccountDbAccess>()
            .AddSingleton<IRawPostgresDbAccess, RawPostgresDbAccess>();
    }
}