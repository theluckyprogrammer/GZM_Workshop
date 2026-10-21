using System.Net;
using GzmApp.Tests.Infrastructure;

namespace GzmApp.Tests;

// Level 4, E4 (branch ratunek/etap-4): favorites need login and the antiforgery token from <body hx-headers:inherited>.
[Collection(AppsCollection.Name)]
public class FavoritesTests(AppsFixture apps)
{
    public static TheoryData<string> Codes => ScenarioData.Codes;

    [Theory]
    [MemberData(nameof(Codes))]
    [Trait("Spec", "E4-01")]
    [Trait("Poziom", "4")]
    public async Task Anonymous_user_is_sent_to_login_and_gets_401_for_the_star(string code)
    {
        var client = apps.For(code).CreateBrowser();
        var page = await client.GetAsync("/Ulubione");
        var star = await StarAsync(client, HttpMethod.Post, await Record7Async(client, code), await TestUser.TokenAsync(client));

        Assert.Equal(HttpStatusCode.Redirect, page.StatusCode);
        Assert.Contains("/Identity/Account/Login", page.Headers.Location!.ToString(), StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.Unauthorized, star.StatusCode);
    }

    [Theory]
    [MemberData(nameof(Codes))]
    [Trait("Spec", "E4-02")]
    [Trait("Poziom", "4")]
    public async Task Signed_in_user_without_token_gets_400(string code)
    {
        var client = await SignedInAsync(code);

        Assert.Equal(HttpStatusCode.BadRequest, (await StarAsync(client, HttpMethod.Post, await Record7Async(client, code), token: null)).StatusCode);
    }

    [Theory]
    [MemberData(nameof(Codes))]
    [Trait("Spec", "E4-03")]
    [Trait("Poziom", "4")]
    public async Task Signed_in_user_with_token_gets_200(string code)
    {
        var client = await SignedInAsync(code);
        var response = await StarAsync(client, HttpMethod.Post, await Record7Async(client, code), await TestUser.TokenAsync(client));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("data-favorite=\"true\"", await response.Content.ReadAsStringAsync(), StringComparison.Ordinal);
    }

    [Theory]
    [MemberData(nameof(Codes))]
    [Trait("Spec", "E4-04")]
    [Trait("Poziom", "4")]
    public async Task Adding_twice_gives_one_row(string code)
    {
        var client = await SignedInAsync(code);
        var (record, token) = (await Record7Async(client, code), await TestUser.TokenAsync(client));
        await StarAsync(client, HttpMethod.Post, record, token);
        await StarAsync(client, HttpMethod.Post, record, token);

        Assert.Single(await FavoriteTitlesAsync(client));
    }

    [Theory]
    [MemberData(nameof(Codes))]
    [Trait("Spec", "E4-05")]
    [Trait("Poziom", "4")]
    public async Task Removing_twice_does_not_fail(string code)
    {
        var client = await SignedInAsync(code);
        var (record, token) = (await Record7Async(client, code), await TestUser.TokenAsync(client));
        await StarAsync(client, HttpMethod.Post, record, token);

        Assert.Equal(HttpStatusCode.OK, (await StarAsync(client, HttpMethod.Delete, record, token)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await StarAsync(client, HttpMethod.Delete, record, token)).StatusCode);
        Assert.Empty(await FavoriteTitlesAsync(client));
    }

    [Theory]
    [MemberData(nameof(Codes))]
    [Trait("Spec", "E4-06")]
    [Trait("Poziom", "4")]
    public async Task User_B_does_not_see_favorites_of_user_A(string code)
    {
        var a = await SignedInAsync(code);
        await StarAsync(a, HttpMethod.Post, await Record7Async(a, code), await TestUser.TokenAsync(a));
        var b = await SignedInAsync(code);

        Assert.Single(await FavoriteTitlesAsync(a));
        Assert.Empty(await FavoriteTitlesAsync(b));
    }

    [Theory]
    [MemberData(nameof(Codes))]
    [Trait("Spec", "E4-07")]
    [Trait("Poziom", "4")]
    public async Task Favorite_record_7_has_the_title_E4(string code)
    {
        var client = await SignedInAsync(code);
        await StarAsync(client, HttpMethod.Post, await Record7Async(client, code), await TestUser.TokenAsync(client));

        Assert.Equal([Normalize(ScenarioData.Unescape(ScenarioData.Get(code).Value("E4.title")))], await FavoriteTitlesAsync(client));
    }

    private async Task<HttpClient> SignedInAsync(string code)
    {
        var client = apps.For(code).CreateBrowser();
        await TestUser.RegisterAsync(client);
        return client;
    }

    // RecordId of the record with SourceRow 7, from the list on the home page.
    private static async Task<string> Record7Async(HttpClient client, string code) =>
        Html.Parse(await client.GetStringAsync("/")).QuerySelectorAll("#lista > li")
            .First(li => li.TextContent.Contains("Nr 7:", StringComparison.Ordinal)).GetAttribute("data-record-id")
        ?? throw new InvalidOperationException($"{code}: brak rekordu nr 7 na liście.");

    private static Task<HttpResponseMessage> StarAsync(HttpClient client, HttpMethod method, string recordId, string? token)
    {
        var request = new HttpRequestMessage(method, $"/?handler=Ulubione&id={recordId}");
        request.Headers.Add("HX-Request", "true");
        if (token is not null)
            request.Headers.Add("RequestVerificationToken", token);
        return client.SendAsync(request);
    }

    private static async Task<IReadOnlyList<string>> FavoriteTitlesAsync(HttpClient client) =>
        Html.Parse(await client.GetStringAsync("/Ulubione")).QuerySelectorAll("#ulubione .tytul").Select(t => Normalize(t.TextContent)).ToList();

    private static string Normalize(string text) => string.Join(' ', text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
