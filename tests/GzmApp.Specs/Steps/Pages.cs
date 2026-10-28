using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using GzmApp.Specs.Infrastructure;

namespace GzmApp.Specs.Steps;

public sealed record SearchItem(int SourceRow, string Title, string Text);

public sealed record SearchResult(int Count, IReadOnlyList<SearchItem> Items)
{
    public IReadOnlyList<int> SourceRows => Items.Select(i => i.SourceRow).ToList();
}

// HTTP requests of the steps, as the browser and HTMX send them, and the list fragment read back.
public static partial class Pages
{
    public const string SearchPath = "/?handler=Szukaj&q=";

    public static async Task<(HttpStatusCode Status, string Html)> GetAsync(string path, bool htmx, HttpClient? client = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, path);
        if (htmx)
            request.Headers.Add("HX-Request", "true");
        var response = await (client ?? await ActiveScenario.BrowserAsync()).SendAsync(request);
        return (response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    public static async Task<SearchResult> SearchAsync(string phrase)
    {
        var (status, html) = await GetAsync(SearchPath + Uri.EscapeDataString(phrase), htmx: true);
        Assert.True(status == HttpStatusCode.OK, $"Wyszukiwarka dla frazy „{phrase}” odpowiada statusem {(int)status} zamiast 200.");
        Assert.False(html.Contains("<html", StringComparison.OrdinalIgnoreCase), "Wyszukiwarka odsyła całą stronę zamiast fragmentu z listą (handler OnGetSzukaj i Partial(\"_Lista\", …)).");
        return ReadList(Html.Parse(html));
    }

    public static SearchResult ReadList(IParentNode page)
    {
        ArgumentNullException.ThrowIfNull(page);

        var counter = page.QuerySelector("#licznik")?.TextContent ?? string.Empty;
        var count = Counter().Match(DisplayForms.Normalize(counter));
        Assert.True(count.Success, "Brak licznika `<p id=\"licznik\">Wyników: N</p>` w liście wyników.");
        var items = (page.QuerySelector("#lista") ?? page.QuerySelector("ul"))?.QuerySelectorAll(":scope > li").ToList() ?? [];
        return new SearchResult(int.Parse(count.Groups[1].Value.Replace(" ", string.Empty), CultureInfo.InvariantCulture), items.Select(Item).ToList());
    }

    public static async Task<HttpResponseMessage> StarAsync(HttpClient client, HttpMethod method, int recordId, string? token)
    {
        ArgumentNullException.ThrowIfNull(client);

        var request = new HttpRequestMessage(method, $"/?handler=Ulubione&id={recordId}");
        request.Headers.Add("HX-Request", "true");
        if (token is not null)
            request.Headers.Add("RequestVerificationToken", token);
        return await client.SendAsync(request);
    }

    public static async Task<HttpClient> SignedInAsync()
    {
        var client = await ActiveScenario.BrowserAsync();
        await TestUser.RegisterAsync(client);
        return client;
    }

    private static SearchItem Item(IElement li)
    {
        var text = DisplayForms.Normalize(li.TextContent);
        var line = ItemLine().Match(text);
        Assert.True(line.Success, $"Wynik na liście nie zaczyna się od „Nr SourceRow: tytuł”: „{text}”.");
        var title = li.QuerySelector(".tytul")?.TextContent ?? line.Groups["title"].Value;
        return new SearchItem(int.Parse(line.Groups["row"].Value, CultureInfo.InvariantCulture), DisplayForms.Normalize(title), text);
    }

    [GeneratedRegex(@"Wyników: (\d[\d ]*)")]
    private static partial Regex Counter();

    [GeneratedRegex(@"Nr (?<row>\d+): ?(?<title>.*)")]
    private static partial Regex ItemLine();
}
