using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using GzmApp.Tests.Infrastructure;

namespace GzmApp.Tests;

// Level 3 (branch ratunek/etap-2): GET /?handler=Szukaj&q=... over HTTP. Requests carry "HX-Request: true",
// so they get the list fragment also on the later branches.
[Collection(AppsCollection.Name)]
public partial class SearchTests(AppsFixture apps)
{
    public static TheoryData<string> Codes => ScenarioData.Codes;

    [Theory]
    [MemberData(nameof(Codes))]
    [Trait("Spec", "E2-01")]
    [Trait("Poziom", "3")]
    public async Task Filter_for_the_checkpoint_phrase_gives_E2(string code)
    {
        var scenario = ScenarioData.Get(code);
        var result = await SearchAsync(code, scenario.Value("E2.phrase"));

        Assert.Equal(int.Parse(scenario.Value("E2.count"), CultureInfo.InvariantCulture), result.Count);
        Assert.Equal(scenario.Numbered("E2.result").Select(c => Normalize(Title(c.Value))).Order(), result.Titles.Order());
    }

    [Theory]
    [MemberData(nameof(Codes))]
    [Trait("Spec", "E2-02")]
    [Trait("Poziom", "3")]
    public async Task Letter_case_does_not_matter(string code)
    {
        var phrase = ScenarioData.Get(code).Value("E2.phrase");

        Assert.Equal((await SearchAsync(code, phrase)).Count, (await SearchAsync(code, phrase.ToUpper(CultureInfo.GetCultureInfo("pl-PL")))).Count);
    }

    // Phrases with Polish letters: the same phrase without them finds at least the same records (collation _CI_AI).
    [Theory]
    [MemberData(nameof(Codes))]
    [Trait("Spec", "E2-03")]
    [Trait("Poziom", "3")]
    public async Task Phrase_with_Polish_letters_works_also_without_them(string code)
    {
        var phrase = ScenarioData.Get(code).Value("E2.phrase");
        var plain = WithoutPolishLetters(phrase);
        var with = await SearchAsync(code, phrase);

        Assert.Equal(int.Parse(ScenarioData.Get(code).Value("E2.count"), CultureInfo.InvariantCulture), with.Count);
        Assert.True(plain == phrase || (await SearchAsync(code, plain)).Count >= with.Count, $"„{plain}” daje mniej wyników niż „{phrase}”");
    }

    [Theory]
    [MemberData(nameof(Codes))]
    [Trait("Spec", "E2-04")]
    [Trait("Poziom", "3")]
    public async Task Empty_phrase_returns_the_first_20_records(string code)
    {
        var result = await SearchAsync(code, string.Empty);
        var table = ScenarioData.Get(code).Text("table");
        var first = await apps.For(code).Database.PairsAsync($"SELECT TOP 20 [SourceRow], NULL FROM dbo.[{table}] ORDER BY [SourceRow]");

        Assert.Equal(first.Select(r => r.SourceRow), result.SourceRows);
    }

    [Theory]
    [MemberData(nameof(Codes))]
    [Trait("Spec", "E2-05")]
    [Trait("Poziom", "3")]
    public async Task SQL_injection_gives_0_results_and_the_log_shows_parameter_q(string code)
    {
        const string attack = "' OR 1=1 --";
        var result = await SearchAsync(code, attack);
        var sql = apps.For(code).Logs.Messages("SQL").ToList();

        Assert.Equal(0, result.Count);
        Assert.Contains(sql, m => m.Contains("@q", StringComparison.Ordinal) && m.Contains("OR 1=1", StringComparison.Ordinal));
        Assert.DoesNotContain(sql, m => m.Split(" | parametry:")[0].Contains("OR 1=1", StringComparison.Ordinal));
    }

    [Theory]
    [MemberData(nameof(Codes))]
    [Trait("Spec", "E2-06")]
    [Trait("Poziom", "3")]
    public async Task Phrase_longer_than_100_characters_gives_400(string code)
    {
        var response = await GetAsync(code, new string('a', 101));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.DoesNotContain("<html", await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
    }

    private sealed record Result(int Count, IReadOnlyList<string> Titles, IReadOnlyList<int> SourceRows);

    private async Task<Result> SearchAsync(string code, string phrase)
    {
        var response = await GetAsync(code, phrase);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("<html", html, StringComparison.OrdinalIgnoreCase);
        var document = Html.Parse(html);
        var items = document.QuerySelectorAll("#lista > li").ToList();
        var count = int.Parse(Counter().Match(document.QuerySelector("#licznik")!.TextContent).Groups[1].Value, CultureInfo.InvariantCulture);
        return new Result(count, items.Select(ItemTitle).ToList(), items.Select(ItemRow).ToList());
    }

    private Task<HttpResponseMessage> GetAsync(string code, string phrase)
    {
        var client = apps.For(code).CreateBrowser();
        var request = new HttpRequestMessage(HttpMethod.Get, "/?handler=Szukaj&q=" + Uri.EscapeDataString(phrase));
        request.Headers.Add("HX-Request", "true");
        return client.SendAsync(request);
    }

    // Etap 2 writes "Nr N: title" in <li>; later stages put the title in <span class="tytul">.
    private static string ItemTitle(IElement item) =>
        Normalize(item.QuerySelector(".tytul")?.TextContent ?? ItemLine().Match(item.TextContent).Groups["title"].Value);

    private static int ItemRow(IElement item) =>
        int.Parse(ItemLine().Match(item.TextContent).Groups["row"].Value, CultureInfo.InvariantCulture);

    private static string Title(string checkpoint) =>
        checkpoint == "NULL" ? "(bez nazwy)" : ScenarioData.Unescape(checkpoint);

    private static string Normalize(string text) => Whitespace().Replace(text, " ").Trim();

    private static string WithoutPolishLetters(string phrase) =>
        string.Concat(phrase.Replace('ł', 'l').Replace('Ł', 'L').Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark));

    [GeneratedRegex(@"Wyników: (\d+)")]
    private static partial Regex Counter();

    [GeneratedRegex(@"Nr (?<row>\d+): (?<title>.*)", RegexOptions.Singleline)]
    private static partial Regex ItemLine();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
