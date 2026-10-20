using Expect;

namespace Cards;

// SQL shown to students: the table for stage 1 and the statistic query for stage 3.
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

    public static string Statistic(ScenarioMapping mapping)
    {
        ArgumentNullException.ThrowIfNull(mapping);

        var field = $"[{mapping.Statistic.Field}]";
        var table = $"dbo.[{mapping.Table}]";
        return mapping.Statistic.Kind switch
        {
            "sum" => $"SELECT SUM({field}) FROM {table}",
            "avg" => $"SELECT ROUND(AVG(CAST({field} AS decimal(18,2))), 2) FROM {table}",
            "top5" => $"SELECT TOP 5 {field} AS Value, COUNT(*) AS Count FROM {table} WHERE {field} IS NOT NULL GROUP BY {field} ORDER BY COUNT(*) DESC, {field}",
            _ => $"SELECT {field} AS Value, COUNT(*) AS Count FROM {table} WHERE {field} IS NOT NULL GROUP BY {field} ORDER BY COUNT(*) DESC, {field}",
        };
    }

    public static string Description(ScenarioMapping mapping)
    {
        ArgumentNullException.ThrowIfNull(mapping);

        var field = $"`{mapping.Statistic.Field}`";
        return mapping.Statistic.Kind switch
        {
            "sum" => $"suma pola {field}.",
            "avg" => $"średnia pola {field} z 2 miejscami po przecinku (puste wartości pomijamy).",
            "top5" => $"5 najczęstszych wartości pola {field} z liczbą rekordów (puste pomijamy; przy równej liczbie – alfabetycznie).",
            _ => $"liczba rekordów dla każdej wartości pola {field} (puste pomijamy; od największej liczby).",
        };
    }

    private static string KeyColumns(ScenarioMapping mapping) =>
        mapping.Key.IsRowHash ? "[RowHash]" : string.Join(", ", mapping.Key.Fields.Select(f => $"[{f}]"));
}
