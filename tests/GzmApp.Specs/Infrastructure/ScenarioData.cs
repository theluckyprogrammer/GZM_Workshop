using System.Text.Json;
using System.Text.RegularExpressions;

namespace GzmApp.Specs.Infrastructure;

// One scenario: mapping.json (as JSON, the test must not use app types from later stages) and checkpoints.md.
public sealed record Scenario(string Code, JsonElement Mapping, IReadOnlyDictionary<string, Checkpoint> Checkpoints)
{
    public string Value(string id) =>
        Checkpoints.TryGetValue(id, out var checkpoint) ? checkpoint.Value : throw new KeyNotFoundException($"{Code}: brak {id} w checkpoints.md.");

    public string Text(string property) => Mapping.GetProperty(property).GetString() ?? string.Empty;

    // Values with this ID prefix in the order of their number (E2.result.1, E2.result.2, …).
    public IReadOnlyList<Checkpoint> Numbered(string prefix) =>
        Checkpoints.Values.Where(c => c.Id.StartsWith(prefix + ".", StringComparison.Ordinal))
            .OrderBy(c => int.Parse(c.Id[(prefix.Length + 1)..])).ToList();

    public override string ToString() => Code;
}

public sealed record Checkpoint(string Id, string Label, string Value);

// TheoryData for all scenarios: scenarios/*/mapping.json + checkpoints.md. No code per scenario.
public static partial class ScenarioData
{
    public static IReadOnlyList<Scenario> All { get; } = Load();

    public static TheoryData<string> Codes => new(All.Select(s => s.Code));

    public static Scenario Get(string code) =>
        All.FirstOrDefault(s => s.Code == code) ?? throw new KeyNotFoundException($"Nie ma scenariusza {code}.");

    private static List<Scenario> Load() =>
        Directory.GetDirectories(RepoPaths.Scenarios, "s*").Order(StringComparer.Ordinal)
            .Where(d => File.Exists(Path.Combine(d, "mapping.json")))
            .Select(d => new Scenario(
                Path.GetFileName(d),
                JsonDocument.Parse(File.ReadAllText(Path.Combine(d, "mapping.json"))).RootElement.Clone(),
                ParseCheckpoints(Path.Combine(d, "checkpoints.md"))))
            .ToList();

    // checkpoints.md rows: | ID | label | value |; "\|" and "\n" stay escaped as in the file.
    public static IReadOnlyDictionary<string, Checkpoint> ParseCheckpoints(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        return File.ReadLines(path).Select(l => Row().Match(l)).Where(m => m.Success)
            .Select(m => new Checkpoint(m.Groups["id"].Value, m.Groups["label"].Value, m.Groups["value"].Value))
            .ToDictionary(c => c.Id);
    }

    // The checkpoint value as plain text: "\n" → new line, "\|" → "|".
    public static string Unescape(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value.Replace("\\n", "\n").Replace("\\|", "|");
    }

    [GeneratedRegex(@"^\| (?<id>E\d\.\S+) \| (?<label>.*?) \| (?<value>(?:[^|\\]|\\.)*) \|$")]
    private static partial Regex Row();
}
