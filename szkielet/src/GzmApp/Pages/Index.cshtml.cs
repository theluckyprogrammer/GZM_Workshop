using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GzmApp.Pages;

public class IndexModel(IConfiguration configuration) : PageModel
{
    public string Scenario { get; private set; } = string.Empty;

    public void OnGet() => Scenario = configuration["Scenario"] ?? "(brak scenariusza)";
}
