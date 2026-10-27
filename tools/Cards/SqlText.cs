using Expect;

namespace Cards;

// SQL shown to students (SQLite): the table for step 1 and the statistic queries for step 3.
public static class SqlText
{
    public static string CreateTable(ScenarioMapping mapping)
    {
        ArgumentNullException.ThrowIfNull(mapping);

        var lines = new List<string>
        {
            "    [RecordId] INTEGER PRIMARY KEY AUTOINCREMENT,",
            "    [SourceRow] INTEGER NOT NULL,",
        };
        lines.AddRange(mapping.Imported.Select(c => $"    [{c.Field}] {SqliteType(c.SqlType!)} {(c.Nullable ? "NULL" : "NOT NULL")},"));
        if (mapping.Key.IsRowHash)
            lines.Add("    [RowHash] BLOB NOT NULL,");
        lines.Add($"    CONSTRAINT [UQ_{mapping.Table}_Key] UNIQUE ({KeyColumns(mapping)})");
        return $"CREATE TABLE IF NOT EXISTS [{mapping.Table}] (\n{string.Join("\n", lines)}\n);";
    }

    // SQLite storage: texts, numbers with decimals, dates and times in TEXT (canonical formats),
    // whole numbers and 0/1 flags in INTEGER.
    public static string SqliteType(string sqlType) => sqlType switch
    {
        _ when sqlType.StartsWith("nvarchar", StringComparison.Ordinal) => "TEXT",
        _ when sqlType.StartsWith("decimal", StringComparison.Ordinal) => "TEXT",
        _ when sqlType.StartsWith("date", StringComparison.Ordinal) => "TEXT",
        _ when sqlType.StartsWith("time", StringComparison.Ordinal) => "TEXT",
        _ => "INTEGER",
    };

    public static string Statistic(string kind, string field, string table)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);
        ArgumentException.ThrowIfNullOrWhiteSpace(field);
        ArgumentException.ThrowIfNullOrWhiteSpace(table);

        var column = $"[{field}]";
        var from = $"[{table}]";
        return kind switch
        {
            "sum" => $"SELECT SUM({column}) FROM {from}",
            "avg" => $"SELECT ROUND(AVG(CAST({column} AS REAL)), 2) FROM {from}",
            "top5" => $"SELECT {column} AS Value, COUNT(*) AS Count FROM {from} WHERE {column} IS NOT NULL GROUP BY {column} ORDER BY COUNT(*) DESC, {column} LIMIT 5",
            _ => $"SELECT {column} AS Value, COUNT(*) AS Count FROM {from} WHERE {column} IS NOT NULL GROUP BY {column} ORDER BY COUNT(*) DESC, {column}",
        };
    }

    public static string Description(string kind, string field)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);
        ArgumentException.ThrowIfNullOrWhiteSpace(field);

        var name = $"`{field}`";
        return kind switch
        {
            "sum" => $"suma pola {name}.",
            "avg" => $"średnia pola {name} z 2 miejscami po przecinku (puste pomijamy).",
            "top5" => $"5 najczęstszych wartości pola {name} z liczbą rekordów (puste pomijamy).",
            _ => $"liczba rekordów dla każdej wartości pola {name}, od największej (puste pomijamy).",
        };
    }

    private static string KeyColumns(ScenarioMapping mapping) =>
        mapping.Key.IsRowHash ? "[RowHash]" : string.Join(", ", mapping.Key.Fields.Select(f => $"[{f}]"));
}
