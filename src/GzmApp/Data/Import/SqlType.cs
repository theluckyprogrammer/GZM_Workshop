using System.Text.RegularExpressions;

namespace GzmApp.Data.Import;

public enum SqlKind { Text, Int, BigInt, Decimal, Date, DateTime, Time, Bit }

// Length: nvarchar(n); Precision/Scale: decimal(p,s).
public sealed partial record SqlType(SqlKind Kind, int Length, int Precision, int Scale)
{
    public static SqlType Parse(string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        var match = Pattern().Match(text.Trim().ToLowerInvariant());
        if (!match.Success)
            throw new InvalidDataException($"Nieznany typ SQL: {text}");

        return Create(match.Groups["name"].Value, match.Groups["a"].Value, match.Groups["b"].Value, text);
    }

    private static SqlType Create(string name, string first, string second, string text) =>
        name switch
        {
            "nvarchar" => new SqlType(SqlKind.Text, first == "max" ? int.MaxValue : int.Parse(first), 0, 0),
            "decimal" => new SqlType(SqlKind.Decimal, 0, int.Parse(first), second.Length > 0 ? int.Parse(second) : 0),
            "int" => new SqlType(SqlKind.Int, 0, 0, 0),
            "bigint" => new SqlType(SqlKind.BigInt, 0, 0, 0),
            "date" => new SqlType(SqlKind.Date, 0, 0, 0),
            "datetime2" => new SqlType(SqlKind.DateTime, 0, 0, 0),
            "time" => new SqlType(SqlKind.Time, 0, 0, 0),
            "bit" => new SqlType(SqlKind.Bit, 0, 0, 0),
            _ => throw new InvalidDataException($"Nieobsługiwany typ SQL: {text}"),
        };

    [GeneratedRegex(@"^(?<name>[a-z0-9]+)(\((?<a>max|\d+)(,\s*(?<b>\d+))?\))?$")]
    private static partial Regex Pattern();
}
