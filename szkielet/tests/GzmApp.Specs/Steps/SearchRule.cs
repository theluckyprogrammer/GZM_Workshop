using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using GzmApp.Data;
using GzmApp.Specs.Infrastructure;

namespace GzmApp.Specs.Steps;

// What the search of the pair does: rule 1 of scenarios/sNN/spec/sNN-002-szukaj-rekordow.md (decision 1) gives the
// field and the kind of match. The phrase for the steps: E2.phrase of checkpoints.md when it finds something in that
// field, otherwise a word taken from the data, so the search always has results to compare.
public sealed partial record SearchRule(string Field, bool Prefix)
{
    public static SearchRule FromSpec()
    {
        var path = Path.Combine(RepoPaths.Scenarios, ActiveScenario.Code, "spec", $"{ActiveScenario.Code}-002-szukaj-rekordow.md");
        var line = File.ReadLines(path).FirstOrDefault(l => RuleOne().IsMatch(l))
            ?? throw new InvalidOperationException($"{path}: brak reguły 1 z polem wyszukiwania (`Pole`).");
        return new SearchRule(RuleOne().Match(line).Groups["field"].Value, line.Contains("zaczyna się od frazy", StringComparison.Ordinal));
    }

    // The pattern is plain(): without case and Polish diacritics, exactly like the SQL function plain().
    public string Pattern(string phrase) => Prefix ? PlainText.Of(phrase) + "%" : "%" + PlainText.Of(phrase) + "%";

    public string Where => $"plain(CAST({Records.Field(Field)} AS TEXT)) LIKE @pattern";

    public async Task<string> PhraseAsync()
    {
        var checkpoint = ActiveScenario.Data.Value("E2.phrase");
        if (await Records.CountAsync(Where, new { pattern = Pattern(checkpoint) }) > 0)
            return checkpoint;

        var rows = await Records.RowsAsync($"SELECT CAST({Records.Field(Field)} AS TEXT) FROM {Records.Table} WHERE LENGTH(CAST({Records.Field(Field)} AS TEXT)) >= 3 ORDER BY [SourceRow] LIMIT 1");
        Assert.True(rows.Count == 1, $"Pole `{Field}` jest puste we wszystkich rekordach – nie ma czego szukać.");
        var value = (string)rows[0][0]!;
        var word = Words().Matches(value).Select(m => m.Value).OrderByDescending(w => w.Length).FirstOrDefault() ?? value;
        return (Prefix ? value[..Math.Min(3, value.Length)] : word).ToLower(CultureInfo.GetCultureInfo("pl-PL"));
    }

    public static string WithoutPolishLetters(string phrase) =>
        string.Concat(phrase.Replace('ł', 'l').Replace('Ł', 'L').Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark));

    [GeneratedRegex(@"^1\.\s.*?pol[ae] `(?<field>\w+)`")]
    private static partial Regex RuleOne();

    [GeneratedRegex(@"\p{L}{3,}")]
    private static partial Regex Words();
}
