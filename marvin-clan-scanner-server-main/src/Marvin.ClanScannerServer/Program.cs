using System.Net;
using DotNetBungieAPI;
using DotNetBungieAPI.DefinitionProvider.Sqlite;
using DotNetBungieAPI.Extensions;
using DotNetBungieAPI.Models;
using DotNetBungieAPI.Models.Destiny;
using Marvin.Application.Sdk;
using Marvin.ClanScannerServer.Extensions;
using Marvin.ClanScannerServer.Options;
using Marvin.ClanScannerServer.Services;
using Marvin.ClanScannerServer.Services.Hosted;
using Marvin.ClanScannerServer.Services.Hosted.Interfaces;
using Marvin.ClanScannerServer.Services.Scanning;
using Marvin.ClanScannerServer.Services.Scanning.Scanners;
using Marvin.ClanScannerServer.Services.ScanningStrategies;
using Marvin.DbAccess.Options;
using Marvin.DefinitionProvider.Postgresql;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Unleash;

using (var builder = MarvinApplicationBuilder.Create(args))
{
    builder.AddDefaultServices();

    var logging = builder.ConfigureLogging();
    logging.AddConsole();
    if (!builder.Environment.IsDevelopment())
    {
        logging.AddElasticsearch();
    }
    else
    {
        logging.AddFile();
    }

    logging.Apply();

    var bungieNetOptions = builder.Configuration.GetSection("BungieNetOptions").Get<BungieNetOptions>();
    var dbConnectionString = builder.Configuration.GetSection("DatabaseOptions:ConnectionString").Value;

    builder.Services.UseBungieApiClient(bungieClientBuilder =>
    {
        bungieClientBuilder.ClientConfiguration.ApiKey = bungieNetOptions.ApiKey;
        bungieClientBuilder.ClientConfiguration.UsedLocales.Add(BungieLocales.EN);
        
        if (builder.Environment.IsDevelopment())
        {
            bungieClientBuilder.DefinitionProvider.UseSqliteDefinitionProvider(provider =>
            {
                provider.ManifestFolderPath = builder
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
                provider.ConnectionString = dbConnectionString;
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
                    }
                );

                provider.AutoUpdateOnStartup = false;
                provider.CleanUpOldManifestsAfterUpdate = false;
            });
        }

        bungieClientBuilder.DotNetBungieApiHttpClient.ConfigureDefaultHttpClient(apiHttpClient =>
        {
            apiHttpClient.SetRateLimitSettings(
                bungieNetOptions.RateLimitPerInterval,
                TimeSpan.FromSeconds(bungieNetOptions.RateLimitInterval));
            apiHttpClient.MaxConcurrentRequestsAtOnce = bungieNetOptions.MaxConcurrentRequestsAtOnce;
            apiHttpClient.MaxRequestsPerSecond = bungieNetOptions.MaxRequestsPerSecond;
        });
    });

    builder.Services.AddSingleton<ClanScannerService>();
    builder.Services.AddSingleton<BungieNetApiCallLogger>();
    builder.Services.AddSingleton<ScanMetricsService>();
    builder.Services.AddDestinyProcessors();
    builder.Services.AddSingleton<ClanQueueHubConnection>();

    builder.Services.AddHostedService<DefinitionsLoaderService>();
    builder.Services.AddHostedService<SignalRClientBootstrap>();
    builder.Services.AddHostedService<BackgroundClanScanner>();
    builder.Services.AddHostedService<BackgroundBungieApiMetricReporter>();

    builder.Services.AddSingleton<GeneralScanningStrategy>();
    builder.Services.AddSingleton<PatreonScanningStrategy>();
    builder.Services.AddSingleton<CurrentScanningStrategyHolder>();


    builder.Services.AddControllers().AddJsonOptions(x => { });

    builder.Services.AddSingleton<IUnleash>(x =>
    {
        var jsonOptions = x.GetRequiredService<IOptions<JsonOptions>>();
        return new DefaultUnleash(new UnleashSettings()
        {
            AppName = "Marvin.ClanScannerServer",
            Environment = "dev",
            UnleashApi = new Uri("https://gitlab.com/api/v4/feature_flags/unleash/39524359"),
            InstanceTag = "YDh7BQV9eYqNGGLkzH54",
            JsonSerializer = new UnleashJsonSerializer(jsonOptions.Value.JsonSerializerOptions)
        });
    });

    builder.Services.AddSingleton<MemberScanner>();
    builder.Services.AddSingleton<SilentMemberScanner>();
    builder.Services.AddSingleton<ClanScanner>();
    builder.Services.AddSingleton<FirstTimeClanScanner>();
    builder.Services.AddHostedServiceWithInterface<IUserQueue, UserQueueBackgroundProcessor>();

    builder.Services.AddOptions();
    builder.Services.Configure<ClanQueueServerOptions>(builder.Configuration.GetSection("ClanQueueServer"));
    builder.Services.Configure<InstanceMetadataOptions>(builder.Configuration.GetSection("InstanceMetadata"));
    builder.Services.Configure<DatabaseOptions>(builder.Configuration.GetSection("DatabaseOptions"));

    var app = builder.BuildApplication();

    app.Application.MapControllers();
    
    ServicePointManager.DefaultConnectionLimit = 30;
    
    await app.RunAsync();
}