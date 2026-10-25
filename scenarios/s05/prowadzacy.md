# ZgubaDąbrowa (s05) – materiał dla prowadzącego

Nie rozdawaj uczniom. Uczniowie mają `karta.md`, `prompty.md` i specyfikację `spec/` (4 pliki, po jednym na krok, oraz `spec/dodatkowe/` z ulubionymi do „Co dalej?”).

Każde zdanie z sekcji „Reguły” w `spec/` to test w `tests/GzmApp.Specs` (generator `tools/Spectree.Generator`, kroki w `tests/GzmApp.Specs/Steps`). Testy liczą wartości oczekiwane z bazy i z `checkpoints.md`, więc działają dla każdego wariantu decyzji. Testy biorą scenariusz z `"Scenario"` w `src/GzmApp/appsettings.json` (po `ratunek.ps1` – z `appsettings.Development.json`); po zmianie trzeba przebudować projekt.

- **Pomysł:** Rzeczy znalezione w Dąbrowie Górniczej: co znaleziono i do kiedy można to odebrać.
- **Dane:** „Dąbrowa Górnicza: rzeczy znalezione”, plik `data/raw/s05_dabrowa-rzeczy-znalezione.csv`, tabela `DabrowaLostItems`.

## Co powinno wyjść przy wariantach A

Gałęzie `ratunek/etap-1..4` realizują warianty A wszystkich decyzji i dają dokładnie te wartości. Przy innych wariantach (B, C) liczby z kroków 2–4 będą inne – to w porządku.

- Krok 1: wierszy w tabeli **64** (E1.rows) (w pliku **64** (E1.fileRows)); wiersz 7, pole `FoundDate`: **2024-11-21** (E1.row7.value); rekordy z pustym albo nieczytelnym polem: **0** (E1.weird).
- Krok 2: fraza **zawartością** (E2.phrase) daje **4** (E2.count) wyników:
   - Nr 5: **Torba z zawartością** (E2.result.1)
   - Nr 7: **Plecak z zawartością** (E2.result.2)
   - Nr 21: **Walizka z zawartością** (E2.result.3)
   - Nr 57: **Torebka z zawartością** (E2.result.4)
- Krok 3: kafelek – 5 najczęstszych wartości pola `ItemName` z liczbą rekordów (puste pomijamy).
   - Telefon komórkowy: **20** (E3.group.1)
   - Portfel: **5** (E3.group.2)
   - Pieniądze: **4** (E3.group.3)
   - Portfel + pieniądze: **3** (E3.group.4)
   - Klucze: **2** (E3.group.5)
- Krok 4: karta „✨ Analiza AI” z odpowiedzią modelu AI GPT-6 Luna (treść za każdym razem trochę inna – nie ma wartości kontrolnej). Wariant A zadania: Pogrupuj znalezione rzeczy w kategorie i wskaż te, których termin odbioru mija najwcześniej. Bez pliku `src/GzmApp/appsettings.Local.json` karta pokazuje „Brak klucza do modelu AI – poproś prowadzącego.”.
- Dodatkowo (ulubione): tytuł rekordu z wiersza 7 (np. do sprawdzenia „Moich ulubionych”): **Plecak z zawartością** (E4.title).

Pełna lista wartości: `checkpoints.md` (generuje `tools/Expect`).

## Pułapki w danych

Co może pójść nie tak w tym zbiorze i jak to szybko zobaczyć.

### Krok 1 – import

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Kodowanie UTF-8 | Plik jest dobry, ale zapisany ponownie w Excelu dostanie Windows-1250 i ogonki się zepsują. Nie zapisuj go w Excelu. | rozszerzenie mssql: `SELECT TOP 5 * FROM DabrowaLostItems` |
| Kolumny, których nie importujemy: 11 | Nie dodawaj dla nich pól – tabela jest gotowa w prompcie. | rozszerzenie mssql: lista kolumn tabeli |
| Daty w `collectionDate` jako milisekundy od 1970 r. | Bez przeliczenia w bazie będzie liczba albo błąd typu. Przeliczaj w UTC, inaczej data przesunie się o dzień. | rozszerzenie mssql: wiersz 7 i wartość E1.row7.value |
| Daty w `foundDate` jako milisekundy od 1970 r. | Bez przeliczenia w bazie będzie liczba albo błąd typu. Przeliczaj w UTC, inaczej data przesunie się o dzień. | rozszerzenie mssql: wiersz 7 i wartość E1.row7.value |
| Daty odbioru (`collectionDate`) są w przyszłości | To nie błąd przeliczenia – to termin, do którego można odebrać rzecz. | rozszerzenie mssql: `SELECT TOP 5 FoundDate, CollectionDate FROM DabrowaLostItems` |

### Krok 2 – wyszukiwarka

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Fraza z ogonkami: „zawartością” | Przy złym kodowaniu albo kulturze wyników jest 0. W adresie fraza jest zakodowana (`%C5%...`). | Network: adres żądania; log SQL: wartość parametru `q` |
| Wielkość liter i ogonki | Porównanie `=` albo LIKE z collation `CS`/`AS` nie znajdzie „ZAWARTOŚCIĄ”. | log SQL: czy zapytanie ma `COLLATE ..._CI_AI` |
| Fraza `' OR 1=1 --` | Sklejany tekst SQL zwróci wszystkie rekordy. Z parametrem `@q` wyników jest 0. | Network + log SQL |

### Krok 3 – strona i statystyka

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Statystyka liczona z listy wyników | Lista pokazuje najwyżej 20 rekordów. Statystyka ma objąć całą tabelę – licz ją w SQL. | log SQL: zapytanie statystyki bez `TOP 20` |
| Wielkość liter i spacje w grupach | SQL (collation bez wielkości liter) łączy „Abc” i „abc” w jedną grupę, a `GroupBy` w C# nie. Licz w SQL. | Watch na liście grup + log SQL |

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
