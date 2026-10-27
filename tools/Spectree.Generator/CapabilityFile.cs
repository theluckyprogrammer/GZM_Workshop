using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Spectree.Generator;

public sealed class Rule
{
    public Rule(int number, string sentence)
    {
        Number = number;
        Sentence = sentence ?? throw new ArgumentNullException(nameof(sentence));
    }

    public int Number { get; }

    public string Sentence { get; }
}

// A capability file (e.g. scenarios/sNN/spec/sNN-NNN-*.md): the slug from the front matter ("capability: <slug>") and the
// numbered rules of the "## Reguły" (or "## Rules") section. A rule may continue on the next lines; a blank line,
// the next number or the next heading ends it. Lines starting with "<!--" are comments.
public sealed class CapabilityFile
{
    private static readonly Regex Slug = new(@"^capability:\s*(?<slug>\S+)\s*$");
    private static readonly Regex Item = new(@"^(?<number>\d+)\.\s+(?<text>.*)$");
    private static readonly Regex Heading = new(@"^##\s+(?<name>Reguły|Rules)\s*$", RegexOptions.IgnoreCase);

    private CapabilityFile(string? capability, string ruleLabel, IReadOnlyList<Rule> rules)
    {
        Capability = capability;
        RuleLabel = ruleLabel;
        Rules = rules;
    }

    public string? Capability { get; }

    // "Reguła" for "## Reguły", "Rule" for "## Rules" – the first word of every test name.
    public string RuleLabel { get; }

    public IReadOnlyList<Rule> Rules { get; }

    // Null when the pair can run the tests; otherwise the message of the first problem (missing slug,
    // no rules, repeated rule number) – the caller prefixes the file name.
    public string? Problem()
    {
        if (Capability is null)
            return "brak nagłówka '---' z linią 'capability: <slug>'";
        if (Rules.Count == 0)
            return "brak sekcji '## Reguły' z ponumerowanymi zdaniami (1. …)";
        var duplicate = Rules.GroupBy(rule => rule.Number).FirstOrDefault(group => group.Count() > 1);
        return duplicate is null ? null : $"numer reguły {duplicate.Key} występuje więcej niż raz – numerów nie zmienia się ani nie powtarza";
    }

    public static CapabilityFile Parse(string text)
    {
        if (text is null)
            throw new ArgumentNullException(nameof(text));

        var lines = text.Replace("\r\n", "\n").Split('\n').Select(l => l.Trim()).ToList();
        var capability = FrontMatter(lines).Select(l => Slug.Match(l)).Where(m => m.Success).Select(m => m.Groups["slug"].Value).FirstOrDefault();
        var start = lines.FindIndex(l => Heading.IsMatch(l));
        if (start < 0)
            return new CapabilityFile(capability, "Rule", new List<Rule>());

        var label = lines[start].IndexOf("Reguły", StringComparison.OrdinalIgnoreCase) >= 0 ? "Reguła" : "Rule";
        var section = lines.Skip(start + 1).TakeWhile(l => !l.StartsWith("#", StringComparison.Ordinal)).Where(l => !l.StartsWith("<!--", StringComparison.Ordinal));
        return new CapabilityFile(capability, label, ReadRules(section));
    }

    private static IEnumerable<string> FrontMatter(List<string> lines) =>
        lines.Count > 0 && lines[0] == "---" ? lines.Skip(1).TakeWhile(l => l != "---") : Enumerable.Empty<string>();

    private static List<Rule> ReadRules(IEnumerable<string> section)
    {
        var rules = new List<(int Number, List<string> Parts)>();
        var open = false;
        foreach (var line in section)
            open = Append(rules, line, open);
        return rules.Select(r => new Rule(r.Number, string.Join(" ", r.Parts))).Where(r => r.Sentence.Length > 0).ToList();
    }

    // Returns whether the current rule is still open after this line.
    private static bool Append(List<(int Number, List<string> Parts)> rules, string line, bool open)
    {
        var item = Item.Match(line);
        if (item.Success)
            rules.Add((int.Parse(item.Groups["number"].Value), new List<string> { item.Groups["text"].Value.Trim() }));
        else if (open && line.Length > 0)
            rules[rules.Count - 1].Parts.Add(line);
        return item.Success || (open && line.Length > 0);
    }
}
