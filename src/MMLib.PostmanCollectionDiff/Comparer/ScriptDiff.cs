using DiffPlex.DiffBuilder.Model;

namespace MMLib.PostmanCollectionDiff.Comparer;
public sealed record ScriptDiff : IDiff
{
    public static ScriptDiff NoDiff { get; } = new ScriptDiff();

    public bool HasDifferences { get; private set; }

    public DiffValueChunk Id { get; private set; } = DiffValueChunk.NoDiff;
    public DiffValueChunk Name { get; private set; } = DiffValueChunk.NoDiff;
    public DiffValueChunk Type { get; private set; } = DiffValueChunk.NoDiff;

    public DiffChunk Exec { get; private set; } = DiffChunk.NoDiff;

    internal static ScriptDiff SingleValue(string? id, string? name, string? type, string[] exec, ChangeType changeType)
    {
        var code = string.Concat(exec);

        DiffChunk diff = changeType switch
        {
            ChangeType.Inserted => DiffChunk.Create(code, string.Empty),
            ChangeType.Deleted => DiffChunk.Create(string.Empty, code),
            _ => DiffChunk.Create(code, code)
        };

        return new()
        {
            Id = DiffValueChunk.SingleValue(id),
            Name = DiffValueChunk.SingleValue(name),
            Type = DiffValueChunk.SingleValue(type),
            Exec = diff,
            HasDifferences = true
        };
    }

    internal static ScriptDiff Create(string? id, string? name, string? type, string[] original, string[] updated)
    {
        var originalCode = string.Join(Environment.NewLine, original.Select(p => p.TrimEnd('\r')));
        var updatedCode = string.Join(Environment.NewLine, updated.Select(p => p.TrimEnd('\r')));
        var diff = DiffChunk.Create(updatedCode, originalCode);

        return new()
        {
            Id = DiffValueChunk.SingleValue(id),
            Name = DiffValueChunk.SingleValue(name),
            Type = DiffValueChunk.SingleValue(type),
            Exec = diff,
            HasDifferences = diff.HasDifferences
        };
    }
}
