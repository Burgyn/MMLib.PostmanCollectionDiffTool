using MMLib.PostmanCollectionDiff.Comparer;
using RazorLight;

namespace MMLib.PostmanCollectionDiff.HtmlReporter;

/// <summary>
/// Class for generate HTML report.
/// </summary>
/// <seealso cref="IDiffReporter" />
public class HtmlDiffReporter : IDiffReporter
{
    /// <summary>
    /// Gets the content of the HTML report.
    /// </summary>
    /// <value>
    /// The content of the HTML report.
    /// </value>
    public string? HtmlReportContent { get; private set; }

    public async Task GenerateReportAsync(CollectionDiffResult result)
    {
        var engine = new RazorLightEngineBuilder()
            .UseEmbeddedResourcesProject(typeof(HtmlDiffReporter))
            .SetOperatingAssembly(typeof(HtmlDiffReporter).Assembly)
            .UseMemoryCachingProvider()
            .Build();

        HtmlReportContent = await engine.CompileRenderAsync("View.cshtml", result);
    }
}
