using System.Globalization;
using System.Text.Json;

namespace GzmApp.Tests.Data;

// One cell after the rules of mapping.json, written independently of tools/Expect:
// null = empty, Bad = cannot be read, otherwise Text = the value as SQL Server shows it.
public sealed record Cell(string Raw, string? Text, bool Bad)
{
    public bool Empty => Text is null && !Bad;
}

public static class IndependentValues
{
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    public static Cell Read(JsonElement column, string raw)
    {
        ArgumentNullException.ThrowIfNull(raw);

        var text = raw.Trim();
        if (text.Length == 0)
            return new Cell(raw, null, false);
        var value = Convert(column, text);
        return new Cell(raw, value, value is null);
    }

    private static string? Convert(JsonElement column, string text)
    {
        var type = column.GetProperty("sqlType").GetString()!;
        var culture = column.GetProperty("culture").GetString() == "invariant" ? Invariant : CultureInfo.GetCultureInfo("pl-PL");
        var format = column.GetProperty("format").GetString();
        return type switch
        {
            _ when type.StartsWith("nvarchar(", StringComparison.Ordinal) => text.Length <= Length(type) ? text : null,
            "int" => int.TryParse(text, NumberStyles.AllowLeadingSign, culture, out var i) ? i.ToString(Invariant) : null,
            _ when type.StartsWith("decimal(", StringComparison.Ordinal) => Decimal(text, culture, type),
            "bit" => Bit(text, format ?? "true|false"),
            "date" => Date(text, format!, culture)?.ToString("yyyy-MM-dd", Invariant),
            _ when type.StartsWith("datetime2", StringComparison.Ordinal) => Date(text, format!, culture)?.ToString("yyyy-MM-dd HH:mm:ss", Invariant),
            _ when type.StartsWith("time", StringComparison.Ordinal) => Date(text, format!, culture)?.ToString("HH:mm", Invariant),
            _ => throw new NotSupportedException(type),
        };
    }

    private static int Length(string type) => int.Parse(type[9..^1], Invariant);

    // decimal(p,s): at most s digits after the point and p-s before it; shown with exactly s decimals.
    private static string? Decimal(string text, CultureInfo culture, string type)
    {
        var parts = type[8..^1].Split(',');
        var (precision, scale) = (int.Parse(parts[0], Invariant), int.Parse(parts[1], Invariant));
        var separator = culture.NumberFormat.NumberDecimalSeparator;
        var ok = decimal.TryParse(text, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, culture, out var value);
        var digits = text.TrimStart('-').Split(separator);
        var fits = ok && digits[0].TrimStart('0').Length <= precision - scale && (digits.Length < 2 || digits[1].Length <= scale);
        return fits ? value.ToString("F" + scale, Invariant) : null;
    }

    private static string? Bit(string text, string format)
    {
        var words = format.Split('|');
        return string.Equals(text, words[0], StringComparison.OrdinalIgnoreCase) ? "1"
            : string.Equals(text, words[1], StringComparison.OrdinalIgnoreCase) ? "0" : null;
    }

    private static DateTime? Date(string text, string format, CultureInfo culture) =>
        format == "unix-ms"
            ? long.TryParse(text, NumberStyles.None, Invariant, out var ms) ? DateTime.UnixEpoch.AddMilliseconds(ms) : null
            : DateTime.TryParseExact(text, format.Split('|'), culture, DateTimeStyles.None, out var d) ? d : null;
}
