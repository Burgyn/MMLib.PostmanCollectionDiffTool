using DiffPlex.DiffBuilder.Model;

namespace MMLib.PostmanCollectionDiff.HtmlReporter;

public sealed class LinesInfo
{
    public string Language { get; set; } = null!;

    public IEnumerable<DiffPiece> Pieces { get; set; } = null!;
}
