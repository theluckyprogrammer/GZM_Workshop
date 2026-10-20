using System.Globalization;

namespace GzmApp.Data.Import;

public enum ValueState { Ok, Empty, Unparsable }

public sealed record ParsedValue(ValueState State, object? Value);

// Values are trimmed first. Empty after trimming = NULL. A value that does not fit the SQL type = unparsable (also NULL).
public static class ValueParser
{
    private const NumberStyles DecimalStyle = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;
    private const long MinUnixMs = -62_135_596_800_000;
    private const long MaxUnixMs = 253_402_300_799_999;

    public static ParsedValue Parse(ColumnMapping column, SqlType type, string raw)
    {
        ArgumentNullException.ThrowIfNull(column);
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(raw);

        var text = raw.Trim();
        if (text.Length == 0)
            return new ParsedValue(ValueState.Empty, null);

        var value = Convert(column, type, text);
        return new ParsedValue(value is null ? ValueState.Unparsable : ValueState.Ok, value);
    }

    private static object? Convert(ColumnMapping column, SqlType type, string text) => type.Kind switch
    {
        SqlKind.Text => text.Length <= type.Length ? text : null,
        SqlKind.Int => int.TryParse(text, NumberStyles.AllowLeadingSign, column.CultureInfo, out var i) ? i : null,
        SqlKind.BigInt => long.TryParse(text, NumberStyles.AllowLeadingSign, column.CultureInfo, out var l) ? l : null,
        SqlKind.Decimal => ParseDecimal(text, column.CultureInfo, type),
        SqlKind.Bit => ParseBit(text, column.Format),
        _ => ParseDate(text, column, type.Kind),
    };

    private static object? ParseDecimal(string text, CultureInfo culture, SqlType type)
    {
        if (!decimal.TryParse(text, DecimalStyle, culture, out var value))
            return null;

        var integerDigits = Math.Truncate(Math.Abs(value)).ToString(CultureInfo.InvariantCulture).TrimStart('0').Length;
        return value.Scale <= type.Scale && integerDigits <= type.Precision - type.Scale ? value : null;
    }

    // Format "Tak|Nie": first word = 1, second word = 0.
    private static object? ParseBit(string text, string? format)
    {
        var words = (format ?? "true|false").Split('|');
        if (text.Equals(words[0], StringComparison.OrdinalIgnoreCase))
            return true;
        return text.Equals(words[1], StringComparison.OrdinalIgnoreCase) ? false : null;
    }

    private static object? ParseDate(string text, ColumnMapping column, SqlKind kind)
    {
        var parsed = column.Format == "unix-ms" ? FromUnixMs(text) : FromPattern(text, column);
        return parsed switch
        {
            null => null,
            _ when kind == SqlKind.Date => parsed.Value.Date,
            _ when kind == SqlKind.Time => parsed.Value.TimeOfDay,
            _ => parsed.Value,
        };
    }

    // Milliseconds since 1970-01-01 UTC, read as a UTC date and time.
    private static DateTime? FromUnixMs(string text) =>
        long.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var ms) && ms is >= MinUnixMs and <= MaxUnixMs
            ? DateTimeOffset.FromUnixTimeMilliseconds(ms).UtcDateTime
            : null;

    // Several patterns are separated with "|", e.g. "HH:mm|HH.mm".
    private static DateTime? FromPattern(string text, ColumnMapping column) =>
        DateTime.TryParseExact(text, (column.Format ?? "").Split('|'), column.CultureInfo, DateTimeStyles.None, out var value)
            ? value
            : null;
}
