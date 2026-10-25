using System.Net;
using System.Text.RegularExpressions;
using GzmApp.Tests.Infrastructure;
using Spectree;

namespace GzmApp.Specs.Steps;

// Krok 4, decision 4: what the "Moje ulubione" page shows. Every step stars two records (SourceRow 7 and the next one).
public sealed partial class FavoritesPageSteps
{
    [Step("Strona „Moje ulubione” pokazuje tytuły ulubionych rekordów.")]
    public async Task ShowsTitles()
    {
        var (client, records) = await StarredAsync(pause: false);
        var titles = await FavoritesPage.TitlesAsync(client);

        Assert.True(titles.Count == records.Count, $"„Moje ulubione” pokazuje {titles.Count} rekordów, a w ulubionych są {records.Count}.");
        var missing = records.Where(r => r.Title is not null && !titles.Any(t => t.Contains(DisplayForms.Normalize(r.Title), StringComparison.Ordinal))).ToList();
        Assert.True(missing.Count == 0, $"Na stronie „Moje ulubione” brakuje tytułów: {string.Join(", ", missing.Select(r => $"„{r.Title}”"))}.");
    }

    [Step("Strona „Moje ulubione” pokazuje tytuł i datę dodania każdego rekordu, od najnowszych.")]
    public async Task ShowsTitlesAndDatesNewestFirst()
    {
        var (client, records) = await StarredAsync(pause: true);
        var items = (await FavoritesPage.ItemsAsync(client)).Select(li => DisplayForms.Normalize(li.TextContent)).ToList();

        Assert.True(items.Count == records.Count, $"„Moje ulubione” pokazuje {items.Count} rekordów, a w ulubionych są {records.Count}.");
        var undated = items.FirstOrDefault(i => !Date().IsMatch(i));
        Assert.True(undated is null, $"Wpis „{undated}” nie ma daty dodania w formacie dd.MM.yyyy HH:mm.");
        var newest = records[^1].Title;
        Assert.True(newest is null || items[0].Contains(DisplayForms.Normalize(newest), StringComparison.Ordinal), $"Pierwszy wpis to „{items[0]}”, a ma być ostatnio dodany „{newest}” (od najnowszych).");
    }

    [Step("Strona „Moje ulubione” pokazuje tytuł i przycisk „Usuń”, który usuwa rekord z listy bez przeładowania strony.")]
    public async Task RemoveButtonDeletesWithoutReload()
    {
        var (client, records) = await StarredAsync(pause: false);
        var button = (await FavoritesPage.ItemsAsync(client)).Select(li => li.QuerySelector("[hx-delete]")).FirstOrDefault();
        Assert.True(button is not null, "Przy rekordzie na stronie „Moje ulubione” nie ma przycisku „Usuń” z `hx-delete`.");

        var request = new HttpRequestMessage(HttpMethod.Delete, new Uri(new Uri("http://localhost" + FavoritesPage.Path), button!.GetAttribute("hx-delete")));
        request.Headers.Add("HX-Request", "true");
        request.Headers.Add("RequestVerificationToken", await TestUser.TokenAsync(client));
        var response = await client.SendAsync(request);

        Assert.True(response.StatusCode == HttpStatusCode.OK, $"Przycisk „Usuń” (`hx-delete=\"{button.GetAttribute("hx-delete")}\"`) dostaje status {(int)response.StatusCode} zamiast 200.");
        Assert.True((await FavoritesPage.TitlesAsync(client)).Count == records.Count - 1, "Po kliknięciu „Usuń” rekord nadal jest w ulubionych.");
    }

    // pause: CreatedAt has whole seconds, so "newest first" needs a second between the two stars.
    private static async Task<(HttpClient Client, IReadOnlyList<(int RecordId, string? Title)> Records)> StarredAsync(bool pause)
    {
        var client = await Pages.SignedInAsync();
        var token = await TestUser.TokenAsync(client);
        var records = await Records.FavoriteCandidatesAsync();
        await Pages.StarAsync(client, HttpMethod.Post, records[0].RecordId, token);
        if (pause)
            await Task.Delay(TimeSpan.FromSeconds(1.1));
        await Pages.StarAsync(client, HttpMethod.Post, records[1].RecordId, token);
        return (client, records);
    }

    [GeneratedRegex(@"\d{2}\.\d{2}\.\d{4} \d{2}:\d{2}")]
    private static partial Regex Date();
}
