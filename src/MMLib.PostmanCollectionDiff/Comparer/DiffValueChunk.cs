namespace MMLib.PostmanCollectionDiff.Comparer;

public sealed record DiffValueChunk : IDiff
{
    public DiffValueChunk(string? original, string? updated)
    {
        Original = original;
        Updated = updated;
        HasDifferences = original?.Equals(updated, StringComparison.OrdinalIgnoreCase) == false;
    }

    public DiffValueChunk() { }

    public static DiffValueChunk SingleValue(string? value) => new(value, value);

    public static DiffValueChunk NoDiff { get; } = new DiffValueChunk();

    public string? Original { get; }

    public string? Updated { get; }

    public bool HasDifferences { get; }

    public string GetValue(DiffValueType type) => type switch
    {
        DiffValueType.Original => Original ?? string.Empty,
        DiffValueType.Updated => Updated ?? string.Empty,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
}
