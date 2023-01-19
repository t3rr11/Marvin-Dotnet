using DotNetBungieAPI.Service.Abstractions;

namespace Marvin.DefinitionProvider.Postgresql;

public static class ConfigurationExtensions
{
    public static void UsePostgresqlDefinitionProvider(
        this IServiceConfigurator<IDefinitionProvider> serviceConfigurator,
        Action<PostgresqlDefinitionProviderConfiguration> configureAction)
    {
        serviceConfigurator.Use<PostgresqlDefinitionProvider, PostgresqlDefinitionProviderConfiguration>(
            configureAction);
    }
}