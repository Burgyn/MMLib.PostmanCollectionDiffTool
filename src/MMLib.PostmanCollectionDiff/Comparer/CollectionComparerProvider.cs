namespace MMLib.PostmanCollectionDiff.Comparer;

public sealed class CollectionComparerProvider
{
    private readonly Dictionary<string, ICollectionComparer> _comparers = new(StringComparer.OrdinalIgnoreCase);

    public CollectionComparerProvider()
    {
        _comparers.Add("v2.1", new CollectionComparerV21());
    }

    public ICollectionComparer GetComparer(string version)
    {
        if (_comparers.TryGetValue(version, out var comparer))
        {
            return comparer;
        }

        throw new NotSupportedException($"Version {version} is not supported.");
    }

    public void Clear()
    {
        _comparers.Clear();
    }

    public void Add(string version, ICollectionComparer comparer)
    {
        _comparers.Add(version, comparer);
    }
}
