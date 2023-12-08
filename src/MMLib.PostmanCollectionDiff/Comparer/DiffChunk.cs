using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;

namespace MMLib.PostmanCollectionDiff.Comparer;

public sealed record DiffChunk : IDiff
{
    public static DiffChunk NoDiff { get; } = new();

    internal DiffChunk()
    {
        HasDifferences = false;
    }

    internal DiffChunk(SideBySideDiffModel diff)
    {
        Diff = diff;
        HasDifferences = diff.NewText.HasDifferences || diff.OldText.HasDifferences;
    }

    internal DiffChunk(string? raw)
    {
        Raw = raw;
        HasDifferences = false;
    }

    public static DiffChunk Create(string? updated, string? original)
        => new(SideBySideDiffBuilder.Instance.CreateDiff(original ?? string.Empty, updated ?? string.Empty));

    public static DiffChunk SingleValue(string? value) => new(value) { };

    public SideBySideDiffModel? Diff { get; } = null;

    public bool HasDifferences { get; }

    public string? Raw { get; set; }

    public string? RawOriginal { get; set; }

    public string GetValue(DiffValueType type) => type switch
    {
        DiffValueType.Original => Diff is not null ? Diff.OldText.ToString() ?? string.Empty : string.Empty,
        DiffValueType.Updated => Diff is not null ? Diff.NewText.ToString() ?? string.Empty : string.Empty,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };

    public DiffPaneModel GetDiff(DiffValueType type) => type switch
    {
        DiffValueType.Original => Diff is not null ? Diff!.OldText : FromRaw(),
        DiffValueType.Updated => Diff is not null ? Diff!.NewText : new DiffPaneModel(),
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };

    private DiffPaneModel FromRaw()
        => SideBySideDiffBuilder.Instance.BuildDiffModel(Raw, string.Empty).OldText;
}
