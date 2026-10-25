using System.Net;
using System.Text.RegularExpressions;
using GzmApp.Ai;
using GzmApp.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Spectree;

namespace GzmApp.Specs.Steps;

// Krok 4 (sNN-004-analiza-ai): the button "✨ Analizuj z AI", the records sent to the model and the card with the answer.
// The model is FakeModel (SpecApp), so the steps need no key and every answer is known. Decision 4 is rule 3.
// Only rule 9 asks the real model (one call, Sol6:Model from appsettings.json, key from appsettings.Local.json).
public sealed partial class AiSteps
{
    private const string AnalyzePath = "/?handler=Analizuj&q=";
    private const int Take = 20;

    [Step("Przycisk „✨ Analizuj z AI” na stronie głównej wysyła frazę z wyszukiwarki do analizy.")]
    public async Task ButtonSendsThePhrase()
    {
        var (_, html) = await Pages.GetAsync("/", htmx: false);
        var page = Html.Parse(html);
        var button = page.QuerySelector("[hx-get*='handler=Analizuj']");

        Assert.True(button is not null, "Na stronie głównej nie ma przycisku z `hx-get=\"?handler=Analizuj\"`.");
        Assert.True(button.GetAttribute("hx-target") == "#analiza", "Przycisk analizy nie ma `hx-target=\"#analiza\"`.");
        Assert.True(button.GetAttribute("hx-include")?.Contains("q", StringComparison.Ordinal) == true, "Przycisk analizy nie wysyła frazy: brak `hx-include=\"[name=q]\"`.");
        Assert.True(page.QuerySelector("#analiza") is not null, "Na stronie głównej nie ma miejsca na kartę: `<div id=\"analiza\">`.");
    }

    [Step("Do modelu trafiają rekordy z listy wyników dla wpisanej frazy, każdy z numerem `SourceRow`.")]
    public async Task ModelGetsTheListedRecords()
    {
        var rule = SearchRule.FromSpec();
        var phrase = await rule.PhraseAsync();
        var listed = await Records.FirstRowsAsync(Take, rule.Where, new { pattern = rule.Pattern(phrase) });
        var sent = await SentForAsync(phrase);

        var missing = listed.Where(row => !SentRow(sent, row)).ToList();
        Assert.True(missing.Count == 0, $"Do modelu nie trafiły rekordy z listy dla frazy „{phrase}”: Nr {string.Join(", Nr ", missing)}. Jedna linia na rekord: `Nr <SourceRow>: Pole=wartość; …`.");
        var other = await Records.FirstRowsAsync(1, $"NOT ({rule.Where}) OR {Records.Field(rule.Field)} IS NULL", new { pattern = rule.Pattern(phrase) });
        Assert.True(other.All(row => !SentRow(sent, row)), $"Do modelu trafił rekord Nr {other.FirstOrDefault()}, którego nie ma na liście dla frazy „{phrase}”. Wyślijcie wyniki `Search(q, 20)`, a nie całą tabelę.");
    }

    [Step("Model dostaje zadanie: {zadanie}")]
    public async Task ModelGetsTheTask(string task)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(task);

        var sent = DisplayForms.Normalize(await SentForAsync(string.Empty));

