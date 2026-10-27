# PamięćKatowic (s11) – materiał dla prowadzącego

Nie rozdawaj uczniom. Uczniowie mają `karta.md`, `prompty.md` i specyfikację `spec/` (5 plików, po jednym na krok, oraz `spec/dodatkowe/` z ulubionymi do „Co dalej?”).

Para **losuje** scenariusz (wylosowana karta pary), czyta `scenarios/s11/karta.md` i zaczyna od kroku 0 w `prompty.md`. Pierwszy prompt uruchamia `tools/nowy-portal.ps1`: kopiuje szkielet (`szkielet/` → `src`, `tests`, `db`), wpisuje `"Scenario": "s11"` w `src/GzmApp/appsettings.json` (pary NIE wpisują scenariusza ręcznie), pobiera plik danych z portalu GZM i zakłada `LOG-DECYZJI.md`. Potem Kilo buduje i uruchamia testy.

Decyzje 1–5 nie mają wydrukowanych wariantów: para wkleja Kilo prośbę o propozycje, Kilo proponuje 3 warianty według wzorców zdań (z `prompty.md`), bierze pod uwagę dane i `LOG-DECYZJI.md`, pyta parę i czeka. Para wkleja wybrane zdanie do specyfikacji (poza tym plikiem `scenarios/` są dla Kilo zablokowane). Bez decyzji działają zdania domyślne.

- **Pomysł:** Tablice, pomniki i groby pamięci narodowej w Katowicach.
- **Dane:** „Katowice: miejsca pamięci narodowej”, plik `data/raw/s11_katowice-miejsca-pamieci.csv`, tabela `KatowiceMemorials`. Gdy pobieranie w kroku 0 się nie powiedzie (sieć), połóż plik z pendrive'a w `data/raw/` i poproś parę o ponowne uruchomienie skryptu – pominie pobieranie.

## Co powinno wyjść przy zdaniach domyślnych

Gałęzie `ratunek/etap-1..4` realizują zdania domyślne wszystkich decyzji i dają dokładnie te wartości. Przy innych decyzjach par liczby z kroków 2–4 będą inne – to w porządku.

- Krok 0: skrypt pisze „GOTOWE”, testy `s11-000-dane` zielone (plik, liczba wierszy **391** (E1.fileRows), nagłówki).
- Krok 1: wierszy w tabeli **391** (E1.rows); wiersz 7, pole `Street`: **pl. Bolesława Chrobrego** (E1.row7.value); rekordy z pustym albo nieczytelnym polem: **11** (E1.weird).
- Krok 2: fraza **kościoła** (E2.phrase) daje **5** (E2.count) wyników:
   - Nr 93: **Tablica upamiętniająca fundatorów Kościoła Garnizonowego w Katowicach** (E2.result.1)
   - Nr 121: **Tablica upamiętniająca księdza prałata Józefa Kubisa, pierwszego proboszcza w latach 1900-42 parafii rzymskokatolickiej pw św. Józefa. Tablica odsłonięta na 65 lecie kościoła i parafii** (E2.result.2)
   - Nr 187: **Tablica upamiętniająca Pasterzy Kościoła Katowickiego w latach 1925-2000.** (E2.result.3)
   - Nr 204: **Wotum wdzięczności za odzyskaną wolność wraz z tablicą upamiętniającą 85. rocznicę wzniesienia Kościoła Matki Boskiej Częstochowskiej** (E2.result.4)
   - Nr 308: **Grób Ludwika Musioła, nauczyciela, historyka górnośląskiego, prekursora krajoznawstwa, historyka kultury oraz Kościoła katolickiego, lingwisty, etnografa** (E2.result.5)
- Krok 3: kafelek – liczba rekordów dla każdej wartości pola `ObjectType`, od największej (puste pomijamy).
   - tablica: **182** (E3.group.1)
   - upamiętnienie: **74** (E3.group.2)
   - grób: **72** (E3.group.3)
   - pomnik: **34** (E3.group.4)
   - grób wojenny: **28** (E3.group.5)
   - cmentarz wojenny: **1** (E3.group.6)
- Krok 4: karta „✨ Analizuj z AI” z odpowiedzią modelu AI GPT-6 Luna (treść za każdym razem trochę inna – nie ma wartości kontrolnej). Zadanie zależy od decyzji 4 pary (pytają Kilo o propozycje). Bez pliku `src/GzmApp/appsettings.Local.json` karta pokazuje „Brak klucza do modelu AI – poproś prowadzącego.”.
- Dodatkowo (ulubione): tytuł rekordu z wiersza 7 (np. do sprawdzenia „Moich ulubionych”): **Pomnik Józefa Piłsudskiego** (E4.title).

Pełna lista wartości: `checkpoints.md` (generuje `tools/Expect`).

## Pułapki w danych

Co może pójść nie tak w tym zbiorze i jak to szybko zobaczyć.

### Krok 1 – import

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Kodowanie UTF-8 | Plik jest dobry, ale zapisany ponownie w Excelu dostanie Windows-1250 i ogonki się zepsują. Nie zapisuj go w Excelu. | rozszerzenie mssql: `SELECT TOP 5 * FROM KatowiceMemorials` |
| Kolumny, których nie importujemy: 20 | Nie dodawaj dla nich pól – tabela jest gotowa w prompcie. | rozszerzenie mssql: lista kolumn tabeli |
| Puste pola w `addressstreetName`: 11 | Muszą trafić do bazy jako NULL, nie jako pusty tekst ani 0. | rozszerzenie mssql: `SELECT COUNT(*) FROM KatowiceMemorials WHERE Street IS NULL` |

### Krok 2 – wyszukiwarka

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Fraza z ogonkami: „kościoła” | Przy złym kodowaniu albo kulturze wyników jest 0. W adresie fraza jest zakodowana (`%C5%...`). | Network: adres żądania; log SQL: wartość parametru `q` |
| Wielkość liter i ogonki | Porównanie `=` albo LIKE z collation `CS`/`AS` nie znajdzie „KOŚCIOŁA”. | log SQL: czy zapytanie ma `COLLATE ..._CI_AI` |
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
| Tytuł rekordu to długi opis (nawet 390 znaków) | Lista ulubionych robi się szeroka – skróć tekst w widoku CSS-em, nie w bazie. | rozszerzenie mssql: `SELECT MAX(LEN(Description)) FROM KatowiceMemorials` |
