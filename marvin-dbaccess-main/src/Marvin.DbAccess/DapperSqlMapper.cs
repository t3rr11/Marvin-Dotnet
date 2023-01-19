using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Dapper;
using Marvin.DbAccess.Attributes;
using Marvin.DbAccess.DapperTypeHandlers;
using Marvin.DbAccess.Models.BackendMetrics;
using Marvin.DbAccess.Models.Clan;
using Marvin.DbAccess.Models.Guild;
using Marvin.DbAccess.Models.SystemLogs;
using Marvin.DbAccess.Models.User;

namespace Marvin.DbAccess;

public static class DapperSqlMapper
{
    public static void RegisterTypes()
    {
        MapAllTypesWithAttribute();

        var serializationOptions = new JsonSerializerOptions
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            TypeInfoResolver = JsonDbSerializerContext.Default
        };

        // System logs
        MapTypeProperties<SystemLogEntry<BungieNetApiMetrics>>();

        RegisterJsonContextHandler<ClanBannerDataDbModel>(serializationOptions);
        RegisterJsonContextHandler<GuildBroadcastsConfig>(serializationOptions);
        RegisterJsonContextHandler<GuildAnnouncementConfig>(serializationOptions);
        RegisterJsonContextHandler<List<int>>(serializationOptions);
        RegisterJsonContextHandler<List<uint>>(serializationOptions);
        RegisterJsonContextHandler<List<long>>(serializationOptions);
        RegisterJsonContextHandler<List<ulong>>(serializationOptions);
        RegisterJsonContextHandler<Dictionary<string, string>>(serializationOptions);
        RegisterJsonContextHandler<Dictionary<uint, bool>>(serializationOptions);
        RegisterJsonContextHandler<Dictionary<uint, int>>(serializationOptions);
        RegisterJsonContextHandler<Dictionary<uint, UserMetricData>>(serializationOptions);
        RegisterJsonContextHandler<Dictionary<uint, UserRecordData>>(serializationOptions);
        RegisterJsonContextHandler<Dictionary<uint, UserProgressionData>>(serializationOptions);
        RegisterJsonContextHandler<DestinyProfileComputedData>(serializationOptions);
        RegisterJsonContextHandler<BungieNetApiMetrics>(serializationOptions);
    }

    private static void MapAllTypesWithAttribute()
    {
        var assembly = Assembly.GetAssembly(typeof(DapperSqlMapper))!;
        var typesToMap = assembly
            .GetTypes()
            .Where(x => x.GetCustomAttribute<MapDapperPropertiesAttribute>() is not null);

        foreach (var type in typesToMap)
        {
            var map = new CustomPropertyTypeMap(type, (type, columnName) =>
            {
                var properties = type.GetProperties();
                foreach (var property in properties)
                {
                    var attribute = property.GetCustomAttribute<DapperColumnAttribute>();
                    if (attribute is null)
                        continue;
                    if (attribute.ColumnName == columnName) return property;
                }

                throw new Exception($"Couldn't find matching property for {columnName} in {type.Name}");
            });

            SqlMapper.SetTypeMap(type, map);
        }
    }

    private static void MapTypeProperties<TMappedType>()
    {
        var mappedType = typeof(TMappedType);
        var map = new CustomPropertyTypeMap(mappedType, (type, columnName) =>
        {
            var properties = type.GetProperties();
            for (var i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                var attribute = property.GetCustomAttribute<DapperColumnAttribute>();
                if (attribute is null)
                    continue;
                if (attribute.ColumnName == columnName) return property;
            }

            throw new Exception($"Couldn't find matching property for {columnName} in {type.Name}");
        });

        SqlMapper.SetTypeMap(mappedType, map);
    }

    private static void RegisterJsonContextHandler<THandledType>(JsonSerializerOptions serializationOptions)
    {
        SqlMapper.AddTypeHandler(typeof(THandledType), new JsonTypeHandler<THandledType>(serializationOptions));
    }

}