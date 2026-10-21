namespace GzmApp;

// The place for your classes: register services here and run start-up work (SQL scripts, import).
// Program.cs already calls both methods – leave Program.cs unchanged.
public static class AppServices
{
    public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        return services;
    }

    // Runs once at start-up, after the database and the login tables are ready.
    public static Task RunAtStartupAsync(IServiceProvider services, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(services);

        return Task.CompletedTask;
    }
}
