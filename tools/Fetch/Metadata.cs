namespace Fetch;

public sealed record SourceMeta(string Id, string Title, string Label, string Publisher, string PageUrl, string FileUrl, string ResourceName, string? FileDate);

public sealed record LicenseMeta(string Id, string Title, string Attribution);

public sealed record FileMeta(string Path, DateTimeOffset DownloadedAt, string Sha256, long SizeBytes, string? WorkCopyPath, int? WorkCopyRows);

public sealed record FormatMeta(string Encoding, string Separator, int HeaderLine, IReadOnlyList<string> Headers);

public sealed record ContentMeta(int Rows, int InconsistentRows, IReadOnlyList<ColumnProfile> Columns);

public sealed record DatasetMeta(
    string Code, string? Scenario, string Role, string Status,
    IReadOnlyList<string> RejectionReasons, string? ReplacesCode,
    SourceMeta? Source, LicenseMeta? License, FileMeta? File, FormatMeta? Format, ContentMeta? Content,
    IReadOnlyList<PiiHit> PiiForManualReview);

public sealed record MetadataDocument(DateTimeOffset GeneratedAt, string Portal, IReadOnlyList<DatasetMeta> Datasets);

// Where a dataset lands: used in a scenario, kept as reserve, or rejected (outside data/raw).
public sealed record Placement(string Directory, string FileName, string? Scenario, string Status, string? ReplacesCode)
{
    public string RelativePath => $"{Directory}/{FileName}";

    public static Placement For(FetchedDataset dataset, IReadOnlyList<Slot> slots)
    {
        ArgumentNullException.ThrowIfNull(dataset);
        ArgumentNullException.ThrowIfNull(slots);

        var slug = dataset.Definition.Slug;
        var slot = slots.FirstOrDefault(s => s.Dataset == dataset && s.IsFilled);
        if (slot is not null)
            return new Placement("data/raw", $"{slot.Code}_{slug}.csv", slot.Code, "used", slot.Replaced?.Definition.Code);
        if (dataset.IsRejected)
            return new Placement("data/work/rejected", $"{dataset.Definition.Code}_{slug}.csv", null, "rejected", null);

        return new Placement("data/raw", $"{dataset.Definition.Code}_{slug}.csv", null, "reserve", null);
    }
}
