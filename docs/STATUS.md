# Status

| NN | operacja | status | data | uwaga |
|---|---|---|---|---|
| 01 | repo startowe | OK | 2026-10-21 | Zweryfikowano na Linuksie z SQL Server 2022 (Docker, GZM_CONNSTR); LocalDB do sprawdzenia na Windows w kroku 12. |
| 02 | dane | OK | 2026-10-21 | 15/15 scenariuszy; zamiany jak w poprzednim przebiegu (R2, R4, R5, R6, R7, szczegóły w docs/ZAMIANY.md); pliki identyczne z pobraniem z 20.10. |
| 03 | mapowania i wartości kontrolne | OK | 2026-10-21 | 15 × mapping.json i checkpoints.md z tools/Expect (426 wartości, wszystkie w docs/expect-log.txt); ponowne uruchomienie daje identyczne pliki. |
| 04 | karty i prompty uczniów | OK | 2026-10-21 | karta.md + prompty.md (4 kroki, pauzy, decyzje A/B/C, „Co dalej?”) + prowadzacy.md z tools/Cards, `-- check` bez błędów; do GzmApp dodano CsvHelper i pakiety AI. |
| 05 | ratunek etap-1: generyczny import | OK | 2026-10-21 | 15/15 scenariuszy: liczba wierszy = E1.rows i „dziwnych” = E1.weird, drugi import bez zmian; db/sNN_create.sql = CREATE TABLE z prompty.md (15/15). |
| 06 | ratunek etap-2 i etap-3: endpoint i widok | OK | 2026-10-21 | 15/15 scenariuszy przez HTTP: E2 i E3 = checkpoints.md, fragment HTMX bez `<html>`, `' OR 1=1 --` → 0, fraza > 100 znaków → 400, błąd 500 w HTMX → fragment. |
| 07 | ratunek etap-4: ulubione, SOL-6, ratunek.ps1 | OK | 2026-10-21 | 15/15 przez HTTP: /Ulubione bez logowania → logowanie, bez tokenu 400, z tokenem 200, 2× dodanie = 1 wpis, 2× usunięcie bez błędu, B nie widzi A, SOL-6 bez klucza → komunikat; ratunek.ps1 s02 etapy 1–4 OK. |
| 08 | reguły Kilo | OK | 2026-10-21 | kilo.jsonc (poprawny JSONC), .kilo/rules/stack.md i htmx4.md, kopie w .kilocode/rules i AGENTS.md, docs/KILO-TEST.md; prompty uczniów zmieniają AppServices.cs zamiast Program.cs. |
| 09 | check.ps1 | OK | 2026-10-21 | tools/check.ps1 + check.cmd: składnia OK, przebieg pwsh 7 na Linuksie (LocalDB pominięte, brak VS Code w kontenerze); pełny test na komputerze szkolnym (Windows PowerShell 5.1). |
| 10 | materiały do druku | OK | 2026-10-21 | 15 kart par + ściąga prowadzącego z tools/Cards; ściąga = checkpoints.md (skrypt), karty bez wartości oczekiwanych, HTML poprawny, wydruk w Chromium: 2 strony A4. |
| 11 | instrukcja prowadzącego | OK | 2026-10-21 | docs/PROWADZACY.md: przygotowanie, checklista 15 stanowisk, przebieg 30 min, teksty, problemy, klucze, atrybucja; ścieżki i polecenia sprawdzone skryptem. |
| 12 | smoke test | OK | 2026-10-21 | s02 w czystym klonie: main → ratunek etap 1 (E1) → etap 3 (fragment, E3) → etap 4 (★ → 1 wiersz w Favorites) → main z czystym git status; ratunek.ps1 podaje teraz drogę powrotu. |
| 13 | środowisko i infrastruktura testów | OK | 2026-10-21 | tests/GzmApp.Tests (xUnit, Mvc.Testing, AngleSharp): DbFixture/AppFactory/ScenarioData/helpery, test dymny D-00 + INF-01 zielone na wszystkich gałęziach; docs/SPEC-TESTY.md i TEST-STATUS.md; build całego rozwiązania bez ostrzeżeń. |
| 14 | dane i wartości kontrolne | OK | 2026-10-21 | Poziom 1: 61 testów zielonych (D-01–D-05); drugi parser bez CsvHelpera daje te same E1–E4 co tools/Expect – 0 rozbieżności; kontrola testu: celowo zmieniona wartość i format są wyłapane. |
| 15 | testy importu | OK | 2026-10-21 | Poziom 2 na ratunek/etap-1: 106 testów zielonych (E1-01–E1-08, 15 scenariuszy; wartości decimal i dat porównane we wszystkich wierszach); zmergowane w górę, zielone też na etap-4. |
| 16 | testy endpointu z filtrowaniem | OK | 2026-10-21 | Poziom 3 na ratunek/etap-2: 90 testów zielonych (E2-01–E2-06, 15 scenariuszy, HTTP przez WebApplicationFactory, log SQL przechwycony); zmergowane w górę. |
| 17 | testy widoku HTMX i ulubionych | OK | 2026-10-21 | Poziom 4: E3-01–E3-07 na ratunek/etap-3 (91 testów), E4-01–E4-07 na ratunek/etap-4 (razem 196 testów poziomu 4), 15 scenariuszy, HTTP + AngleSharp. |
| 18 | testy w przeglądarce (Playwright) | OK | 2026-10-21 | Poziom 5 na ratunek/etap-4: 15 testów zielonych (UI-01–UI-05 × s06, s04, s02), Chromium headless z /opt/pw-browsers, aplikacja na Kestrelu z bazą GzmTest_{scenariusz}_ui. |
| 19 | testy gałęzi ratunkowych | OK | 2026-10-21 | 4 przebiegi (etap-1: 169, etap-2: 259, etap-3: 350, etap-4: 470 testów) zielone; ratunek.ps1: schowek odzyskiwalny, -Etap 3 s06 importuje i jest idempotentny; gałęzie spójne (is-ancestor). |
| 23 | testy statyczne: kod, Kilo, sekrety, licencje | OK | 2026-10-21 | tools/CodeRules (Roslyn): 9 naruszeń poprawionych, 0 na etap-4; kilo.jsonc OK; 0 sekretów w historii; licencje: 2 przechodnie pakiety Microsoft (SNI, NativeInterop) uzasadnione w findings. |

## Gałęzie do utworzenia lub zmergowania ręcznie

Środowisko Claude Code w chmurze pushuje na gałąź sesji `claude/admiring-franklin-wpdxux`; `main` aktualizuje się przez PR.

- `main`: reset jest już w `main` (PR #6). Zmerguj `claude/admiring-franklin-wpdxux` (nowy PR) z commitami od „01: repo startowe”.
- `ratunek/etap-1`: zbudowana od nowa w kroku 05 na stanie po „04: karty i prompty uczniów” i wypchnięta (fast-forward starej gałęzi).
- `ratunek/etap-2`, `ratunek/etap-3`: utworzone w kroku 06 i wypchnięte.
- `ratunek/etap-4`: utworzona w kroku 07 i wypchnięta; `tools/ratunek.ps1` jest na wszystkich gałęziach.
