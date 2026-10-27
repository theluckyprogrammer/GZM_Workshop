# BiletBot (s14) – materiał dla prowadzącego

Nie rozdawaj uczniom. Uczniowie mają `karta.md`, `prompty.md` i specyfikację `spec/` (5 plików, po jednym na krok, oraz `spec/dodatkowe/` z ulubionymi do „Co dalej?”).

Para **losuje** scenariusz (wylosowana karta pary), czyta `scenarios/s14/karta.md` i zaczyna od kroku 0 w `prompty.md`. Pierwszy prompt uruchamia `tools/nowy-portal.ps1`: kopiuje szkielet (`szkielet/` → `src`, `tests`, `db`), wpisuje `"Scenario": "s14"` w `src/GzmApp/appsettings.json` (pary NIE wpisują scenariusza ręcznie), pobiera plik danych z portalu GZM i zakłada `LOG-DECYZJI.md`. Potem Kilo buduje i uruchamia testy.

Decyzje 1–5 nie mają wydrukowanych wariantów: para wkleja Kilo prośbę o propozycje, Kilo proponuje 3 warianty według wzorców zdań (z `prompty.md`), bierze pod uwagę dane i `LOG-DECYZJI.md`, pyta parę i czeka. Para wkleja wybrane zdanie do specyfikacji (poza tym plikiem `scenarios/` są dla Kilo zablokowane). Bez decyzji działają zdania domyślne.

- **Pomysł:** Gdzie kupisz bilet ZTM: automaty biletowe na przystankach w Twojej gminie.
- **Dane:** „ZTM: automaty biletowe”, plik `data/raw/s14_ztm-automaty-biletowe.csv`, tabela `TicketMachines`. Gdy pobieranie w kroku 0 się nie powiedzie (sieć), połóż plik z pendrive'a w `data/raw/` i poproś parę o ponowne uruchomienie skryptu – pominie pobieranie.

## Co powinno wyjść przy zdaniach domyślnych

Gałęzie `ratunek/etap-1..4` realizują zdania domyślne wszystkich decyzji i dają dokładnie te wartości. Przy innych decyzjach par liczby z kroków 2–4 będą inne – to w porządku.

- Krok 0: skrypt pisze „GOTOWE”, testy `s14-000-dane` zielone (plik, liczba wierszy **272** (E1.fileRows), nagłówki).
- Krok 1: wierszy w tabeli **266** (E1.rows); wiersz 7, pole `Coordinates`: **50.39760177303035 18.877324014902115** (E1.row7.value); rekordy z pustym albo nieczytelnym polem: **25** (E1.weird).
- Krok 2: fraza **brynów** (E2.phrase) daje **5** (E2.count) wyników:
   - Nr 96: **Brynów Centrum Przesiadkowe** (E2.result.1)
   - Nr 97: **Brynów Centrum Przesiadkowe** (E2.result.2)
   - Nr 98: **Brynów Centrum Przesiadkowe** (E2.result.3)
   - Nr 99: **Brynów Centrum Przesiadkowe** (E2.result.4)
   - Nr 100: **Brynów Centrum Przesiadkowe** (E2.result.5)
- Krok 3: kafelek – 5 najczęstszych wartości pola `Commune` z liczbą rekordów (puste pomijamy).
   - KATOWICE: **47** (E3.group.1)
   - SOSNOWIEC: **29** (E3.group.2)
   - GLIWICE: **18** (E3.group.3)
   - BYTOM: **13** (E3.group.4)
   - ZABRZE: **13** (E3.group.5)
- Krok 4: karta „✨ Analizuj z AI” z odpowiedzią modelu AI GPT-6 Luna (treść za każdym razem trochę inna – nie ma wartości kontrolnej). Zadanie zależy od decyzji 4 pary (pytają Kilo o propozycje). Bez pliku `src/GzmApp/appsettings.Local.json` karta pokazuje „Brak klucza do modelu AI – poproś prowadzącego.”.
- Dodatkowo (ulubione): tytuł rekordu z wiersza 7 (np. do sprawdzenia „Moich ulubionych”): **Stroszek Osiedle** (E4.title).

Pełna lista wartości: `checkpoints.md` (generuje `tools/Expect`).

## Pułapki w danych

Co może pójść nie tak w tym zbiorze i jak to szybko zobaczyć.

### Krok 1 – import

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Kodowanie Windows-1250 | Czytany jako UTF-8 plik zamienia ogonki w „�”. | breakpoint w imporcie + Watch na tekście z ogonkiem; rozszerzenie mssql: `SELECT TOP 5 * FROM TicketMachines` |
| Separator to `;`, nie przecinek | Parser ustawiony na przecinek widzi jedną kolumnę w każdym wierszu. | breakpoint w imporcie + Watch na liczbie pól w wierszu |
| Nagłówek dopiero w linii 5 | Linie tytułu zostaną wzięte za nagłówek i nazwy kolumn się nie zgodzą. | breakpoint w imporcie + Watch na nazwach kolumn |
| Notatki pod tabelą (wiersze 267, 268, 269, 270, 271, 272) | Bez pomijania w tabeli będzie 272 zamiast 266 wierszy. | rozszerzenie mssql: `SELECT COUNT(*)` i wartość E1.rows |
| Kolumny, których nie importujemy: 1 | Nie dodawaj dla nich pól – tabela jest gotowa w prompcie. | rozszerzenie mssql: lista kolumn tabeli |
| Puste pola w `NR`: 14 | Muszą trafić do bazy jako NULL, nie jako pusty tekst ani 0. | rozszerzenie mssql: `SELECT COUNT(*) FROM TicketMachines WHERE StopNumber IS NULL` |
| Puste pola w `Opis lokalizacji`: 1 | Muszą trafić do bazy jako NULL, nie jako pusty tekst ani 0. | rozszerzenie mssql: `SELECT COUNT(*) FROM TicketMachines WHERE LocationDescription IS NULL` |
| Puste pola w `Nr stanowiska`: 11 | Muszą trafić do bazy jako NULL, nie jako pusty tekst ani 0. | rozszerzenie mssql: `SELECT COUNT(*) FROM TicketMachines WHERE Platform IS NULL` |
| Puste pola w `Współrzędne geograficzne`: 1 | Muszą trafić do bazy jako NULL, nie jako pusty tekst ani 0. | rozszerzenie mssql: `SELECT COUNT(*) FROM TicketMachines WHERE Coordinates IS NULL` |
| Współrzędne „szerokość długość” w jednym polu, oddzielone spacją | Zostają tekstem (`Coordinates`). Nie rozdzielaj ich. | Watch na polu `Współrzędne geograficzne` |
| Ostatnia kolumna nie ma nazwy i jest pusta (średnik na końcu linii) | Nie importujemy jej; CsvHelper zobaczy 9 kolumn. | breakpoint + Watch na nagłówkach |

### Krok 2 – wyszukiwarka

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Fraza z ogonkami: „brynów” | Przy złym kodowaniu albo kulturze wyników jest 0. W adresie fraza jest zakodowana (`%C5%...`). | Network: adres żądania; log SQL: wartość parametru `q` |
| Wielkość liter i ogonki | Porównanie `=` albo LIKE z collation `CS`/`AS` nie znajdzie „BRYNÓW”. | log SQL: czy zapytanie ma `COLLATE ..._CI_AI` |
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
