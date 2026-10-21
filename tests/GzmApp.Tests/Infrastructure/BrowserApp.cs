using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;

namespace GzmApp.Tests.Infrastructure;

// The app on a real Kestrel port (the browser cannot reach TestServer) with its own database GzmTest_{scenario}_ui,
// plus a headless Chromium. Browser: GZM_CHROMIUM, else /opt/pw-browsers/chromium-*/chrome-linux/chrome,
// else the browser installed by Playwright (playwright.ps1 install chromium).
public sealed class BrowserApp : IAsyncDisposable
{
    private readonly AppFactory _app;
    private IPlaywright? _playwright;

    private BrowserApp(AppFactory app) => _app = app;

    public AppFactory App => _app;

    public string BaseUrl { get; private set; } = string.Empty;

    public IBrowser Browser { get; private set; } = null!;

    public static async Task<BrowserApp> StartAsync(string scenario)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scenario);

        var app = new AppFactory(scenario, "_ui");
        app.UseKestrel(0);
        app.StartServer();
        var browserApp = new BrowserApp(app) { BaseUrl = Address(app) };
        browserApp._playwright = await Playwright.CreateAsync();
        browserApp.Browser = await browserApp._playwright.Chromium.LaunchAsync(new() { Headless = true, ExecutablePath = Chromium() });
        return browserApp;
    }

    public async ValueTask DisposeAsync()
    {
        await Browser.DisposeAsync();
        _playwright?.Dispose();
        await _app.DisposeAsync();
    }

    private static string Address(AppFactory app) =>
        app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.First().Replace("[::]", "localhost").Replace("127.0.0.1", "localhost");

    private static string? Chromium() =>
        Environment.GetEnvironmentVariable("GZM_CHROMIUM")
        ?? (Directory.Exists("/opt/pw-browsers")
            ? Directory.GetFiles("/opt/pw-browsers", "chrome", SearchOption.AllDirectories).Where(f => f.Contains("chrome-linux", StringComparison.Ordinal)).Order().LastOrDefault()
            : null);
}
