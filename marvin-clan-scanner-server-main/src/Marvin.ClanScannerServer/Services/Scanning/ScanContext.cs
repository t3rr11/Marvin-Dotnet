namespace Marvin.ClanScannerServer.Services.Scanning;

public class ScanContext
{
    private Dictionary<string, object> _storage = new();

    public object this[string key]
    {
        get => _storage[key];
        set => _storage[key] = value;
    }

    public T? Get<T>(string key)
    {
        if (_storage.TryGetValue(key, out var obj))
        {
            return (T)obj;
        }

        return default;
    }
}