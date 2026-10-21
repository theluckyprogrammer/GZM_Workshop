using GzmApp.Data.Import;

namespace GzmApp.Data;

public sealed record RecordItem(int RecordId, int SourceRow, string? Title);

// Total: all matching records; Items: at most "take" of them, ordered by SourceRow.
public sealed record SearchResult(string Query, int Total, IReadOnlyList<RecordItem> Items);

public interface IRecordRepository
{
    Task<SearchResult> SearchAsync(string? q, int take, CancellationToken ct = default);
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
        var items = await _sql.QueryAsync<RecordItem>(
            $"SELECT TOP (@take) [RecordId], [SourceRow], [{_mapping.TitleField}] AS [Title] FROM {Table} {Where} ORDER BY [SourceRow]", param, ct);
        return new SearchResult(phrase, total, items);
    }

    private string Table => $"dbo.[{_mapping.Table}]";

    private string Where =>
        $"WHERE @q = N'' OR [{_mapping.Filter.Field}] LIKE N'%' + @q + N'%' COLLATE Latin1_General_CI_AI";

    // %, _ and [ typed by the user are plain characters, not LIKE wildcards.
    private static string EscapeLike(string phrase) =>
        phrase.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");
}
