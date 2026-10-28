using System.Text.RegularExpressions;

namespace GzmApp.Data;

public static partial class GzmConnection
{
    public const string EnvironmentVariable = "GZM_CONNSTR";
    public const string ConfigurationName = "Gzm";

    // GZM_CONNSTR wins (tests set it as a setting); otherwise SQLite from appsettings.json
    // (Data Source=gzm.db – the file next to the app, not the current working directory).
    public static string Resolve(IConfiguration configuration, string contentRootPath)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentRootPath);

        var fromEnvironment = configuration[EnvironmentVariable];
        if (!string.IsNullOrWhiteSpace(fromEnvironment))
            return fromEnvironment;

        var connectionString = configuration.GetConnectionString(ConfigurationName)
            ?? throw new InvalidOperationException($"Brak ConnectionStrings:{ConfigurationName} w appsettings.json.");
        return AnchorDataSource(connectionString, contentRootPath);
    }

    // A relative "Data Source=file.db" depends on the working directory (dotnet run, F5, tests);
    // we anchor it to the application folder, so the file is always in one place.
    private static string AnchorDataSource(string connectionString, string contentRootPath)
    {
        var file = DataSourcePattern().Match(connectionString).Groups["file"].Value;
        return Path.IsPathRooted(file) || file is "" or ":memory:"
            ? connectionString
            : $"Data Source={Path.GetFullPath(Path.Combine(contentRootPath, file))}";
    }

    [GeneratedRegex(@"Data Source=(?<file>[^;]+)")]
    private static partial Regex DataSourcePattern();
}
