using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace GzmApp.Specs.Steps;

// Reads the pair's table straight from the test database: the expected values of the steps come from here
// (and from checkpoints.md), so every variant of a decision can be checked.
public static partial class Records
{
    public static async Task<SpecApp> TableReadyAsync()
    {
        var app = await ActiveScenario.AppAsync();
        var exists = await app.Database.ScalarAsync<int>("SELECT CASE WHEN OBJECT_ID(@name) IS NULL THEN 0 ELSE 1 END", new { name = $"dbo.{ActiveScenario.Table}" });
        Assert.True(exists == 1, $"Tabela `{ActiveScenario.Table}` jeszcze nie istnieje – tworzy ją krok 1 (skrypt db/{ActiveScenario.Code}_create.sql przy starcie).");
        return app;
    }

    public static async Task<int> CountAsync(string where = "1 = 1", object? param = null)
    {
        var app = await TableReadyAsync();
        return await app.Database.ScalarAsync<int>($"SELECT COUNT(*) FROM {Table} WHERE {where}", param);
    }

    public static async Task<IReadOnlyList<int>> FirstRowsAsync(int take, string where = "1 = 1", object? param = null) =>
        (await RowsAsync($"SELECT TOP {take} [SourceRow] FROM {Table} WHERE {where} ORDER BY [SourceRow]", param)).Select(r => (int)r[0]!).ToList();

    // The value as text, in the same form as checkpoints.md (style 121: yyyy-mm-dd hh:mi:ss).
    public static async Task<IReadOnlyList<(int SourceRow, string? Value)>> ColumnTextAsync(JsonElement column)
    {
        var app = await TableReadyAsync();
        var field = Field(column.GetProperty("field").GetString()!);
        var text = column.GetProperty("sqlType").GetString()!.StartsWith("time", StringComparison.Ordinal)
            ? $"LEFT(CONVERT(nvarchar(4000), {field}, 121), 5)"
            : $"CONVERT(nvarchar(4000), {field}, 121)";
        return await app.Database.PairsAsync($"SELECT [SourceRow], {text} FROM {Table}");
    }

    public static async Task<IReadOnlyList<object?[]>> RowsAsync(string sql, object? param = null)
    {
        var app = await TableReadyAsync();
        await using var connection = new SqlConnection(app.Database.ConnectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
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
        return (int)rows[0][0]!;
    }

    // The two records of the favorites steps: SourceRow 7 and the next one.
    public static async Task<IReadOnlyList<(int RecordId, string? Title)>> FavoriteCandidatesAsync() =>
        (await RowsAsync($"SELECT TOP 2 [RecordId], CONVERT(nvarchar(4000), {Field(ActiveScenario.TitleField)}) FROM {Table} WHERE [SourceRow] >= 7 ORDER BY [SourceRow]"))
            .Select(r => ((int)r[0]!, (string?)r[1])).ToList();

    public static string Table => $"dbo.[{ActiveScenario.Table}]";

    // A field name from the specification goes into SQL, so it must be a plain name.
    public static string Field(string name) =>
        PlainName().IsMatch(name) ? $"[{name}]" : throw new InvalidOperationException($"„{name}” nie jest nazwą pola.");

    [GeneratedRegex(@"^\w+$")]
    private static partial Regex PlainName();
}
