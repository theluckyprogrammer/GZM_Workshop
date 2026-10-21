# SportGliwice (s07) – materiał dla prowadzącego

Nie rozdawaj uczniom. Uczniowie mają `karta.md` i `prompty.md`.

- **Pomysł:** Obiekty sportowe w Gliwicach: hale, boiska, baseny – gdzie są i kto nimi zarządza.
- **Dane:** „Gliwice: obiekty sportowe”, plik `data/raw/s07_gliwice-obiekty-sportowe.csv`, tabela `SportsFacilities`.

## Co powinno wyjść przy wariantach A

Gałęzie `ratunek/etap-1..4` realizują warianty A wszystkich decyzji i dają dokładnie te wartości. Przy innych wariantach (B, C) liczby z kroków 2–4 będą inne – to w porządku.

- Krok 1: wierszy w tabeli **71** (E1.rows) (w pliku **71** (E1.fileRows)); wiersz 7, pole `AddressManager`: **ul. Okrzei 20 / GKS „PIAST” S.A.** (E1.row7.value); rekordy z pustym albo nieczytelnym polem: **0** (E1.weird).
- Krok 2: fraza **nawierzchnią** (E2.phrase) daje **3** (E2.count) wyników:
   - Nr 20: **Boisko wielofunkcyjne (ze sztuczną nawierzchnią)** (E2.result.1)
   - Nr 21: **Boisko wielofunkcyjne (ze sztuczną nawierzchnią)** (E2.result.2)
   - Nr 22: **Boisko wielofunkcyjne (ze sztuczną nawierzchnią)** (E2.result.3)
- Krok 3: kafelek – 5 najczęstszych wartości pola `Facility` z liczbą rekordów (puste pomijamy).
   - Hala sportowa: **11** (E3.group.1)
   - Boisko trawiaste: **5** (E3.group.2)
   - Korty tenisowe: **5** (E3.group.3)
   - Kryta pływalnia: **4** (E3.group.4)
   - Boisko wielofunkcyjne (ze sztuczną nawierzchnią): **3** (E3.group.5)
- Krok 4: tytuł rekordu z wiersza 7 (np. do sprawdzenia „Moich ulubionych”): **Boisko do minipiłki nożnej przy stadionie GKS „PIAST”** (E4.title).

Pełna lista wartości: `checkpoints.md` (generuje `tools/Expect`).

## Pułapki w danych

Co może pójść nie tak w tym zbiorze i jak to szybko zobaczyć.

### Krok 1 – import

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Kodowanie UTF-8 | Plik jest dobry, ale zapisany ponownie w Excelu dostanie Windows-1250 i ogonki się zepsują. Nie zapisuj go w Excelu. | rozszerzenie mssql: `SELECT TOP 5 * FROM SportsFacilities` |
| Nazwa kolumny `ADRES___ZARZĄDZAJĄCY` ma ogonek i trzy podkreślniki | Literówka w nazwie kolumny = pusta kolumna w bazie albo wyjątek CsvHelper. | breakpoint + Watch na nagłówkach pliku |

### Krok 2 – wyszukiwarka

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Fraza z ogonkami: „nawierzchnią” | Przy złym kodowaniu albo kulturze wyników jest 0. W adresie fraza jest zakodowana (`%C5%...`). | Network: adres żądania; log SQL: wartość parametru `q` |
| Wielkość liter i ogonki | Porównanie `=` albo LIKE z collation `CS`/`AS` nie znajdzie „NAWIERZCHNIĄ”. | log SQL: czy zapytanie ma `COLLATE ..._CI_AI` |
| Fraza `' OR 1=1 --` | Sklejany tekst SQL zwróci wszystkie rekordy. Z parametrem `@q` wyników jest 0. | Network + log SQL |

### Krok 3 – strona i statystyka

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Statystyka liczona z listy wyników | Lista pokazuje najwyżej 20 rekordów. Statystyka ma objąć całą tabelę – licz ją w SQL. | log SQL: zapytanie statystyki bez `TOP 20` |
| Wielkość liter i spacje w grupach | SQL (collation bez wielkości liter) łączy „Abc” i „abc” w jedną grupę, a `GroupBy` w C# nie. Licz w SQL. | Watch na liście grup + log SQL |
| Spacja na końcu nazwy obiektu („Boisko wielofunkcyjne ”) | Bez Trim ta sama nazwa tworzy dwie grupy w top 5. | rozszerzenie mssql: `SELECT Facility, LEN(Facility) FROM SportsFacilities` |

### Krok 4 – ulubione

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Dwa kliknięcia ★ w dwóch kartach | Bez klucza `PK_Favorites` w tabeli są dwa takie same wiersze. | rozszerzenie mssql: `SELECT COUNT(*) FROM Favorites` |
| Brak tokenu antiforgery | Żądanie POST/DELETE dostaje status 400, a handler się nie uruchamia. | Network: nagłówek `RequestVerificationToken` |
