using MMLib.PostmanCollectionDiff.Comparer;

namespace MMLib.PostmanCollectionDiff;

/// <summary>
/// Interface for diff reporter.
/// </summary>
public interface IDiffReporter
{
    /// <summary>
    /// Generates the report asynchronous.
    /// </summary>
    /// <param name="result">The result.</param>
    Task GenerateReportAsync(CollectionDiffResult result);
}
