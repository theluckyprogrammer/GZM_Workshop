using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace GzmApp.Data.Import;

// SourceRow: data row number in the file, from 1, header not counted.
public sealed record CsvRow(int SourceRow, string[] Fields);

public sealed record CsvTable(string EncodingName, IReadOnlyList<string> Headers, IReadOnlyList<CsvRow> Rows);

// Reads the CSV file: encoding detection, lines above the header skipped, CsvHelper with pl-PL.
public static class CsvSource
{
    public static CsvTable Read(string path, SourceMapping source)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(source);

        var bytes = File.ReadAllBytes(path);
        var encoding = EncodingDetector.Detect(bytes);
        var text = SkipLines(encoding.Decode(bytes), source.HeaderLine - 1);
        var records = Parse(text, source.Separator);
        if (records.Count == 0)
            throw new InvalidDataException($"{path}: brak nagłówka.");

        var rows = records.Skip(1).Select((fields, index) => new CsvRow(index + 1, fields)).ToList();
        return new CsvTable(encoding.Name, records[0], rows);
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
