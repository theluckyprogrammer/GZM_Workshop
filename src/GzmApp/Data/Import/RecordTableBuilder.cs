namespace GzmApp.Data.Import;

// CREATE TABLE for the scenario table (saved to db/sNN_create.sql) and for the #Staging table.
public static class RecordTableBuilder
{
    public const string StagingTable = "#Staging";

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

    // Writes db/<scenario>_create.sql and returns its text.
    public static async Task<string> WriteScriptAsync(ScenarioMapping mapping, string dbDirectory, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(mapping);
        ArgumentException.ThrowIfNullOrWhiteSpace(dbDirectory);

        var sql = CreateTable(mapping);
        var path = Path.Combine(dbDirectory, $"{mapping.Scenario}_create.sql");
        await File.WriteAllTextAsync(path, sql + "\n", ct);
        return sql;
    }

    // Staging: every field nullable, plus SourceRow, RowHash and the "weird" flag.
    public static string CreateStaging(ScenarioMapping mapping)
    {
        ArgumentNullException.ThrowIfNull(mapping);

        var fields = mapping.Imported.Select(c => $"    [{c.Field}] {c.SqlType} NULL,");
        return $"CREATE TABLE {StagingTable} (\n    [SourceRow] int NOT NULL,\n{string.Join("\n", fields)}\n" +
               "    [RowHash] binary(32) NOT NULL,\n    [IsWeird] bit NOT NULL\n);";
    }

    // Column order of #Staging = order of ImportRecord values.
    public static IReadOnlyList<string> StagingColumns(ScenarioMapping mapping)
    {
        ArgumentNullException.ThrowIfNull(mapping);

        return ["SourceRow", .. mapping.Imported.Select(c => c.Field!), "RowHash", "IsWeird"];
    }

    public static string KeyColumns(ScenarioMapping mapping)
    {
        ArgumentNullException.ThrowIfNull(mapping);

        return mapping.Key.IsRowHash ? "[RowHash]" : string.Join(", ", mapping.Key.Fields.Select(f => $"[{f}]"));
    }
}
