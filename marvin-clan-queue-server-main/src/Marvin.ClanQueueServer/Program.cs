using Discord;
using DotNetBungieAPI;
using DotNetBungieAPI.Models;
using DotNetBungieAPI.Models.Destiny;
using Marvin.Application.Sdk;
using Marvin.ClanQueueServer;
using Marvin.ClanQueueServer.Hubs;
using Marvin.ClanQueueServer.Hubs.Filters;
using Marvin.ClanQueueServer.Options;
using Marvin.ClanQueueServer.Services;
using Marvin.ClanQueueServer.Services.Hosted;
using Marvin.ClanQueueServer.Services.Interfaces;
using Marvin.DbAccess.Options;
using Marvin.DefinitionProvider.Postgresql;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR;

using (var appBuilder = MarvinApplicationBuilder.Create(args))
{
    appBuilder.AddDefaultServices();

    appBuilder.Services.AddControllers();
    
    appBuilder.Services.AddSignalR(options =>
    {
        options.HandshakeTimeout = TimeSpan.FromSeconds(15);
        options.AddFilter<HubErrorFilter>();
    });
    appBuilder.Services.AddSingleton<HubErrorFilter>();
    
    var bnetOptions = appBuilder.Configuration.GetSection("BungieNetOptions").Get<BungieNetOptions>();
    appBuilder.Services.Configure<DatabaseOptions>(appBuilder.Configuration.GetSection("DatabaseOptions"));
    
    appBuilder.Services.UseBungieApiClient(options =>
    {
        options.ClientConfiguration.ApiKey = bnetOptions.ApiKey;
        options.ClientConfiguration.ClientId = bnetOptions.ClientId;
        options.ClientConfiguration.ClientSecret = bnetOptions.ClientSecret;
        options.ClientConfiguration.UsedLocales.Add(BungieLocales.EN);

        options.DefinitionProvider.UsePostgresqlDefinitionProvider(x =>
        {
            x.ConnectionString = appBuilder.Configuration.GetSection("DatabaseOptions:ConnectionString").Value;
            x.DefinitionsToLoad.AddRange(new[]
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
            x.MaxAmountOfLeftoverManifests = 5;
            x.CleanUpOldManifestsAfterUpdate = true;
            x.AutoUpdateOnStartup = false;
        });
    });

    appBuilder.Services.AddDiscord(
        client => { client.GatewayIntents = GatewayIntents.Guilds; },
        interactionService => { },
        textCommands => { },
        appBuilder.Configuration);
    appBuilder.Services.AddSingleton<DiscordClientService>();
    
    appBuilder.Services.AddSingleton<BungieNetManifestValidator>();

    AddClanTrackingServices(appBuilder.Services);
    AddBungieNetBackgroundServices(appBuilder.Services);

    appBuilder
        .ConfigureLogging()
        .AddConsole()
        .AddElasticsearch()
        .Apply();
    
    var application = appBuilder.BuildApplication();

    application.Application.MapControllers();

    application.Application.MapHub<ClanQueueHub>("/clanQueueHub", options => { options.Transports = HttpTransportType.WebSockets; });
    
    await application.RunAsync();
}

void AddClanTrackingServices(IServiceCollection services)
{
    services.AddSingleton<ClanScanningTrackerService>();
    services.AddHostedService<HostedClanTracker>();
    services.AddHostedService<HostedClanFirstTimeEventHandler>();
}

void AddBungieNetBackgroundServices(IServiceCollection services)
{
    services.AddHostedServiceWithInterface<IBungieNetHealthCheck, BungieNetApiBackgroundHealthCheckService>();
    services.AddHostedServiceWithInterface<IBungieNetManifestUpdater, BungieNetManifestUpdaterService>();
}