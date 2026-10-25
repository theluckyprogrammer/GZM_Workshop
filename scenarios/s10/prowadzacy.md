# BoiskoMap (s10) – materiał dla prowadzącego

Nie rozdawaj uczniom. Uczniowie mają `karta.md`, `prompty.md` i specyfikację `spec/` (4 pliki, po jednym na krok).

Każde zdanie z sekcji „Reguły” w `spec/` to test w `tests/GzmApp.Specs` (generator `tools/Spectree.Generator`, kroki w `tests/GzmApp.Specs/Steps`). Testy liczą wartości oczekiwane z bazy i z `checkpoints.md`, więc działają dla każdego wariantu decyzji. Testy biorą scenariusz z `"Scenario"` w `src/GzmApp/appsettings.json` (po `ratunek.ps1` – z `appsettings.Development.json`); po zmianie trzeba przebudować projekt.

- **Pomysł:** Boiska i stadiony w Gliwicach: znajdź boisko przy swojej szkole.
- **Dane:** „Gliwice: boiska i stadiony”, plik `data/raw/s10_gliwice-boiska-stadiony.csv`, tabela `SportsFields`.

## Co powinno wyjść przy wariantach A

Gałęzie `ratunek/etap-1..4` realizują warianty A wszystkich decyzji i dają dokładnie te wartości. Przy innych wariantach (B, C) liczby z kroków 2–4 będą inne – to w porządku.

- Krok 1: wierszy w tabeli **121** (E1.rows) (w pliku **121** (E1.fileRows)); wiersz 7, pole `X`: **2077273.6463570932** (E1.row7.value); rekordy z pustym albo nieczytelnym polem: **2** (E1.weird).
- Krok 2: fraza **śląska** (E2.phrase) daje **5** (E2.count) wyników:
   - Nr 63: **Politechnika Śląska** (E2.result.1)
   - Nr 101: **Politechnika Śląska** (E2.result.2)
   - Nr 102: **Politechnika Śląska** (E2.result.3)
   - Nr 103: **Politechnika Śląska** (E2.result.4)
   - Nr 105: **Politechnika Śląska** (E2.result.5)
- Krok 3: kafelek – 5 najczęstszych wartości pola `Kind` z liczbą rekordów (puste pomijamy).
   - boisko do piłki nożnej i koszykówki: **26** (E3.group.1)
   - boisko do piłki nożnej: **19** (E3.group.2)
   - boisko do koszykówki: **10** (E3.group.3)
   - boisko do koszykówki i siatkówki: **8** (E3.group.4)
   - boisko trawiaste do piłki nożnej: **8** (E3.group.5)
- Krok 4: tytuł rekordu z wiersza 7 (np. do sprawdzenia „Moich ulubionych”): **Szkoła Podstawowa Nr 6** (E4.title).

Pełna lista wartości: `checkpoints.md` (generuje `tools/Expect`).

## Pułapki w danych

Co może pójść nie tak w tym zbiorze i jak to szybko zobaczyć.

### Krok 1 – import

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Kodowanie UTF-8 | Plik jest dobry, ale zapisany ponownie w Excelu dostanie Windows-1250 i ogonki się zepsują. Nie zapisuj go w Excelu. | rozszerzenie mssql: `SELECT TOP 5 * FROM SportsFields` |
| Kolumny, których nie importujemy: 3 | Nie dodawaj dla nich pól – tabela jest gotowa w prompcie. | rozszerzenie mssql: lista kolumn tabeli |
| Kropka dziesiętna w `x` | Z kulturą pl-PL (przecinek) liczba się nie odczyta albo straci kropkę. | breakpoint w imporcie + Watch na wartości po odczycie |
| Kropka dziesiętna w `y` | Z kulturą pl-PL (przecinek) liczba się nie odczyta albo straci kropkę. | breakpoint w imporcie + Watch na wartości po odczycie |
| Puste pola w `NAZWA`: 2 | Muszą trafić do bazy jako NULL, nie jako pusty tekst ani 0. | rozszerzenie mssql: `SELECT COUNT(*) FROM SportsFields WHERE Name IS NULL` |
| `x` i `y` to metry w układzie PL-2000 (np. 2077206.03), a nie stopnie | Liczby mają 7 cyfr przed kropką i 10 po – typ `decimal(20,10)`, kultura `invariant`. | breakpoint + Watch na sparsowanej wartości `x` |

### Krok 2 – wyszukiwarka

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Fraza z ogonkami: „śląska” | Przy złym kodowaniu albo kulturze wyników jest 0. W adresie fraza jest zakodowana (`%C5%...`). | Network: adres żądania; log SQL: wartość parametru `q` |
| Wielkość liter i ogonki | Porównanie `=` albo LIKE z collation `CS`/`AS` nie znajdzie „ŚLĄSKA”. | log SQL: czy zapytanie ma `COLLATE ..._CI_AI` |
| Fraza `' OR 1=1 --` | Sklejany tekst SQL zwróci wszystkie rekordy. Z parametrem `@q` wyników jest 0. | Network + log SQL |
| Puste pole `Name` w 2 rekordach | Te rekordy nie pojawią się w wynikach wyszukiwania – to poprawne. | rozszerzenie mssql: `SELECT COUNT(*) FROM SportsFields WHERE Name IS NULL` |

### Krok 3 – strona i statystyka

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Statystyka liczona z listy wyników | Lista pokazuje najwyżej 20 rekordów. Statystyka ma objąć całą tabelę – licz ją w SQL. | log SQL: zapytanie statystyki bez `TOP 20` |
| Wielkość liter i spacje w grupach | SQL (collation bez wielkości liter) łączy „Abc” i „abc” w jedną grupę, a `GroupBy` w C# nie. Licz w SQL. | Watch na liście grup + log SQL |

### Krok 4 – ulubione

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Rekordy bez tytułu (`Name` puste: 2) | Na liście ulubionych pojawi się pusta pozycja. Pokaż np. „(bez nazwy)”. | rozszerzenie mssql: `SELECT COUNT(*) FROM SportsFields WHERE Name IS NULL` |
| Dwa kliknięcia ★ w dwóch kartach | Bez klucza `PK_Favorites` w tabeli są dwa takie same wiersze. | rozszerzenie mssql: `SELECT COUNT(*) FROM Favorites` |
| Brak tokenu antiforgery | Żądanie POST/DELETE dostaje status 400, a handler się nie uruchamia. | Network: nagłówek `RequestVerificationToken` |
