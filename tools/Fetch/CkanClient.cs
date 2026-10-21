using System.Text.Json;

namespace Fetch;

public sealed class CkanClient
{
    public const string BaseUrl = "https://otwartedane.metropoliagzm.pl";

    private readonly HttpClient _http;

    public CkanClient(HttpClient http)
    {
        ArgumentNullException.ThrowIfNull(http);
        _http = http;
    }

    public async Task<CkanPackage> GetPackageAsync(string id, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        var url = $"{BaseUrl}/api/3/action/package_show?id={Uri.EscapeDataString(id)}";
        var json = await Retry.RunAsync(() => _http.GetStringAsync(url, ct), ct);
        using var document = JsonDocument.Parse(json);
        return CkanPackage.From(document.RootElement.GetProperty("result"), BaseUrl);
    }

    public Task<byte[]> DownloadAsync(string url, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        return Retry.RunAsync(() => _http.GetByteArrayAsync(url, ct), ct);
    }
}
