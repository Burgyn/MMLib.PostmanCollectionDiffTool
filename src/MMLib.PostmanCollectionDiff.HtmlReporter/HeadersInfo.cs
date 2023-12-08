using MMLib.PostmanCollectionDiff.Comparer;

namespace MMLib.PostmanCollectionDiff.HtmlReporter;

public sealed class HeadersInfo
{
    public DiffValueType Type { get; set; }

    public HeadersDiff Headers { get; set; } = null!;
}
