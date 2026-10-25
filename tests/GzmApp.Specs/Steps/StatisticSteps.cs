using AngleSharp.Dom;
using GzmApp.Tests.Infrastructure;
using Spectree;

namespace GzmApp.Specs.Steps;

// Krok 3, decision 3: the statistic tile in <div id="statystyka">, compared with the same query run on the test
// database. One number sits in [data-stat-value], a list of values in <li data-stat>value: <strong>count</strong></li>.
// Several tiles are fine ("Co dalej?"): one of them has to match.
public sealed class StatisticSteps
{
    [Step("Kafelek ze statystyką pokazuje sumę pola `{pole}`.")]
    public Task ShowsSum(string field) => AssertValueAsync($"SUM({Records.Field(field)})", $"suma pola `{field}`");

    [Step("Kafelek ze statystyką pokazuje średnią pola `{pole}` z 2 miejscami po przecinku.")]
    public Task ShowsAverage(string field) =>
        AssertValueAsync($"ROUND(AVG(CAST({Records.Field(field)} AS decimal(18,2))), 2)", $"średnia pola `{field}`");

    [Step("Kafelek ze statystyką pokazuje 5 najczęstszych wartości pola `{pole}` z liczbą rekordów.")]
    public Task ShowsTop5(string field) => AssertGroupsAsync(field, "TOP 5 ");

    [Step("Kafelek ze statystyką pokazuje liczbę rekordów dla każdej wartości pola `{pole}`.")]
    public Task ShowsCountBy(string field) => AssertGroupsAsync(field, string.Empty);

    private static async Task AssertValueAsync(string aggregate, string description)
    {
        var expected = (await Records.RowsAsync($"SELECT {aggregate} FROM {Records.Table}")).Single()[0];
        var shown = (await TileAsync()).QuerySelectorAll("[data-stat-value]").Select(e => e.TextContent.Trim()).ToList();
        Assert.True(shown.Count > 0, "W `<div id=\"statystyka\">` nie ma liczby w `<p data-stat-value>`.");

        var value = expected is null ? (decimal?)null : Convert.ToDecimal(expected, System.Globalization.CultureInfo.InvariantCulture);
        Assert.True(shown.Any(s => DisplayForms.Number(s) == value), $"Kafelek pokazuje {string.Join(" / ", shown)}, a {description} w bazie to {value}.");
    }

    private static async Task AssertGroupsAsync(string field, string top)
    {
        var column = Records.Field(field);
        var expected = await Records.RowsAsync($"SELECT {top}{column}, COUNT(*) FROM {Records.Table} WHERE {column} IS NOT NULL GROUP BY {column} ORDER BY COUNT(*) DESC, {column}");
        var lists = (await TileAsync()).QuerySelectorAll("li[data-stat]").GroupBy(li => li.ParentElement).Select(g => g.ToList()).ToList();
        Assert.True(lists.Count > 0, "W `<div id=\"statystyka\">` nie ma listy `<li data-stat>wartość: <strong>liczba</strong></li>`.");

        var matches = lists.Any(shown => shown.Count == expected.Count && expected.Zip(shown).All(p => Same(p.Second, p.First[0], (int)p.First[1]!)));
        Assert.True(matches, $"Kafelek pokazuje: {string.Join("; ", lists.SelectMany(l => l).Select(s => DisplayForms.Normalize(s.TextContent)))}. W bazie: {string.Join("; ", expected.Select(e => $"{DisplayForms.Of(e[0]).FirstOrDefault()}: {e[1]}"))}.");
    }

    // "Bieruń: 6" – the label before the last colon shows the value, <strong> holds the count.
    private static bool Same(IElement item, object? value, int count)
    {
        var text = DisplayForms.Normalize(item.TextContent);
        var label = text.Contains(':') ? text[..text.LastIndexOf(':')] : text;
        return DisplayForms.Number(item.QuerySelector("strong")?.TextContent ?? string.Empty) == count && DisplayForms.Shows(label, value);
    }

    private static async Task<IElement> TileAsync()
    {
        var (_, html) = await Pages.GetAsync("/", htmx: false);
        return Html.Parse(html).QuerySelector("#statystyka") ?? throw new InvalidOperationException("Na stronie głównej nie ma kafelka `<div id=\"statystyka\">`.");
    }
}
