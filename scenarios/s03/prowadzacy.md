# RowerStat (s03) – materiał dla prowadzącego

Nie rozdawaj uczniom. Uczniowie mają `karta.md` i `prompty.md`.

- **Pomysł:** Ile rowerów miejskich jest dostępnych w każdej gminie GZM i gdzie jest ich za mało.
- **Dane:** „Dzienna dostępność rowerów w gminach”, plik `data/raw/s03_rowery-dostepnosc-gminy.csv`, tabela `BikeAvailability`.

## Co powinno wyjść przy wariantach A

Gałęzie `ratunek/etap-1..4` realizują warianty A wszystkich decyzji i dają dokładnie te wartości. Przy innych wariantach (B, C) liczby z kroków 2–4 będą inne – to w porządku.

- Krok 1: wierszy w tabeli **40** (E1.rows) (w pliku **40** (E1.fileRows)); wiersz 7, pole `AvailableBikes`: **400** (E1.row7.value); rekordy z pustym albo nieczytelnym polem: **0** (E1.weird).
- Krok 2: fraza **śląski** (E2.phrase) daje **3** (E2.count) wyników:
   - Nr 6: **Chełm Śląski** (E2.result.1)
   - Nr 20: **Piekary Śląskie** (E2.result.2)
   - Nr 27: **Siemianowice Śląskie** (E2.result.3)
- Krok 3: kafelek – suma pola `AvailableBikes`.
   - **7449** (E3.value)
- Krok 4: tytuł rekordu z wiersza 7 (np. do sprawdzenia „Moich ulubionych”): **Chorzów** (E4.title).

Pełna lista wartości: `checkpoints.md` (generuje `tools/Expect`).

## Pułapki w danych

Co może pójść nie tak w tym zbiorze i jak to szybko zobaczyć.

### Krok 1 – import

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Kodowanie UTF-8 | Plik jest dobry, ale zapisany ponownie w Excelu dostanie Windows-1250 i ogonki się zepsują. Nie zapisuj go w Excelu. | rozszerzenie mssql: `SELECT TOP 5 * FROM BikeAvailability` |
| `Przekroczono` zawiera słowa „Tak”/„Nie” | `bool.Parse` ich nie przyjmie – zna tylko true/false. | breakpoint w imporcie + Watch na wartości kolumny |
| Wszystkie wiersze mają ten sam dzień | Klucz to para Day + Commune. Sama data nie jest unikalna – import z kluczem Day przerwie się błędem. | log SQL: komunikat o naruszeniu klucza UNIQUE |

### Krok 2 – wyszukiwarka

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Fraza z ogonkami: „śląski” | Przy złym kodowaniu albo kulturze wyników jest 0. W adresie fraza jest zakodowana (`%C5%...`). | Network: adres żądania; log SQL: wartość parametru `q` |
| Wielkość liter i ogonki | Porównanie `=` albo LIKE z collation `CS`/`AS` nie znajdzie „ŚLĄSKI”. | log SQL: czy zapytanie ma `COLLATE ..._CI_AI` |
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
