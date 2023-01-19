using System.Text.Json;
using Npgsql.Internal;
using Npgsql.Internal.TypeHandling;

namespace Marvin.DbAccess.EntityFramework.DbTypeResolvers;

public class JsonOverrideTypeHandlerResolverFactory : TypeHandlerResolverFactory
{
    private readonly JsonSerializerOptions _options;

    public JsonOverrideTypeHandlerResolverFactory(JsonSerializerOptions options)
    {
        _options = options;
    }

    public override TypeHandlerResolver Create(NpgsqlConnector connector)
    {
        return new JsonOverrideTypeHandlerResolver(connector, _options);
    }

    public override string? GetDataTypeNameByClrType(Type clrType)
    {
        return null;
    }

    public override TypeMappingInfo? GetMappingByDataTypeName(string dataTypeName)
    {
        return null;
    }
}