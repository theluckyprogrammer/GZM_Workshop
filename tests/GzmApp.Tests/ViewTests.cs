using System.Globalization;
using System.Net;
using GzmApp.Tests.Infrastructure;

namespace GzmApp.Tests;

// Level 4, E3 (branch ratunek/etap-3): the HTMX page, the list fragment and the statistic tile.
[Collection(AppsCollection.Name)]
public class ViewTests(AppsFixture apps)
{
    public static TheoryData<string> Codes => ScenarioData.Codes;

    [Theory]
    [MemberData(nameof(Codes))]
    [Trait("Spec", "E3-01")]
    [Trait("Poziom", "4")]
    public async Task HTMX_request_gets_a_fragment_without_html(string code)
    {
        var (status, html) = await GetAsync(code, Search(code), htmx: true);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.DoesNotContain("<html", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("id=\"licznik\"", html, StringComparison.Ordinal);
    }

    [Theory]
    [MemberData(nameof(Codes))]
    [Trait("Spec", "E3-02")]
    [Trait("Poziom", "4")]
    public async Task Request_without_HTMX_header_gets_the_whole_page(string code)
    {
        var (status, html) = await GetAsync(code, Search(code), htmx: false);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.NotNull(Html.Parse(html).QuerySelector("html body #wyniki #licznik"));
    }

    [Theory]
    [MemberData(nameof(Codes))]
    [Trait("Spec", "E3-03")]
    [Trait("Poziom", "4")]
    public async Task Statistic_tile_equals_E3(string code)
    {
        var scenario = ScenarioData.Get(code);
        var tile = Html.Parse((await GetAsync(code, "/", htmx: false)).Html).QuerySelector("#statystyka")!;
        var groups = tile.QuerySelectorAll("li[data-stat]").Select(li => (li.TextContent.Split(": ")[0].Trim(), li.QuerySelector("strong")!.TextContent)).ToList();

        if (scenario.Checkpoints.ContainsKey("E3.value"))
            Assert.Equal(scenario.Value("E3.value"), tile.QuerySelector("[data-stat-value]")!.TextContent.Trim().Replace(',', '.'));
        else
            Assert.Equal(scenario.Numbered("E3.group").Select(c => (c.Label.Split(": ", 2)[1], c.Value)), groups);
    }

    [Theory]
    [MemberData(nameof(Codes))]
    [Trait("Spec", "E3-04")]
    [Trait("Poziom", "4")]
    public async Task Page_loads_the_local_htmx_4_file(string code)
    {
        var scripts = Html.Parse((await GetAsync(code, "/", htmx: false)).Html).QuerySelectorAll("script[src]").Select(s => s.GetAttribute("src")!).ToList();

        Assert.Contains(scripts, s => s.StartsWith("/lib/htmx/4.0.0/htmx.min", StringComparison.Ordinal));
        Assert.DoesNotContain(scripts, s => s.Contains("://", StringComparison.Ordinal));
    }

    [Theory]
    [MemberData(nameof(Codes))]
    [Trait("Spec", "E3-05")]
    [Trait("Poziom", "4")]
    public async Task Body_has_inherited_hx_headers_with_the_token(string code) =>
        Assert.False(string.IsNullOrWhiteSpace(Html.BodyToken((await GetAsync(code, "/", htmx: false)).Html)));

    [Theory]
    [MemberData(nameof(Codes))]
    [Trait("Spec", "E3-06")]
    [Trait("Poziom", "4")]
    public async Task Search_box_waits_300_ms(string code)
    {
        var input = Html.Parse((await GetAsync(code, "/", htmx: false)).Html).QuerySelector("input[name=q]")!;

        Assert.Equal("?handler=Szukaj", input.GetAttribute("hx-get"));
        Assert.Contains("delay:300ms", input.GetAttribute("hx-trigger"), StringComparison.Ordinal);
        Assert.Equal("#wyniki", input.GetAttribute("hx-target"));
    }

    // The table disappears for a moment: the HTMX request gets a short fragment with status 500, not the error page.
    [Fact]
    [Trait("Spec", "E3-07")]
    [Trait("Poziom", "4")]
    public async Task Server_error_in_HTMX_request_returns_a_short_fragment()
    {
        const string code = "s02";
        var app = apps.For(code);
        await GetAsync(code, "/", htmx: false);
        var table = ScenarioData.Get(code).Text("table");
        await app.Database.ExecuteAsync($"EXEC sp_rename 'dbo.{table}', '{table}_Ukryta'");
        try
        {
            var (status, html) = await GetAsync(code, Search(code), htmx: true);
            Assert.Equal(HttpStatusCode.InternalServerError, status);
            Assert.DoesNotContain("<html", html, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("alert", html, StringComparison.Ordinal);
        }
        finally
        {
            await app.Database.ExecuteAsync($"EXEC sp_rename 'dbo.{table}_Ukryta', '{table}'");
        }
    }

    private static string Search(string code) =>
        "/?handler=Szukaj&q=" + Uri.EscapeDataString(ScenarioData.Get(code).Value("E2.phrase"));

    private async Task<(HttpStatusCode Status, string Html)> GetAsync(string code, string path, bool htmx)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, path);
        if (htmx)
            request.Headers.Add("HX-Request", "true");
        var response = await apps.For(code).CreateBrowser().SendAsync(request);
        return (response.StatusCode, await response.Content.ReadAsStringAsync());
    }
}
