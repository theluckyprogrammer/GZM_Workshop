using System.Text.Json;
using System.Text.RegularExpressions;
using GzmApp.Data;
using Microsoft.Data.Sqlite;

namespace GzmApp.Specs.Steps;

// Reads the pair's table straight from the test database: the expected values of the steps come from here
// (and from checkpoints.md), so every variant of a decision can be checked.
public static partial class Records
{
    public static async Task<SpecApp> TableReadyAsync()
    {
        var app = await ActiveScenario.AppAsync();
        var exists = await app.Database.ScalarAsync<long>("SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = @name", new { name = ActiveScenario.Table });
        Assert.True(exists == 1, $"Tabela `{ActiveScenario.Table}` jeszcze nie istnieje – tworzy ją krok 1 (skrypt db/{ActiveScenario.Code}_create.sql przy starcie).");
        return app;
    }

    public static async Task<int> CountAsync(string where = "1 = 1", object? param = null)
    {
        var app = await TableReadyAsync();
        return await app.Database.ScalarAsync<int>($"SELECT COUNT(*) FROM {Table} WHERE {where}", param);
    }

    public static async Task<IReadOnlyList<int>> FirstRowsAsync(int take, string where = "1 = 1", object? param = null) =>
        (await RowsAsync($"SELECT [SourceRow] FROM {Table} WHERE {where} ORDER BY [SourceRow] LIMIT {take}", param)).Select(r => Convert.ToInt32(r[0]!)).ToList();

    // The value as text, in the same form as checkpoints.md (dates yyyy-MM-dd, hours HH:mm).
    public static async Task<IReadOnlyList<(int SourceRow, string? Value)>> ColumnTextAsync(JsonElement column)
    {
        var app = await TableReadyAsync();
        var field = Field(column.GetProperty("field").GetString()!);
        var text = column.GetProperty("sqlType").GetString()!.StartsWith("time", StringComparison.Ordinal)
            ? $"substr(CAST({field} AS TEXT), 1, 5)"
            : $"CAST({field} AS TEXT)";
        return await app.Database.PairsAsync($"SELECT [SourceRow], {text} FROM {Table}");
    }

    public static async Task<IReadOnlyList<object?[]>> RowsAsync(string sql, object? param = null)
    {
        var app = await TableReadyAsync();
        await using var connection = new SqliteConnection(app.Database.ConnectionString);
        await connection.OpenAsync();
        connection.CreateFunction("plain", (string? value) => PlainText.Of(value));
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        (param?.GetType().GetProperties() ?? []).ToList().ForEach(p => command.Parameters.AddWithValue("@" + p.Name, p.GetValue(param) ?? DBNull.Value));
        await using var reader = await command.ExecuteReaderAsync();
        var rows = new List<object?[]>();
        while (await reader.ReadAsync())
            rows.Add(Enumerable.Range(0, reader.FieldCount).Select(i => reader.IsDBNull(i) ? null : reader.GetValue(i)).ToArray());
        return rows;
    }

    public static async Task<int> RecordIdAsync(int sourceRow)
    {
        var rows = await RowsAsync($"SELECT [RecordId] FROM {Table} WHERE [SourceRow] = @sourceRow", new { sourceRow });
        Assert.True(rows.Count == 1, $"W tabeli `{ActiveScenario.Table}` nie ma rekordu z `SourceRow` = {sourceRow}.");
        return Convert.ToInt32(rows[0][0]!);
    }

    // The two records of the favorites steps: SourceRow 7 and the next one.
    public static async Task<IReadOnlyList<(int RecordId, string? Title)>> FavoriteCandidatesAsync() =>
        (await RowsAsync($"SELECT [RecordId], CAST({Field(ActiveScenario.TitleField)} AS TEXT) FROM {Table} WHERE [SourceRow] >= 7 ORDER BY [SourceRow] LIMIT 2"))
            .Select(r => (Convert.ToInt32(r[0]!), (string?)r[1])).ToList();

    public static string Table => $"[{ActiveScenario.Table}]";

    // A field name from the specification goes into SQL, so it must be a plain name.
    public static string Field(string name) =>
        PlainName().IsMatch(name) ? $"[{name}]" : throw new InvalidOperationException($"„{name}” nie jest nazwą pola.");

    [GeneratedRegex(@"^\w+$")]
    private static partial Regex PlainName();
}
