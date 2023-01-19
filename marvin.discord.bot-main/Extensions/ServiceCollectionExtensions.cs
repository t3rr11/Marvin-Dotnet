namespace Marvin.Bot.Extensions;

public static class ServiceCollectionExtensions
{
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