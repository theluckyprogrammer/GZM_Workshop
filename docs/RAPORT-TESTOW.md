# Raport testów – warsztat „AI + otwarte dane GZM” (stan 21.10.2026)

Pełny zestaw na `ratunek/etap-4` (bez Playwright, bez prawdziwego SOL-6): 509 testów, 508 zielonych, 0 czerwonych, 1 pominięty (AI-04 – brak klucza SOL-6), 38 s.

| Poziom | Zakres | Status |
|---|---|---|
| 0–4 | środowisko, dane, import, endpoint, widok HTMX i ulubione | OK, 15/15 scenariuszy |
| 5 / 6 | przeglądarka (s02, s04, s06; tu pominięta) / gałęzie ratunkowe etap-1…4 | OK / OK |
| 7 | przejście promptami ucznia: s06, s04, s02 (`docs/WALIDACJA.md`) | OK |
| 8 / 9 | statyczne (kod, Kilo, sekrety, licencje) / materiały i SOL-6 | OK (2 wyjątki licencyjne) / OK (AI-04 pominięte, brak klucza) |

**Poprawki:**
- `src/GzmApp/Data/GzmConnection.cs` – connection string z konfiguracji, żeby testy podały własną bazę.
- `src/GzmApp/Data/Import/SqlType.cs`, `tools/Expect`, `tools/Fetch`, `tools/Cards` – 9 naruszeń reguł kodu (długie metody, brak guard clause) → 0.
- `src/GzmApp/Ai/AiSummarizer.cs` – opcjonalny `ChatClientFactory`, żeby test mógł podstawić fałszywy model.
- `tools/ratunek.ps1` – wypisuje drogę powrotu do zmian pary.
- `tools/Cards/Variables.cs`, `Decisions.cs` – MERGE po kluczu i czas polski w wariancie 4B.

**Prompty uczniów** (szablon, 15 scenariuszy, 13 zmian w `docs/WALIDACJA.md`): ścieżka `db/*.sql` z `AppContext.BaseDirectory`, NULL tylko w kolumnach, które mogą być puste, `MERGE` po kluczu, log „Import pominięty”, `take` = 20 i licznik wszystkich wyników, `IsHtmx()` w `OnGetSzukaj`, liczby w pl-PL, widok `_Gwiazdka.cshtml`, stan ☆/★ w `_Lista` przez `@inject` zamiast `ViewData`, uwagi w pauzach o encjach i adresie fragmentu.

**Top 5 z `findings.md`:**
1. Stan ★ przez `ViewData` ginie w `Partial()`, więc w HTMX ★ wraca do „niedodane” (2 z 3 przejść). Prompt kroku 4 zabrania tego teraz wprost.
2. Razor zapisuje polskie litery jako encje (`&#x144;`). Naprawa wymaga `Program.cs` (`WebEncoderOptions`) – decyzja prowadzącego.
3. `Microsoft.Data.SqlClient.SNI.runtime` i `Microsoft.Identity.Client.NativeInterop` mają licencję Microsoft, nie MIT/Apache, a wymaga ich SqlClient – decyzja prowadzącego.
4. Drugi, niezależny parser CSV daje te same wartości kontrolne we wszystkich 15 scenariuszach (0 rozbieżności).
5. `ratunek.ps1` zmienia `appsettings.Development.json` – skrypt mówi teraz, jak wrócić do zmian pary.

**Czasy etapów** (symulacja, model + pauza, s): s06 – 29+14, 20+12, 34+9, 74+28; s04 – 43+19, 23+11, 26+11, 64+50; s02 – 58+17, 31+11, 25+23, 70+59.

Plan 6 min na etap jest realny, jeśli `glm-5.3-flash` w Kilo jest najwyżej ~3× wolniejszy od symulacji i para nie musi poprawiać błędów. Najbardziej ryzykowny jest krok 4 (najwięcej plików); w razie problemów zostaje `ratunek.ps1 -Etap 4`.

## Czynności ręczne przed 28.10.2026
1. `tools\check.cmd` na fizycznym komputerze szkolnym (LocalDB, VS Code z rozszerzeniami, Windows PowerShell 5.1).
2. Test reguł Kilo według `docs/KILO-TEST.md` (zakazy z `kilo.jsonc`, reguły HTMX 4) z prawdziwym modelem.
3. Test kluczy przy 15 równoczesnych sesjach Kilo (limity, opóźnienia).
4. AI-04 z kluczem SOL-6: `dotnet test tests/GzmApp.Tests --filter "Spec=AI-04"` na `ratunek/etap-4`.
5. Wydruk 15 kart `print/karta-pary-sNN.html` i `print/sciaga-prowadzacego.html` (po 2 strony A4).
6. PR `claude/admiring-franklin-wpdxux` → `main`, potem `git fetch origin "ratunek/*:ratunek/*"` na stanowiskach.
7. Decyzje: encje w `Program.cs` i wyjątki licencyjne (punkty 2–3 wyżej).
