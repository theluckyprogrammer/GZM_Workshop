namespace Fetch;

public static class Retry
{
    public const int Attempts = 3;

    public static async Task<T> RunAsync<T>(Func<Task<T>> action, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(action);

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await action();
            }
            catch (Exception ex) when (IsTransient(ex) && attempt < Attempts)
            {
                await Task.Delay(TimeSpan.FromSeconds(2 * attempt), ct);
            }
        }
    }

    public static bool IsTransient(Exception ex) => ex is HttpRequestException or TaskCanceledException;
}
