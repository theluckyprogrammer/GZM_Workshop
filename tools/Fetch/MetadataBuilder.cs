using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Fetch;

public static class MetadataBuilder
{
    private static readonly Regex UrlDate = new(@"(20\d{2})[-_]?(\d{2})[-_]?(\d{2})");

    public static DatasetMeta Build(FetchedDataset dataset, Placement placement)
    {
        ArgumentNullException.ThrowIfNull(dataset);
        ArgumentNullException.ThrowIfNull(placement);

        var definition = dataset.Definition;
        return new DatasetMeta(
            definition.Code, placement.Scenario, definition.IsReserve ? "reserve" : "main", placement.Status,
            dataset.Rejections, placement.ReplacesCode,
            Source(dataset), License(dataset.Package), File(dataset, placement), Format(dataset), Content(dataset),
            dataset.Pii);
    }

    public static string Attribution(CkanPackage package)
    {
        ArgumentNullException.ThrowIfNull(package);

        var license = package.LicenseId == "CC-BY-4.0" ? "CC BY 4.0" : package.LicenseTitle;
        return $"Źródło: {package.Publisher}, otwartedane.metropoliagzm.pl, {license}";
    }

    private static SourceMeta? Source(FetchedDataset d) => d.Package is null ? null : new SourceMeta(
        d.Package.Id, d.Package.Title, d.Definition.Label, d.Package.Publisher, d.Package.PageUrl,
        d.Resource?.Url ?? string.Empty, d.Resource?.Name ?? string.Empty, FileDate(d.Resource?.Url));

    private static LicenseMeta? License(CkanPackage? package) =>
        package is null ? null : new LicenseMeta(package.LicenseId, package.LicenseTitle, Attribution(package));

    private static FileMeta? File(FetchedDataset d, Placement placement)
    {
        if (d.Bytes is null)
            return null;

        var workCopy = DatasetRules.NeedsWorkCopy(d.RowCount);
        return new FileMeta(placement.RelativePath, d.DownloadedAt, Convert.ToHexStringLower(SHA256.HashData(d.Bytes)),
            d.Bytes.LongLength, workCopy ? $"data/work/{placement.FileName}" : null, workCopy ? DatasetRules.WorkCopyRows : null);
    }

    private static FormatMeta? Format(FetchedDataset d) =>
        d.Table is null || d.Encoding is null ? null : new FormatMeta(d.Encoding.Name, d.Table.Separator.ToString(), d.Table.HeaderLine, d.Table.Headers);

    private static ContentMeta? Content(FetchedDataset d) =>
        d.Table is null ? null : new ContentMeta(d.Table.Rows.Count, d.Table.InconsistentRows, d.Columns);

    private static string? FileDate(string? url)
    {
        var match = UrlDate.Match(url ?? string.Empty);
        return match.Success ? $"{match.Groups[1].Value}-{match.Groups[2].Value}-{match.Groups[3].Value}" : null;
    }
}
