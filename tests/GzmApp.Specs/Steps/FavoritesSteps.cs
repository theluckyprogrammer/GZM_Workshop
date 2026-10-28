using System.Net;
using GzmApp.Specs.Infrastructure;
using Spectree;

namespace GzmApp.Specs.Steps;

// Krok 4 (sNN-004-zapisz-ulubione): login, the star and the antiforgery token from <body hx-headers:inherited>.
// Every step registers its own user, so the steps do not see each other's favorites. Decision 4 is in FavoritesPageSteps.
public sealed class FavoritesSteps
{
    [Step("Strona „Moje ulubione” wymaga zalogowania.")]
    public async Task FavoritesPageNeedsLogin()
    {
        var response = await (await ActiveScenario.BrowserAsync()).GetAsync("/Ulubione");

        Assert.True(response.StatusCode == HttpStatusCode.Redirect && response.Headers.Location?.ToString().Contains("/Identity/Account/Login", StringComparison.Ordinal) == true,
            $"Niezalogowany użytkownik na /Ulubione dostaje status {(int)response.StatusCode} zamiast przekierowania do logowania ([Authorize]).");
    }

    [Step("Kliknięcie ☆ bez logowania pokazuje prośbę o zalogowanie.")]
    public async Task AnonymousStarAsksForLogin()
    {
        var client = await ActiveScenario.BrowserAsync();
        var response = await Pages.StarAsync(client, HttpMethod.Post, await Records.RecordIdAsync(7), await TestUser.TokenAsync(client));

        Assert.True(response.StatusCode == HttpStatusCode.Unauthorized, $"Niezalogowany klik ☆ dostaje status {(int)response.StatusCode} zamiast 401.");
        Assert.True((await response.Content.ReadAsStringAsync()).Contains("Zaloguj się", StringComparison.Ordinal), "Odpowiedź 401 nie ma komunikatu „Zaloguj się, aby dodać do ulubionych”.");
    }

    [Step("Zalogowany użytkownik klika ☆, gwiazdka zmienia się w ★, a rekord trafia do ulubionych.")]
    public async Task StarAddsFavorite()
    {
        var client = await Pages.SignedInAsync();
        var response = await Pages.StarAsync(client, HttpMethod.Post, await Records.RecordIdAsync(7), await TestUser.TokenAsync(client));
        var star = await response.Content.ReadAsStringAsync();

        Assert.True(response.StatusCode == HttpStatusCode.OK, $"Klik ☆ po zalogowaniu dostaje status {(int)response.StatusCode} zamiast 200.");
        Assert.True(star.Contains("★", StringComparison.Ordinal) && star.Contains("data-favorite=\"true\"", StringComparison.Ordinal), $"Po kliknięciu wraca „{star.Trim()}”, a ma wrócić ★ z `data-favorite=\"true\"`.");
        Assert.True((await FavoritesPage.TitlesAsync(client)).Count == 1, "Po kliknięciu ☆ rekord nie pojawia się na stronie „Moje ulubione”.");
    }

    [Step("Dwa kliknięcia tej samej gwiazdki dają jeden wpis w ulubionych.")]
    public async Task TwoClicksGiveOneFavorite()
    {
        var client = await Pages.SignedInAsync();
        var (record, token) = (await Records.RecordIdAsync(7), await TestUser.TokenAsync(client));
        await Pages.StarAsync(client, HttpMethod.Post, record, token);
        await Pages.StarAsync(client, HttpMethod.Post, record, token);
        var titles = await FavoritesPage.TitlesAsync(client);

        Assert.True(titles.Count == 1, $"Po dwóch kliknięciach ☆ „Moje ulubione” pokazuje {titles.Count} wpisów zamiast 1.");
    }

    [Step("Usunięcie z ulubionych można powtórzyć bez błędu.")]
    public async Task RemovingTwiceDoesNotFail()
    {
        var client = await Pages.SignedInAsync();
        var (record, token) = (await Records.RecordIdAsync(7), await TestUser.TokenAsync(client));
        await Pages.StarAsync(client, HttpMethod.Post, record, token);
        var first = await Pages.StarAsync(client, HttpMethod.Delete, record, token);
        var second = await Pages.StarAsync(client, HttpMethod.Delete, record, token);

        Assert.True(first.StatusCode == HttpStatusCode.OK && second.StatusCode == HttpStatusCode.OK, $"Usunięcie dwa razy daje statusy {(int)first.StatusCode} i {(int)second.StatusCode} zamiast 200 i 200.");
        Assert.True((await FavoritesPage.TitlesAsync(client)).Count == 0, "Po usunięciu rekord nadal jest na stronie „Moje ulubione”.");
    }

    [Step("Użytkownik nie widzi ulubionych innych użytkowników.")]
    public async Task UserSeesOnlyOwnFavorites()
    {
        var a = await Pages.SignedInAsync();
        await Pages.StarAsync(a, HttpMethod.Post, await Records.RecordIdAsync(7), await TestUser.TokenAsync(a));
        var b = await Pages.SignedInAsync();
        var seenByB = await FavoritesPage.TitlesAsync(b);

        Assert.True(seenByB.Count == 0, $"Nowy użytkownik widzi {seenByB.Count} ulubionych innej osoby. Filtrujcie po `UserId`.");
    }

    [Step("Żądanie bez tokenu antiforgery zostaje odrzucone.")]
    public async Task RequestWithoutTokenIsRejected()
    {
        var client = await Pages.SignedInAsync();
        var record = await Records.RecordIdAsync(7);
        var withToken = await Pages.StarAsync(client, HttpMethod.Post, record, await TestUser.TokenAsync(client));
        var response = await Pages.StarAsync(client, HttpMethod.Post, record, token: null);

        var star = await withToken.Content.ReadAsStringAsync();
        Assert.True(withToken.StatusCode == HttpStatusCode.OK && star.Contains("data-favorite", StringComparison.Ordinal), $"Klik ☆ z tokenem nie zwraca gwiazdki (status {(int)withToken.StatusCode}) – najpierw zróbcie handler OnPostUlubione i `_Gwiazdka`.");
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest, $"Klik ☆ bez tokenu antiforgery dostaje status {(int)response.StatusCode} zamiast 400. Nie wyłączajcie walidacji antiforgery.");
    }
}
