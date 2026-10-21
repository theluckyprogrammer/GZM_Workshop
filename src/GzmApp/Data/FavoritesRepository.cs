using GzmApp.Data.Import;

namespace GzmApp.Data;

public sealed record FavoriteItem(int RecordId, int SourceRow, string? Title, DateTime CreatedAt);

public interface IFavoritesRepository
{
    Task AddAsync(string userId, int recordId, CancellationToken ct = default);

    Task RemoveAsync(string userId, int recordId, CancellationToken ct = default);

    Task<IReadOnlyList<FavoriteItem>> ListForAsync(string userId, CancellationToken ct = default);

    Task<IReadOnlySet<int>> IdsForAsync(string userId, CancellationToken ct = default);
}

// dbo.Favorites (db/900_favorites.sql): one row per user, scenario and record. Add and Remove are idempotent.
public sealed class FavoritesRepository : IFavoritesRepository
{
    private readonly SqlRunner _sql;
    private readonly ScenarioMapping _mapping;

    public FavoritesRepository(SqlRunner sql, ScenarioMapping mapping)
    {
        ArgumentNullException.ThrowIfNull(sql);
        ArgumentNullException.ThrowIfNull(mapping);

        _sql = sql;
        _mapping = mapping;
    }

    // UPDLOCK + HOLDLOCK: two clicks at the same moment still give one row.
    public async Task AddAsync(string userId, int recordId, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        await _sql.ExecuteAsync($"""
            INSERT INTO dbo.Favorites (UserId, Scenario, RecordId)
            SELECT @userId, @scenario, @recordId
            WHERE EXISTS (SELECT 1 FROM {Table} WHERE [RecordId] = @recordId)
              AND NOT EXISTS (SELECT 1 FROM dbo.Favorites WITH (UPDLOCK, HOLDLOCK)
                              WHERE UserId = @userId AND Scenario = @scenario AND RecordId = @recordId)
            """, Param(userId, recordId), ct);
    }

    public async Task RemoveAsync(string userId, int recordId, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        await _sql.ExecuteAsync(
            "DELETE FROM dbo.Favorites WHERE UserId = @userId AND Scenario = @scenario AND RecordId = @recordId",
            Param(userId, recordId), ct);
    }

    public async Task<IReadOnlyList<FavoriteItem>> ListForAsync(string userId, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        return await _sql.QueryAsync<FavoriteItem>($"""
            SELECT f.RecordId, r.[SourceRow], r.[{_mapping.TitleField}] AS Title, f.CreatedAt
            FROM dbo.Favorites f JOIN {Table} r ON r.[RecordId] = f.RecordId
            WHERE f.UserId = @userId AND f.Scenario = @scenario
            ORDER BY f.CreatedAt DESC, r.[SourceRow]
            """, new { userId, scenario = _mapping.Scenario }, ct);
    }

    public async Task<IReadOnlySet<int>> IdsForAsync(string userId, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var ids = await _sql.QueryAsync<int>(
            "SELECT RecordId FROM dbo.Favorites WHERE UserId = @userId AND Scenario = @scenario",
            new { userId, scenario = _mapping.Scenario }, ct);
        return ids.ToHashSet();
    }

    private string Table => $"dbo.[{_mapping.Table}]";

    private object Param(string userId, int recordId) => new { userId, scenario = _mapping.Scenario, recordId };
}
