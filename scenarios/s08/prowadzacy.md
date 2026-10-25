# RezerwacjaRadar (s08) – materiał dla prowadzącego

Nie rozdawaj uczniom. Uczniowie mają `karta.md`, `prompty.md` i specyfikację `spec/` (4 pliki, po jednym na krok).

Każde zdanie z sekcji „Reguły” w `spec/` to test w `tests/GzmApp.Specs` (generator `tools/Spectree.Generator`, kroki w `tests/GzmApp.Specs/Steps`). Testy liczą wartości oczekiwane z bazy i z `checkpoints.md`, więc działają dla każdego wariantu decyzji. Testy biorą scenariusz z `"Scenario"` w `src/GzmApp/appsettings.json` (po `ratunek.ps1` – z `appsettings.Development.json`); po zmianie trzeba przebudować projekt.

- **Pomysł:** Ostatnie rezerwacje Metroroweru: które kończą się wypożyczeniem, a które wygasają albo są anulowane.
- **Dane:** „Metrorower: rezerwacje”, plik `data/raw/s08_metrorower-rezerwacje.csv`, tabela `BikeReservations`.

## Co powinno wyjść przy wariantach A

Gałęzie `ratunek/etap-1..4` realizują warianty A wszystkich decyzji i dają dokładnie te wartości. Przy innych wariantach (B, C) liczby z kroków 2–4 będą inne – to w porządku.

- Krok 1: wierszy w tabeli **98** (E1.rows) (w pliku **100** (E1.fileRows)); wiersz 7, pole `ReservedAt`: **2026-10-06 14:55:37** (E1.row7.value); rekordy z pustym albo nieczytelnym polem: **12** (E1.weird).
- Krok 2: fraza **27402** (E2.phrase) daje **5** (E2.count) wyników:
   - Nr 67: **27402** (E2.result.1)
   - Nr 72: **27402** (E2.result.2)
   - Nr 73: **27402** (E2.result.3)
   - Nr 86: **27402** (E2.result.4)
   - Nr 97: **27402** (E2.result.5)
- Krok 3: kafelek – liczba rekordów dla każdej wartości pola `Status`, od największej (puste pomijamy).
   - CLOSED: **54** (E3.group.1)
   - CANCELLED: **26** (E3.group.2)
   - EXPIRED: **18** (E3.group.3)
- Krok 4: tytuł rekordu z wiersza 7 (np. do sprawdzenia „Moich ulubionych”): **27681** (E4.title).

Pełna lista wartości: `checkpoints.md` (generuje `tools/Expect`).

## Pułapki w danych

Co może pójść nie tak w tym zbiorze i jak to szybko zobaczyć.

### Krok 1 – import

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Kodowanie UTF-8 | Plik jest dobry, ale zapisany ponownie w Excelu dostanie Windows-1250 i ogonki się zepsują. Nie zapisuj go w Excelu. | rozszerzenie mssql: `SELECT TOP 5 * FROM BikeReservations` |
| Identyczne wiersze w pliku (drugie z par: 63, 74) | Drugi taki sam wiersz łamie unikalny `RowHash` i import przerywa się błędem. | komunikat błędu w terminalu + log SQL |
| Czas w `Data` jest w UTC (litera Z) | Godziny są o 1–2 h wcześniejsze niż czas polski. Nie przeliczaj – tak jest w danych. | breakpoint w imporcie + Watch na wartości `DateTime` |
| Czas w `Czas wygaśnięcia` jest w UTC (litera Z) | Godziny są o 1–2 h wcześniejsze niż czas polski. Nie przeliczaj – tak jest w danych. | breakpoint w imporcie + Watch na wartości `DateTime` |
| Puste pola w `Numer stacji rezerwacji`: 12 | Muszą trafić do bazy jako NULL, nie jako pusty tekst ani 0. | rozszerzenie mssql: `SELECT COUNT(*) FROM BikeReservations WHERE StationNumber IS NULL` |

### Krok 2 – wyszukiwarka

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Wielkość liter i ogonki | Porównanie `=` albo LIKE z collation `CS`/`AS` nie znajdzie „27402”. | log SQL: czy zapytanie ma `COLLATE ..._CI_AI` |
| Fraza `' OR 1=1 --` | Sklejany tekst SQL zwróci wszystkie rekordy. Z parametrem `@q` wyników jest 0. | Network + log SQL |
| „Nazwa stacji” to zwykle numer (np. 27402), czasem „BIKE 582980” | Wyszukiwarka szuka w tekście – fraza z cyframi jest tu normalna. | Network: odpowiedź `?handler=Szukaj` |

### Krok 3 – strona i statystyka

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Statystyka liczona z listy wyników | Lista pokazuje najwyżej 20 rekordów. Statystyka ma objąć całą tabelę – licz ją w SQL. | log SQL: zapytanie statystyki bez `TOP 20` |
| Wielkość liter i spacje w grupach | SQL (collation bez wielkości liter) łączy „Abc” i „abc” w jedną grupę, a `GroupBy` w C# nie. Licz w SQL. | Watch na liście grup + log SQL |

### Krok 4 – ulubione

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Dwa kliknięcia ★ w dwóch kartach | Bez klucza `PK_Favorites` w tabeli są dwa takie same wiersze. | rozszerzenie mssql: `SELECT COUNT(*) FROM Favorites` |
| Brak tokenu antiforgery | Żądanie POST/DELETE dostaje status 400, a handler się nie uruchamia. | Network: nagłówek `RequestVerificationToken` |
