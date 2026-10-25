namespace GzmApp.Ai;

// Section "Sol6": Model in appsettings.json; Endpoint and ApiKey in appsettings.Local.json (copied by the teacher,
// ignored by git) or in the environment variables SOL6__ENDPOINT and SOL6__APIKEY – never in the repository.
public sealed class Sol6Options
{
    public string Endpoint { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Endpoint) && !string.IsNullOrWhiteSpace(Model) && !string.IsNullOrWhiteSpace(ApiKey);
}

// The only error the AI model gives to the pages: a short Polish message, safe to show (no key, no request details).
public sealed class Sol6Exception(string message) : Exception(message)
{
    public const string MissingKey = "Brak klucza do modelu AI – poproś prowadzącego.";
    public const string NoAnswer = "Model AI nie odpowiedział – spróbujcie za chwilę.";
}
