using System.Text.Json;

namespace MMLib.PostmanCollectionDiff.Comparer;

public interface ICollectionComparer
{
    CollectionDiffResult Compare(JsonDocument original, JsonDocument updated);
}
