using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fetch;

// Writes data/raw/*.csv (bytes as downloaded), data/work copies, data/metadata.json and docs/ZAMIANY.md.
public sealed class OutputWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private readonly string _root;

    public OutputWriter(string root)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);
        _root = root;
    }

    public IReadOnlyList<DatasetMeta> Write(IReadOnlyList<FetchedDataset> datasets, IReadOnlyList<Slot> slots)
    {
        ArgumentNullException.ThrowIfNull(datasets);
        ArgumentNullException.ThrowIfNull(slots);

        CleanRawCsv();
        var metas = datasets.Select(d => WriteDataset(d, Placement.For(d, slots))).ToList();
        WriteText("data/metadata.json", JsonSerializer.Serialize(new MetadataDocument(DateTimeOffset.UtcNow, CkanClient.BaseUrl, metas), JsonOptions));
        WriteText("docs/ZAMIANY.md", ReplacementReport.Build(datasets, slots));
        return metas;
    }

    private DatasetMeta WriteDataset(FetchedDataset dataset, Placement placement)
    {
        if (dataset.Bytes is not null)
            WriteBytes(placement.RelativePath, dataset.Bytes);
        if (DatasetRules.NeedsWorkCopy(dataset.RowCount))
            WriteWorkCopy(dataset, $"data/work/{placement.FileName}");

        return MetadataBuilder.Build(dataset, placement);
    }

    // Header + first N raw records, same encoding (and BOM) as the original.
    private void WriteWorkCopy(FetchedDataset dataset, string relativePath)
    {
        var table = dataset.Table!;
        var encoding = dataset.Encoding!.Encoding;
        var text = table.RawHeader + string.Concat(table.RawRows.Take(DatasetRules.WorkCopyRows));
        WriteBytes(relativePath, [.. encoding.GetPreamble(), .. encoding.GetBytes(text)]);
    }

    private void CleanRawCsv()
    {
        var raw = Path.Combine(_root, "data", "raw");
        Directory.CreateDirectory(raw);
        foreach (var file in Directory.GetFiles(raw, "*.csv"))
            File.Delete(file);
    }

    private void WriteBytes(string relativePath, byte[] bytes)
    {
        var path = Path.Combine(_root, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, bytes);
    }

    private void WriteText(string relativePath, string text) =>
        WriteBytes(relativePath, new UTF8Encoding(false).GetBytes(text.ReplaceLineEndings("\n")));
}
