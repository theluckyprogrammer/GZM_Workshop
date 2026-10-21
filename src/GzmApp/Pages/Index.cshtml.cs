using GzmApp.Data;
using GzmApp.Data.Import;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GzmApp.Pages;

public class IndexModel(ScenarioMapping mapping, IRecordRepository records) : PageModel
{
    public const int PageSize = 20;

    public ScenarioMapping Mapping => mapping;

    public SearchResult Results { get; private set; } = new(string.Empty, 0, []);

    public StatisticResult Statistic { get; private set; } = new(string.Empty, null, []);

    public async Task OnGetAsync(string? q, CancellationToken ct) => await LoadAsync(q, ct);

    // GET /?handler=Szukaj&q=...: HTMX gets the list fragment, a normal request gets the whole page.
    public async Task<IActionResult> OnGetSzukajAsync(string? q, CancellationToken ct)
    {
        if ((q?.Trim().Length ?? 0) > RecordRepository.MaxQueryLength)
            return Fragment(StatusCodes.Status400BadRequest, $"Fraza może mieć najwyżej {RecordRepository.MaxQueryLength} znaków.");
        if (Request.IsHtmx())
            return Partial("_Lista", await records.SearchAsync(q, PageSize, ct));

        await LoadAsync(q, ct);
        return Page();
    }

    private async Task LoadAsync(string? q, CancellationToken ct)
    {
        Results = await records.SearchAsync(q, PageSize, ct);
        Statistic = await records.StatisticAsync(ct);
    }

    private ContentResult Fragment(int statusCode, string message) => new()
    {
        StatusCode = statusCode,
        ContentType = "text/html; charset=utf-8",
        Content = $"<p class=\"alert alert-warning\" role=\"alert\">{System.Net.WebUtility.HtmlEncode(message)}</p>",
    };
}
