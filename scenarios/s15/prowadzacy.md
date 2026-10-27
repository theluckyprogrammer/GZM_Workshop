# KlubKatowice (s15) – materiał dla prowadzącego

Nie rozdawaj uczniom. Uczniowie mają `karta.md`, `prompty.md` i specyfikację `spec/` (5 plików, po jednym na krok, oraz `spec/dodatkowe/` z ulubionymi do „Co dalej?”).

Para **losuje** scenariusz (wylosowana karta pary), czyta `scenarios/s15/karta.md` i zaczyna od kroku 0 w `prompty.md`. Pierwszy prompt uruchamia `tools/nowy-portal.ps1`: kopiuje szkielet (`szkielet/` → `src`, `tests`, `db`), wpisuje `"Scenario": "s15"` w `src/GzmApp/appsettings.json` (pary NIE wpisują scenariusza ręcznie), pobiera plik danych z portalu GZM i zakłada `LOG-DECYZJI.md`. Potem Kilo buduje i uruchamia testy.

Decyzje 1–5 nie mają wydrukowanych wariantów: para wkleja Kilo prośbę o propozycje, Kilo proponuje 3 warianty według wzorców zdań (z `prompty.md`), bierze pod uwagę dane i `LOG-DECYZJI.md`, pyta parę i czeka. Para wkleja wybrane zdanie do specyfikacji (poza tym plikiem `scenarios/` są dla Kilo zablokowane). Bez decyzji działają zdania domyślne.

- **Pomysł:** Uczniowskie kluby sportowe w Katowicach: znajdź klub w swojej okolicy.
- **Dane:** „Katowice: uczniowskie kluby sportowe”, plik `data/raw/s15_katowice-uczniowskie-kluby-sportowe.csv`, tabela `SchoolSportsClubs`. Gdy pobieranie w kroku 0 się nie powiedzie (sieć), połóż plik z pendrive'a w `data/raw/` i poproś parę o ponowne uruchomienie skryptu – pominie pobieranie.

## Co powinno wyjść przy zdaniach domyślnych

Gałęzie `ratunek/etap-1..4` realizują zdania domyślne wszystkich decyzji i dają dokładnie te wartości. Przy innych decyzjach par liczby z kroków 2–4 będą inne – to w porządku.

- Krok 0: skrypt pisze „GOTOWE”, testy `s15-000-dane` zielone (plik, liczba wierszy **94** (E1.fileRows), nagłówki).
- Krok 1: wierszy w tabeli **94** (E1.rows); wiersz 7, pole `RegisteredOn`: **2001-07-23** (E1.row7.value); rekordy z pustym albo nieczytelnym polem: **2** (E1.weird).
- Krok 2: fraza **naprzód** (E2.phrase) daje **4** (E2.count) wyników:
   - Nr 8: **Uczniowski Klub Sportowy\n„Naprzód” Katowice** (E2.result.1)
   - Nr 10: **Hokejowy Uczniowski Klub Sportowy „Naprzód Pawłowski Janów”** (E2.result.2)
   - Nr 19: **Międzyszkolny Uczniowski Klub Sportowy „ Naprzód Janów”** (E2.result.3)
   - Nr 85: **Uczniowski Klub Sportowy Katowice Naprzód Janów** (E2.result.4)
- Krok 3: kafelek – liczba rekordów dla każdej wartości pola `Status`, od największej (puste pomijamy).
   - ACTIVE: **58** (E3.group.1)
   - UNKNOWN: **22** (E3.group.2)
   - EXPUNGED: **14** (E3.group.3)
- Krok 4: karta „✨ Analizuj z AI” z odpowiedzią modelu AI GPT-6 Luna (treść za każdym razem trochę inna – nie ma wartości kontrolnej). Zadanie zależy od decyzji 4 pary (pytają Kilo o propozycje). Bez pliku `src/GzmApp/appsettings.Local.json` karta pokazuje „Brak klucza do modelu AI – poproś prowadzącego.”.
- Dodatkowo (ulubione): tytuł rekordu z wiersza 7 (np. do sprawdzenia „Moich ulubionych”): **Międzyszkolny Uczniowski \nKlub Sportowy „Tornado”** (E4.title).

Pełna lista wartości: `checkpoints.md` (generuje `tools/Expect`).

## Pułapki w danych

Co może pójść nie tak w tym zbiorze i jak to szybko zobaczyć.

### Krok 1 – import

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Kodowanie UTF-8 | Plik jest dobry, ale zapisany ponownie w Excelu dostanie Windows-1250 i ogonki się zepsują. Nie zapisuj go w Excelu. | rozszerzenie mssql: `SELECT TOP 5 * FROM SchoolSportsClubs` |
| Kolumny, których nie importujemy: 16 | Nie dodawaj dla nich pól – tabela jest gotowa w prompcie. | rozszerzenie mssql: lista kolumn tabeli |
| Daty w `registrationDate` jako milisekundy od 1970 r. | Bez przeliczenia w bazie będzie liczba albo błąd typu. Przeliczaj w UTC, inaczej data przesunie się o dzień. | rozszerzenie mssql: wiersz 7 i wartość E1.row7.value |
| Puste pola w `registrationDate`: 2 | Muszą trafić do bazy jako NULL, nie jako pusty tekst ani 0. | rozszerzenie mssql: `SELECT COUNT(*) FROM SchoolSportsClubs WHERE RegisteredOn IS NULL` |
| Daty rejestracji sprzed 2001 r. mają 12 cyfr zamiast 13 | Parsowanie po długości tekstu się pomyli – licz z liczby milisekund. | breakpoint + Watch na `registrationDate` w wierszu 1 |

### Krok 2 – wyszukiwarka

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Fraza z ogonkami: „naprzód” | Przy złym kodowaniu albo kulturze wyników jest 0. W adresie fraza jest zakodowana (`%C5%...`). | Network: adres żądania; log SQL: wartość parametru `q` |
| Wielkość liter i ogonki | Porównanie `=` albo LIKE z collation `CS`/`AS` nie znajdzie „NAPRZÓD”. | log SQL: czy zapytanie ma `COLLATE ..._CI_AI` |
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
| Nazwa klubu w wierszu 7 ma znak nowej linii w środku | W przeglądarce widać spację, na karcie zapis `\n`. To ta sama wartość. | rozszerzenie mssql: `SELECT Name FROM SchoolSportsClubs WHERE SourceRow = 7` |
