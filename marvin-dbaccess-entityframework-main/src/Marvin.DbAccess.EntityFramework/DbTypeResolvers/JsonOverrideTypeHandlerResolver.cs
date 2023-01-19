using System.Text.Json;
using Npgsql.Internal;
using Npgsql.Internal.TypeHandlers;
using Npgsql.Internal.TypeHandling;

namespace Marvin.DbAccess.EntityFramework.DbTypeResolvers;

public class JsonOverrideTypeHandlerResolver : TypeHandlerResolver
{
    private readonly JsonHandler _jsonbHandler;

    internal JsonOverrideTypeHandlerResolver(
        NpgsqlConnector connector,
        JsonSerializerOptions options)
    {
        _jsonbHandler ??= new JsonHandler(
            connector.DatabaseInfo.GetPostgresTypeByName("jsonb"),
            connector.TextEncoding,
            isJsonb: true,
            options);
    }

    public override NpgsqlTypeHandler? ResolveByDataTypeName(string typeName)
    {
        return typeName == "jsonb" ? _jsonbHandler : null;
    }

    public override NpgsqlTypeHandler? ResolveByClrType(Type type)
    {
        return type == typeof(JsonDocument)
            ? _jsonbHandler
            : null;
    }

    public override TypeMappingInfo? GetMappingByDataTypeName(string dataTypeName)
    {
        return null;
    }
}