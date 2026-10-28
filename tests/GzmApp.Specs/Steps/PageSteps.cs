using System.Net;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using GzmApp.Specs.Infrastructure;
using Spectree;

namespace GzmApp.Specs.Steps;

// Krok 3 (sNN-003-pokaz-strone): the home page with HTMX, the fields on the list (decision 2) and the footer.
// The statistic tile (decision 3) is in StatisticSteps.
public sealed partial class PageSteps
{
    private const int CheckedItems = 5;

    [Step("Wyszukiwarka na stronie głównej wysyła frazę 300 ms po ostatnim wpisanym znaku.")]
    public async Task SearchBoxWaits300Ms()
    {
        var input = await SearchBoxAsync();

        Assert.True(input.GetAttribute("hx-get")?.Contains("handler=Szukaj", StringComparison.Ordinal) == true, "Pole wyszukiwania nie ma `hx-get=\"?handler=Szukaj\"`.");
        Assert.True(input.GetAttribute("hx-trigger")?.Contains("delay:300ms", StringComparison.Ordinal) == true, "Pole wyszukiwania nie ma `hx-trigger=\"keyup changed delay:300ms\"`.");
    }

    [Step("Wyszukiwarka podmienia tylko listę wyników, a nie całą stronę.")]
    public async Task SearchSwapsOnlyTheList()
    {
        var input = await SearchBoxAsync();
        var (_, html) = await Pages.GetAsync(Pages.SearchPath + Uri.EscapeDataString(await SearchRule.FromSpec().PhraseAsync()), htmx: true);

        Assert.True(input.GetAttribute("hx-target") == "#wyniki", "Pole wyszukiwania nie ma `hx-target=\"#wyniki\"`.");
        Assert.False(html.Contains("<html", StringComparison.OrdinalIgnoreCase), "Żądanie HTMX (nagłówek HX-Request) dostaje całą stronę, a ma dostać sam fragment `_Lista`.");
        Assert.True(html.Contains("id=\"licznik\"", StringComparison.Ordinal), "Fragment z wyszukiwarki nie ma licznika `<p id=\"licznik\">`.");
    }

    [Step("Adres wyszukiwarki otwarty wprost w przeglądarce pokazuje całą stronę z wynikami.")]
    public async Task DirectAddressShowsWholePage()
    {
        var (status, html) = await Pages.GetAsync(Pages.SearchPath + Uri.EscapeDataString(await SearchRule.FromSpec().PhraseAsync()), htmx: false);

        Assert.True(status == HttpStatusCode.OK, $"Adres wyszukiwarki bez HTMX odpowiada statusem {(int)status}.");
        Assert.True(Html.Parse(html).QuerySelector("html body #wyniki #licznik") is not null, "Adres wyszukiwarki otwarty wprost pokazuje sam fragment, a ma pokazać całą stronę z listą w `<div id=\"wyniki\">`.");
    }

    [Step("Przy każdym rekordzie lista pokazuje tytuł `{tytul}` i pola: {pola}.")]
    public async Task ListShowsFields(string title, string fields)
    {
        var names = FieldName().Matches(fields).Select(m => m.Groups[1].Value).ToList();
        var items = (await Pages.SearchAsync(string.Empty)).Items.Take(CheckedItems).ToList();
        Assert.True(items.Count > 0, "Lista dla pustej frazy jest pusta.");
        var columns = string.Join(", ", new[] { title }.Concat(names).Select(Records.Field));
        foreach (var item in items)
            await AssertItemAsync(item, columns, [title, .. names]);
    }

    [Step("Stopka pokazuje źródło danych z licencją CC BY 4.0.")]
    public async Task FooterShowsAttribution()
    {
        var (_, html) = await Pages.GetAsync("/", htmx: false);
        var footer = DisplayForms.Normalize(Html.Parse(html).QuerySelector("#data-attribution")?.TextContent ?? string.Empty);

        Assert.True(footer == ActiveScenario.Data.Text("attribution"), $"Stopka pokazuje „{footer}”, a ma pokazać „{ActiveScenario.Data.Text("attribution")}” (DataAttribution w appsettings.json).");
    }

    private static async Task AssertItemAsync(SearchItem item, string columns, IReadOnlyList<string> names)
    {
        var values = (await Records.RowsAsync($"SELECT {columns} FROM {Records.Table} WHERE [SourceRow] = @row", new { row = item.SourceRow })).Single();
        var missing = names.Select((name, i) => (Name: name, Value: values[i])).Where(f => !DisplayForms.Shows(item.Text, f.Value)).ToList();

        Assert.True(missing.Count == 0, $"Wynik nr {item.SourceRow} nie pokazuje: {string.Join(", ", missing.Select(f => $"`{f.Name}` = {DisplayForms.Of(f.Value).FirstOrDefault()}"))}. Tekst wyniku: „{item.Text}”.");
    }

    private static async Task<IElement> SearchBoxAsync()
    {
        var (_, html) = await Pages.GetAsync("/", htmx: false);
        return Html.Parse(html).QuerySelector("input[name=q]") ?? throw new InvalidOperationException("Na stronie głównej nie ma pola `<input type=\"search\" name=\"q\">`.");
    }

    [GeneratedRegex(@"`(\w+)`")]
    private static partial Regex FieldName();
}
