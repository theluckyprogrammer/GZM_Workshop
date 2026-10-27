# Stos projektu i zasady (warsztat „AI + otwarte dane GZM”)

Odpowiadaj po polsku, prostym językiem – pracujesz z uczniami, którzy nie programują. Na koniec napisz w 3 punktach, co zrobiłeś.

## Start portalu (krok 0)
- Portal pary powstaje w kroku 0: skrypt `tools/nowy-portal.ps1 -Scenariusz sNN` kopiuje szkielet (`szkielet/` → `src/`, `tests/`), pobiera dane z portalu GZM i wpisuje scenariusz. Uruchamiasz go tylko wtedy, gdy poprosi o to prompt kroku 0; plików `szkielet/` nie zmieniasz.
- `LOG-DECYZJI.md` w głównym katalogu to log decyzji pary: po każdej decyzji dopisujesz jeden wiersz do tabeli (Krok | Pytanie | Wasza decyzja | Dlaczego). Niczego z niego nie usuwasz.

## Stos – używaj tylko tego
- .NET 10, jeden projekt ASP.NET Core Razor Pages: `src/GzmApp`. Port 5080.
- Widok: HTMX 4.0.0 z pliku lokalnego `wwwroot/lib/htmx/4.0.0/htmx.min.js` (już podpięty w `_Layout.cshtml`). Fragmenty HTML zwracają handlery Razor Pages (`Partial(...)`). Zasady HTMX 4: `.kilo/rules/htmx4.md`.
- Wygląd: biblioteka CSS Tabler 1.6.1 (klasy Bootstrap 5: `card`, `list-group`, `badge`, `btn`, `row`/`col-*`), plik lokalny już podpięty w `_Layout.cshtml`. Wszystkie teksty na stronie po polsku.
- AI: model GPT-6 Luna jest już podłączony w `Program.cs` – z DI bierzesz `IChatClient` (`Microsoft.Extensions.AI`) i `IOptions<Sol6Options>` (`GzmApp.Ai`). Klucz jest w `src/GzmApp/appsettings.Local.json` (od prowadzącego). Odpowiedź modelu pokazuj jako tekst (`@`), nigdy przez `Html.Raw`.
- Dane: SQLite – baza to jeden plik `src/GzmApp/gzm.db`, powstaje sama przy pierwszym starciu (nic nie instalujemy). Dapper + jawne skrypty SQL w `db/*.sql`. Każde zapytanie przez istniejącą klasę `SqlRunner` (loguje SQL; na każdym połączeniu rejestruje funkcję `plain()` – wyszukiwanie bez rozróżniania wielkości liter i ogonków). Parametry zawsze przez Dappera (`@q`), nigdy sklejanie tekstu SQL. Liczby z ułamkiem i daty zapisuj do bazy jako tekst, dokładnie w formacie podanym w prompcie.
- CSV: pakiet CsvHelper (jest w projekcie), `CultureInfo("pl-PL")` albo kultura z promptu.
- Logowanie: ASP.NET Core Identity (już skonfigurowane). Token antiforgery wysyła `<body hx-headers:inherited=...>` w `_Layout.cshtml`.
- Rejestracja Twoich klas i zadania przy starcie: `src/GzmApp/AppServices.cs` (`AddAppServices`, `RunAtStartupAsync`).

## Zakazy
- React, Vue, Vite, npm, Node.js, bundler, `fetch` + JSON zamiast HTMX, biblioteka Htmx.Net, CDN (unpkg, jsdelivr), inne biblioteki CSS niż Tabler.
- Klucz AI w kodzie albo w `appsettings.json`, własny klient AI (`OpenAIClient`, `HttpClient` do modelu) zamiast `IChatClient` z DI.
- Entity Framework Core poza folderem `src/GzmApp/Identity/`.
- Zmiany logowania (Identity, cookie, antiforgery), dodawanie pakietów NuGet, zmiana portu, edycja `launch.json`, `launchSettings.json`, `*.csproj`, `Program.cs`.
- `htmx.logAll()`, `float`/`double` dla liczb z danych (używaj `decimal`), `DateTime.Parse` bez kultury i formatu.
- Ręczne wpisywanie scenariusza: klucz `"Scenario"` w `appsettings.json` ustawia wyłącznie skrypt z kroku 0. Ty go nie zmieniasz.

## Kod
- Guard clauses na początku publicznych metod (`ArgumentNullException.ThrowIfNull`, `ArgumentException.ThrowIfNullOrWhiteSpace`).
- Zero zagnieżdżonych if-ów i pętli: wczesny `return`, LINQ, małe metody. Metody do 20 linii, klasy do 150 linii.
- Nazwy w kodzie po angielsku, komunikaty dla użytkownika po polsku.

## Specyfikacja i testy – najpierw zdania, potem kod
- Każdy krok ma plik specyfikacji `scenarios/sNN/spec/sNN-NNN-*.md` (krok 0 też: `sNN-000-dane.md`). Przeczytaj go, zanim napiszesz kod: „Co się dzieje” opisuje krok, a „Reguły” mówią, co dokładnie ma działać.
- Każda ponumerowana reguła to jeden test w `tests/GzmApp.Specs` (nazwa testu = zdanie reguły). Krok jest gotowy, gdy wszystkie jego testy są zielone.
- Zrób to, co mówią reguły i prompt – nie mniej i nie więcej.
- Nie zmieniaj specyfikacji, testów ani narzędzi (`scenarios/`, `tests/`, `tools/`). Czerwony test naprawiasz w kodzie aplikacji, nigdy w teście.
- Sprawdzenie kroku: `dotnet test tests/GzmApp.Specs --filter Capability=<capability z nagłówka pliku>`. Komunikat czerwonego testu mówi po polsku, czego brakuje.

## Decyzje pary – proponuj i pytaj
- Gdy para prosi o propozycje decyzji („Zaproponuj nam … warianty decyzji N”), zaproponuj dokładnie 3 różne zdania według wzorców z bloku wklejonego przez parę – podstaw pola z kolumn danych i uwzględnij wcześniejsze decyzje z `LOG-DECYZJI.md`.
- Przedstaw propozycje krótko po polsku, powiedz, co każda daje, **zapytaj, którą wybierają, i czekaj na odpowiedź**. Nie pisz kodu, dopóki para nie wybierze.
- Po wyborze przypomnij parze, żeby wybrane zdanie wkleiła do pliku specyfikacji (para to robi ręcznie, nie Ty), i dopisz decyzję do `LOG-DECYZJI.md`.

## Pliki
- Zmieniaj tylko pliki z listy „Nie zmieniaj plików poza: …” w prompcie. Jeśli potrzebny jest inny plik – zatrzymaj się i zapytaj.
