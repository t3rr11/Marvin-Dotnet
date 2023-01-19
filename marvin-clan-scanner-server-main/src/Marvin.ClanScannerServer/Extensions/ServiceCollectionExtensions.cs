using Marvin.ClanScannerServer.Services.ProfileProcessors;
using Marvin.ClanScannerServer.Services.ProfileProcessors.Interfaces;
using Marvin.ClanScannerServer.Services.Scanning;

namespace Marvin.ClanScannerServer.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDestinyProcessors(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddSingleton<IClanProcessor, ClanProcessor>()
            .AddSingleton<ICollectibleProcessor, CollectibleProcessor>()
            .AddSingleton<IProgressionProcessor, ProgressionProcessor>()
            .AddSingleton<IMetricProcessor, MetricProcessor>()
            .AddSingleton<IRecordProcessor, RecordProcessor>();
    }

    public static IServiceCollection AddHostedServiceWithInterface<THostedServiceInterface, THostedService>(
        this IServiceCollection serviceCollection)
        where THostedService : class, THostedServiceInterface, IHostedService
        where THostedServiceInterface : class
    {
        serviceCollection.AddSingleton<THostedServiceInterface, THostedService>();

        serviceCollection.AddSingleton<IHostedService>(x =>
            (THostedService)x.GetRequiredService<THostedServiceInterface>());

        return serviceCollection;
    }
}