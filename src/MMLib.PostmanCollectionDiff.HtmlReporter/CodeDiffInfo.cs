using MMLib.PostmanCollectionDiff.Comparer;

namespace MMLib.PostmanCollectionDiff.HtmlReporter;

public class CodeDiffInfo
{
    public string Language { get; set; } = null!;

    public DiffChunk CodeDiff { get; set; } = null!;
}
