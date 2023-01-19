using DotNetBungieAPI;
using DotNetBungieAPI.Models;
using DotNetBungieAPI.Models.Applications;
using DotNetBungieAPI.Models.Destiny;
using DotNetBungieAPI.Service.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Marvin.DefinitionProvider.Postgresql.Tests;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services
            .UseBungieApiClient(builder =>
            {
                builder.ClientConfiguration.ApiKey = "";
                builder.ClientConfiguration.CacheDefinitions = true;
                builder.ClientConfiguration.ClientId = 0;
                builder.ClientConfiguration.ClientSecret = "";

                builder.ClientConfiguration.ApplicationScopes = ApplicationScopes.ReadUserData |
                                                                ApplicationScopes.ReadBasicUserProfile |
                                                                ApplicationScopes.MoveEquipDestinyItems |
                                                                ApplicationScopes.AdminGroups;
                builder.ClientConfiguration.UsedLocales.Add(BungieLocales.EN);
                builder.DefinitionProvider.UsePostgresqlDefinitionProvider(options =>
                {
                    options.ConnectionString = "";
                    options.AutoUpdateOnStartup = false;
                    options.MaxAmountOfLeftoverManifests = 5;
                    options.CleanUpOldManifestsAfterUpdate = true;
                    
                    options.DefinitionsToLoad.Add(DefinitionsEnum.DestinyRecordDefinition);
                    options.DefinitionsToLoad.Add(DefinitionsEnum.DestinyInventoryItemDefinition);
                    options.DefinitionsToLoad.Add(DefinitionsEnum.DestinyPresentationNodeDefinition);
                });
            });

        services.BuildServiceProvider().GetRequiredService<IBungieClient>();
    }
}