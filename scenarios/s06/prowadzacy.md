# DrzewoRekord (s06) – materiał dla prowadzącego

Nie rozdawaj uczniom. Uczniowie mają `karta.md` i `prompty.md`.

- **Pomysł:** Pomniki przyrody Dąbrowy Górniczej: jakie to drzewa i jaki mają obwód.
- **Dane:** „Dąbrowa Górnicza: pomniki przyrody”, plik `data/raw/s06_dabrowa-pomniki-przyrody.csv`, tabela `NatureMonuments`.

## Co powinno wyjść przy wariantach A

Gałęzie `ratunek/etap-1..4` realizują warianty A wszystkich decyzji i dają dokładnie te wartości. Przy innych wariantach (B, C) liczby z kroków 2–4 będą inne – to w porządku.

- Krok 1: wierszy w tabeli **31** (E1.rows) (w pliku **31** (E1.fileRows)); wiersz 7, pole `Circumference`: **290** (E1.row7.value); rekordy z pustym albo nieczytelnym polem: **2** (E1.weird).
- Krok 2: fraza **wyniosły** (E2.phrase) daje **3** (E2.count) wyników:
   - Nr 5: **Jesion wyniosły (Fraxinus excelsior)** (E2.result.1)
   - Nr 14: **Jesion wyniosły (Fraxinus excelsior)** (E2.result.2)
   - Nr 28: **Jesion wyniosły (Fraxinus excelsior)** (E2.result.3)
- Krok 3: kafelek – średnia pola `Circumference` z 2 miejscami po przecinku (puste pomijamy).
   - **311.40** (E3.value)
- Krok 4: tytuł rekordu z wiersza 7 (np. do sprawdzenia „Moich ulubionych”): **Buk pospolity (Fagus sylvatica)** (E4.title).

Pełna lista wartości: `checkpoints.md` (generuje `tools/Expect`).

## Pułapki w danych

Co może pójść nie tak w tym zbiorze i jak to szybko zobaczyć.

### Krok 1 – import

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Kodowanie UTF-8 | Plik jest dobry, ale zapisany ponownie w Excelu dostanie Windows-1250 i ogonki się zepsują. Nie zapisuj go w Excelu. | rozszerzenie mssql: `SELECT TOP 5 * FROM NatureMonuments` |
| Kolumny, których nie importujemy: 2 | Nie dodawaj dla nich pól – tabela jest gotowa w prompcie. | rozszerzenie mssql: lista kolumn tabeli |
| Puste pola w `circumference`: 1 | Muszą trafić do bazy jako NULL, nie jako pusty tekst ani 0. | rozszerzenie mssql: `SELECT COUNT(*) FROM NatureMonuments WHERE Circumference IS NULL` |
| Puste pola w `coordinateData`: 1 | Muszą trafić do bazy jako NULL, nie jako pusty tekst ani 0. | rozszerzenie mssql: `SELECT COUNT(*) FROM NatureMonuments WHERE Coordinates IS NULL` |
| Współrzędne w `coordinateData` mają stopnie, minuty i cudzysłów („N50° 23' 29.9"”) | Cudzysłów w środku pola psuje dzielenie przez `Split`. Zostają tekstem. | breakpoint + Watch na polu `coordinateData` |

### Krok 2 – wyszukiwarka

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Fraza z ogonkami: „wyniosły” | Przy złym kodowaniu albo kulturze wyników jest 0. W adresie fraza jest zakodowana (`%C5%...`). | Network: adres żądania; log SQL: wartość parametru `q` |
| Wielkość liter i ogonki | Porównanie `=` albo LIKE z collation `CS`/`AS` nie znajdzie „WYNIOSŁY”. | log SQL: czy zapytanie ma `COLLATE ..._CI_AI` |
| Fraza `' OR 1=1 --` | Sklejany tekst SQL zwróci wszystkie rekordy. Z parametrem `@q` wyników jest 0. | Network + log SQL |
| Ten sam gatunek zapisany różnie: „Fagus silvatica” i „Fagus sylvatica” | Fraza „sylvatica” nie znajdzie wszystkich buków. | rozszerzenie mssql: `SELECT Name, COUNT(*) FROM NatureMonuments GROUP BY Name` |

### Krok 3 – strona i statystyka

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Statystyka liczona z listy wyników | Lista pokazuje najwyżej 20 rekordów. Statystyka ma objąć całą tabelę – licz ją w SQL. | log SQL: zapytanie statystyki bez `TOP 20` |
| AVG na kolumnie liczb całkowitych | SQL liczy wtedy średnią jako liczbę całkowitą i gubi część po przecinku. Rzutuj na `decimal`. | log SQL + wartość E3 |
| Puste wartości w polu `Circumference`: 1 | NULL nie wchodzi do statystyki. Zamiana pustych na 0 albo „” da inny wynik niż na karcie. | rozszerzenie mssql: `SELECT COUNT(*) FROM NatureMonuments WHERE Circumference IS NULL` |

### Krok 4 – ulubione

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Dwa kliknięcia ★ w dwóch kartach | Bez klucza `PK_Favorites` w tabeli są dwa takie same wiersze. | rozszerzenie mssql: `SELECT COUNT(*) FROM Favorites` |
| Brak tokenu antiforgery | Żądanie POST/DELETE dostaje status 400, a handler się nie uruchamia. | Network: nagłówek `RequestVerificationToken` |
