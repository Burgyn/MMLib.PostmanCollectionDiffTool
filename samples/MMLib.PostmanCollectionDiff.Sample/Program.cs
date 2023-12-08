using MMLib.PostmanCollectionDiff.Comparer;
using MMLib.PostmanCollectionDiff.ConsoleReporter;
using MMLib.PostmanCollectionDiff.HtmlReporter;
using MMLib.PostmanCollectionDiff.Sample.Properties;
using System.Text.Json;

Console.OutputEncoding = System.Text.Encoding.UTF8;

using var original = JsonDocument.Parse(Resources.Original_products_collection_postman_collection);
using var updated = JsonDocument.Parse(Resources.Updated_products_collection_postman_collection);

//using var original = JsonDocument.Parse(Resources.Empty_collection___original_postman_collection);
//using var updated = JsonDocument.Parse(Resources.Empty_collection___updated_postman_collection);

//using var original = JsonDocument.Parse(Resources.Events___original_postman_collection);
//using var updated = JsonDocument.Parse(Resources.Events___updated_postman_collection);

var comparerProvider = new CollectionComparerProvider();

var comparer = comparerProvider.GetComparer("v2.1");

var result = comparer.Compare(original, updated);

var reporter = new DiffReporter() { Verbose = true };

await reporter.GenerateReportAsync(result);

var htmlReporter = new HtmlDiffReporter();
await htmlReporter.GenerateReportAsync(result);

var outputFileName = Path.Combine(Path.GetTempPath(), "report.html");
if (File.Exists(outputFileName))
{
    File.Delete(outputFileName);
}

await File.WriteAllTextAsync(outputFileName, htmlReporter.HtmlReportContent);
Console.WriteLine($"HTML report was saved to {outputFileName}");
