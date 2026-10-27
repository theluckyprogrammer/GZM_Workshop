using System.Text.RegularExpressions;
using GzmApp.Tests.Infrastructure;
using Spectree;

namespace GzmApp.Specs;

// Every sentence of the specification has exactly one step: the rules of all 15 scenarios (scenarios/*/spec/**/*.md,
// also the extra step in spec/dodatkowe/)
// and every variant of the decisions in prompty.md, also those not chosen. No app and no database.
public partial class SpecCoverageTests
{
    [Fact(DisplayName = "Każde zdanie specyfikacji (15 scenariuszy, wszystkie warianty decyzji) ma jeden krok")]
    [Trait("Capability", "kontrola")]
    public void Every_sentence_of_every_scenario_has_one_step()
    {
        var sentences = Directory.GetDirectories(RepoPaths.Scenarios, "s*").SelectMany(Sentences).Distinct().ToList();
        var missing = sentences.Where(s => StepRegistry.For(typeof(SpecCoverageTests).Assembly).Count(b => b.Matches(s)) != 1).ToList();

        Assert.True(sentences.Count > 100, $"Za mało zdań ({sentences.Count}) – sprawdź wzorce.");
        Assert.True(missing.Count == 0, "Zdania bez dokładnie jednego kroku:\n" + string.Join("\n", missing));
    }

    private static IEnumerable<string> Sentences(string scenario)
    {
        var rules = Directory.GetFiles(Path.Combine(scenario, "spec"), "*.md", SearchOption.AllDirectories).SelectMany(File.ReadLines).Select(l => Rule().Match(l)).Where(m => m.Success);
        var variants = Variant().Matches(File.ReadAllText(Path.Combine(scenario, "prompty.md")));
        return rules.Select(m => m.Groups[1].Value.Trim()).Concat(variants.Select(m => m.Groups[1].Value.Trim()));
    }

    [GeneratedRegex(@"^\d+\.\s+(.+)$")]
    private static partial Regex Rule();

    // A decision variant in prompty.md: "**A – label**", a blank line, then the sentence in a ~~~text block.
    [GeneratedRegex(@"^\*\*[A-Z] – .+\*\*\n\n~~~text\n(.+)\n~~~$", RegexOptions.Multiline)]
    private static partial Regex Variant();
}
