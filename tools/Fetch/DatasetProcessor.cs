namespace Fetch;

// One dataset: package_show -> CSV resource -> bytes -> encoding -> CSV -> profile -> rules.
public sealed class DatasetProcessor
{
    private readonly CkanClient _ckan;

    public DatasetProcessor(CkanClient ckan)
    {
        ArgumentNullException.ThrowIfNull(ckan);
        _ckan = ckan;
    }

    public async Task<FetchedDataset> ProcessAsync(DatasetDefinition definition, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(definition);

        try
        {
            return await FetchAsync(definition, ct);
        }
        catch (Exception ex) when (Retry.IsTransient(ex))
        {
            return FetchedDataset.Failed(definition, $"błąd URL/HTTP po {Retry.Attempts} próbach: {ex.Message}");
        }
    }

    private async Task<FetchedDataset> FetchAsync(DatasetDefinition definition, CancellationToken ct)
    {
        var package = await _ckan.GetPackageAsync(definition.Id, ct);
        var resource = package.LatestCsv();
        if (resource is null)
            return FetchedDataset.Failed(definition, "brak zasobu CSV", package);

        var bytes = await _ckan.DownloadAsync(resource.Url, ct);
        return Profile(definition, package, resource, bytes);
    }

    private static FetchedDataset Profile(DatasetDefinition definition, CkanPackage package, CkanResource resource, byte[] bytes)
    {
        var encoding = EncodingDetector.Detect(bytes);
        var table = CsvLoader.Load(encoding.Decode(bytes));
        var columns = table.Headers.Select((name, index) => ColumnProfiler.Profile(name, table.Column(index))).ToList();
        var pii = PiiScanner.Scan(table, columns);
        var rejections = DatasetRules.Rejections(table.Rows.Count, columns);
        return new FetchedDataset(definition, DateTimeOffset.UtcNow, package, resource, bytes, encoding, table, columns, pii, rejections);
    }
}
