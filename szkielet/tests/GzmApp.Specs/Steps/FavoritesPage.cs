using AngleSharp.Dom;
using GzmApp.Tests.Infrastructure;

namespace GzmApp.Specs.Steps;

// The "Moje ulubione" page of a signed-in user: <ul id="ulubione">, one <li> per record, the title in <span class="tytul">.
public static class FavoritesPage
{
    public const string Path = "/Ulubione";

    public static async Task<IReadOnlyList<IElement>> ItemsAsync(HttpClient client)
    {
        ArgumentNullException.ThrowIfNull(client);

        var response = await client.GetAsync(Path);
        Assert.True(response.IsSuccessStatusCode, $"Strona „Moje ulubione” po zalogowaniu odpowiada statusem {(int)response.StatusCode}.");
        var list = Html.Parse(await response.Content.ReadAsStringAsync()).QuerySelector("#ulubione");
        Assert.True(list is not null, "Na stronie „Moje ulubione” nie ma listy `<ul id=\"ulubione\">`.");
        return list!.QuerySelectorAll(":scope > li").ToList();
    }

    public static async Task<IReadOnlyList<string>> TitlesAsync(HttpClient client) =>
        (await ItemsAsync(client)).Select(li => DisplayForms.Normalize(li.QuerySelector(".tytul")?.TextContent ?? li.TextContent)).ToList();
}
