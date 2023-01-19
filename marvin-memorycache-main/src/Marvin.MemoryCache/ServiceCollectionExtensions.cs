using Microsoft.Extensions.DependencyInjection;

namespace Marvin.MemoryCache;

public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Adds implementation for <see cref="ICacheProvider" />, which can be used for caching
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <returns></returns>
    public static IServiceCollection AddCacheProvider(
        this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddSingleton<ICacheProvider, CacheProvider>();
    }
}