using Microsoft.AspNetCore.Identity;

namespace GzmApp.Identity;

// Polish messages for the password and user name rules of ASP.NET Core Identity (registration page).
public sealed class PolishIdentityErrors : IdentityErrorDescriber
{
    public override IdentityError PasswordTooShort(int length) => Error(nameof(PasswordTooShort), $"Hasło musi mieć co najmniej {length} znaków.");

    public override IdentityError PasswordRequiresNonAlphanumeric() => Error(nameof(PasswordRequiresNonAlphanumeric), "Hasło musi mieć co najmniej jeden znak specjalny, np. ! albo #.");

    public override IdentityError PasswordRequiresDigit() => Error(nameof(PasswordRequiresDigit), "Hasło musi mieć co najmniej jedną cyfrę.");

    public override IdentityError PasswordRequiresLower() => Error(nameof(PasswordRequiresLower), "Hasło musi mieć co najmniej jedną małą literę.");

    public override IdentityError PasswordRequiresUpper() => Error(nameof(PasswordRequiresUpper), "Hasło musi mieć co najmniej jedną wielką literę.");

    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) => Error(nameof(PasswordRequiresUniqueChars), $"Hasło musi mieć co najmniej {uniqueChars} różnych znaków.");

    public override IdentityError DuplicateUserName(string userName) => Error(nameof(DuplicateUserName), $"Konto {userName} już istnieje. Zaloguj się.");

    public override IdentityError DuplicateEmail(string email) => Error(nameof(DuplicateEmail), $"Konto z adresem {email} już istnieje.");

    public override IdentityError InvalidUserName(string? userName) => Error(nameof(InvalidUserName), $"Nazwa użytkownika „{userName}” ma niedozwolone znaki.");

    public override IdentityError InvalidEmail(string? email) => Error(nameof(InvalidEmail), $"Adres „{email}” nie jest poprawny.");

    private static IdentityError Error(string code, string description) => new() { Code = code, Description = description };
}
