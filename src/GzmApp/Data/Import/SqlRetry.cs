using Microsoft.Data.SqlClient;

namespace GzmApp.Data.Import;

// Up to 3 attempts when SQL Server reports a transient error (deadlock, timeout, lost connection, database starting).
public static class SqlRetry
{
    public const int MaxAttempts = 3;

    private static readonly HashSet<int> TransientErrors = [-2, 20, 64, 233, 1205, 4060, 10053, 10054, 10060, 10928, 10929, 40143, 40197, 40501, 40613, 49918, 49919, 49920];

    public static async Task<T> RunAsync<T>(Func<Task<T>> work, ILogger logger, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(work);
        ArgumentNullException.ThrowIfNull(logger);

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await work();
            }
            catch (SqlException ex) when (attempt < MaxAttempts && IsTransient(ex))
            {
                logger.LogWarning("Przejściowy błąd SQL {Number} (próba {Attempt}/{Max}): {Message}", ex.Number, attempt, MaxAttempts, ex.Message);
                await Task.Delay(TimeSpan.FromSeconds(attempt), ct);
            }
        }
    }

    public static bool IsTransient(SqlException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception.Errors.Cast<SqlError>().Any(e => TransientErrors.Contains(e.Number));
    }
}
