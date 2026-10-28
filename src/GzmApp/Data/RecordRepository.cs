namespace GzmApp.Data;

public sealed class RecordRow
{
    public int RecordId { get; init; }

    public int SourceRow { get; init; }

    public string Day { get; init; } = string.Empty;

    public string Commune { get; init; } = string.Empty;

    public int AvailableBikes { get; init; }

    public int RequiredBikes { get; init; }

    public int Exceeded { get; init; }
}

public sealed record RecordList(int Count, IReadOnlyList<RecordRow> Items);

public sealed class MapRecord
{
    public string Commune { get; init; } = string.Empty;

    public int AvailableBikes { get; init; }

    public int RequiredBikes { get; init; }
}

public interface IRecordRepository
{
    Task<RecordList> SearchAsync(string? q, int take, CancellationToken ct = default);

    Task<long?> SumAvailableAsync(CancellationToken ct = default);

    Task<IReadOnlyList<MapRecord>> MapAsync(CancellationToken ct = default);

    Task<int> DeficitCountAsync(CancellationToken ct = default);

    Task<MapRecord?> FindByCommuneAsync(string commune, CancellationToken ct = default);
}

// Reads the BikeAvailability table: search by the phrase (decision 1 – field Commune) and the statistic tile.
public sealed class RecordRepository(SqlRunner sql) : IRecordRepository
{
    private const string SearchSql = """
        SELECT [RecordId], [SourceRow], [Day], [Commune], [AvailableBikes], [RequiredBikes], [Exceeded]
        FROM [BikeAvailability]
        WHERE plain(CAST([Commune] AS TEXT)) LIKE '%' || plain(@q) || '%'
        ORDER BY [SourceRow]
        LIMIT @take
        """;

    private const string CountSql = """
        SELECT COUNT(*)
        FROM [BikeAvailability]
        WHERE plain(CAST([Commune] AS TEXT)) LIKE '%' || plain(@q) || '%'
        """;

    public async Task<RecordList> SearchAsync(string? q, int take, CancellationToken ct = default)
    {
        var param = new { q = q ?? string.Empty, take };
        var items = await sql.QueryAsync<RecordRow>(SearchSql, param, ct);
        var count = await sql.ExecuteScalarAsync<int>(CountSql, new { q = q ?? string.Empty }, ct);
        return new RecordList(count, items);
    }

    public Task<long?> SumAvailableAsync(CancellationToken ct = default) =>
        sql.ExecuteScalarAsync<long?>("SELECT SUM([AvailableBikes]) FROM [BikeAvailability]", ct: ct);

    public Task<IReadOnlyList<MapRecord>> MapAsync(CancellationToken ct = default) =>
        sql.QueryAsync<MapRecord>(
            "SELECT [Commune], [AvailableBikes], [RequiredBikes] FROM [BikeAvailability] ORDER BY [Commune]",
            ct: ct);

    public Task<int> DeficitCountAsync(CancellationToken ct = default) =>
        sql.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM [BikeAvailability] WHERE [AvailableBikes] < [RequiredBikes]",
            ct: ct);

    public async Task<MapRecord?> FindByCommuneAsync(string commune, CancellationToken ct = default)
    {
        var rows = await sql.QueryAsync<MapRecord>(
            """
            SELECT [Commune], [AvailableBikes], [RequiredBikes]
            FROM [BikeAvailability]
            WHERE plain(CAST([Commune] AS TEXT)) = plain(@commune)
            """,
            new { commune }, ct);
        return rows.FirstOrDefault();
    }
}
