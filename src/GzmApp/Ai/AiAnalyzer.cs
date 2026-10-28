using System.Globalization;
using GzmApp.Data;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

namespace GzmApp.Ai;

public sealed record AiAnalysis(string Text, string Model, int RecordCount, string Prompt);

public interface IAiAnalyzer
{
    Task<AiAnalysis> AnalyzeAsync(string? q, CancellationToken ct = default);
}

// Sends the records from the search results (decision 1 field, the same list as on the page) to the AI model.
// The task for the model comes from rule 3 of the specification (decision 4).
public sealed class AiAnalyzer(IChatClient chat, IOptions<Sol6Options> options, IRecordRepository repository) : IAiAnalyzer
{
    private const string SystemPrompt =
        "Jesteś analitykiem danych. Odpowiadaj po polsku, krótko (najwyżej 8 punktów zaczynających się od „- ”), " +
        "tylko na podstawie przekazanych rekordów. Nie wymyślaj faktów. Podawaj numery rekordów (Nr). " +
        "Pisz zwykłym tekstem, bez Markdown (bez ** i #).";

    private const string Task =
        "Wskaż dni i gminy, w których dostępnych rowerów było mniej niż wymaganych, i napisz, co z tego wynika.";

    public async Task<AiAnalysis> AnalyzeAsync(string? q, CancellationToken ct = default)
    {
        var records = await repository.SearchAsync(q, 20, ct);
        var prompt = Task + "\n\nRekordy:\n" + string.Join("\n", records.Items.Select(Line));
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, SystemPrompt),
            new(ChatRole.User, prompt),
        };
        var response = await chat.GetResponseAsync(messages, cancellationToken: ct);
        return new AiAnalysis(response.Text ?? string.Empty, options.Value.Model, records.Items.Count, prompt);
    }

    // One line per record: "Nr 7: dzień=2026-10-04; gmina=Chorzów; dostępne rowery=400; …".
    private static string Line(RecordRow r) =>
        $"Nr {r.SourceRow}: dzień={r.Day}; gmina={r.Commune}; dostępne rowery={r.AvailableBikes}; " +
        $"wymagane rowery={r.RequiredBikes}; przekroczono={(r.Exceeded == 1 ? "tak" : "nie")}";
}
