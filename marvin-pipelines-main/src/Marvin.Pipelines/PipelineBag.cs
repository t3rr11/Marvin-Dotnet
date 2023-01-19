namespace Marvin.Pipelines;

public class PipelineBag
{
    private readonly Dictionary<Type, object> _bag;

    internal PipelineBag()
    {
        _bag = new Dictionary<Type, object>();
    }

    public T? Get<T>() where T : class
    {
        if (_bag.TryGetValue(typeof(T), out var data)) return (T)data;

        return default;
    }

    public T GetOrThrow<T>() where T : class
    {
        var type = typeof(T);
        if (_bag.TryGetValue(type, out var data)) return (T)data;

        throw new BagTypeMissingException(type);
    }

    public PipelineBag Set<T>(T value)
    {
        _bag.TryAdd(typeof(T), value);
        return this;
    }

    public PipelineBag Copy()
    {
        var bag = new PipelineBag();

        foreach (var data in _bag)
        {
            bag._bag.Add(data.Key, data.Value);
        }

        return bag;
    }

    public void CopyFromBagOrThrow<T>(PipelineBag otherBag) where T : class
    {
        var value = otherBag.GetOrThrow<T>();
        Set(value);
    }
}