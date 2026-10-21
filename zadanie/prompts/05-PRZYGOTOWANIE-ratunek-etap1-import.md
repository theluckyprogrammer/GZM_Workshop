# 05 – Przygotowanie: gałąź ratunek/etap-1 (generyczny import)

Przeczytaj `zadanie/prompts/00-KONTEKST-wspolny.md` (sekcje: Stos, Zasady kodu, Oszczędzanie tokenów, Status, Gałęzie).

- **Wymaga:** 04 OK.
- **Sesja:** nowa.

## Zadanie
1. Utwórz gałąź `ratunek/etap-1` z `main`.
2. Zbuduj generyczny import sterowany plikiem `scenarios/<Scenario>/mapping.json`:
   - `ICsvImporter`, `CsvImporter`, `RecordTableBuilder` (generuje `CREATE TABLE` z mapowania i zapisuje go do `db/sNN_create.sql`),
   - tabela staging + `MERGE` w transakcji,
   - retry 3× dla przejściowych `SqlException`,
   - `SqlBulkCopy`, gdy wierszy jest więcej niż 5000,
   - kodowanie: BOM → ścisły UTF-8 → fallback Windows-1250,
   - pl-PL i formaty dat z mapowania.
2a. Tabela ma dokładnie postać `CREATE TABLE` z kroku 1 w `scenarios/<Scenario>/prompty.md` (`RecordId` IDENTITY, `SourceRow`, pola z mapowania, `UQ_<Tabela>_Key`).
3. Import uruchamia się przy starcie aplikacji, jeśli tabela scenariusza jest pusta, albo poleceniem `dotnet run -- import`. Na koniec loguje: liczbę wierszy, liczbę „dziwnych” rekordów, czas.
4. Każdy wiersz staging ma numer źródłowy (`SourceRow`). Dzięki temu „wiersz 7” z `prowadzacy.md` da się znaleźć w bazie.

## Kryterium ukończenia
- Dla każdego scenariusza (pętla w skrypcie, ustawienie `Scenario` przez zmienną `Scenario`) liczba wierszy w bazie = E1 z `checkpoints.md`.
- Drugi import nie zmienia liczby wierszy.
- `dotnet build` przechodzi bez ostrzeżeń.

## Na koniec
Commit na `ratunek/etap-1` i wiersz w `docs/STATUS.md`. `docs/STATUS.md` scal z powrotem do `main`: tylko ten plik, przez checkout pliku.
