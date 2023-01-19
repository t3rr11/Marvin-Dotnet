using Marvin.ProfileProcessors.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Marvin.ProfileProcessors;

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
}