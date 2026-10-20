# 04 – Przygotowanie: karty scenariuszy i prompty krok po kroku

Przeczytaj `zadanie/prompts/00-KONTEKST-wspolny.md` (sekcje: Projekt, Stos, Oszczędzanie tokenów, Status).

- **Wymaga:** 03 OK.
- **Sesja:** można kontynuować po 03.

## Zadanie
Materiały generuje skrypt `tools/Cards` z szablonów `tools/Cards/templates/` i tekstów `tools/Cards/scenarios.json`. Nie pisz 15 kopii ręcznie. Dla każdego `scenarios/sNN/` powstają:

1. `karta.md` (dla uczniów):
   - nazwa aplikacji, jednozdaniowy pomysł, co znaczą „ulubione”, atrybucja danych,
   - jak pracuje para: Pilot wkleja prompty, Nawigator prowadzi pauzy; role zmieniają się co krok.

   Nazwy aplikacji (dostosuj, jeśli w `docs/ZAMIANY.md` jest zamiana zbioru):

   | Scenariusz | Nazwa | Scenariusz | Nazwa | Scenariusz | Nazwa |
   |---|---|---|---|---|---|
   | s01 | RowerRadar | s06 | DrzewoRekord | s11 | PamięćKatowic |
   | s02 | StacjaFinder | s07 | SiewierzGo | s12 | ZabytkiBytom |
   | s03 | RowerStat | s08 | VictoriaVibe | s13 | SekcjaSCK |
   | s04 | ZgubaBytom | s09 | POK Planer | s14 | BiletBot |
   | s05 | ZgubaDąbrowa | s10 | BoiskoMap | s15 | KinoTeatr GL |

2. `prompty.md` (dla uczniów) – jeden plik, kroki po kolei, które stawiają całą aplikację:
   - **Krok 1** dane → baza: gotowy `CREATE TABLE` z `mapping.json`, import przez Dapper i CsvHelper; kolumny, separator, kultura i formaty dat wypisane wprost.
   - **Krok 2** endpoint z filtrowaniem: fragment HTML, parametr `@q`.
   - **Krok 3** widok HTMX 4: `hx-get`, `hx-trigger="keyup changed delay:300ms"`, `hx-target`, lista i kafelek statystyki.
   - **Krok 4** logowanie i ulubione: token antiforgery przez `hx-headers:inherited`.
   - Każdy prompt to blok do skopiowania i kończy się zdaniem: „Nie zmieniaj plików poza: <lista>. Nie dodawaj pakietów.”
   - Po każdym kroku pauza „zobaczcie, co powstało”: co otworzyć (mssql, przeglądarka, zakładka Network) i co tam widać. Bez porównywania liczb, bez sprawdzania wiedzy.
   - Pauzy z decyzjami: 1) pole wyszukiwania, 2) pola na liście, 3) kafelek statystyki, 4) widok „Moje ulubione”. Każda decyzja ma 2–3 warianty dopasowane do zbioru; każdy wariant to gotowa linia do wklejenia w miejsce `[DECYZJA n]` w następnym prompcie. Wariant A = `mapping.json` (bez wklejenia obowiązuje A).
   - Na końcu „Co dalej?”: pomysły do samodzielnej zabawy, w tym opis rekordu przez SOL-6 (prompt na start: `IChatClient`, fragment HTML przez `hx-get`, brak klucza daje komunikat, nie wyjątek).
   - Język prosty, dla ucznia, który nie programuje.
3. `prowadzacy.md` (dla prowadzącego, nie dla uczniów):
   - wartości z `checkpoints.md` dla wariantów A (to samo dają gałęzie ratunkowe),
   - pułapki w danych dla każdego kroku, wynikające z `data/metadata.json` (kodowanie, przecinek dziesiętny, formaty dat, puste pola, wielkość liter, ogonki), z narzędziem, którym je szybko widać.

## Kryterium ukończenia
- 15 folderów, każdy z kompletem: `karta.md`, `prompty.md`, `prowadzacy.md`.
- Żaden prompt nie zawiera pustych pól; materiały uczniów nie zawierają wartości oczekiwanych.
- Wartości w `prowadzacy.md` zgadzają się z `checkpoints.md`. Sprawdź to skryptem: `dotnet run --project tools/Cards -- check`.

## Na koniec
Commit `"04: karty i prompty uczniów"` i wiersz w `docs/STATUS.md`.
