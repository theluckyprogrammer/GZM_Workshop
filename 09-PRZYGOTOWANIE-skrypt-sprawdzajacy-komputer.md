# 09 – Przygotowanie: skrypt sprawdzający komputer (check.ps1)

Przeczytaj `zadanie/prompts/00-KONTEKST-wspolny.md` (sekcje: Stos, Zasady kodu, Status).

- **Wymaga:** 08 OK.
- **Sesja:** można kontynuować po 08.

## Zadanie
Napisz `tools/check.ps1` (Windows PowerShell 5.1 i pwsh 7, bez uprawnień administratora). Skrypt wypisuje OK / BŁĄD dla każdego punktu i krótką podpowiedź naprawy:

1. `dotnet --list-sdks` zawiera wersję 10.0.
2. `sqllocaldb info MSSQLLocalDB` działa, a połączenie z bazą zwraca wynik `SELECT 1`. Użyj sqlcmd, jeśli jest; inaczej `System.Data.SqlClient` z .NET.
3. `code --list-extensions` zawiera: ms-dotnettools.csharp, ms-dotnettools.csdevkit, ms-mssql.mssql, kilocode.kilo-code.
4. Port 5080 jest wolny.
5. `git --version` działa, a repo ma gałęzie `ratunek/etap-1..4`.
6. Plik `wwwroot/lib/htmx/4.0.0/htmx.min.js` istnieje i ma SHA-256 zgodny z `docs/HTMX.md`.
7. `dotnet build` przechodzi (offline, z cache NuGet).
8. Endpoint Kilo odpowiada przez HTTPS. Klucz NIE może trafić do logu ani na ekran.
9. Opcjonalnie: jeśli ustawiony jest sekret `Sol6:ApiKey`, jedno krótkie wywołanie SOL-6.

Kod wyjścia: 0, gdy wszystko jest OK; 1 w przeciwnym razie. Dodaj `tools/check.cmd`, żeby uczeń mógł uruchomić skrypt dwuklikiem (`-ExecutionPolicy Bypass` w zakresie procesu).

## Kryterium ukończenia
- Składnia skryptu jest poprawna (parser PowerShell).
- Na Windows: przebieg z wynikiem OK.
- Na innym systemie: analiza składni + wpis w `findings.md`, że pełny test trzeba wykonać na komputerze szkolnym.

## Na koniec
Commit `"09: check.ps1"` (w `main`, zmerguj w górę) i wiersz w `docs/STATUS.md`.
