using GzmApp.Ai;
using GzmApp.Data;
using Microsoft.AspNetCore.Identity;

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

        services.AddSingleton<ICsvImporter, CsvImporter>();
        services.AddSingleton<IRecordRepository, RecordRepository>();
        services.AddSingleton<IAiAnalyzer, AiAnalyzer>();
        return services;
    }

    // Runs once at start-up, after the database and the login tables are ready.
    public static async Task RunAtStartupAsync(IServiceProvider services, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(services);

        await using var scope = services.CreateAsyncScope();
        await SeedPairLoginAsync(scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>());
        await RunDataImportAsync(scope.ServiceProvider, ct);
    }

    // Creates the BikeAvailability table (SQL script from db/) and imports the CSV file when the table is empty.
    private static async Task RunDataImportAsync(IServiceProvider services, CancellationToken ct)
    {
        var sql = services.GetRequiredService<SqlRunner>();
        var scriptPath = Path.Combine(AppContext.BaseDirectory, "db", "s03_create.sql");
        var script = await File.ReadAllTextAsync(scriptPath, ct);
        await sql.ExecuteAsync(script, ct: ct);
        await services.GetRequiredService<ICsvImporter>().ImportAsync(ct);
    }

    // The pair's ready login from the printed work card (adm@portal.local / TestPass123!) – created once, when missing.
    private const string PairEmail = "adm@portal.local";

    private static async Task SeedPairLoginAsync(UserManager<IdentityUser> users)
    {
        if (await users.FindByNameAsync(PairEmail) is not null)
            return;

        var renamed = await users.FindByNameAsync("adm");
        if (renamed is not null)
        {
            renamed.UserName = PairEmail;
            await users.UpdateAsync(renamed);
            return;
        }

        var user = new IdentityUser { UserName = PairEmail, Email = PairEmail };
        var result = await users.CreateAsync(user, "TestPass123!");
        if (result != IdentityResult.Success)
            throw new InvalidOperationException($"Nie udało się utworzyć konta adm: {string.Join("; ", result.Errors.Select(e => e.Description))}");
    }
}
