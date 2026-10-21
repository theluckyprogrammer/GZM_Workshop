# Status testów: poziom × scenariusz

`OK` – zielone, `BŁĄD` – czerwone, `—` – jeszcze nie uruchomione, `n/d` – poziom nie dotyczy scenariusza. Szczegóły wymagań: `docs/SPEC-TESTY.md`.

| Poziom | Zakres | s01 | s02 | s03 | s04 | s05 | s06 | s07 | s08 | s09 | s10 | s11 | s12 | s13 | s14 | s15 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 0 | środowisko, infrastruktura (13) | OK | OK | OK | OK | OK | OK | OK | OK | OK | OK | OK | OK | OK | OK | OK |
| 1 | dane i wartości kontrolne (14) | OK | OK | OK | OK | OK | OK | OK | OK | OK | OK | OK | OK | OK | OK | OK |
| 2 | import (15) | OK | OK | OK | OK | OK | OK | OK | OK | OK | OK | OK | OK | OK | OK | OK |
| 3 | endpoint z filtrowaniem (16) | OK | OK | OK | OK | OK | OK | OK | OK | OK | OK | OK | OK | OK | OK | OK |
| 4 | widok HTMX i ulubione (17) | OK | OK | OK | OK | OK | OK | OK | OK | OK | OK | OK | OK | OK | OK | OK |
| 5 | przeglądarka, Playwright (18) | n/d | OK | n/d | OK | n/d | OK | n/d | n/d | n/d | n/d | n/d | n/d | n/d | n/d | n/d |
| 6 | gałęzie ratunkowe (19) | OK | OK | OK | OK | OK | OK | OK | OK | OK | OK | OK | OK | OK | OK | OK |
| 7 | przejście promptami (20–22) | n/d | OK | n/d | OK | n/d | OK | n/d | n/d | n/d | n/d | n/d | n/d | n/d | n/d | n/d |
| 8 | statyczne: kod, Kilo, sekrety, licencje (23) | n/d | n/d | n/d | n/d | n/d | n/d | n/d | n/d | n/d | n/d | n/d | n/d | n/d | n/d | n/d |
| 9 | materiały i SOL-6 (24) | OK | OK | OK | OK | OK | OK | OK | OK | OK | OK | OK | OK | OK | OK | OK |

## Gałęzie (krok 19)

| Gałąź | Poziomy | Testów | Wynik |
|---|---|---|---|
| `ratunek/etap-1` | 0–2 | 169 | OK |
| `ratunek/etap-2` | 0–3 | 259 | OK |
| `ratunek/etap-3` | 0–4 (E3) | 350 | OK |
| `ratunek/etap-4` | 0–5 | 470 | OK |

Poprawek w gałęziach nie było potrzeba.

Poziom 9: M-01–M-05 dla 15 scenariuszy; AI-01–AI-03 na s02 (fałszywy `IChatClient`), AI-04 pominięte bez klucza SOL-6.

Poziom 7: pełne przejście promptami tylko dla s06, s04 i s02 (prompt 22 nie każe przechodzić pozostałych); wspólne poprawki naniesione na 15 scenariuszy, szczegóły w `docs/WALIDACJA.md`.

Poziom 8 nie zależy od scenariusza (`n/d`): Q-01–Q-04 OK, szczegóły w `docs/SPEC-TESTY.md`.
