using System.Globalization;
using System.Text.Json;

namespace Fetch;

public sealed record CkanResource(string Name, string Format, string Url, DateTimeOffset? Modified);

public sealed record CkanPackage(
    string Id, string Name, string Title, string Publisher,
    string LicenseId, string LicenseTitle, string PageUrl, IReadOnlyList<CkanResource> Resources)
{
    // The newest CSV resource. The file URL always comes from the API, never built by hand.
    public CkanResource? LatestCsv() => Resources
        .Where(IsCsv)
        .OrderByDescending(r => r.Modified)
        .FirstOrDefault();

    public static CkanPackage From(JsonElement result, string baseUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(baseUrl);

        var name = Text(result, "name");
        var organization = result.TryGetProperty("organization", out var org) ? Text(org, "title") : string.Empty;
        var resources = result.GetProperty("resources").EnumerateArray().Select(ToResource).ToList();
        return new CkanPackage(Text(result, "id"), name, Text(result, "title"), organization,
            Text(result, "license_id"), Text(result, "license_title"), $"{baseUrl}/dataset/{name}", resources);
    }

    private static bool IsCsv(CkanResource resource) =>
        resource.Format.Equals("CSV", StringComparison.OrdinalIgnoreCase)
        || resource.Url.EndsWith(".csv", StringComparison.OrdinalIgnoreCase);

    private static CkanResource ToResource(JsonElement r) =>
        new(Text(r, "name"), Text(r, "format"), Text(r, "url"), Date(Text(r, "last_modified")) ?? Date(Text(r, "created")));

    private static DateTimeOffset? Date(string value) =>
        DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var date) ? date : null;

    private static string Text(JsonElement element, string property) =>
        element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : string.Empty;
}
