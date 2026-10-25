using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GzmApp.Areas.Identity.Pages.Account;

// Polish registration page; replaces the English page from Microsoft.AspNetCore.Identity.UI (same address and field names).
// Password rules come from Identity; their messages are Polish thanks to PolishIdentityErrors.
public sealed class RegisterModel(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager) : PageModel
{
    [BindProperty]
    public RegisterInput Input { get; set; } = new();

    public string ReturnUrl { get; private set; } = "/";

    public void OnGet(string? returnUrl = null) => ReturnUrl = LocalOrHome(returnUrl);

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = LocalOrHome(returnUrl);
        if (!ModelState.IsValid)
            return Page();

        var user = new IdentityUser { UserName = Input.Email, Email = Input.Email };
        var result = await userManager.CreateAsync(user, Input.Password);
        if (!result.Succeeded)
            return PageWithErrors(result);

        await signInManager.SignInAsync(user, isPersistent: false);
        return LocalRedirect(ReturnUrl);
    }

    private PageResult PageWithErrors(IdentityResult result)
    {
        result.Errors.ToList().ForEach(e => ModelState.AddModelError(string.Empty, e.Description));
        return Page();
    }

    private string LocalOrHome(string? returnUrl) => Url.IsLocalUrl(returnUrl) ? returnUrl : "/";
}

public sealed class RegisterInput
{
    [Required(ErrorMessage = "Podaj adres e-mail.")]
    [EmailAddress(ErrorMessage = "To nie jest poprawny adres e-mail.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Podaj hasło.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Hasła nie są takie same.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
