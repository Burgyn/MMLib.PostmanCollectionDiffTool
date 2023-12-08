namespace MMLib.PostmanCollectionDiff.Comparer;

internal static class StringExtensions
{
    public static DiffValueChunk CompareDiff(this string? original, string? updated) => new(original, updated);
}
