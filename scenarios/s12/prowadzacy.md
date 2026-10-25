# ZabytkiBytom (s12) – materiał dla prowadzącego

Nie rozdawaj uczniom. Uczniowie mają `karta.md`, `prompty.md` i specyfikację `spec/` (4 pliki, po jednym na krok, oraz `spec/dodatkowe/` z ulubionymi do „Co dalej?”).

Każde zdanie z sekcji „Reguły” w `spec/` to test w `tests/GzmApp.Specs` (generator `tools/Spectree.Generator`, kroki w `tests/GzmApp.Specs/Steps`). Testy liczą wartości oczekiwane z bazy i z `checkpoints.md`, więc działają dla każdego wariantu decyzji. Testy biorą scenariusz z `"Scenario"` w `src/GzmApp/appsettings.json` (po `ratunek.ps1` – z `appsettings.Development.json`); po zmianie trzeba przebudować projekt.

- **Pomysł:** Zabytki Bytomia: wyszukaj budynek i sprawdź jego styl.
- **Dane:** „Bytom: zabytki”, plik `data/raw/s12_bytom-zabytki.csv`, tabela `HistoricBuildings`.

## Co powinno wyjść przy wariantach A

Gałęzie `ratunek/etap-1..4` realizują warianty A wszystkich decyzji i dają dokładnie te wartości. Przy innych wariantach (B, C) liczby z kroków 2–4 będą inne – to w porządku.

- Krok 1: wierszy w tabeli **2313** (E1.rows) (w pliku **2313** (E1.fileRows)); wiersz 7, pole `Style`: **Modernizm** (E1.row7.value); rekordy z pustym albo nieczytelnym polem: **176** (E1.weird).
- Krok 2: fraza **garaż** (E2.phrase) daje **5** (E2.count) wyników:
   - Nr 319: **zespół domów w ogrodach, z garażami w głębi działek** (E2.result.1)
   - Nr 320: **zespół domów w ogrodach, z garażami  w głębi działek** (E2.result.2)
   - Nr 1248: **garaż drezyn z budynkiem socjalnym Zespołu Stacji Kolei Wąskotorowej Bytom-Karb** (E2.result.3)
   - Nr 1755: **willa i budynek garażowo-mieszkalny** (E2.result.4)
   - Nr 2234: **dom wielorodzinny, garaże** (E2.result.5)
- Krok 3: kafelek – 5 najczęstszych wartości pola `Style` z liczbą rekordów (puste pomijamy).
   - Modernizm: **512** (E3.group.1)
   - Funkcjonalizm: **225** (E3.group.2)
   - Historyzm: **199** (E3.group.3)
   - Historyzm ceglany prosty: **191** (E3.group.4)
   - Neorenesans: **153** (E3.group.5)
- Krok 4: karta „✨ Analiza AI” z odpowiedzią modelu AI GPT-6 Luna (treść za każdym razem trochę inna – nie ma wartości kontrolnej). Wariant A zadania: Policz zabytki w każdym stylu architektonicznym i wybierz 3 do zobaczenia na wycieczce. Bez pliku `src/GzmApp/appsettings.Local.json` karta pokazuje „Brak klucza do modelu AI – poproś prowadzącego.”.
- Dodatkowo (ulubione): tytuł rekordu z wiersza 7 (np. do sprawdzenia „Moich ulubionych”): **kamienica-specjalny ośrodek wychowawczy** (E4.title).

Pełna lista wartości: `checkpoints.md` (generuje `tools/Expect`).

## Pułapki w danych

Co może pójść nie tak w tym zbiorze i jak to szybko zobaczyć.

