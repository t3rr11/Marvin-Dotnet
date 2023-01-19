using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DotNetBungieAPI;
using DotNetBungieAPI.DefinitionProvider.Sqlite;
using DotNetBungieAPI.Models;
using DotNetBungieAPI.Models.Destiny;
using Marvin.Application.Sdk;
using Marvin.Bot.Extensions;
using Marvin.Bot.Models;
using Marvin.Bot.Options;
using Marvin.Bot.Services;
using Marvin.Bot.Services.Hosted;
using Marvin.Bot.Services.Interfaces;
using Marvin.DbAccess.EntityFramework;
using Marvin.DbAccess.Options;
using Marvin.DefinitionProvider.Postgresql;

namespace Marvin.Bot;

internal static class Program
{
    public static async Task Main(params string[] args)
    {
        // Setup the Marvin SDK
        using (var appBuilder = MarvinApplicationBuilder.Create(args))
        {
            appBuilder.AddDefaultServices();
            appBuilder
                .ConfigureLogging()
                .AddConsole()
                .AddElasticsearch()
                .Apply();

            var client = new DiscordShardedClient(new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.Guilds
            });

            appBuilder.ApplicationBuilder.Services
                .Configure<DiscordOptions>(appBuilder.Configuration.GetSection("DiscordOptions"))
                .Configure<DatabaseOptions>(appBuilder.Configuration.GetSection("DatabaseOptions"))
                .Configure<BungieNetOptions>(appBuilder.Configuration.GetSection("BungieNetOptions"))
                
                .AddHostedServiceWithInterface<ISystemsStatusService, StartupService>()
                .AddHostedService<BackgroundBroadcastService>()
                
                .AddSingleton(client)
                .AddSingleton(new InteractionService(client))
                .UseBungieApiClient(bungieClientBuilder =>
                {
                    bungieClientBuilder.ClientConfiguration.ApiKey = appBuilder
                        .Configuration
                        .GetSection("BungieNetOptions:ApiKey")
                        .Value!;
                    
                    bungieClientBuilder.ClientConfiguration.UsedLocales.Add(BungieLocales.EN);
                    if (appBuilder.Environment.IsDevelopment())
                    {
                        bungieClientBuilder.DefinitionProvider.UseSqliteDefinitionProvider(provider =>
                        {
                            provider.ManifestFolderPath = appBuilder
                                .Configuration
                                .GetSection("BungieNetOptions:ManifestPath")
                                .Value!;
                            provider.AutoUpdateManifestOnStartup = true;
                            provider.DeleteOldManifestDataAfterUpdates = false;
                            provider.FetchLatestManifestOnInitialize = false;
                        });
                    }
                    else
                    {
                        bungieClientBuilder.DefinitionProvider.UsePostgresqlDefinitionProvider(provider =>
                        {
                            provider.ConnectionString = appBuilder
                                .Configuration
                                .GetSection("DatabaseOptions:ConnectionString")
                                .Value!;

                            provider.DefinitionsToLoad.AddRange(new[]
                            {
                                DefinitionsEnum.DestinyActivityDefinition,
                                DefinitionsEnum.DestinyActivityModeDefinition,
                                DefinitionsEnum.DestinyActivityTypeDefinition,
                                DefinitionsEnum.DestinyCollectibleDefinition,
                                DefinitionsEnum.DestinyInventoryItemDefinition,
                                DefinitionsEnum.DestinyMetricDefinition,
                                DefinitionsEnum.DestinyObjectiveDefinition,
                                DefinitionsEnum.DestinyPresentationNodeDefinition,
                                DefinitionsEnum.DestinyProgressionDefinition,
                                DefinitionsEnum.DestinyRecordDefinition,
                                DefinitionsEnum.DestinySeasonDefinition,
                                DefinitionsEnum.DestinySeasonPassDefinition,
                                DefinitionsEnum.DestinyTalentGridDefinition,
                                DefinitionsEnum.DestinyTraitDefinition,
                                DefinitionsEnum.DestinyVendorDefinition
                            });
                            provider.AutoUpdateOnStartup = false;
                            provider.CleanUpOldManifestsAfterUpdate = false;
                        });
                    }
                })
                .AddSingleton<IUserSearchService, UserSearchService>()
                .AddMarvinDbContext()
                .AddOptions();

            var application = appBuilder.BuildApplication();
            await application.RunAsync();
        }
    }
}