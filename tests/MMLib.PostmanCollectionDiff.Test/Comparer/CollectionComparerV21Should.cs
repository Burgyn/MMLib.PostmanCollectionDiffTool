using MMLib.PostmanCollectionDiff.Comparer;
using System.Text.Json;

namespace MMLib.PostmanCollectionDiff.Test.Comparer;

[UsesVerify]
public class CollectionComparerV21Should
{
    [Theory]
    [InlineData("complex_collection")]
    [InlineData("simple_collection")]
    [InlineData("empty_collection")]
    [InlineData("events_collection")]
    public async Task CompareOriginalAnUpdatedCollection(string testCaseName)
    {
        using var original = await ReadFile(testCaseName, "original");
        using var updated = await ReadFile(testCaseName, "updated");

        var comparer = new CollectionComparerV21();

        var result = comparer.Compare(original, updated);

        await Verify(result)
            .UseParameters(testCaseName);
    }

    private static async Task<JsonDocument> ReadFile(string testCaseName, string type)
    {
        var rawJson = await File.ReadAllTextAsync($"TestCases/{testCaseName}_{type}.json");
        return JsonDocument.Parse(rawJson);
    }
}
