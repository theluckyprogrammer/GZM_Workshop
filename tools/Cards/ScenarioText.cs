using System.Text.Json;

namespace Cards;

// Hand-written texts per scenario (tools/Cards/scenarios.json): everything that cannot be derived from the data.
public sealed record Trap(int Stage, string Text, string Effect, string Tool);

public sealed record ScenarioText(string App, string Idea, string Favorites, string StarTask, IReadOnlyList<ExtraTrap> Traps);

public sealed record ExtraTrap(int Stage, string Trap, string Effect, string Tool);

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
