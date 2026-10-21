# StacjaFinder (s02) – materiał dla prowadzącego

Nie rozdawaj uczniom. Uczniowie mają `karta.md` i `prompty.md`.

- **Pomysł:** Znajdź stację Metroroweru w swojej gminie i sprawdź, ile ma stojaków.
- **Dane:** „Metrorower: stacje aktualne”, plik `data/raw/s02_metrorower-stacje.csv`, tabela `BikeStations`.

## Co powinno wyjść przy wariantach A

Gałęzie `ratunek/etap-1..4` realizują warianty A wszystkich decyzji i dają dokładnie te wartości. Przy innych wariantach (B, C) liczby z kroków 2–4 będą inne – to w porządku.

- Krok 1: wierszy w tabeli **945** (E1.rows) (w pliku **945** (E1.fileRows)); wiersz 7, pole `Lat`: **50.275952000** (E1.row7.value); rekordy z pustym albo nieczytelnym polem: **0** (E1.weird).
- Krok 2: fraza **bieruń** (E2.phrase) daje **6** (E2.count) wyników:
   - Nr 25: **27025** (E2.result.1)
   - Nr 231: **27231** (E2.result.2)
   - Nr 254: **27254** (E2.result.3)
   - Nr 259: **27259** (E2.result.4)
   - Nr 543: **27544** (E2.result.5)
   - Nr 902: **27909** (E2.result.6)
- Krok 3: kafelek – suma pola `Racks`.
   - **4750** (E3.value)
- Krok 4: tytuł rekordu z wiersza 7 (np. do sprawdzenia „Moich ulubionych”): **27007** (E4.title).

Pełna lista wartości: `checkpoints.md` (generuje `tools/Expect`).

## Pułapki w danych

Co może pójść nie tak w tym zbiorze i jak to szybko zobaczyć.

### Krok 1 – import

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Kodowanie UTF-8 | Plik jest dobry, ale zapisany ponownie w Excelu dostanie Windows-1250 i ogonki się zepsują. Nie zapisuj go w Excelu. | rozszerzenie mssql: `SELECT TOP 5 * FROM BikeStations` |
| Kolumny, których nie importujemy: 1 | Nie dodawaj dla nich pól – tabela jest gotowa w prompcie. | rozszerzenie mssql: lista kolumn tabeli |
| `Dostępna` zawiera słowa „Tak”/„Nie” | `bool.Parse` ich nie przyjmie – zna tylko true/false. | breakpoint w imporcie + Watch na wartości kolumny |
| Kropka dziesiętna w `Lat` | Z kulturą pl-PL (przecinek) liczba się nie odczyta albo straci kropkę. | breakpoint w imporcie + Watch na wartości po odczycie |
| Kropka dziesiętna w `Lng` | Z kulturą pl-PL (przecinek) liczba się nie odczyta albo straci kropkę. | breakpoint w imporcie + Watch na wartości po odczycie |

### Krok 2 – wyszukiwarka

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Fraza z ogonkami: „bieruń” | Przy złym kodowaniu albo kulturze wyników jest 0. W adresie fraza jest zakodowana (`%C5%...`). | Network: adres żądania; log SQL: wartość parametru `q` |
| Wielkość liter i ogonki | Porównanie `=` albo LIKE z collation `CS`/`AS` nie znajdzie „BIERUŃ”. | log SQL: czy zapytanie ma `COLLATE ..._CI_AI` |
| Fraza `' OR 1=1 --` | Sklejany tekst SQL zwróci wszystkie rekordy. Z parametrem `@q` wyników jest 0. | Network + log SQL |

### Krok 3 – strona i statystyka

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Statystyka liczona z listy wyników | Lista pokazuje najwyżej 20 rekordów. Statystyka ma objąć całą tabelę – licz ją w SQL. | log SQL: zapytanie statystyki bez `TOP 20` |

### Krok 4 – ulubione

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Dwa kliknięcia ★ w dwóch kartach | Bez klucza `PK_Favorites` w tabeli są dwa takie same wiersze. | rozszerzenie mssql: `SELECT COUNT(*) FROM Favorites` |
| Brak tokenu antiforgery | Żądanie POST/DELETE dostaje status 400, a handler się nie uruchamia. | Network: nagłówek `RequestVerificationToken` |
| Kolumna „Nazwa” zawiera numer stacji (np. 27007), a nie nazwę | Tytuł ulubionego wygląda jak liczba. To nie błąd importu. | rozszerzenie mssql: `SELECT TOP 5 Name, Number FROM BikeStations` |
