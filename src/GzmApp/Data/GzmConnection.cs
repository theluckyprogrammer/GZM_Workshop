namespace GzmApp.Data;

public static class GzmConnection
{
    public const string EnvironmentVariable = "GZM_CONNSTR";
    public const string ConfigurationName = "Gzm";

    // GZM_CONNSTR wins (Linux, Docker; tests set it as a setting); otherwise LocalDB from appsettings.json.
    // Environment variables are part of the configuration, so the variable GZM_CONNSTR works as before.
    public static string Resolve(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var fromEnvironment = configuration[EnvironmentVariable];
        if (!string.IsNullOrWhiteSpace(fromEnvironment))
            return fromEnvironment;

        return configuration.GetConnectionString(ConfigurationName)
            ?? throw new InvalidOperationException($"Brak ConnectionStrings:{ConfigurationName} w appsettings.json.");
    }
}
