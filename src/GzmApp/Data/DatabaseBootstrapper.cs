using GzmApp.Identity;

namespace GzmApp.Data;

// At startup: create the database (db/000_create.sql), create AspNet* tables, the Favorites table (db/900_favorites.sql), check the connection.
public sealed class DatabaseBootstrapper
{
    private readonly SqlRunner _sql;
    private readonly IServiceScopeFactory _scopeFactory;

    public DatabaseBootstrapper(SqlRunner sql, IServiceScopeFactory scopeFactory)
    {
        ArgumentNullException.ThrowIfNull(sql);
        ArgumentNullException.ThrowIfNull(scopeFactory);

        _sql = sql;
        _scopeFactory = scopeFactory;
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        await CreateDatabaseAsync(ct);
        await EnsureIdentityTablesAsync(ct);
        await _sql.ExecuteAsync(await File.ReadAllTextAsync(ScriptPath("900_favorites.sql"), ct), ct: ct);
        await _sql.ExecuteScalarAsync<int>("SELECT 1", ct: ct);
    }

    private async Task CreateDatabaseAsync(CancellationToken ct)
    {
        var script = await File.ReadAllTextAsync(ScriptPath("000_create.sql"), ct);
        await _sql.ForDatabase("master").ExecuteAsync(script, new { DatabaseName = _sql.DatabaseName }, ct);
    }

    private async Task EnsureIdentityTablesAsync(CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var identityDb = scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();
        await identityDb.Database.EnsureCreatedAsync(ct);
    }

    private static string ScriptPath(string fileName) =>
        Path.Combine(AppContext.BaseDirectory, "db", fileName);
}
