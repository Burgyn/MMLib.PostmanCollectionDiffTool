using DiffPlex.DiffBuilder.Model;
using MMLib.PostmanCollectionDiff.Comparer;

namespace MMLib.PostmanCollectionDiff.HtmlReporter;

public static class RazorHelper
{
    public static string ItemTypeToEmoji(ItemDiff item)
        => item.IsFolder ? "📂" : "🔥";

    public static string ChangeTypeToCssClass(ChangeType changeType)
        => changeType switch
        {
            ChangeType.Inserted => "bg-success",
            ChangeType.Deleted => "bg-danger",
            ChangeType.Modified => "bg-info",
            _ => string.Empty
        };

    public static string ChangeTypeToTextCssClass(ChangeType changeType)
    => changeType switch
    {
        ChangeType.Inserted => "added",
        ChangeType.Deleted => "deleted",
        ChangeType.Modified => "changed",
        _ => "unchanged"
    };
}
