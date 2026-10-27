using System.Text.RegularExpressions;
using GzmApp.Specs.Infrastructure;
using Spectree;

namespace GzmApp.Specs;

// Every sentence of the specification has exactly one step: the rules of all 15 scenarios
// (scenarios/*/spec/**/*.md, also the extra step in spec/dodatkowe/). No app and no database.
// The decisions have no printed variants: the pair asks Kilo, which proposes sentences following
// the patterns from the "decyzja" pauses of karta.md – every pattern must have a step, too,
// so whatever Kilo proposes (within the patterns) becomes a working test.
public partial class SpecCoverageTests
{
    [Fact(DisplayName = "Każde zdanie specyfikacji (15 scenariuszy) ma jeden krok")]
    [Trait("Capability", "kontrola")]
    public void Every_sentence_of_every_scenario_has_one_step()
    {
        var sentences = Directory.GetDirectories(RepoPaths.Scenarios, "s*").SelectMany(Sentences).Distinct().ToList();
        var missing = sentences.Where(s => StepRegistry.For(typeof(SpecCoverageTests).Assembly).Count(b => b.Matches(s)) != 1).ToList();

        Assert.True(sentences.Count > 100, $"Za mało zdań ({sentences.Count}) – sprawdź wzorce.");
        Assert.True(missing.Count == 0, "Zdania bez dokładnie jednego kroku:\n" + string.Join("\n", missing));
    }

    [Fact(DisplayName = "Każdy wzorzec zdania decyzji (propozycje Kilo z karta.md) ma krok")]
    [Trait("Capability", "kontrola")]
    public void Every_decision_sentence_pattern_has_a_step()
    {
        var steps = StepRegistry.For(typeof(SpecCoverageTests).Assembly);
        var missing = new List<string>();
        foreach (var directory in Directory.GetDirectories(RepoPaths.Scenarios, "s*").Order(StringComparer.Ordinal))
        {
            var scenario = ScenarioData.Get(Path.GetFileName(directory));
            foreach (var pattern in Patterns(Path.Combine(directory, "karta.md")))
            {
                var sentence = Substitute(pattern, scenario);
                if (steps.Count(b => b.Matches(sentence)) == 0)
                    missing.Add($"{scenario.Code}: wzorzec bez kroku: {pattern}");
            }
        }

        Assert.True(missing.Count == 0, "Wzorce zdań decyzji bez kroku (Kilo mogłoby zaproponować zdanie bez testu):\n" + string.Join("\n", missing));
    }

    private static IEnumerable<string> Sentences(string scenario)
    {
        var rules = Directory.GetFiles(Path.Combine(scenario, "spec"), "*.md", SearchOption.AllDirectories).SelectMany(File.ReadLines).Select(l => Rule().Match(l)).Where(m => m.Success);
        return rules.Select(m => m.Groups[1].Value.Trim());
    }

    // Sentence patterns live in the "ask Kilo" blocks (~~~text) of the decision pauses: lines that end
    // with a period and either have a {pole}-style placeholder or are the three fixed favorites sentences.
    private static IEnumerable<string> Patterns(string karta) =>
        DecisionAsk().Matches(File.ReadAllText(karta))
            .SelectMany(m => m.Groups["body"].Value.Split('\n'))
            .Select(l => l.Trim())
            .Where(l => l.EndsWith('.') && (l.Contains('{') || l.StartsWith("Strona „", StringComparison.Ordinal)));

    // A concrete sentence a pair could get from Kilo: placeholders replaced with the scenario's fields.
    private static string Substitute(string pattern, Scenario scenario)
    {
        var title = scenario.Text("titleField");
        return pattern
            .Replace("{Pole}", title)
            .Replace("{kolumna}", CsvOf(scenario, title))
            .Replace("{pola}", string.Join(", ", scenario.Mapping.GetProperty("listFields").EnumerateArray().Select(f => $"`{f.GetString()}`")))
            .Replace("{zadanie}", "Znajdź w tych rekordach 3 ciekawostki i krótko je opisz.");
    }

    private static string CsvOf(Scenario scenario, string field) =>
        scenario.Mapping.GetProperty("columns").EnumerateArray()
            .First(c => c.TryGetProperty("field", out var f) && f.GetString() == field).GetProperty("csv").GetString() ?? string.Empty;

    [GeneratedRegex(@"^\d+\.\s+(.+)$")]
    private static partial Regex Rule();

    // The paste-to-Kilo block of a decision pause: "### ⏸ Pauza – decyzja N: …" and its ~~~text block.
    [GeneratedRegex(@"### ⏸ Pauza – decyzja \d.*?\n+~~~text\n(?<body>.*?)\n~~~", RegexOptions.Singleline)]
    private static partial Regex DecisionAsk();
}
