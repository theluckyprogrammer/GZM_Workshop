using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace Expect;

// Number: data row number in the file, from 1, header not counted (= SourceRow in the database).
public sealed record SourceRow(int Number, string[] Fields);

public sealed record SourceTable(string Path, string EncodingName, IReadOnlyList<string> Headers, IReadOnlyList<SourceRow> Rows);

public static class SourceReader
{
    public static SourceTable Read(string root, ScenarioMapping mapping)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);
        ArgumentNullException.ThrowIfNull(mapping);

        var path = ResolvePath(root, mapping.Source.File);
        var bytes = File.ReadAllBytes(Path.Combine(root, path));
        var encoding = EncodingDetector.Detect(bytes);
        var text = SkipLines(encoding.Decode(bytes), mapping.Source.HeaderLine - 1);
        var records = Parse(text, mapping.Source.Separator);
        var rows = records.Skip(1).Select((fields, index) => new SourceRow(index + 1, fields)).ToList();
        return new SourceTable(path, encoding.Name, records[0], rows);
    }

    // data/work/<file> (cleaned copy) wins over data/raw/<file>.
    private static string ResolvePath(string root, string file)
    {
        var work = Path.Combine("data", "work", Path.GetFileName(file)).Replace('\\', '/');
        return File.Exists(Path.Combine(root, work)) ? work : file;
    }

    // Lines above the header (title block) are not part of the table.
    private static string SkipLines(string text, int count)
    {
        var start = 0;
        for (var i = 0; i < count; i++)
            start = text.IndexOf('\n', start) + 1;
        return text[start..];
    }

    private static List<string[]> Parse(string text, string separator)
    {
        var config = new CsvConfiguration(CultureInfo.GetCultureInfo("pl-PL"))
        {
            Delimiter = separator,
            HasHeaderRecord = false,
            BadDataFound = null,
            MissingFieldFound = null,
        };
        using var parser = new CsvParser(new StringReader(text), config);
        var records = new List<string[]>();
        while (parser.Read())
            records.Add(parser.Record ?? []);
        return records;
    }
}
