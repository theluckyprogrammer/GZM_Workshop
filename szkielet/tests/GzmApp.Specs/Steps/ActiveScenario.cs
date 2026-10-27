using System.Reflection;
using System.Text.Json;
using GzmApp.Tests.Infrastructure;

namespace GzmApp.Specs.Steps;

// The pair's scenario, chosen at build time from "Scenario" in src/GzmApp/appsettings(.Development).json
// (GzmApp.Specs.csproj). One app for the whole test run; its test database is dropped before the first start
// and after the run.
public static class ActiveScenario
{
    private static readonly Lazy<Task<SpecApp>> Started = new(StartAsync);

    public static string Code { get; } =
        typeof(ActiveScenario).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().FirstOrDefault(a => a.Key == "GzmScenario")?.Value ?? string.Empty;

    public static Scenario Data => string.IsNullOrEmpty(Code)
        ? throw new InvalidOperationException("Brak scenariusza: uruchomcie pierwszy prompt z prompty.md (krok 0) – skrypt tools/nowy-portal.ps1 tworzy aplikację i wpisuje scenariusz. Potem przebudujcie projekt.")
        : ScenarioData.Get(Code);

    public static string Table => Data.Text("table");

    public static string TitleField => Data.Text("titleField");

    public static Task<SpecApp> AppAsync() => Started.Value;

    public static async Task<HttpClient> BrowserAsync() => (await AppAsync()).CreateBrowser();

    public static IEnumerable<JsonElement> Imported =>
        Data.Mapping.GetProperty("columns").EnumerateArray().Where(c => !c.TryGetProperty("import", out var i) || i.GetBoolean());

    public static JsonElement Column(string field) =>
        Imported.FirstOrDefault(c => c.GetProperty("field").GetString() == field) is { ValueKind: JsonValueKind.Object } column
            ? column
            : throw new InvalidOperationException($"Pola `{field}` nie ma w tabeli `{Table}` (scenarios/{Code}/mapping.json).");

    private static async Task<SpecApp> StartAsync()
    {
        var database = new TestDatabase(Data.Code);
        await database.DropAsync();
        var app = new SpecApp(Data.Code, database, ownsDatabase: true);
        AppDomain.CurrentDomain.ProcessExit += (_, _) => app.DisposeAsync().AsTask().GetAwaiter().GetResult();
        await StartedOrExplainedAsync(app);
        return app;
    }

    // The first request starts the app (start-up script and import); an exception there is the pair's start-up code.
    public static async Task StartedOrExplainedAsync(SpecApp app)
    {
        ArgumentNullException.ThrowIfNull(app);

        try
        {
            await app.CreateBrowser().GetAsync("/");
        }
        catch (Exception e)
        {
            throw new InvalidOperationException($"Aplikacja nie startuje (AppServices.RunAtStartupAsync albo import): {e.GetBaseException().Message}", e);
        }
    }
}
