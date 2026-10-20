using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace Fetch;

// HeaderLine: line in the file with the column names (some files start with a title block).
// RawHeader: everything up to and including the header line, exactly as in the file.
public sealed record CsvTable(
    char Separator, int HeaderLine, IReadOnlyList<string> Headers, string RawHeader,
    IReadOnlyList<string[]> Rows, IReadOnlyList<string> RawRows)
{
    public int InconsistentRows => Rows.Count(r => r.Length != Headers.Count);

    public IReadOnlyList<string> Column(int index) =>
        Rows.Select(r => index < r.Length ? r[index] : string.Empty).ToList();
}

public static class CsvLoader
{
    private static readonly char[] Candidates = [';', ',', '\t', '|'];

    public static CsvTable Load(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var separator = DetectSeparator(FirstLine(text));
        var records = ReadAll(text, separator);
        if (records.Count == 0)
            return new CsvTable(separator, 0, [], string.Empty, [], []);

        var header = HeaderIndex(records);
        var data = records.Skip(header + 1).ToList();
        return new CsvTable(separator, records[header].Line, records[header].Fields,
            string.Concat(records.Take(header + 1).Select(r => r.Raw)),
            data.Select(r => r.Fields).ToList(), data.Select(r => r.Raw).ToList());
    }

    // Header = first of the opening records that is (almost) as full as the fullest one.
    private static int HeaderIndex(List<(string[] Fields, string Raw, int Line)> records)
    {
        var filled = records.Take(20).Select(r => r.Fields.Count(f => !string.IsNullOrWhiteSpace(f))).ToList();
        var max = filled.Max();
        return filled.FindIndex(count => count >= max * 0.8);
    }

    private static List<(string[] Fields, string Raw, int Line)> ReadAll(string text, char separator)
    {
        var config = new CsvConfiguration(CultureInfo.GetCultureInfo("pl-PL"))
        {
            Delimiter = separator.ToString(),
            HasHeaderRecord = false,
            BadDataFound = null,
            MissingFieldFound = null,
        };
        using var parser = new CsvParser(new StringReader(text), config);
        var records = new List<(string[], string, int)>();
        while (parser.Read())
            records.Add((parser.Record ?? [], parser.RawRecord, parser.RawRow));
        return records;
    }

    private static string FirstLine(string text) => text.Split('\n', 2)[0].TrimEnd('\r');

    private static char DetectSeparator(string line) =>
        Candidates.OrderByDescending(c => CountOutsideQuotes(line, c)).First();

    private static int CountOutsideQuotes(string line, char separator)
    {
        var inQuotes = false;
        var count = 0;
        foreach (var ch in line)
        {
            inQuotes ^= ch == '"';
            count += !inQuotes && ch == separator ? 1 : 0;
        }
        return count;
    }
}
