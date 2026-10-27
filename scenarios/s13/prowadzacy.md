# SekcjaSCK (s13) – materiał dla prowadzącego

Nie rozdawaj uczniom. Uczniowie mają `karta.md`, `prompty.md` i specyfikację `spec/` (5 plików, po jednym na krok, oraz `spec/dodatkowe/` z ulubionymi do „Co dalej?”).

Para **losuje** scenariusz (wylosowana karta pary), czyta `scenarios/s13/karta.md` i zaczyna od kroku 0 w `prompty.md`. Pierwszy prompt uruchamia `tools/nowy-portal.ps1`: kopiuje szkielet (`szkielet/` → `src`, `tests`, `db`), wpisuje `"Scenario": "s13"` w `src/GzmApp/appsettings.json` (pary NIE wpisują scenariusza ręcznie), pobiera plik danych z portalu GZM i zakłada `LOG-DECYZJI.md`. Potem Kilo buduje i uruchamia testy.

Decyzje 1–5 nie mają wydrukowanych wariantów: para wkleja Kilo prośbę o propozycje, Kilo proponuje 3 warianty według wzorców zdań (z `prompty.md`), bierze pod uwagę dane i `LOG-DECYZJI.md`, pyta parę i czeka. Para wkleja wybrane zdanie do specyfikacji (poza tym plikiem `scenarios/` są dla Kilo zablokowane). Bez decyzji działają zdania domyślne.

- **Pomysł:** Zajęcia w Siemianowickim Centrum Kultury: znajdź sekcję dla siebie albo rodzeństwa.
- **Dane:** „SCK Siemianowice: zajęcia sekcyjne”, plik `data/raw/s13_sck-zajecia-sekcyjne.csv`, tabela `CultureClasses`. Gdy pobieranie w kroku 0 się nie powiedzie (sieć), połóż plik z pendrive'a w `data/raw/` i poproś parę o ponowne uruchomienie skryptu – pominie pobieranie.

## Co powinno wyjść przy zdaniach domyślnych

Zdania domyślne (wpisane w specyfikacji) dają dokładnie te wartości. Przy innych decyzjach par liczby z kroków 2–4 będą inne – to w porządku.

- Krok 0: skrypt pisze „GOTOWE”, testy `s13-000-dane` zielone (plik, liczba wierszy **38** (E1.fileRows), nagłówki).
- Krok 1: wierszy w tabeli **38** (E1.rows); wiersz 7, pole `MinAge`: **5.0** (E1.row7.value); rekordy z pustym albo nieczytelnym polem: **23** (E1.weird).
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
- Krok 4: karta „✨ Analizuj z AI” z odpowiedzią modelu AI GPT-6 Luna (treść za każdym razem trochę inna – nie ma wartości kontrolnej). Zadanie zależy od decyzji 4 pary (pytają Kilo o propozycje). Bez pliku `src/GzmApp/appsettings.Local.json` karta pokazuje „Brak klucza do modelu AI – poproś prowadzącego.”.
- Dodatkowo (ulubione): tytuł rekordu z wiersza 7 (np. do sprawdzenia „Moich ulubionych”): **Pracownia Plastyczna „Okienko”** (E4.title).

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
