using GzmApp.Tests.Infrastructure;
using Microsoft.Playwright;

namespace GzmApp.Tests;

// Level 5 (branch ratunek/etap-4): a real browser with HTMX 4, only scenarios s06, s04, s02.
public class BrowserTests
{
    public static TheoryData<string> Codes => new("s06", "s04", "s02");

    [Theory]
    [MemberData(nameof(Codes))]
    [Trait("Spec", "UI-01")]
    [Trait("Poziom", "5")]
    public async Task Typing_sends_one_request_after_300_ms_and_changes_the_list(string code)
    {
        await using var ui = await BrowserApp.StartAsync(code);
        var page = await OpenAsync(ui);
        var requests = new List<string>();
        page.Request += (_, r) => requests.AddIf(r.Url.Contains("handler=Szukaj", StringComparison.Ordinal), r.Url);

        await page.Locator("input[name=q]").PressSequentiallyAsync(ScenarioData.Get(code).Value("E2.phrase"), new() { Delay = 60 });
        await Assertions.Expect(page.Locator("#licznik")).ToHaveTextAsync($"Wyników: {ScenarioData.Get(code).Value("E2.count")}");
        await page.WaitForTimeoutAsync(700);

        Assert.Single(requests);
    }

    [Theory]
    [MemberData(nameof(Codes))]
    [Trait("Spec", "UI-02")]
    [Trait("Poziom", "5")]
    public async Task No_JavaScript_errors_in_the_console(string code)
    {
        await using var ui = await BrowserApp.StartAsync(code);
        var errors = new List<string>();
        var page = await ui.Browser.NewPageAsync();
        page.Console += (_, m) => errors.AddIf(m.Type == "error", m.Text);
        page.PageError += (_, e) => errors.Add(e);

        await page.GotoAsync(ui.BaseUrl);
        await page.Locator("input[name=q]").PressSequentiallyAsync(ScenarioData.Get(code).Value("E2.phrase"));
        await Assertions.Expect(page.Locator("#licznik")).ToHaveTextAsync($"Wyników: {ScenarioData.Get(code).Value("E2.count")}");

        Assert.Empty(errors);
    }

    [Theory]
    [MemberData(nameof(Codes))]
    [Trait("Spec", "UI-03")]
    [Trait("Poziom", "5")]
    public async Task Signed_in_user_clicks_the_star_and_the_record_is_saved(string code)
    {
        await using var ui = await BrowserApp.StartAsync(code);
        var page = await ui.Browser.NewPageAsync();
        await RegisterAsync(page, ui.BaseUrl);
        await page.GotoAsync(ui.BaseUrl);
        var record7 = page.Locator("#lista > li", new() { HasText = "Nr 7:" });

        await record7.Locator("button[data-favorite=false]").ClickAsync();
        await Assertions.Expect(record7.Locator("button[data-favorite=true]")).ToBeVisibleAsync();

        Assert.Equal(1, await ui.App.Database.ScalarAsync<int>("SELECT COUNT(*) FROM dbo.Favorites"));
    }

    [Theory]
    [MemberData(nameof(Codes))]
    [Trait("Spec", "UI-04")]
    [Trait("Poziom", "5")]
    public async Task Server_error_shows_a_message_in_the_results_not_an_error_page(string code)
    {
        await using var ui = await BrowserApp.StartAsync(code);
        var page = await OpenAsync(ui);
        var table = ScenarioData.Get(code).Text("table");
        await ui.App.Database.ExecuteAsync($"EXEC sp_rename 'dbo.{table}', '{table}_Ukryta'");

        await page.Locator("input[name=q]").PressSequentiallyAsync(ScenarioData.Get(code).Value("E2.phrase"));
        await Assertions.Expect(page.Locator("#wyniki .alert")).ToBeVisibleAsync();

        Assert.Equal(1, await page.Locator("input[name=q]").CountAsync());
        await ui.App.Database.ExecuteAsync($"EXEC sp_rename 'dbo.{table}_Ukryta', '{table}'");
    }

    [Theory]
    [MemberData(nameof(Codes))]
    [Trait("Spec", "UI-05")]
    [Trait("Poziom", "5")]
    public async Task Page_works_with_requests_outside_the_app_blocked(string code)
    {
        await using var ui = await BrowserApp.StartAsync(code);
        var page = await ui.Browser.NewPageAsync();
        var blocked = new List<string>();
        await page.RouteAsync("**/*", async route => await (route.Request.Url.StartsWith(ui.BaseUrl, StringComparison.Ordinal)
            ? route.ContinueAsync()
            : blocked.AddAndAbort(route)));

        await page.GotoAsync(ui.BaseUrl);
        await page.Locator("input[name=q]").PressSequentiallyAsync(ScenarioData.Get(code).Value("E2.phrase"));

        await Assertions.Expect(page.Locator("#licznik")).ToHaveTextAsync($"Wyników: {ScenarioData.Get(code).Value("E2.count")}");
        Assert.Empty(blocked);
    }

    private static async Task<IPage> OpenAsync(BrowserApp ui)
    {
        var page = await ui.Browser.NewPageAsync();
        await page.GotoAsync(ui.BaseUrl);
        return page;
    }

    private static async Task RegisterAsync(IPage page, string baseUrl)
    {
        await page.GotoAsync(baseUrl + "/Identity/Account/Register");
        await page.FillAsync("#Input_Email", $"ui-{Guid.NewGuid():N}@example.com");
        await page.FillAsync("#Input_Password", TestUser.Password);
        await page.FillAsync("#Input_ConfirmPassword", TestUser.Password);
        await page.ClickAsync("#registerSubmit");
        await page.WaitForURLAsync(url => !url.Contains("/Register", StringComparison.Ordinal));
    }
}

internal static class BrowserTestExtensions
{
    public static void AddIf(this List<string> list, bool condition, string item)
    {
        if (condition)
            list.Add(item);
    }

    // A request to another host (CDN): recorded and blocked.
    public static Task AddAndAbort(this List<string> list, IRoute route)
    {
        list.Add(route.Request.Url);
        return route.AbortAsync();
    }
}
