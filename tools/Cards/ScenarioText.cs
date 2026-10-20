using System.Text.Json;

namespace Cards;

// Hand-written texts per scenario (tools/Cards/scenarios.json): everything that cannot be derived from the data.
// Search, ListFields and Stats are the variants of the decisions in prompty.md; variant A (first) is what mapping.json
// and the rescue branches use.
public sealed record ScenarioText(
    string App, string Idea, string Favorites, string StarTask, IReadOnlyList<ExtraTrap> Traps,
    IReadOnlyList<SearchOption> Search, IReadOnlyList<ListOption> ListFields, IReadOnlyList<StatOption> Stats);

public sealed record ExtraTrap(int Stage, string Trap, string Effect, string Tool);

// Match: "contains" (LIKE '%' + @q + '%') or "prefix" (LIKE @q + '%').
public sealed record SearchOption(string Field, string Label, string Match = "contains");

public sealed record ListOption(IReadOnlyList<string> Fields, string Label);

public sealed record StatOption(string Kind, string Field, string Label);

public sealed record Trap(int Stage, string Text, string Effect, string Tool);

public static class ScenarioTexts
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    public static IReadOnlyDictionary<string, ScenarioText> Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        return JsonSerializer.Deserialize<Dictionary<string, ScenarioText>>(File.ReadAllText(path), Options)
            ?? throw new InvalidDataException($"{path}: pusty plik.");
    }
}
