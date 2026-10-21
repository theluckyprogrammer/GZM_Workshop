using System.Text;
using Fetch;

// Usage: dotnet run --project tools/Fetch [-- <repo root>]
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var root = args.Length > 0 ? Path.GetFullPath(args[0]) : RepoRoot.Find(Directory.GetCurrentDirectory());
using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(120) };
http.DefaultRequestHeaders.UserAgent.ParseAdd("GzmWorkshopFetch/1.0");
var processor = new DatasetProcessor(new CkanClient(http));

var mains = await FetchAllAsync(DatasetCatalog.Main);
var reserves = await FetchAllAsync(DatasetCatalog.Reserves);
var slots = SlotAssigner.Assign(mains, reserves);
var metas = new OutputWriter(root).Write([.. mains, .. reserves], slots);
Summary.Print(metas);
return 0;

async Task<List<FetchedDataset>> FetchAllAsync(IReadOnlyList<DatasetDefinition> definitions)
{
    var results = new List<FetchedDataset>();
    foreach (var definition in definitions)
    {
        Console.WriteLine($"{definition.Code}: {definition.Label}");
        var dataset = await processor.ProcessAsync(definition, CancellationToken.None);
        Console.WriteLine($"    wiersze: {dataset.RowCount}; odrzucenie: {string.Join("; ", dataset.Rejections.DefaultIfEmpty("brak"))}");
        results.Add(dataset);
    }
    return results;
}
