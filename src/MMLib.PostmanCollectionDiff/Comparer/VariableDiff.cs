using DiffPlex.DiffBuilder.Model;

namespace MMLib.PostmanCollectionDiff.Comparer;

public sealed record VariableDiff : IDiff
{
    private VariableDiff(string key, DiffValueChunk value)
        : this(key, value, value.HasDifferences ? ChangeType.Modified : ChangeType.Unchanged)
    {
    }

    private VariableDiff(string key, DiffValueChunk value, ChangeType changeType)
    {
        Key = key;
        Value = value;
        ChangeType = changeType;
    }

    public static VariableDiff Create(string key, string? originalValue, string? updatedValue)
        => new(key, new DiffValueChunk(originalValue, updatedValue));

    public static VariableDiff Create(string key, string? value)
        => new(key, new DiffValueChunk(value, value));

    public static VariableDiff NewCreated(string key, string? value)
        => new(key, new DiffValueChunk(null, value), ChangeType.Inserted);

    public static VariableDiff Removed(string key, string? value)
        => new(key, new DiffValueChunk(value, null), ChangeType.Deleted);

    public string Key { get; }

    public DiffValueChunk Value { get; }

    public bool HasDifferences => ChangeType != ChangeType.Unchanged;

    public ChangeType ChangeType { get; }
}