### Krok 1 – import

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Kodowanie UTF-8 | Plik jest dobry, ale zapisany ponownie w Excelu dostanie Windows-1250 i ogonki się zepsują. Nie zapisuj go w Excelu. | rozszerzenie mssql: `SELECT TOP 5 * FROM HistoricBuildings` |
| Kolumny, których nie importujemy: 25 | Nie dodawaj dla nich pól – tabela jest gotowa w prompcie. | rozszerzenie mssql: lista kolumn tabeli |
| Puste pola w `addressbuildingNumber`: 129 | Muszą trafić do bazy jako NULL, nie jako pusty tekst ani 0. | rozszerzenie mssql: `SELECT COUNT(*) FROM HistoricBuildings WHERE BuildingNumber IS NULL` |
| Puste pola w `addressstreetName`: 6 | Muszą trafić do bazy jako NULL, nie jako pusty tekst ani 0. | rozszerzenie mssql: `SELECT COUNT(*) FROM HistoricBuildings WHERE Street IS NULL` |
| Puste pola w `style`: 126 | Muszą trafić do bazy jako NULL, nie jako pusty tekst ani 0. | rozszerzenie mssql: `SELECT COUNT(*) FROM HistoricBuildings WHERE Style IS NULL` |

### Krok 2 – wyszukiwarka

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Fraza z ogonkami: „garaż” | Przy złym kodowaniu albo kulturze wyników jest 0. W adresie fraza jest zakodowana (`%C5%...`). | Network: adres żądania; log SQL: wartość parametru `q` |
| Wielkość liter i ogonki | Porównanie `=` albo LIKE z collation `CS`/`AS` nie znajdzie „GARAŻ”. | log SQL: czy zapytanie ma `COLLATE ..._CI_AI` |
| Fraza `' OR 1=1 --` | Sklejany tekst SQL zwróci wszystkie rekordy. Z parametrem `@q` wyników jest 0. | Network + log SQL |

### Krok 3 – strona i statystyka

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Statystyka liczona z listy wyników | Lista pokazuje najwyżej 20 rekordów. Statystyka ma objąć całą tabelę – licz ją w SQL. | log SQL: zapytanie statystyki bez `TOP 20` |
| Wielkość liter i spacje w grupach | SQL (collation bez wielkości liter) łączy „Abc” i „abc” w jedną grupę, a `GroupBy` w C# nie. Licz w SQL. | Watch na liście grup + log SQL |
| Puste wartości w polu `Style`: 126 | NULL nie wchodzi do statystyki. Zamiana pustych na 0 albo „” da inny wynik niż na karcie. | rozszerzenie mssql: `SELECT COUNT(*) FROM HistoricBuildings WHERE Style IS NULL` |
| Styl „Modernizm” i „modernizm” | SQL (collation bez wielkości liter) liczy je razem, LINQ w C# osobno – wynik top 5 będzie inny niż na karcie. | log SQL + Watch na liście grup |

### Krok 4 – analiza AI

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Brak pliku `src/GzmApp/appsettings.Local.json` (klucz do modelu AI) | Karta pokazuje „Brak klucza do modelu AI – poproś prowadzącego.”, reguła 9 kroku 4 (prawdziwy model) jest czerwona, reguły 1–8 przechodzą (udawany model). | terminal: wpis kategorii `AI`; skopiuj plik z pendrive'a i uruchom aplikację ponownie |
| Model odpowiada kilka sekund | Bez wskaźnika „AI analizuje…” uczniowie klikają kilka razy – każde kliknięcie to osobne zapytanie (i tokeny). | terminal: wpisy `AI (…): wysyłam…` i `odpowiedź po … s` |
| Model coś zmyśla albo myli numery rekordów | To nie błąd kodu – tak działa AI. Porównajcie odpowiedź z listą wyników („Co poszło do modelu?”). | karta analizy: rozwijane „Co poszło do modelu?” |
| Odpowiedź modelu wstawiona jako HTML (`@Html.Raw`) | Model może odesłać znaczniki HTML i zepsuć stronę – reguła 7 jest wtedy czerwona. | F12 → Elements: karta `#analiza` |

### Dodatkowo – ulubione

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Dwa kliknięcia ★ w dwóch kartach | Bez klucza `PK_Favorites` w tabeli są dwa takie same wiersze. | rozszerzenie mssql: `SELECT COUNT(*) FROM Favorites` |
| Brak tokenu antiforgery | Żądanie POST/DELETE dostaje status 400, a handler się nie uruchamia. | Network: nagłówek `RequestVerificationToken` |
