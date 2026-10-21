using System.Security.Claims;
using GzmApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GzmApp.Pages;

// "Moje ulubione": only for a signed-in user (otherwise a redirect to the login page).
[Authorize]
public class UlubioneModel(IFavoritesRepository favorites) : PageModel
{
    public IReadOnlyList<FavoriteItem> Items { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Zalogowany użytkownik bez identyfikatora.");
        Items = await favorites.ListForAsync(userId, ct);
    }
}
