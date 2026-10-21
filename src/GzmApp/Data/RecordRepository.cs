using GzmApp.Data.Import;

namespace GzmApp.Data;

public sealed record RecordField(string Name, string Value);

public sealed record RecordItem(int RecordId, int SourceRow, string? Title, IReadOnlyList<RecordField> Fields);

// Total: all matching records; Items: at most "take" of them, ordered by SourceRow.
public sealed record SearchResult(string Query, int Total, IReadOnlyList<RecordItem> Items);

public interface IRecordRepository
{
    Task<SearchResult> SearchAsync(string? q, int take, CancellationToken ct = default);

    Task<StatisticResult> StatisticAsync(CancellationToken ct = default);
}

// Records of the scenario table (mapping.json). Search: LIKE on the filter field, ignoring case and accents.
public sealed class RecordRepository : IRecordRepository
{
    public const int MaxQueryLength = 100;

    private readonly SqlRunner _sql;
    private readonly ScenarioMapping _mapping;

    public RecordRepository(SqlRunner sql, ScenarioMapping mapping)
    {
        ArgumentNullException.ThrowIfNull(sql);
        ArgumentNullException.ThrowIfNull(mapping);

        _sql = sql;
        _mapping = mapping;
    }

    public async Task<SearchResult> SearchAsync(string? q, int take, CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(take);
        var phrase = (q ?? string.Empty).Trim();
        if (phrase.Length > MaxQueryLength)
            throw new ArgumentException($"Fraza może mieć najwyżej {MaxQueryLength} znaków.", nameof(q));

        var param = new { q = EscapeLike(phrase), take };
        var total = await _sql.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM {Table} {Where}", param, ct);
        var rows = await _sql.QueryAsync<dynamic>(
            $"SELECT TOP (@take) [RecordId], [SourceRow], [{_mapping.TitleField}] AS [Title]{ListColumns} FROM {Table} {Where} ORDER BY [SourceRow]", param, ct);
        return new SearchResult(phrase, total, rows.Select(r => ToItem((IDictionary<string, object?>)r)).ToList());
    }

    public Task<StatisticResult> StatisticAsync(CancellationToken ct = default) =>
        new StatisticQuery(_sql, _mapping).RunAsync(ct);

    private string Table => $"dbo.[{_mapping.Table}]";

    private string Where =>
        $"WHERE @q = N'' OR [{_mapping.Filter.Field}] LIKE N'%' + @q + N'%' COLLATE Latin1_General_CI_AI";

    private IReadOnlyList<string> ListFields => _mapping.ListFields ?? [];

    private string ListColumns => string.Concat(ListFields.Select(f => $", [{f}]"));

    private RecordItem ToItem(IDictionary<string, object?> row) => new(
        (int)row["RecordId"]!,
        (int)row["SourceRow"]!,
        row["Title"] as string,
        ListFields.Select(f => new RecordField(f, DisplayValue.Format(row[f]))).ToList());

    // %, _ and [ typed by the user are plain characters, not LIKE wildcards.
    private static string EscapeLike(string phrase) =>
        phrase.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");
}
