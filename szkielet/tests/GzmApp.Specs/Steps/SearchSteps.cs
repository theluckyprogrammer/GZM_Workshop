using System.Globalization;
using System.Net;
using Spectree;

namespace GzmApp.Specs.Steps;

// Krok 2 (sNN-002-szukaj-rekordow): GET /?handler=Szukaj&q=… with "HX-Request: true", compared with the same
// LIKE query run on the test database.
public sealed class SearchSteps
{
    private const int Take = 20;

    [Step("Wyszukiwarka szuka frazy w dowolnym miejscu pola `{pole}` (kolumna w pliku: `{kolumna}`).")]
    public Task FindsPhraseAnywhere(string field, string csv) => AssertFindsAsync(new SearchRule(field, Prefix: false));

    [Step("Wyszukiwarka pokazuje rekordy, w których pole `{pole}` (kolumna w pliku: `{kolumna}`) zaczyna się od frazy.")]
    public Task FindsPhraseAtStart(string field, string csv) => AssertFindsAsync(new SearchRule(field, Prefix: true));

    [Step("Wielkość liter i polskie znaki we frazie nie mają znaczenia.")]
    public async Task CaseAndPolishLettersDoNotMatter()
    {
        var rule = SearchRule.FromSpec();
        var phrase = await rule.PhraseAsync();
        var expected = await Records.CountAsync(rule.Where, new { pattern = rule.Pattern(phrase) });
        var upper = (await Pages.SearchAsync(phrase.ToUpper(CultureInfo.GetCultureInfo("pl-PL")))).Count;
        var lower = (await Pages.SearchAsync(phrase)).Count;
        var plain = (await Pages.SearchAsync(SearchRule.WithoutPolishLetters(phrase))).Count;

        Assert.True(lower == expected && upper == expected, $"„{phrase}” daje {lower} wyników, a wielkimi literami – {upper}; pasujących rekordów jest {expected}. Użyjcie COLLATE Latin1_General_CI_AI.");
        Assert.True(plain >= expected, $"„{SearchRule.WithoutPolishLetters(phrase)}” (bez polskich znaków) daje {plain} wyników, a pasujących rekordów jest {expected}.");
    }

    [Step("Licznik pokazuje liczbę wszystkich trafień, a lista najwyżej 20 rekordów.")]
    public async Task CounterShowsAllListShowsTwenty()
    {
        var result = await Pages.SearchAsync(string.Empty);
        var all = await Records.CountAsync();
        var withField = await Records.CountAsync($"{Records.Field(SearchRule.FromSpec().Field)} IS NOT NULL");

        Assert.True(result.Count == all || result.Count == withField, $"Licznik dla pustej frazy pokazuje {result.Count}, a pasujących rekordów jest {all}.");
        Assert.True(result.Items.Count == Math.Min(Take, result.Count), $"Lista ma {result.Items.Count} rekordów, a powinna mieć najwyżej {Take} (licznik: {result.Count}).");
    }

    [Step("Pusta fraza pokazuje pierwsze 20 rekordów według numeru wiersza.")]
    public async Task EmptyPhraseShowsFirstTwenty()
    {
        var shown = (await Pages.SearchAsync(string.Empty)).SourceRows;
        var first = await Records.FirstRowsAsync(Take);
        var firstWithField = await Records.FirstRowsAsync(Take, $"{Records.Field(SearchRule.FromSpec().Field)} IS NOT NULL");

        Assert.True(shown.SequenceEqual(first) || shown.SequenceEqual(firstWithField),
            $"Pusta fraza pokazuje wiersze {string.Join(", ", shown)}, a powinna pierwsze {Take} według `SourceRow`: {string.Join(", ", first)}.");
    }

    [Step("Fraza `' OR 1=1 --` nie znajduje żadnego rekordu, bo trafia do zapytania jako parametr.")]
    public async Task InjectionIsAParameter()
    {
        const string attack = "' OR 1=1 --";
        var result = await Pages.SearchAsync(attack);
        var sql = (await ActiveScenario.AppAsync()).Logs.Messages("SQL").ToList();

        Assert.True(result.Count == 0, $"Fraza „{attack}” znalazła {result.Count} rekordów – tekst SQL jest sklejany z frazą. Użyjcie parametru @q.");
        Assert.False(sql.Any(m => m.Split(" | parametry:")[0].Contains("OR 1=1", StringComparison.Ordinal)), "Fraza trafiła do treści zapytania SQL (log „SQL”), a ma być parametrem @q.");
    }

    [Step("Fraza dłuższa niż 100 znaków dostaje krótki komunikat ze statusem 400.")]
    public async Task LongPhraseGets400()
    {
        var (status, html) = await Pages.GetAsync(Pages.SearchPath + new string('a', 101), htmx: true);

        Assert.True(status == HttpStatusCode.BadRequest, $"Fraza ze 101 znaków dostaje status {(int)status} zamiast 400.");
        Assert.False(html.Contains("<html", StringComparison.OrdinalIgnoreCase), "Przy statusie 400 wraca cała strona, a ma wrócić krótki fragment `<p class=\"alert alert-warning\">…</p>`.");
    }

    private static async Task AssertFindsAsync(SearchRule rule)
    {
        var phrase = await rule.PhraseAsync();
        var result = await Pages.SearchAsync(phrase);
        var count = await Records.CountAsync(rule.Where, new { pattern = rule.Pattern(phrase) });
        var first = await Records.FirstRowsAsync(Take, rule.Where, new { pattern = rule.Pattern(phrase) });

        Assert.True(result.Count == count, $"Fraza „{phrase}” w polu `{rule.Field}`: licznik pokazuje {result.Count}, a pasujących rekordów jest {count}.");
        Assert.True(result.SourceRows.SequenceEqual(first), $"Fraza „{phrase}”: lista ma wiersze {string.Join(", ", result.SourceRows)}, a powinna {string.Join(", ", first)} (według `SourceRow`).");
    }
}
