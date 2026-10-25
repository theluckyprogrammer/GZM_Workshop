# Stos projektu i zasady (warsztat „AI + otwarte dane GZM”)

Odpowiadaj po polsku, prostym językiem – pracujesz z uczniami, którzy nie programują. Na koniec napisz w 3 punktach, co zrobiłeś.

## Stos – używaj tylko tego
- .NET 10, jeden projekt ASP.NET Core Razor Pages: `src/GzmApp`. Port 5080.
- Widok: HTMX 4.0.0 z pliku lokalnego `wwwroot/lib/htmx/4.0.0/htmx.min.js` (już podpięty w `_Layout.cshtml`). Fragmenty HTML zwracają handlery Razor Pages (`Partial(...)`). Zasady HTMX 4: `.kilo/rules/htmx4.md`.
- Wygląd: biblioteka CSS Tabler 1.6.1 (klasy Bootstrap 5: `card`, `list-group`, `badge`, `btn`, `row`/`col-*`), plik lokalny już podpięty w `_Layout.cshtml`. Wszystkie teksty na stronie po polsku.
- AI: model SOL-6 jest już podłączony w `Program.cs` – z DI bierzesz `IChatClient` (`Microsoft.Extensions.AI`) i `IOptions<Sol6Options>` (`GzmApp.Ai`). Klucz jest w `src/GzmApp/appsettings.Local.json` (od prowadzącego). Odpowiedź modelu pokazuj jako tekst (`@`), nigdy przez `Html.Raw`.
- Dane: Dapper + jawne skrypty SQL w `db/*.sql`. Każde zapytanie przez istniejącą klasę `SqlRunner` (loguje SQL). Parametry zawsze przez Dappera (`@q`), nigdy sklejanie tekstu SQL.
- CSV: pakiet CsvHelper (jest w projekcie), `CultureInfo("pl-PL")` albo kultura z promptu.
- Logowanie: ASP.NET Core Identity (już skonfigurowane). Token antiforgery wysyła `<body hx-headers:inherited=...>` w `_Layout.cshtml`.
- Rejestracja Twoich klas i zadania przy starcie: `src/GzmApp/AppServices.cs` (`AddAppServices`, `RunAtStartupAsync`).

## Zakazy
- React, Vue, Vite, npm, Node.js, bundler, `fetch` + JSON zamiast HTMX, biblioteka Htmx.Net, CDN (unpkg, jsdelivr), inne biblioteki CSS niż Tabler.
- Klucz AI w kodzie albo w `appsettings.json`, własny klient AI (`OpenAIClient`, `HttpClient` do modelu) zamiast `IChatClient` z DI.
- Entity Framework Core poza folderem `src/GzmApp/Identity/`.
- Zmiany logowania (Identity, cookie, antiforgery), dodawanie pakietów NuGet, zmiana portu, edycja `launch.json`, `launchSettings.json`, `*.csproj`, `Program.cs`.
- `htmx.logAll()`, `float`/`double` dla liczb z danych (używaj `decimal`), `DateTime.Parse` bez kultury i formatu.

## Kod
- Guard clauses na początku publicznych metod (`ArgumentNullException.ThrowIfNull`, `ArgumentException.ThrowIfNullOrWhiteSpace`).
- Zero zagnieżdżonych if-ów i pętli: wczesny `return`, LINQ, małe metody. Metody do 20 linii, klasy do 150 linii.
- Nazwy w kodzie po angielsku, komunikaty dla użytkownika po polsku.

## Specyfikacja i testy – najpierw zdania, potem kod
- Każdy krok ma plik specyfikacji `scenarios/sNN/spec/sNN-NNN-*.md`. Przeczytaj go, zanim napiszesz kod: „Co się dzieje” opisuje krok, a „Reguły” mówią, co dokładnie ma działać.
- Każda ponumerowana reguła to jeden test w `tests/GzmApp.Specs` (nazwa testu = zdanie reguły). Krok jest gotowy, gdy wszystkie jego testy są zielone.
- Zrób to, co mówią reguły i prompt – nie mniej i nie więcej.
- Nie zmieniaj specyfikacji, testów ani narzędzi (`scenarios/`, `tests/`, `tools/`). Czerwony test naprawiasz w kodzie aplikacji, nigdy w teście.
- Sprawdzenie kroku: `dotnet test tests/GzmApp.Specs --filter Capability=<capability z nagłówka pliku>`. Komunikat czerwonego testu mówi po polsku, czego brakuje.

## Pliki
- Zmieniaj tylko pliki z listy „Nie zmieniaj plików poza: …” w prompcie. Jeśli potrzebny jest inny plik – zatrzymaj się i zapytaj.