        Assert.True(sent.Contains(DisplayForms.Normalize(task), StringComparison.Ordinal), $"Do modelu nie trafiło zadanie z reguły 3: „{task}”. Wyślijcie je w wiadomości razem z rekordami.");
    }

    [Step("Odpowiedź modelu pojawia się w karcie `#analiza` bez przeładowania strony.")]
    public async Task AnswerAppearsInTheCard()
    {
        const string answer = "Analiza testowa 4711: najwięcej rekordów ma Nr 7.";
        var html = await CardAsync(() => FakeModel.Reply(answer));

        Assert.True(DisplayForms.Normalize(Html.Parse(html).Body?.TextContent ?? html).Contains(answer, StringComparison.Ordinal), $"Karta nie pokazuje odpowiedzi modelu („{answer}”). Wstawcie `response.Text` do `_Analiza.cshtml`.");
    }

    [Step("Karta mówi, że tekst wygenerowała sztuczna inteligencja, i podaje nazwę modelu.")]
    public async Task CardSaysItIsAi()
    {
        var text = DisplayForms.Normalize(Html.Parse(await CardAsync()).Body?.TextContent ?? string.Empty);
        var model = await ModelNameAsync();

        Assert.True(text.Contains("sztuczna inteligencja", StringComparison.OrdinalIgnoreCase), $"Karta nie mówi, że tekst wygenerowała „sztuczna inteligencja”. Tekst karty: „{text}”.");
        Assert.True(text.Contains(model, StringComparison.Ordinal), $"Karta nie podaje nazwy modelu „{model}” (`Model` z `IOptions<Sol6Options>`).");
    }

    [Step("Jedno kliknięcie to jedno zapytanie do modelu.")]
    public async Task OneClickOneCall()
    {
        await CardAsync();

        Assert.True(FakeModel.Instance.Calls == 1, $"Jedno kliknięcie „✨ Analizuj z AI” to {FakeModel.Instance.Calls} zapytań do modelu zamiast 1.");
    }

    [Step("Odpowiedź modelu jest pokazana jako tekst, a nie jako kod HTML.")]
    public async Task AnswerIsText()
    {
        var html = await CardAsync(() => FakeModel.Reply("<b>pogrubione</b> i <script>alert(1)</script>"));

        Assert.False(html.Contains("<b>pogrubione</b>", StringComparison.Ordinal) || html.Contains("<script>alert(1)", StringComparison.Ordinal),
            "Odpowiedź modelu trafiła na stronę jako kod HTML. Pokażcie ją przez zwykłe `@Model.Text`, bez `Html.Raw`.");
        Assert.True(html.Contains("&lt;b&gt;pogrubione", StringComparison.Ordinal), "Karta nie pokazuje odpowiedzi modelu jako tekstu (`&lt;b&gt;…`).");
    }

    [Step("Gdy model nie odpowiada, karta pokazuje krótki komunikat zamiast strony błędu.")]
    public async Task ModelErrorGivesAMessage()
    {
        var html = await CardAsync(() => throw new Sol6Exception(Sol6Exception.NoAnswer), expectOk: false);

        Assert.True(DisplayForms.Normalize(html).Contains(Sol6Exception.NoAnswer, StringComparison.Ordinal), $"Gdy model nie odpowiada, karta nie pokazuje komunikatu „{Sol6Exception.NoAnswer}”. Złapcie `Sol6Exception` i pokażcie jego `Message`.");
    }

    [Step("Prawdziwy model AI odpowiada na analizę danych z Waszej bazy.")]
    public async Task RealModelAnswers()
    {
        var app = await Records.TableReadyAsync();
        var options = app.Services.GetRequiredService<IOptions<Sol6Options>>().Value;
        Assert.True(options.IsConfigured, "Brak klucza do modelu AI: plik src/GzmApp/appsettings.Local.json (Sol6:Endpoint i Sol6:ApiKey) – poproś prowadzącego. Ta reguła jako jedyna pyta prawdziwy model.");

        FakeModel.Instance.UseRealModel(Sol6Setup.CreateClient(options, app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("AI")));
        var (_, html) = await Pages.GetAsync(AnalyzePath, htmx: true);
        var (calls, answer) = (FakeModel.Instance.Calls, FakeModel.Instance.RealAnswer);
        FakeModel.Instance.Reset();
        var card = DisplayForms.Normalize(Html.Parse(html).Body?.TextContent ?? html);

        Assert.True(calls == 1, $"Analiza nie zapytała modelu (zapytań: {calls}). Handler `OnGetAnalizuj` ma wołać `IChatClient` z DI.");
        Assert.True(!string.IsNullOrWhiteSpace(answer), $"Model {options.Model} nie odpowiedział. Karta pokazuje: „{card}”.");
        Assert.True(card.Contains(Start(answer), StringComparison.Ordinal), $"Model {options.Model} odpowiedział, ale karta nie pokazuje jego odpowiedzi („{Start(answer)}…”).");
    }

    // The fragment of "✨ Analizuj z AI" for an empty phrase, with the given answer of the model.
    private static async Task<string> CardAsync(Func<Microsoft.Extensions.AI.ChatResponse>? answer = null, bool expectOk = true)
    {
        await Records.TableReadyAsync();
        FakeModel.Instance.Reset(answer);
        var (status, html) = await Pages.GetAsync(AnalyzePath, htmx: true);

        Assert.False(html.Contains("<html", StringComparison.OrdinalIgnoreCase), $"Analiza odsyła całą stronę (status {(int)status}) zamiast fragmentu `_Analiza`. Zróbcie handler `OnGetAnalizuj` i `Partial(\"_Analiza\", …)`.");
        Assert.True(!expectOk || status == HttpStatusCode.OK, $"Analiza odpowiada statusem {(int)status} zamiast 200.");
        return html;
    }

    // What the app sent to the model for the phrase (one click).
    private static async Task<string> SentForAsync(string phrase)
    {
        await Records.TableReadyAsync();
        FakeModel.Instance.Reset();
        await Pages.GetAsync(AnalyzePath + Uri.EscapeDataString(phrase), htmx: true);

        Assert.True(FakeModel.Instance.Calls > 0, "Model nie dostał żadnego zapytania. Handler `OnGetAnalizuj` ma wołać `IChatClient` z DI (`GetResponseAsync`) – nie twórzcie własnego klienta.");
        return FakeModel.Instance.SentText;
    }

    private static async Task<string> ModelNameAsync() =>
        (await ActiveScenario.AppAsync()).Services.GetRequiredService<IOptions<Sol6Options>>().Value.Model;

    // The first words of the model's answer, as the card shows them.
    private static string Start(string text)
    {
        var normalized = DisplayForms.Normalize(text);
        return normalized[..Math.Min(30, normalized.Length)];
    }

    private static bool SentRow(string sent, int row) => Regex.IsMatch(sent, $@"\bNr\s*{row}\b");
}
