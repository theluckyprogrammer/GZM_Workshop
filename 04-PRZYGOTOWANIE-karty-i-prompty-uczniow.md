# 04 – Przygotowanie: karty scenariuszy, prompty uczniów, pułapki

Przeczytaj `zadanie/prompts/00-KONTEKST-wspolny.md` (sekcje: Projekt, Stos, Oszczędzanie tokenów, Status).

- **Wymaga:** 03 OK.
- **Sesja:** można kontynuować po 03.

## Zadanie
Dla każdego `scenarios/sNN/` utwórz:

1. `karta.md`:
   - nazwa aplikacji,
   - jednozdaniowy pomysł,
   - co znaczą „ulubione”,
   - atrybucja danych.

   Nazwy aplikacji (dostosuj, jeśli w `docs/ZAMIANY.md` jest zamiana zbioru):

   | Scenariusz | Nazwa | Scenariusz | Nazwa | Scenariusz | Nazwa |
   |---|---|---|---|---|---|
   | s01 | RowerRadar | s06 | DrzewoRekord | s11 | PamięćKatowic |
   | s02 | StacjaFinder | s07 | SiewierzGo | s12 | ZabytkiBytom |
   | s03 | RowerStat | s08 | VictoriaVibe | s13 | SekcjaSCK |
   | s04 | ZgubaBytom | s09 | POK Planer | s14 | BiletBot |
   | s05 | ZgubaDąbrowa | s10 | BoiskoMap | s15 | KinoTeatr GL |

2. `prompty/etap1.md` … `prompty/etap4.md`. Szablon: CEL / DANE / ZASADY / GOTOWE GDY. Wymagania:
   - Pola są już wypełnione pod ten zbiór: nazwy kolumn dokładnie jak w pliku, separator, kultura, format daty, nazwa tabeli, pole filtra, statystyka.
   - Etap 1 zawiera gotowe `CREATE TABLE` z `mapping.json`. Uczeń prosi tylko o import przez Dapper.
   - Etapy 2 i 3 wymagają odpowiedzi w formie fragmentu HTML i HTMX 4 (`hx-get`, `hx-trigger="keyup changed delay:300ms"`, `hx-target`).
   - Etap 4 wymaga logowania i tokenu antiforgery przez `hx-headers:inherited`.
   - Każdy prompt kończy się zdaniem: „Nie zmieniaj plików poza: <lista>. Nie dodawaj pakietów.”
   - Sekcja GOTOWE GDY wskazuje, jakim narzędziem para to sprawdzi i z którą wartością z `checkpoints.md` porówna wynik.
   - Język prosty, dla ucznia, który nie programuje.
3. `prompty/detektyw.md`. Szablon naprawy błędu: „Oczekiwałem X (z karty). Debugger pokazuje Y w zmiennej Z w linii N. Network pokazuje status S. Nie zmieniaj niczego poza metodą M. Najpierw wyjaśnij przyczynę w 2 zdaniach, potem popraw.”
4. `prompty/gwiazdka.md` (zadanie * z SOL-6). Jedno wywołanie `IChatClient` na danych z tego zbioru, wynik jako fragment HTML przez `hx-get`. Treść zadania dobierz do zbioru, np.:
   - „podsumuj 5 najbliższych wydarzeń”,
   - „opisz w jednym zdaniu ten zabytek, nie wymyślaj faktów spoza rekordu”.

   Brak klucza ma dawać komunikat, nie wyjątek.
5. `pulapki.md`. Pułapki dla każdego etapu, wynikające z `data/metadata.json` (kodowanie, przecinek dziesiętny, formaty dat, puste pola, wielkość liter, ogonki). Przy każdej pułapce: narzędzie, którym para ją wykryje.

## Kryterium ukończenia
- 15 folderów, każdy z kompletem: `karta.md`, 4 prompty etapów, `detektyw.md`, `gwiazdka.md`, `pulapki.md`.
- Żaden prompt nie zawiera pustych pól.
- Wartości z sekcji GOTOWE GDY zgadzają się z `checkpoints.md`. Sprawdź to skryptem, nie wzrokiem.

## Na koniec
Commit `"04: karty i prompty uczniów"` i wiersz w `docs/STATUS.md`.
