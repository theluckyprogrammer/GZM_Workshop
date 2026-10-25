# StacjaFinder (s02) – materiał dla prowadzącego

Nie rozdawaj uczniom. Uczniowie mają `karta.md`, `prompty.md` i specyfikację `spec/` (4 pliki, po jednym na krok, oraz `spec/dodatkowe/` z ulubionymi do „Co dalej?”).

Każde zdanie z sekcji „Reguły” w `spec/` to test w `tests/GzmApp.Specs` (generator `tools/Spectree.Generator`, kroki w `tests/GzmApp.Specs/Steps`). Testy liczą wartości oczekiwane z bazy i z `checkpoints.md`, więc działają dla każdego wariantu decyzji. Testy biorą scenariusz z `"Scenario"` w `src/GzmApp/appsettings.json` (po `ratunek.ps1` – z `appsettings.Development.json`); po zmianie trzeba przebudować projekt.

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
- Krok 4: karta „✨ Analiza AI” z odpowiedzią modelu SOL-6 (treść za każdym razem trochę inna – nie ma wartości kontrolnej). Wariant A zadania: Wskaż stacje z największą i najmniejszą liczbą stojaków i zaproponuj, gdzie warto dostawić rowery. Bez pliku `src/GzmApp/appsettings.Local.json` karta pokazuje „Brak klucza SOL-6 – poproś prowadzącego.”.
- Dodatkowo (ulubione): tytuł rekordu z wiersza 7 (np. do sprawdzenia „Moich ulubionych”): **27007** (E4.title).

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

### Krok 4 – analiza AI

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Brak pliku `src/GzmApp/appsettings.Local.json` (klucz SOL-6) | Karta pokazuje „Brak klucza SOL-6 – poproś prowadzącego.”, testy przechodzą (używają udawanego modelu). | terminal: wpis kategorii `AI`; skopiuj plik z pendrive'a i uruchom aplikację ponownie |
| Model odpowiada kilka sekund | Bez wskaźnika „AI analizuje…” uczniowie klikają kilka razy – każde kliknięcie to osobne zapytanie (i tokeny). | terminal: wpisy `SOL-6 (…): wysyłam…` i `odpowiedź po … s` |
| Model coś zmyśla albo myli numery rekordów | To nie błąd kodu – tak działa AI. Porównajcie odpowiedź z listą wyników („Co poszło do modelu?”). | karta analizy: rozwijane „Co poszło do modelu?” |
| Odpowiedź modelu wstawiona jako HTML (`@Html.Raw`) | Model może odesłać znaczniki HTML i zepsuć stronę – reguła 7 jest wtedy czerwona. | F12 → Elements: karta `#analiza` |

### Dodatkowo – ulubione

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Dwa kliknięcia ★ w dwóch kartach | Bez klucza `PK_Favorites` w tabeli są dwa takie same wiersze. | rozszerzenie mssql: `SELECT COUNT(*) FROM Favorites` |
| Brak tokenu antiforgery | Żądanie POST/DELETE dostaje status 400, a handler się nie uruchamia. | Network: nagłówek `RequestVerificationToken` |
| Kolumna „Nazwa” zawiera numer stacji (np. 27007), a nie nazwę | Tytuł ulubionego wygląda jak liczba. To nie błąd importu. | rozszerzenie mssql: `SELECT TOP 5 Name, Number FROM BikeStations` |
