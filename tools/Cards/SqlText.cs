using Expect;

namespace Cards;

// SQL shown to students: the table for step 1 and the statistic queries for step 3.
public static class SqlText
{
    public static string CreateTable(ScenarioMapping mapping)
    {
        ArgumentNullException.ThrowIfNull(mapping);

        var lines = new List<string>
        {
            $"    [RecordId] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_{mapping.Table}] PRIMARY KEY,",
            "    [SourceRow] int NOT NULL,",
        };
        lines.AddRange(mapping.Imported.Select(c => $"    [{c.Field}] {c.SqlType} {(c.Nullable ? "NULL" : "NOT NULL")},"));
        if (mapping.Key.IsRowHash)
            lines.Add("    [RowHash] binary(32) NOT NULL,");
        lines.Add($"    CONSTRAINT [UQ_{mapping.Table}_Key] UNIQUE ({KeyColumns(mapping)})");
        return $"IF OBJECT_ID(N'dbo.{mapping.Table}') IS NULL\nCREATE TABLE dbo.[{mapping.Table}] (\n{string.Join("\n", lines)}\n);";
    }

    public static string Statistic(string kind, string field, string table)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);
        ArgumentException.ThrowIfNullOrWhiteSpace(field);
        ArgumentException.ThrowIfNullOrWhiteSpace(table);

        var column = $"[{field}]";
        var from = $"dbo.[{table}]";
        return kind switch
        {
            "sum" => $"SELECT SUM({column}) FROM {from}",
            "avg" => $"SELECT ROUND(AVG(CAST({column} AS decimal(18,2))), 2) FROM {from}",
            "top5" => $"SELECT TOP 5 {column} AS Value, COUNT(*) AS Count FROM {from} WHERE {column} IS NOT NULL GROUP BY {column} ORDER BY COUNT(*) DESC, {column}",
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
