using System.Runtime.CompilerServices;

namespace Marvin.MemoryCache;

public class CachedItem
{
    public CachedItem(
        object item,
        TimeSpan timeSpan)
    {
        UpdateItem(item, timeSpan);
    }

    public TimeSpan Duration { get; private set; }
    internal object Item { get; private set; }
    public DateTime LastUpdated { get; private set; }

    public bool DidExpire => DateTime.UtcNow - LastUpdated > Duration;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UpdateItem(
        object newItem,
        TimeSpan newDuration)
    {
        Item = newItem;
        Duration = newDuration;
        LastUpdated = DateTime.UtcNow;
    }

    public T GetItem<T>()
    {
        return (T)Item;
    }
}