# ArcheoDąbrowa (s09) – materiał dla prowadzącego

Nie rozdawaj uczniom. Uczniowie mają `karta.md` i `prompty.md`.

- **Pomysł:** Stanowiska archeologiczne Dąbrowy Górniczej: co znaleziono i z jakiej epoki.
- **Dane:** „Dąbrowa Górnicza: zabytki archeologiczne”, plik `data/raw/s09_dabrowa-zabytki-archeologiczne.csv`, tabela `ArchaeologicalSites`.

## Co powinno wyjść przy wariantach A

Gałęzie `ratunek/etap-1..4` realizują warianty A wszystkich decyzji i dają dokładnie te wartości. Przy innych wariantach (B, C) liczby z kroków 2–4 będą inne – to w porządku.

- Krok 1: wierszy w tabeli **135** (E1.rows) (w pliku **135** (E1.fileRows)); wiersz 7, pole `Chronology`: **Wczesne średniowiecze - XI-XII/XIIIw.; Okres nowożytny - XVII-XIXw.** (E1.row7.value); rekordy z pustym albo nieczytelnym polem: **8** (E1.weird).
- Krok 2: fraza **gródki** (E2.phrase) daje **3** (E2.count) wyników:
   - Nr 69: **Ślad osadnictwa - Gródki; Ślad osadnictwa - Gródki; Ślad osadnictwa - Gródki** (E2.result.1)
   - Nr 86: **Ślad osadnictwa - Gródki** (E2.result.2)
   - Nr 96: **Osada - Gródki** (E2.result.3)
- Krok 3: kafelek – liczba rekordów dla każdej wartości pola `AzpArea`, od największej (puste pomijamy).
   - 95-50: **61** (E3.group.1)
   - 96-50: **21** (E3.group.2)
   - 96-51: **21** (E3.group.3)
   - 97-50: **14** (E3.group.4)
   - 96-52: **12** (E3.group.5)
   - 97-51: **5** (E3.group.6)
   - 96-49: **1** (E3.group.7)
- Krok 4: tytuł rekordu z wiersza 7 (np. do sprawdzenia „Moich ulubionych”): **Osada; Osada** (E4.title).

Pełna lista wartości: `checkpoints.md` (generuje `tools/Expect`).

## Pułapki w danych

Co może pójść nie tak w tym zbiorze i jak to szybko zobaczyć.

### Krok 1 – import

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Kodowanie UTF-8 | Plik jest dobry, ale zapisany ponownie w Excelu dostanie Windows-1250 i ogonki się zepsują. Nie zapisuj go w Excelu. | rozszerzenie mssql: `SELECT TOP 5 * FROM ArchaeologicalSites` |
| Kolumny, których nie importujemy: 11 | Nie dodawaj dla nich pól – tabela jest gotowa w prompcie. | rozszerzenie mssql: lista kolumn tabeli |
| Puste pola w `chronology`: 4 | Muszą trafić do bazy jako NULL, nie jako pusty tekst ani 0. | rozszerzenie mssql: `SELECT COUNT(*) FROM ArchaeologicalSites WHERE Chronology IS NULL` |
| Puste pola w `comment`: 8 | Muszą trafić do bazy jako NULL, nie jako pusty tekst ani 0. | rozszerzenie mssql: `SELECT COUNT(*) FROM ArchaeologicalSites WHERE Comment IS NULL` |
| Pole `comment` ma kilka linii w cudzysłowie | Czytanie pliku linia po linii (`ReadAllLines` + `Split`) rozbije rekord na kilka. CsvHelper czyta go poprawnie. | breakpoint + Watch na liczbie odczytanych wierszy (ma być E1.fileRows) |

### Krok 2 – wyszukiwarka

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Fraza z ogonkami: „gródki” | Przy złym kodowaniu albo kulturze wyników jest 0. W adresie fraza jest zakodowana (`%C5%...`). | Network: adres żądania; log SQL: wartość parametru `q` |
| Wielkość liter i ogonki | Porównanie `=` albo LIKE z collation `CS`/`AS` nie znajdzie „GRÓDKI”. | log SQL: czy zapytanie ma `COLLATE ..._CI_AI` |
| Fraza `' OR 1=1 --` | Sklejany tekst SQL zwróci wszystkie rekordy. Z parametrem `@q` wyników jest 0. | Network + log SQL |

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
