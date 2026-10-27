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

        return services;
    }

    // Runs once at start-up, after the database and the login tables are ready.
    public static async Task RunAtStartupAsync(IServiceProvider services, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(services);

        await using var scope = services.CreateAsyncScope();
        await SeedPairLoginAsync(scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>());
    }

    // The pair's ready login from the printed work card (adm / TestPass123!) – created once, when missing.
    private static async Task SeedPairLoginAsync(UserManager<IdentityUser> users)
    {
        if (await users.FindByNameAsync("adm") is not null)
            return;

        var user = new IdentityUser { UserName = "adm", Email = "adm@portal.local" };
        var result = await users.CreateAsync(user, "TestPass123!");
        if (result != IdentityResult.Success)
            throw new InvalidOperationException($"Nie udało się utworzyć konta adm: {string.Join("; ", result.Errors.Select(e => e.Description))}");
    }
}
