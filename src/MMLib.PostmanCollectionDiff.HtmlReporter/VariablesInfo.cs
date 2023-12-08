using MMLib.PostmanCollectionDiff.Comparer;

namespace MMLib.PostmanCollectionDiff.HtmlReporter;

public sealed class VariablesInfo
{
    public DiffValueType Type { get; set; }

    public VariablesDiff Variables { get; set; } = null!;
}
