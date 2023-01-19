using System.Collections.Concurrent;

namespace Marvin.MemoryCache;

internal class CacheProvider : ICacheProvider
{
    private readonly ConcurrentDictionary<string, CachedItem> _storage;

    public CacheProvider()
    {
        _storage = new ConcurrentDictionary<string, CachedItem>();
    }

    public async ValueTask<T> GetAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan duration)
    {
        if (_storage.TryGetValue(key, out var item))
        {
            if (item.DidExpire)
            {
                var newValue = await factory();
                item.UpdateItem(newValue, duration);
                return newValue;
            }

            return item.GetItem<T>();
        }

        var valueToStore = await factory();
        var newItem = new CachedItem(valueToStore, duration);
        _storage.TryAdd(key, newItem);
        return valueToStore;
    }

    public bool RemoveItem(string key, out object item)
    {
        item = null;
        if (_storage.TryRemove(key, out var cachedItem))
        {
            item = cachedItem.Item;
            return true;
        }

        return false;
    }
}