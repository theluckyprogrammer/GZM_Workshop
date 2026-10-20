namespace GzmApp.Data.Import;

// #Staging → scenario table. Duplicates of the key in the file: the row with the lowest SourceRow wins.
public static class MergeSql
{
    public static string Merge(ScenarioMapping mapping)
    {
        ArgumentNullException.ThrowIfNull(mapping);

        var targetColumns = TargetColumns(mapping);
        var on = KeyFields(mapping).Select(k => $"t.[{k}] = s.[{k}]");
        var set = targetColumns.Select(c => $"t.[{c}] = s.[{c}]");
        return $"{Deduplicated(mapping)}\n" +
               $"MERGE dbo.[{mapping.Table}] WITH (HOLDLOCK) AS t\n" +
               "USING (SELECT * FROM src WHERE [Rn] = 1) AS s\n" +
               $"ON {string.Join(" AND ", on)}\n" +
               $"WHEN MATCHED THEN UPDATE SET {string.Join(", ", set)}\n" +
               $"WHEN NOT MATCHED BY TARGET THEN INSERT ({Join(targetColumns, "")})\n" +
               $"    VALUES ({Join(targetColumns, "s.")});";
    }

    // "Weird" records among the rows that end up in the table (after removing duplicates).
    public static string CountWeird(ScenarioMapping mapping)
    {
        ArgumentNullException.ThrowIfNull(mapping);

        return $"{Deduplicated(mapping)}\nSELECT COUNT(*) FROM src WHERE [Rn] = 1 AND [IsWeird] = 1;";
    }

    private static string Deduplicated(ScenarioMapping mapping) =>
        $"WITH src AS (SELECT *, ROW_NUMBER() OVER (PARTITION BY {RecordTableBuilder.KeyColumns(mapping)} ORDER BY [SourceRow]) AS [Rn] " +
        $"FROM {RecordTableBuilder.StagingTable})";

    private static IReadOnlyList<string> KeyFields(ScenarioMapping mapping) =>
        mapping.Key.IsRowHash ? ["RowHash"] : mapping.Key.Fields;

    private static List<string> TargetColumns(ScenarioMapping mapping)
    {
        var columns = new List<string> { "SourceRow" };
        columns.AddRange(mapping.Imported.Select(c => c.Field!));
        if (mapping.Key.IsRowHash)
            columns.Add("RowHash");
        return columns;
    }

    private static string Join(IEnumerable<string> columns, string prefix) =>
        string.Join(", ", columns.Select(c => $"{prefix}[{c}]"));
}
