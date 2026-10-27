using System.Text.RegularExpressions;
using Expect;

namespace Cards;

// "check": the generated files of a scenario are complete, student files (and the specification) carry no expected values,
// every prompt ends with the required sentence and points to its spec file, and every **value** (ID) in prowadzacy.md
// equals checkpoints.md.
public static partial class Checker
{
    private const string Ending = "Nie dodawaj pakietów.";
    private static readonly string[] Steps = ["## Krok 0", "## Krok 1", "## Krok 2", "## Krok 3", "## Krok 4"];

    public static IReadOnlyList<string> Check(string scenarioDirectory, IReadOnlyList<Checkpoint> checkpoints)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scenarioDirectory);
        ArgumentNullException.ThrowIfNull(checkpoints);

        var scenario = Path.GetFileName(scenarioDirectory);
        var specs = Generator.SpecTemplates.Concat(Generator.ExtraSpecTemplates).Select(t => Generator.SpecFile(scenario, t));
        var files = Generator.Outputs.Concat(specs).ToList();
        var missing = files.Where(f => !File.Exists(Path.Combine(scenarioDirectory, f))).Select(f => $"brak pliku {f}");
        var present = files.Where(f => File.Exists(Path.Combine(scenarioDirectory, f)));
        return missing.Concat(present.SelectMany(f => CheckFile(f, File.ReadAllText(Path.Combine(scenarioDirectory, f)), checkpoints))).ToList();
    }

    private static IEnumerable<string> CheckFile(string name, string text, IReadOnlyList<Checkpoint> checkpoints)
    {
        var references = Reference().Matches(text).ToList();
        var common = new[]
        {
            text.Contains("{{") ? $"{name}: niewypełnione pole {{{{…}}}}" : null,
            EmptyField().IsMatch(text) ? $"{name}: puste pole" : null,
        };
        var specific = name switch
        {
            "prompty.md" => PromptRules(text),
            "prowadzacy.md" => references.Count == 0 ? ["prowadzacy.md: brak wartości z checkpoints.md"] : [],
            _ => [],
        };
        var values = Generator.StudentOutputs.Contains(name) || name.StartsWith("spec", StringComparison.Ordinal)
            ? references.Select(m => $"{name}: wartość oczekiwana w materiale ucznia: {m.Value}")
            : references.Where(m => !Matches(m, checkpoints)).Select(m => $"{name}: {m.Value} ≠ checkpoints.md");
        return common.OfType<string>().Concat(specific).Concat(values);
    }

    private static IEnumerable<string> PromptRules(string text)
    {
        var prompts = PromptBlock().Matches(text).Select(m => m.Groups["body"].Value.TrimEnd()).ToList();
        if (prompts.Count < Steps.Length)
            yield return $"prompty.md: {prompts.Count} bloków promptu, oczekiwano co najmniej {Steps.Length}";
        foreach (var index in prompts.Select((p, i) => (p, i + 1)).Where(x => !EndsCorrectly(x.p)).Select(x => x.Item2))
            yield return $"prompty.md: prompt nr {index} nie kończy się zdaniem „Nie zmieniaj plików poza: … {Ending}”";
        foreach (var missing in Steps.Where(s => !text.Contains(s, StringComparison.Ordinal)))
            yield return $"prompty.md: brak „{missing}”";
        // Kroki 0–4 (pierwszych 5 bloków promptu) wskazują swoje pliki specyfikacji: 000-dane, 001-import, 002-szukaj, 003-strona, 004-ai.
        foreach (var index in prompts.Take(Generator.SpecTemplates.Length).Select((p, i) => (p, i)).Where(x => !x.p.Contains($"-{Generator.SpecTemplates[x.i]}`", StringComparison.Ordinal)).Select(x => x.i + 1))
            yield return $"prompty.md: prompt kroku {index} nie wskazuje pliku specyfikacji {Generator.SpecTemplates[index - 1]}";
        foreach (var missing in DecisionAsks().Where(m => !text.Contains(m, StringComparison.Ordinal)))
            yield return $"prompty.md: brak „{missing}”";
        if (!text.Contains("LOG-DECYZJI.md", StringComparison.Ordinal))
            yield return "prompty.md: brak odwołania do LOG-DECYZJI.md";
    }

    // Every decision asks Kilo for proposals (no printed A/B/C variants).
    private static IEnumerable<string> DecisionAsks() =>
        ["Zaproponuj nam 3 różne warianty decyzji 1", "Zaproponuj nam 3 różne warianty decyzji 2", "Zaproponuj nam 3 różne warianty decyzji 3", "Zaproponuj nam 3 różne warianty decyzji 4", "Zaproponuj nam 3 warianty decyzji 5"];

    private static bool EndsCorrectly(string prompt) =>
        prompt.EndsWith(Ending, StringComparison.Ordinal) && prompt.Contains("Nie zmieniaj plików poza: ", StringComparison.Ordinal);

    private static bool Matches(Match match, IReadOnlyList<Checkpoint> checkpoints) =>
        checkpoints.Any(c => c.Id == match.Groups["id"].Value && c.Value == match.Groups["value"].Value);

    // **value** (E1.rows)
    [GeneratedRegex(@"\*\*(?<value>(?:(?!\*\*).)+)\*\* \((?<id>E\d\.[\w.]+)\)")]
    private static partial Regex Reference();

    // A prompt to paste: ~~~~text … ~~~~ (four tildes, so it can hold ``` and ~~~ blocks).
    [GeneratedRegex(@"^~~~~text\n(?<body>.*?)\n~~~~$", RegexOptions.Multiline | RegexOptions.Singleline)]
    private static partial Regex PromptBlock();

    // Empty bold, empty code span or a list item / table cell with nothing in it.
    [GeneratedRegex(@"\*\*\*\*|(?<!`)``(?!`)|^[ \t]*-[ \t]*$|\|[ \t]+\|", RegexOptions.Multiline)]
    private static partial Regex EmptyField();
}
