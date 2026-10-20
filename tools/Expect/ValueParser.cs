using System.Globalization;

namespace Expect;

public enum ValueState { Ok, Empty, Unparsable }

// Display: the value as SQL Server shows it after import (dates yyyy-MM-dd, decimals with a dot).
public sealed record ParsedValue(ValueState State, string Raw, object? Value, string Display);

// Values are trimmed first. Empty after trimming = NULL. A value that does not fit the SQL type = unparsable.
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
            return new ParsedValue(ValueState.Empty, raw, null, "NULL");

        var value = Convert(column, type, text);
        return value is null
            ? new ParsedValue(ValueState.Unparsable, raw, null, "NULL")
            : new ParsedValue(ValueState.Ok, raw, value, Format(type, value));
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

        var scale = value.Scale;
        var integerDigits = Math.Truncate(Math.Abs(value)).ToString(CultureInfo.InvariantCulture).TrimStart('0').Length;
        return scale <= type.Scale && integerDigits <= type.Precision - type.Scale ? value : null;
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

    private static string Format(SqlType type, object value) => value switch
    {
        DateTime date when type.Kind == SqlKind.Date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        DateTime dateTime => dateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
        TimeSpan time => time.ToString(@"hh\:mm", CultureInfo.InvariantCulture),
        bool flag => flag ? "1" : "0",
        decimal number => number.ToString($"F{type.Scale}", CultureInfo.InvariantCulture),
        IFormattable number => number.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? "",
    };
}
