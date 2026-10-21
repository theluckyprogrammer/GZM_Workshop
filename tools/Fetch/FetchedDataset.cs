namespace Fetch;

public sealed record FetchedDataset(
    DatasetDefinition Definition,
    DateTimeOffset DownloadedAt,
    CkanPackage? Package,
    CkanResource? Resource,
    byte[]? Bytes,
    DetectedEncoding? Encoding,
    CsvTable? Table,
    IReadOnlyList<ColumnProfile> Columns,
    IReadOnlyList<PiiHit> Pii,
    IReadOnlyList<string> Rejections)
{
    public bool IsRejected => Rejections.Count > 0;

    public int RowCount => Table?.Rows.Count ?? 0;

    public static FetchedDataset Failed(DatasetDefinition definition, string reason, CkanPackage? package = null) =>
        new(definition, DateTimeOffset.UtcNow, package, null, null, null, null, [], [], [reason]);
}
