using DiffPlex.DiffBuilder.Model;

namespace MMLib.PostmanCollectionDiff.Comparer;
public sealed record HeaderDiff : IDiff
{
    internal HeaderDiff(string key, DiffValueChunk value, DiffChunk description, DiffValueChunk disabled)
        : this(key, value, description, disabled,
              value.HasDifferences || description.HasDifferences
              || disabled.HasDifferences ? ChangeType.Modified : ChangeType.Unchanged)
    {
    }

    internal HeaderDiff(string key, DiffValueChunk value, DiffChunk description, DiffValueChunk disabled, ChangeType changeType)
    {
        Key = key;
        Value = value;
        Description = description;
        Disabled = disabled;
        ChangeType = changeType;
    }

    public static HeaderDiff Create(string key, string? value, string? description, bool disabled)
        => new(key, new DiffValueChunk(value, value),
            DiffChunk.SingleValue(description), DiffValueChunk.SingleValue(disabled.ToString()));

    public static HeaderDiff NewCreated(string key, string? value, string? description, bool disabled)
        => new(key, new DiffValueChunk(null, value),
            DiffChunk.SingleValue(description), DiffValueChunk.SingleValue(disabled.ToString()), ChangeType.Inserted);

    public static HeaderDiff Removed(string key, string? value, string? description, bool disabled)
        => new(key, new DiffValueChunk(value, null),
            DiffChunk.SingleValue(description), DiffValueChunk.SingleValue(disabled.ToString()), ChangeType.Deleted);

    public string Key { get; }

    public DiffValueChunk Value { get; }

    public bool HasDifferences => ChangeType != ChangeType.Unchanged;

    public ChangeType ChangeType { get; }

    public DiffValueChunk Disabled { get; }

    public DiffChunk Description { get; }

    public (string key, string value) GetHeader(DiffValueType type)
        => (Key, Value.GetValue(type));
}
