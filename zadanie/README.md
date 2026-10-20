# Warsztat „AI + otwarte dane GZM” – zadanie dla Claude Code

## Jak używać
1. Rozpakuj zawartość do folderu `zadanie/` w repozytorium GitHub. Projekt warsztatu powstanie w katalogu głównym repo, obok tego folderu.
2. Uruchom Claude Code na tym repo i napisz:
   `Przeczytaj zadanie/CLAUDE.md i wykonaj następny krok`
3. Potem wystarczy pisać **„dalej”**. Claude Code czyta `docs/STATUS.md`, wybiera pierwszy niewykonany prompt i wykonuje dokładnie jeden. Jeśli następny prompt wymaga nowej sesji, Claude Code o tym przypomni (`/clear`).
4. Możesz też wskazać krok wprost: `wykonaj 07`.

## Kolejność i podział na sesje
Nowa sesja to `/clear` albo nowe okno Claude Code. Dzięki temu kontekst nie puchnie.

| Sesja | Prompty | Co robi |
|---|---|---|
| A | 01 → 02 | repo startowe, pobranie 15 CSV + rezerwowych, metadane |
| B | 03 → 04 | mapowania, wartości kontrolne ze skryptu, karty i prompty uczniów |
| C | 05 → 06 → 07 | gałęzie ratunkowe etap-1..4, zadanie * SOL-6, ratunek.ps1 |
| D | 08 → 09 | reguły Kilo Code, check.ps1 |
| E | 10 → 11 → 12 | materiały do druku, instrukcja prowadzącego, smoke test |
| F | 13 → 14 → 15 | infrastruktura testów, testy danych i importu |
| G | 16 → 17 | testy endpointu, widoku HTMX i ulubionych |
| H | 18 → 19 | Playwright, gałęzie ratunkowe |
| I | 20 | przejście promptami ucznia: s06 (najtrudniejszy) |
| J | 21 | przejście promptami ucznia: s04 (typowy) |
| K | 22 | przejście promptami ucznia: s02 (najprostszy) + propagacja poprawek |
| L | 23 → 24 → 25 | testy statyczne, materiały i SOL-6, raport końcowy |

Minimum przed warsztatem: A–E, czyli prompty 01–12.

## Uwaga: Claude Code w chmurze (Linux)
Bez LocalDB kroki bazodanowe zostaną oznaczone jako `DO WYKONANIA NA WINDOWS`, chyba że ustawisz `GZM_CONNSTR` albo dostępny jest Docker. Szczegóły są w `CLAUDE.md`, sekcja Środowisko.

## Zmienne środowiskowe (opcjonalne)
- `GZM_CONNSTR`, `GZM_TEST_CONNSTR`: baza poza Windows.
- `GLM_ENDPOINT`, `GLM_API_KEY`: w promptach 20–22 pozwalają sprawdzić prompty uczniów na prawdziwym GLM 5.3 Flash.
- Sekret `Sol6:ApiKey` (`dotnet user-secrets`): prawdziwe wywołanie SOL-6 w promptach 07, 09 i 24.
