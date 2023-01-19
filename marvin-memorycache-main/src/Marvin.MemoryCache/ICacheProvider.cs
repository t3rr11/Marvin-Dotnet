namespace Marvin.MemoryCache;

/// <summary>
///     Interface for providing simple cache access
/// </summary>
public interface ICacheProvider
{
    /// <summary>
    ///     Gets object from cache based on it's key
    /// </summary>
    /// <param name="key">Key to look for</param>
    /// <param name="factory">Task that will provide new instance if item is missing or expired</param>
    /// <param name="duration">Time until item will be marked as expired</param>
    /// <typeparam name="T">Type of the item. Note that if item is of other type, Exception will be thrown!</typeparam>
    /// <returns></returns>
    ValueTask<T> GetAsync<T>(string key, Func<Task<T>> factory, TimeSpan duration);

    /// <summary>
    ///     Removes the key from the storage, if it exists
    /// </summary>
    /// <param name="key">Key to remove</param>
    /// <param name="item">Item, if it exists</param>
    /// <returns></returns>
    bool RemoveItem(string key, out object item);
}