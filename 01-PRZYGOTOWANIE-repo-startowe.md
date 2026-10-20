# 01 – Przygotowanie: repo startowe

Przeczytaj `zadanie/prompts/00-KONTEKST-wspolny.md` (sekcje: Projekt, Stos, Zasady kodu, Oszczędzanie tokenów, Status).

- **Wymaga:** repo GitHub z folderem `zadanie/` (projekt powstaje w katalogu głównym repo, obok `zadanie/`).
- **Sesja:** nowa.

## Zadanie
1. Przygotuj repozytorium (już istnieje, NIE rób `git init`):
   - jeśli w katalogu głównym są już pliki spoza `zadanie/`, wypisz je i zatrzymaj się, nie nadpisuj ich,
   - `.gitignore` (bin, obj, *.user, secrets.json, data/work/),
   - `global.json`,
   - `docs/STATUS.md` z nagłówkiem tabeli,
   - pusty plik `findings.md`.
2. Utwórz strukturę katalogów: `src/GzmApp`, `tests/`, `db/`, `data/raw/`, `scenarios/`, `docs/`, `print/`, `tools/`, `.vscode/`.
3. Projekt `src/GzmApp` (Razor Pages, .NET 10):
   - Pobierz HTMX 4.0.0 do `wwwroot/lib/htmx/4.0.0/htmx.min.js`:
     - źródło: `https://cdnjs.cloudflare.com/ajax/libs/htmx/4.0.0/htmx.min.js`,
     - fallback: jsDelivr `htmx.org@4.0.0`,
     - zapisz SHA-256 pliku w `docs/HTMX.md`.
   - Layout:
     - `<script>` wskazuje plik lokalny,
     - `<body hx-headers:inherited='{"RequestVerificationToken":"@token"}'>`,
     - stopka z atrybucją danych czytaną z konfiguracji (na razie placeholder).
4. Identity:
   - `AddDefaultIdentity` + `IdentityDbContext` w `src/GzmApp/Identity/`,
   - cookie,
   - `EnsureCreated` przy starcie,
   - domyślne strony logowania i rejestracji.
5. Baza:
   - connection string `(localdb)\MSSQLLocalDB`, `Database=GzmWorkshop`,
   - `db/000_create.sql` tworzy bazę, jeśli jej nie ma,
   - klasa `SqlRunner` (Dapper + log SQL).
6. Konfiguracja i strona startowa:
   - `appsettings.json`: `"Scenario": "s01"`,
   - strona startowa pokazuje kod scenariusza i placeholder „Tu pojawi się Twoja aplikacja”.
7. Port i VS Code:
   - `launchSettings.json`: stały port 5080,
   - `.vscode/launch.json`: typ coreclr, `preLaunchTask: build`, `serverReadyAction` otwiera przeglądarkę,
   - `.vscode/tasks.json`: zadanie build,
   - `.vscode/extensions.json`: ms-dotnettools.csharp, ms-dotnettools.csdevkit, ms-mssql.mssql, kilocode.kilo-code.
8. `docs/LICENSES.md`: każdy pakiet z licencją.

## Kryterium ukończenia
- `dotnet build` przechodzi bez ostrzeżeń.
- Aplikacja startuje na porcie 5080 i strona startowa pokazuje „s01”.
- Rejestracja i logowanie działają. Sprawdź to żądaniami HTTP z cookie (curl).
- W logu widać zapytanie z `SqlRunner` (np. `SELECT 1` przy starcie).

## Na koniec
Commit `"01: repo startowe"` i wiersz w `docs/STATUS.md`.
