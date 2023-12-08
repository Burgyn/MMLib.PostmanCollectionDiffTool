using System.Collections;

namespace MMLib.PostmanCollectionDiff.Comparer;

public sealed record HeadersDiff : IDiff, IEnumerable<HeaderDiff>
{
    public bool HasDifferences { get; private set; }

    public IEnumerable<HeaderDiff> Headers { get; private set; } = new List<HeaderDiff>();

    internal void AddHeader(IEnumerable<HeaderDiff> headers)
    {
        Headers = headers;
        if (headers.Any(v => v.HasDifferences))
        {
            HasDifferences = true;
        }
    }

    public IEnumerator<HeaderDiff> GetEnumerator() => Headers.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Headers.GetEnumerator();
}
