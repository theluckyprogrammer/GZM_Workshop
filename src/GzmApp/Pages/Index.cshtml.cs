using System.Net;
using System.Security.Claims;
using GzmApp.Ai;
using GzmApp.Data;
using GzmApp.Data.Import;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GzmApp.Pages;

public sealed record ListaModel(SearchResult Results, IReadOnlySet<int> Favorites);

public sealed record StarModel(int RecordId, bool IsFavorite);

public class IndexModel(ScenarioMapping mapping, IRecordRepository records, IFavoritesRepository favorites, IAiSummarizer ai) : PageModel
{
    public const int PageSize = 20;
    public const string SignInMessage = "Zaloguj się, aby dodać do ulubionych.";

    public ScenarioMapping Mapping => mapping;

    public ListaModel Lista { get; private set; } = new(new(string.Empty, 0, []), new HashSet<int>());

    public StatisticResult Statistic { get; private set; } = new(string.Empty, null, []);

    public async Task OnGetAsync(string? q, CancellationToken ct) => await LoadAsync(q, ct);

    // GET /?handler=Szukaj&q=...: HTMX gets the list fragment, a normal request gets the whole page.
    public async Task<IActionResult> OnGetSzukajAsync(string? q, CancellationToken ct)
    {
        if ((q?.Trim().Length ?? 0) > RecordRepository.MaxQueryLength)
            return Fragment(StatusCodes.Status400BadRequest, $"Fraza może mieć najwyżej {RecordRepository.MaxQueryLength} znaków.");
        if (Request.IsHtmx())
            return Partial("_Lista", await ListAsync(q, ct));

        await LoadAsync(q, ct);
        return Page();
    }

    // POST / DELETE /?handler=Ulubione&id=...: the antiforgery token comes from <body hx-headers:inherited=...>.
    public Task<IActionResult> OnPostUlubioneAsync(int id, CancellationToken ct) => ToggleAsync(id, true, ct);

    public Task<IActionResult> OnDeleteUlubioneAsync(int id, CancellationToken ct) => ToggleAsync(id, false, ct);

    // GET /?handler=Podsumuj&id=...: a description of one record from SOL-6, as a <p> fragment.
    public async Task<IActionResult> OnGetPodsumujAsync(int id, CancellationToken ct)
    {
        var fields = await records.GetFieldsAsync(id, ct);
        if (fields is null)
            return Fragment(StatusCodes.Status404NotFound, "Nie ma takiego rekordu.");

        var text = await ai.DescribeAsync($"Opisz w jednym zdaniu ten rekord ze zbioru „{mapping.Dataset}”.", fields, ct);
        return Fragment(StatusCodes.Status200OK, text);
    }

    private async Task<IActionResult> ToggleAsync(int id, bool add, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            return Fragment(StatusCodes.Status401Unauthorized, SignInMessage);

        await (add ? favorites.AddAsync(userId, id, ct) : favorites.RemoveAsync(userId, id, ct));
        return Partial("_Gwiazdka", new StarModel(id, add));
    }

    private async Task LoadAsync(string? q, CancellationToken ct)
    {
        Lista = await ListAsync(q, ct);
        Statistic = await records.StatisticAsync(ct);
    }

    private async Task<ListaModel> ListAsync(string? q, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var ids = userId is null ? new HashSet<int>() : await favorites.IdsForAsync(userId, ct);
        return new ListaModel(await records.SearchAsync(q, PageSize, ct), ids);
    }

    private ContentResult Fragment(int statusCode, string message) => new()
    {
        StatusCode = statusCode,
        ContentType = "text/html; charset=utf-8",
        Content = $"<p class=\"{(statusCode < 400 ? "mb-2" : "alert alert-warning")}\" role=\"status\">{WebUtility.HtmlEncode(message)}</p>",
    };
}
