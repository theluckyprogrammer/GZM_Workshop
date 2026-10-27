namespace GzmApp.Tests.Infrastructure;

// Registers a new user through the Identity UI pages; the client keeps the login cookie.
public static class TestUser
{
    public const string Password = "Haslo123!";

    public static async Task<string> RegisterAsync(HttpClient client)
    {
        ArgumentNullException.ThrowIfNull(client);

        var email = $"test-{Guid.NewGuid():N}@example.com";
        var page = await client.GetStringAsync("/Identity/Account/Register");
        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.Email"] = email,
            ["Input.Password"] = Password,
            ["Input.ConfirmPassword"] = Password,
            ["__RequestVerificationToken"] = Html.FormToken(page),
        });
        var response = await client.PostAsync("/Identity/Account/Register", form);
        return response.StatusCode == System.Net.HttpStatusCode.Redirect
            ? email
            : throw new InvalidOperationException($"Rejestracja nie powiodła się: {(int)response.StatusCode}.");
    }

    // The antiforgery token for the signed-in user, taken from the home page.
    public static async Task<string> TokenAsync(HttpClient client)
    {
        ArgumentNullException.ThrowIfNull(client);

        return Html.BodyToken(await client.GetStringAsync("/"));
    }
}
