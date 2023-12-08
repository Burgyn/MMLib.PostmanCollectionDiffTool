using DiffPlex.DiffBuilder;

namespace MMLib.PostmanCollectionDiff.Comparer;

internal static class DiffBuilderExtensions
{
    public static DiffChunk CreateDiff(this ISideBySideDiffBuilder diff, string oldText, string newText)
        => new(diff.BuildDiffModel(oldText, newText)) { Raw = newText, RawOriginal = oldText};
}
