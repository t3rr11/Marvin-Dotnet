namespace Marvin.MemoryCache;

public class AsyncCachedItem<T> where T : class
{
    private readonly SemaphoreSlim _semaphoreSlim;
    private T? _item;

    public AsyncCachedItem()
    {
        _semaphoreSlim = new SemaphoreSlim(1);
    }

    public async ValueTask<bool> HasValueAsync()
    {
        try
        {
            await _semaphoreSlim.WaitAsync();
            return _item is not null;
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public async Task SetAsync(T item)
    {
        try
        {
            await _semaphoreSlim.WaitAsync();
            _item = item;
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public async ValueTask<T?> GetAsync()
    {
        try
        {
            await _semaphoreSlim.WaitAsync();
            return _item;
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
}