namespace GzmApp.Data;

public static class GzmConnection
{
    public const string EnvironmentVariable = "GZM_CONNSTR";
    public const string ConfigurationName = "Gzm";

    // GZM_CONNSTR wins (Linux, Docker); otherwise LocalDB from appsettings.json.
    public static string Resolve(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var fromEnvironment = Environment.GetEnvironmentVariable(EnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(fromEnvironment))
            return fromEnvironment;

        return configuration.GetConnectionString(ConfigurationName)
            ?? throw new InvalidOperationException($"Brak ConnectionStrings:{ConfigurationName} w appsettings.json.");
    }
}
