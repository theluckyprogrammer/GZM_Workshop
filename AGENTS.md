<!-- Kopia reguł z .kilo/rules/ dla narzędzi czytających AGENTS.md. Zmieniaj .kilo/rules/ i skopiuj tutaj. -->

# Stos projektu i zasady (warsztat „AI + otwarte dane GZM”)

Odpowiadaj po polsku, prostym językiem – pracujesz z uczniami, którzy nie programują. Na koniec napisz w 3 punktach, co zrobiłeś.

## Start portalu (krok 0)
- Portal pary powstaje w kroku 0: skrypt `tools/nowy-portal.ps1 -Scenariusz sNN` kopiuje szkielet (`szkielet/` → `src/`, `tests/`, `db/`), pobiera dane z portalu GZM i wpisuje scenariusz. Uruchamiasz go tylko wtedy, gdy poprosi o to prompt kroku 0; plików `szkielet/` nie zmieniasz.
- `LOG-DECYZJI.md` w głównym katalogu to log decyzji pary: po każdej decyzji dopisujesz jeden wiersz do tabeli (Krok | Pytanie | Wasza decyzja | Dlaczego). Niczego z niego nie usuwasz.

## Stos – używaj tylko tego
- .NET 10, jeden projekt ASP.NET Core Razor Pages: `src/GzmApp`. Port 5080.
- Widok: HTMX 4.0.0 z pliku lokalnego `wwwroot/lib/htmx/4.0.0/htmx.min.js` (już podpięty w `_Layout.cshtml`). Fragmenty HTML zwracają handlery Razor Pages (`Partial(...)`). Zasady HTMX 4: `.kilo/rules/htmx4.md`.
- Wygląd: biblioteka CSS Tabler 1.6.1 (klasy Bootstrap 5: `card`, `list-group`, `badge`, `btn`, `row`/`col-*`), plik lokalny już podpięty w `_Layout.cshtml`. Wszystkie teksty na stronie po polsku.
- AI: model GPT-6 Luna jest już podłączony w `Program.cs` – z DI bierzesz `IChatClient` (`Microsoft.Extensions.AI`) i `IOptions<Sol6Options>` (`GzmApp.Ai`). Klucz jest w `src/GzmApp/appsettings.Local.json` (od prowadzącego). Odpowiedź modelu pokazuj jako tekst (`@`), nigdy przez `Html.Raw`.
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

# HTMX 4 – różnice względem 2.x (używamy 4.0.0)

Źródło: https://four.htmx.org/docs/whats-new-in-htmx-4

1. **Dziedziczenie atrybutów jest jawne.** Atrybut na rodzicu działa na dzieci tylko z `:inherited`.
   - Źle: `<div hx-target="#wyniki"><input hx-get="?handler=Szukaj"></div>`
   - Dobrze: `<input hx-get="?handler=Szukaj" hx-target="#wyniki">` albo `<div hx-target:inherited="#wyniki">…</div>`
2. **Token antiforgery jest już na `<body>`** (`hx-headers:inherited='{"RequestVerificationToken":"…"}'` w `_Layout.cshtml`).
   - Źle: drugi `hx-headers` na przycisku albo wyłączenie walidacji (`[IgnoreAntiforgeryToken]`).
   - Dobrze: zwykły `hx-post` / `hx-delete` – token dojdzie sam.
3. **Odpowiedzi 4xx i 5xx są podmieniane w stronie** (nie podmienia się tylko 204 i 304).
   - Źle: zwrócić całą stronę błędu – wyląduje w środku listy.
   - Dobrze: krótki fragment z komunikatem i właściwym statusem, np. `<p class="alert alert-warning">Zaloguj się…</p>` ze statusem 401.
4. **Zdarzenia mają postać `htmx:faza:akcja`.**
   - Źle: `htmx:beforeRequest`, `htmx:afterSwap`, `htmx:responseError`
   - Dobrze: `htmx:before:request`, `htmx:after:swap`, `htmx:response:error`
5. **Logowanie:** `htmx.logAll()` nie istnieje.
   - Dobrze: `htmx.config.logAll = true;`
6. **Domyślny timeout to 60 s** (w 2.x nie było limitu).
7. **Selektory ze spacją lub przecinkiem w `hx-trigger ... from:` muszą być w apostrofach.**
   - Źle: `hx-trigger="keyup from:(closest form)"`
   - Dobrze: `hx-trigger="keyup from:'closest form'"`
8. **Nagłówki żądań:** `HX-Request` (zawsze `true`), `HX-Request-Type` (`full` / `partial`), `HX-Source` (zamiast `HX-Trigger`, format `tag#id`), `HX-Target`, `HX-Current-URL`. Rozpoznawaj żądanie HTMX po `HX-Request: true`.
9. **Usunięte:** `hx-vars` (zamiast tego `hx-vals` z `js:`), `hx-params`, `hx-ext`, `hx-disinherit`, `hx-inherit`, modyfikator `queue` w `hx-trigger` (zamiast tego `hx-sync`).
10. **Wyszukiwarka na żywo:** `hx-trigger="keyup changed delay:300ms"`.
11. **`hx-disable` zmienił znaczenie.** W 4.x `hx-disable="this"` blokuje przycisk na czas żądania (dawne `hx-disabled-elt`). Wyłączenie HTMX dla fragmentu strony to teraz `hx-ignore`.
   - Źle: usuwać `hx-disable="this"` z przycisku „Analizuj”, bo „wyłącza HTMX”.
   - Dobrze: zostawić – chroni przed podwójnym kliknięciem i drugim zapytaniem do AI.
