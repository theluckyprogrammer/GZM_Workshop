# PamięćZabrze (s01) – materiał dla prowadzącego

Nie rozdawaj uczniom. Uczniowie mają `karta.md` i `prompty.md`.

- **Pomysł:** Wyszukiwarka tablic i miejsc pamięci w Zabrzu z informacją, w której dzielnicy jest ich najwięcej.
- **Dane:** „Zabrze: miejsca pamięci”, plik `data/raw/s01_zabrze-miejsca-pamieci.csv`, tabela `ZabrzeMemorials`.

## Co powinno wyjść przy wariantach A

Gałęzie `ratunek/etap-1..4` realizują warianty A wszystkich decyzji i dają dokładnie te wartości. Przy innych wariantach (B, C) liczby z kroków 2–4 będą inne – to w porządku.

- Krok 1: wierszy w tabeli **58** (E1.rows) (w pliku **58** (E1.fileRows)); wiersz 7, pole `City`: **Zabrze** (E1.row7.value); rekordy z pustym albo nieczytelnym polem: **20** (E1.weird).
- Krok 2: fraza **którym** (E2.phrase) daje **5** (E2.count) wyników:
   - Nr 1: **Szkoła Podstawowa Nr 26** (E2.result.1)
   - Nr 2: **ul. Kasprowicza i róg Hagera** (E2.result.2)
   - Nr 8: **róg ul. Wolności** (E2.result.3)
   - Nr 29: **NULL** (E2.result.4)
   - Nr 32: **Budynek Teatru Nowego** (E2.result.5)
- Krok 3: kafelek – 5 najczęstszych wartości pola `City` z liczbą rekordów (puste pomijamy).
   - Zabrze: **37** (E3.group.1)
   - Zabrze – Biskupice: **3** (E3.group.2)
   - Zabrze – Kończyce: **2** (E3.group.3)
   - Zabrze-Mikulczyce: **2** (E3.group.4)
   - Zabrze-Rokitnica: **2** (E3.group.5)
- Krok 4: tytuł rekordu z wiersza 7 (np. do sprawdzenia „Moich ulubionych”): **budynek MPWiK** (E4.title).

Pełna lista wartości: `checkpoints.md` (generuje `tools/Expect`).

## Pułapki w danych

Co może pójść nie tak w tym zbiorze i jak to szybko zobaczyć.

### Krok 1 – import

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Kodowanie UTF-8 | Plik jest dobry, ale zapisany ponownie w Excelu dostanie Windows-1250 i ogonki się zepsują. Nie zapisuj go w Excelu. | rozszerzenie mssql: `SELECT TOP 5 * FROM ZabrzeMemorials` |
| Kolumny, których nie importujemy: 18 | Nie dodawaj dla nich pól – tabela jest gotowa w prompcie. | rozszerzenie mssql: lista kolumn tabeli |
| Puste pola w `addresscity`: 6 | Muszą trafić do bazy jako NULL, nie jako pusty tekst ani 0. | rozszerzenie mssql: `SELECT COUNT(*) FROM ZabrzeMemorials WHERE City IS NULL` |
| Puste pola w `addressstreetName`: 6 | Muszą trafić do bazy jako NULL, nie jako pusty tekst ani 0. | rozszerzenie mssql: `SELECT COUNT(*) FROM ZabrzeMemorials WHERE Street IS NULL` |
| Puste pola w `inscription`: 3 | Muszą trafić do bazy jako NULL, nie jako pusty tekst ani 0. | rozszerzenie mssql: `SELECT COUNT(*) FROM ZabrzeMemorials WHERE Inscription IS NULL` |
| Puste pola w `locationName`: 11 | Muszą trafić do bazy jako NULL, nie jako pusty tekst ani 0. | rozszerzenie mssql: `SELECT COUNT(*) FROM ZabrzeMemorials WHERE LocationName IS NULL` |

### Krok 2 – wyszukiwarka

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Fraza z ogonkami: „którym” | Przy złym kodowaniu albo kulturze wyników jest 0. W adresie fraza jest zakodowana (`%C5%...`). | Network: adres żądania; log SQL: wartość parametru `q` |
| Wielkość liter i ogonki | Porównanie `=` albo LIKE z collation `CS`/`AS` nie znajdzie „KTÓRYM”. | log SQL: czy zapytanie ma `COLLATE ..._CI_AI` |
| Fraza `' OR 1=1 --` | Sklejany tekst SQL zwróci wszystkie rekordy. Z parametrem `@q` wyników jest 0. | Network + log SQL |

### Krok 3 – strona i statystyka

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Statystyka liczona z listy wyników | Lista pokazuje najwyżej 20 rekordów. Statystyka ma objąć całą tabelę – licz ją w SQL. | log SQL: zapytanie statystyki bez `TOP 20` |
| Wielkość liter i spacje w grupach | SQL (collation bez wielkości liter) łączy „Abc” i „abc” w jedną grupę, a `GroupBy` w C# nie. Licz w SQL. | Watch na liście grup + log SQL |
| Puste wartości w polu `City`: 6 | NULL nie wchodzi do statystyki. Zamiana pustych na 0 albo „” da inny wynik niż na karcie. | rozszerzenie mssql: `SELECT COUNT(*) FROM ZabrzeMemorials WHERE City IS NULL` |
| Ta sama dzielnica zapisana różnie: „Zabrze-Biskupice”, „Zabrze – Biskupice”, „Zabrze - Mikulczyce” | W top 5 to osobne grupy. Tak jest w danych – nie poprawiaj ich. | rozszerzenie mssql: `SELECT DISTINCT City FROM ZabrzeMemorials` |

### Krok 4 – ulubione

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Rekordy bez tytułu (`LocationName` puste: 11) | Na liście ulubionych pojawi się pusta pozycja. Pokaż np. „(bez nazwy)”. | rozszerzenie mssql: `SELECT COUNT(*) FROM ZabrzeMemorials WHERE LocationName IS NULL` |
| Dwa kliknięcia ★ w dwóch kartach | Bez klucza `PK_Favorites` w tabeli są dwa takie same wiersze. | rozszerzenie mssql: `SELECT COUNT(*) FROM Favorites` |
| Brak tokenu antiforgery | Żądanie POST/DELETE dostaje status 400, a handler się nie uruchamia. | Network: nagłówek `RequestVerificationToken` |
