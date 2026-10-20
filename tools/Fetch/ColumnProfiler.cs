using System.Globalization;
using System.Text.RegularExpressions;

namespace Fetch;

public sealed record ColumnProfile(string Name, string Type, string? Format, int EmptyCount, int DistinctCount, string? Note)
{
    public bool IsNumericOrDate => Type is "integer" or "decimal" or "date" or "datetime";
}

// Type of a column = the first type that matches ALL non-empty values. Otherwise category or text.
public static class ColumnProfiler
{
    private static readonly string[] DateFormats =
    [
        "yyyy-MM-dd", "dd.MM.yyyy", "dd-MM-yyyy", "yyyy/MM/dd", "dd/MM/yyyy", "d.M.yyyy",
        "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-dd'T'HH:mm:ss'Z'", "yyyy-MM-dd HH:mm:ss.fff",
        "dd.MM.yyyy HH:mm", "dd.MM.yyyy HH:mm:ss", "HH:mm", "HH:mm:ss",
    ];

    private static readonly Regex Integer = new(@"^-?\d+$", RegexOptions.Compiled);
    private static readonly Regex CommaDecimal = new(@"^-?\d+,\d+$", RegexOptions.Compiled);
    private static readonly Regex UnixMilliseconds = new(@"^\d{12,13}$", RegexOptions.Compiled);
    private static readonly long MinUnixMs = new DateTimeOffset(1990, 1, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeMilliseconds();
    private static readonly long MaxUnixMs = new DateTimeOffset(2036, 1, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeMilliseconds();
    private static readonly Regex DotDecimal = new(@"^-?\d+\.\d+$", RegexOptions.Compiled);

    public static ColumnProfile Profile(string name, IReadOnlyList<string> rawValues)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(rawValues);

        var values = rawValues.Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v.Trim()).ToList();
        var empty = rawValues.Count - values.Count;
        var distinct = values.Distinct().Count();
        if (values.Count == 0)
            return new ColumnProfile(name, "empty", null, empty, 0, null);

        var candidates = Candidates(values).ToList();
        var exact = candidates.FirstOrDefault(c => c.Share >= 1.0);
        if (exact.Type is not null)
            return new ColumnProfile(name, exact.Type, exact.Format, empty, distinct, null);

        var fallback = IsCategory(values.Count, distinct) ? "category" : "text";
        return new ColumnProfile(name, fallback, null, empty, distinct, DominantNote(candidates));
    }

    private static IEnumerable<(string Type, string? Format, double Share)> Candidates(List<string> values)
    {
        yield return ("date", "unix-ms", Share(values, IsUnixMilliseconds));
        yield return ("integer", null, Share(values, Integer.IsMatch));
        yield return ("decimal", "przecinek", DecimalShare(values, CommaDecimal));
        yield return ("decimal", "kropka", DecimalShare(values, DotDecimal));
        foreach (var format in DateFormats)
            yield return (format.Contains('H') && format.Contains('y') ? "datetime" : "date", format, Share(values, v => IsDate(v, format)));
    }

    private static double DecimalShare(List<string> values, Regex pattern) =>
        values.Any(pattern.IsMatch) ? Share(values, v => pattern.IsMatch(v) || Integer.IsMatch(v)) : 0;

    private static double Share(List<string> values, Func<string, bool> match) =>
        (double)values.Count(match) / values.Count;

    private static bool IsDate(string value, string format) =>
        DateTime.TryParseExact(value, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);

    // Dates written as milliseconds since 1970 (e.g. 1728518400000).
    private static bool IsUnixMilliseconds(string value) =>
        UnixMilliseconds.IsMatch(value) && InUnixRange(long.Parse(value));

    private static bool InUnixRange(long milliseconds) => milliseconds >= MinUnixMs && milliseconds < MaxUnixMs;

    private static bool IsCategory(int count, int distinct) => distinct <= Math.Min(30, count / 3);

    private static string? DominantNote(List<(string Type, string? Format, double Share)> candidates)
    {
        var best = candidates.OrderByDescending(c => c.Share).First();
        return best.Share < 0.8 ? null : $"{best.Share:P0} wartości to {best.Type} {best.Format}".TrimEnd();
    }
}
