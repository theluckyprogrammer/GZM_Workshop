using System.Text.RegularExpressions;

namespace Fetch;

public sealed record PiiHit(string Column, string Kind, int Count, IReadOnlyList<string> Examples);

// Possible personal data. Hits go to findings.md for MANUAL review, they do not reject automatically.
public static class PiiScanner
{
    private const string Upper = "A-ZĄĆĘŁŃÓŚŹŻ";
    private const string Lower = "a-ząćęłńóśźż";

    private static readonly (string Kind, Regex Pattern, bool TextOnly)[] Rules =
    [
        ("PESEL", new Regex(@"\b\d{11}\b"), false),
        ("telefon komórkowy", new Regex(@"(?<![\d,.])(?:\+48[ -]?)?(?:45|5\d|6\d|7[2-9]|8[18])\d[ -]?\d{3}[ -]?\d{3}(?![\d,.])"), false),
        ("e-mail", new Regex(@"[\w.+-]+@[\w-]+(?:\.[\w-]+)+"), false),
        ("Imię Nazwisko", new Regex($@"\b[{Upper}][{Lower}]{{2,}} [{Upper}][{Lower}]{{2,}}(?:-[{Upper}][{Lower}]{{2,}})?\b"), true),
    ];

    public static IReadOnlyList<PiiHit> Scan(CsvTable table, IReadOnlyList<ColumnProfile> columns)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(columns);

        return columns
            .SelectMany((column, index) => Rules
                .Where(rule => !rule.TextOnly || column.Type == "text")
                .Select(rule => Hit(column.Name, rule.Kind, rule.Pattern, table.Column(index))))
            .Where(hit => hit.Count > 0)
            .ToList();
    }

    private static PiiHit Hit(string column, string kind, Regex pattern, IReadOnlyList<string> values)
    {
        var matches = values.SelectMany(v => pattern.Matches(v).Select(m => m.Value)).ToList();
        return new PiiHit(column, kind, matches.Count, matches.Distinct().Take(3).ToList());
    }
}
