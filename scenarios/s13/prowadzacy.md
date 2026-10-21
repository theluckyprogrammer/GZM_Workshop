# SekcjaSCK (s13) – materiał dla prowadzącego

Nie rozdawaj uczniom. Uczniowie mają `karta.md` i `prompty.md`.

- **Pomysł:** Zajęcia w Siemianowickim Centrum Kultury: znajdź sekcję dla siebie albo rodzeństwa.
- **Dane:** „SCK Siemianowice: zajęcia sekcyjne”, plik `data/raw/s13_sck-zajecia-sekcyjne.csv`, tabela `CultureClasses`.

## Co powinno wyjść przy wariantach A

Gałęzie `ratunek/etap-1..4` realizują warianty A wszystkich decyzji i dają dokładnie te wartości. Przy innych wariantach (B, C) liczby z kroków 2–4 będą inne – to w porządku.

- Krok 1: wierszy w tabeli **38** (E1.rows) (w pliku **38** (E1.fileRows)); wiersz 7, pole `MinAge`: **5.0** (E1.row7.value); rekordy z pustym albo nieczytelnym polem: **23** (E1.weird).
- Krok 2: fraza **dziecięcy** (E2.phrase) daje **4** (E2.count) wyników:
   - Nr 13: **Dziecięcy ZPiT Siemianowice** (E2.result.1)
   - Nr 14: **Dziecięcy ZPiT Siemianowice** (E2.result.2)
   - Nr 15: **Dziecięcy ZPiT Siemianowice** (E2.result.3)
   - Nr 16: **Dziecięcy ZPiT Siemianowice** (E2.result.4)
- Krok 3: kafelek – liczba rekordów dla każdej wartości pola `ClassType`, od największej (puste pomijamy).
   - taneczne: **15** (E3.group.1)
   - muzyczne: **6** (E3.group.2)
   - rękodzieło: **6** (E3.group.3)
   - teatralne: **5** (E3.group.4)
   - plastyczne: **2** (E3.group.5)
   - taneczne, wokalne: **2** (E3.group.6)
   - Indywidualne lekcje śpiewu: **1** (E3.group.7)
   - wokalne: **1** (E3.group.8)
- Krok 4: tytuł rekordu z wiersza 7 (np. do sprawdzenia „Moich ulubionych”): **Pracownia Plastyczna „Okienko”** (E4.title).

Pełna lista wartości: `checkpoints.md` (generuje `tools/Expect`).

## Pułapki w danych

Co może pójść nie tak w tym zbiorze i jak to szybko zobaczyć.

### Krok 1 – import

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Kodowanie UTF-8 | Plik jest dobry, ale zapisany ponownie w Excelu dostanie Windows-1250 i ogonki się zepsują. Nie zapisuj go w Excelu. | rozszerzenie mssql: `SELECT TOP 5 * FROM CultureClasses` |
| Kolumny, których nie importujemy: 4 | Nie dodawaj dla nich pól – tabela jest gotowa w prompcie. | rozszerzenie mssql: lista kolumn tabeli |
| Kropka dziesiętna w `minimalAge` | Z kulturą pl-PL (przecinek) liczba się nie odczyta albo straci kropkę. | breakpoint w imporcie + Watch na wartości po odczycie |
| Dwa zapisy w `startHour`: `HH:mm` i `HH.mm` | Jeden wzorzec odrzuci część wartości – wzrośnie liczba „dziwnych” rekordów. | log importu: liczba „dziwnych” vs wartość E1.weird |
| Puste pola w `day`: 4 | Muszą trafić do bazy jako NULL, nie jako pusty tekst ani 0. | rozszerzenie mssql: `SELECT COUNT(*) FROM CultureClasses WHERE Day IS NULL` |
| Puste pola w `minimalAge`: 12 | Muszą trafić do bazy jako NULL, nie jako pusty tekst ani 0. | rozszerzenie mssql: `SELECT COUNT(*) FROM CultureClasses WHERE MinAge IS NULL` |
| Puste pola w `room`: 5 | Muszą trafić do bazy jako NULL, nie jako pusty tekst ani 0. | rozszerzenie mssql: `SELECT COUNT(*) FROM CultureClasses WHERE Room IS NULL` |
| Puste pola w `startHour`: 10 | Muszą trafić do bazy jako NULL, nie jako pusty tekst ani 0. | rozszerzenie mssql: `SELECT COUNT(*) FROM CultureClasses WHERE StartHour IS NULL` |
| Wiek „0.5” w `minimalAge` (pół roku) | Typ int go odrzuci – pole `MinAge` ma typ `decimal(4,1)`. | breakpoint + Watch na `minimalAge` w wierszu 1 |

### Krok 2 – wyszukiwarka

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Fraza z ogonkami: „dziecięcy” | Przy złym kodowaniu albo kulturze wyników jest 0. W adresie fraza jest zakodowana (`%C5%...`). | Network: adres żądania; log SQL: wartość parametru `q` |
| Wielkość liter i ogonki | Porównanie `=` albo LIKE z collation `CS`/`AS` nie znajdzie „DZIECIĘCY”. | log SQL: czy zapytanie ma `COLLATE ..._CI_AI` |
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
