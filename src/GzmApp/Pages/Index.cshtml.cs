using GzmApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GzmApp.Pages;

public class IndexModel(IConfiguration configuration, IRecordRepository records) : PageModel
{
    public const int PageSize = 20;

    public string Scenario { get; private set; } = string.Empty;

    public void OnGet() => Scenario = configuration["Scenario"] ?? "(brak scenariusza)";

    // GET /?handler=Szukaj&q=...: an HTML fragment with the list (no layout).
    public async Task<IActionResult> OnGetSzukajAsync(string? q, CancellationToken ct)
    {
        if ((q?.Trim().Length ?? 0) > RecordRepository.MaxQueryLength)
            return Fragment(StatusCodes.Status400BadRequest, $"Fraza może mieć najwyżej {RecordRepository.MaxQueryLength} znaków.");

        return Partial("_Lista", await records.SearchAsync(q, PageSize, ct));
    }

    private ContentResult Fragment(int statusCode, string message) => new()
    {
        StatusCode = statusCode,
        ContentType = "text/html; charset=utf-8",
        Content = $"<p class=\"alert alert-warning\" role=\"alert\">{System.Net.WebUtility.HtmlEncode(message)}</p>",
    };
}
