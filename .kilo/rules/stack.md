# Stos projektu i zasady (warsztat „AI + otwarte dane GZM”)

Odpowiadaj po polsku, prostym językiem – pracujesz z uczniami, którzy nie programują. Na koniec napisz w 3 punktach, co zrobiłeś.

## Stos – używaj tylko tego
- .NET 10, jeden projekt ASP.NET Core Razor Pages: `src/GzmApp`. Port 5080.
- Widok: HTMX 4.0.0 z pliku lokalnego `wwwroot/lib/htmx/4.0.0/htmx.min.js` (już podpięty w `_Layout.cshtml`). Fragmenty HTML zwracają handlery Razor Pages (`Partial(...)`). Zasady HTMX 4: `.kilo/rules/htmx4.md`.
- Dane: Dapper + jawne skrypty SQL w `db/*.sql`. Każde zapytanie przez istniejącą klasę `SqlRunner` (loguje SQL). Parametry zawsze przez Dappera (`@q`), nigdy sklejanie tekstu SQL.
- CSV: pakiet CsvHelper (jest w projekcie), `CultureInfo("pl-PL")` albo kultura z promptu.
- Logowanie: ASP.NET Core Identity (już skonfigurowane). Token antiforgery wysyła `<body hx-headers:inherited=...>` w `_Layout.cshtml`.
- Rejestracja Twoich klas i zadania przy starcie: `src/GzmApp/AppServices.cs` (`AddAppServices`, `RunAtStartupAsync`).

## Zakazy
- React, Vue, Vite, npm, Node.js, bundler, `fetch` + JSON zamiast HTMX, biblioteka Htmx.Net, CDN (unpkg, jsdelivr).
- Entity Framework Core poza folderem `src/GzmApp/Identity/`.
- Zmiany logowania (Identity, cookie, antiforgery), dodawanie pakietów NuGet, zmiana portu, edycja `launch.json`, `launchSettings.json`, `*.csproj`, `Program.cs`.
- `htmx.logAll()`, `float`/`double` dla liczb z danych (używaj `decimal`), `DateTime.Parse` bez kultury i formatu.

## Kod
- Guard clauses na początku publicznych metod (`ArgumentNullException.ThrowIfNull`, `ArgumentException.ThrowIfNullOrWhiteSpace`).
- Zero zagnieżdżonych if-ów i pętli: wczesny `return`, LINQ, małe metody. Metody do 20 linii, klasy do 150 linii.
- Nazwy w kodzie po angielsku, komunikaty dla użytkownika po polsku.

## Pliki
- Zmieniaj tylko pliki z listy „Nie zmieniaj plików poza: …” w prompcie. Jeśli potrzebny jest inny plik – zatrzymaj się i zapytaj.
- Jeśli w prompcie zostało `[DECYZJA n]` bez wklejonej linii, użyj wariantu podanego w nawiasie zaraz pod nim.
