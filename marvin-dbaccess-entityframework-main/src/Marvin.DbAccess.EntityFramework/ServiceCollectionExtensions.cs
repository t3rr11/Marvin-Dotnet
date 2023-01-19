using System.Text.Json;
using Marvin.DbAccess.EntityFramework.DbContext;
using Marvin.DbAccess.EntityFramework.DbContext.Interfaces;
using Marvin.DbAccess.EntityFramework.DbTypeResolvers;
using Marvin.DbAccess.EntityFramework.Services;
using Marvin.DbAccess.EntityFramework.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Marvin.DbAccess.EntityFramework;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMarvinDbContext(this IServiceCollection serviceCollection)
    {
        var options = new JsonSerializerOptions
        {
        };
        
        options.AddContext<DbJsonSerializationContext>();
        
        NpgsqlConnection.GlobalTypeMapper.AddTypeResolverFactory(new JsonOverrideTypeHandlerResolverFactory(options));

        return serviceCollection
            .AddSingleton<IDbContextCaller, DbContextCaller>()
            .AddTransient(typeof(IDbContext<>), typeof(SingleSetDbContext<>))
            .AddTransient(typeof(IDbContext<,>), typeof(DoubleSetDbContext<,>))
            .AddDbContext<NullDbContext>();
    }
}