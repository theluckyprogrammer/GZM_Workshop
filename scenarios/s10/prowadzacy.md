# BoiskoMap (s10) – materiał dla prowadzącego

Nie rozdawaj uczniom. Uczniowie mają `karta.md` (opis portalu, zasady i prompty) i specyfikację `spec/` (5 plików, po jednym na krok, oraz `spec/dodatkowe/` z ulubionymi do „Co dalej?”).

Para **losuje** scenariusz (wylosowany wydruk `print/opis-scenariusza-s10.html`), czyta `scenarios/s10/karta.md` i zaczyna od kroku 0 w promptych z tej samej karty. Pierwszy prompt uruchamia `tools/nowy-portal.ps1`: tworzy oddzielny branch pary (s10-<losowy numer>, od main) i przechodzi na niego, kopiuje szkielet (`szkielet/` → `src`, `tests`, `db`), wpisuje `"Scenario": "s10"` w `src/GzmApp/appsettings.json` (pary NIE wpisują scenariusza ręcznie), pobiera plik danych z portalu GZM i zakłada `LOG-DECYZJI.md`. Potem Kilo buduje i uruchamia testy.

Decyzje 1–5 nie mają wydrukowanych wariantów: para wkleja Kilo prośbę o propozycje, Kilo proponuje 3 warianty według wzorców zdań (z `karta.md`), bierze pod uwagę dane i `LOG-DECYZJI.md`, pyta parę i czeka. Para wkleja wybrane zdanie do specyfikacji (poza tym plikiem `scenarios/` są dla Kilo zablokowane). Bez decyzji działają zdania domyślne.

- **Pomysł:** Boiska i stadiony w Gliwicach: znajdź boisko przy swojej szkole.
- **Dane:** „Gliwice: boiska i stadiony”, plik `data/raw/s10_gliwice-boiska-stadiony.csv`, tabela `SportsFields`. Gdy pobieranie w kroku 0 się nie powiedzie (sieć), połóż plik z pendrive'a w `data/raw/` i poproś parę o ponowne uruchomienie skryptu – pominie pobieranie.

## Co powinno wyjść przy zdaniach domyślnych

Zdania domyślne (wpisane w specyfikacji) dają dokładnie te wartości. Przy innych decyzjach par liczby z kroków 2–4 będą inne – to w porządku.

- Krok 0: skrypt pisze „GOTOWE”, testy `s10-000-dane` zielone (plik, liczba wierszy **121** (E1.fileRows), nagłówki).
- Krok 1: wierszy w tabeli **121** (E1.rows); wiersz 7, pole `X`: **2077273.6463570932** (E1.row7.value); rekordy z pustym albo nieczytelnym polem: **2** (E1.weird).
- Krok 2: fraza **śląska** (E2.phrase) daje **5** (E2.count) wyników:
   - Nr 63: **Politechnika Śląska** (E2.result.1)
   - Nr 101: **Politechnika Śląska** (E2.result.2)
   - Nr 102: **Politechnika Śląska** (E2.result.3)
   - Nr 103: **Politechnika Śląska** (E2.result.4)
   - Nr 105: **Politechnika Śląska** (E2.result.5)
- Krok 3: kafelek – 5 najczęstszych wartości pola `Kind` z liczbą rekordów (puste pomijamy).
   - boisko do piłki nożnej i koszykówki: **26** (E3.group.1)
   - boisko do piłki nożnej: **19** (E3.group.2)
   - boisko do koszykówki: **10** (E3.group.3)
   - boisko do koszykówki i siatkówki: **8** (E3.group.4)
   - boisko trawiaste do piłki nożnej: **8** (E3.group.5)
- Krok 4: karta „✨ Analizuj z AI” z odpowiedzią modelu AI GPT-6 Luna (treść za każdym razem trochę inna – nie ma wartości kontrolnej). Zadanie zależy od decyzji 4 pary (pytają Kilo o propozycje). Bez pliku `src/GzmApp/appsettings.Local.json` karta pokazuje „Brak klucza do modelu AI – poproś prowadzącego.”.
- Dodatkowo (ulubione): tytuł rekordu z wiersza 7 (np. do sprawdzenia „Moich ulubionych”): **Szkoła Podstawowa Nr 6** (E4.title).

Pełna lista wartości: `checkpoints.md` (generuje `tools/Expect`).

## Pułapki w danych

Co może pójść nie tak w tym zbiorze i jak to szybko zobaczyć.

### Krok 1 – import

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Kodowanie UTF-8 | Plik jest dobry, ale zapisany ponownie w Excelu dostanie Windows-1250 i ogonki się zepsują. Nie zapisuj go w Excelu. | SQLite Viewer: tabela SportsFields, pierwsze wiersze |
| Kolumny, których nie importujemy: 3 | Nie dodawaj dla nich pól – tabela jest gotowa w prompcie. | SQLite Viewer: lista kolumn tabeli |
| Kropka dziesiętna w `x` | Z kulturą pl-PL (przecinek) liczba się nie odczyta albo straci kropkę. | breakpoint w imporcie + Watch na wartości po odczycie |
| Kropka dziesiętna w `y` | Z kulturą pl-PL (przecinek) liczba się nie odczyta albo straci kropkę. | breakpoint w imporcie + Watch na wartości po odczycie |
| Puste pola w `NAZWA`: 2 | Muszą trafić do bazy jako NULL, nie jako pusty tekst ani 0. | SQLite Viewer: kolumna Name – puste komórki mają być NULL |
| `x` i `y` to metry w układzie PL-2000 (np. 2077206.03), a nie stopnie | Liczby mają 7 cyfr przed kropką i 10 po – typ `decimal(20,10)`, kultura `invariant`. | breakpoint + Watch na sparsowanej wartości `x` |

### Krok 2 – wyszukiwarka

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Fraza z ogonkami: „śląska” | Przy złym kodowaniu albo kulturze wyników jest 0. W adresie fraza jest zakodowana (`%C5%...`). | Network: adres żądania; log SQL: wartość parametru `q` |
| Wielkość liter i ogonki | Zwykłe `LIKE` w SQLite pomija wielkość liter tylko w literach bez ogonków – samo „ŚLĄSKA” znajdzie 0 wyników. | log SQL: czy zapytanie używa funkcji plain() |
| Fraza `' OR 1=1 --` | Sklejany tekst SQL zwróci wszystkie rekordy. Z parametrem `@q` wyników jest 0. | Network + log SQL |
| Puste pole `Name` w 2 rekordach | Te rekordy nie pojawią się w wynikach wyszukiwania – to poprawne. | SQLite Viewer (plik src/GzmApp/gzm.db): kolumna Name – puste komórki mają być NULL |

### Krok 3 – strona i statystyka

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Statystyka liczona z listy wyników | Lista pokazuje najwyżej 20 rekordów. Statystyka ma objąć całą tabelę – licz ją w SQL. | log SQL: zapytanie statystyki bez `TOP 20` |
| Wielkość liter i spacje w grupach | `GROUP BY` w SQLite rozróżnia wielkość liter („Abc” i „abc” to dwie grupy), a `GroupBy` w C# z ignorowaniem wielkości liter jedną. Licz w SQL. | Watch na liście grup + log SQL |

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
| Rekordy bez tytułu (`Name` puste: 2) | Na liście ulubionych pojawi się pusta pozycja. Pokaż np. „(bez nazwy)”. | SQLite Viewer (plik src/GzmApp/gzm.db): kolumna Name – puste komórki mają być NULL |
| Dwa kliknięcia ★ w dwóch kartach | Bez klucza `PK_Favorites` w tabeli są dwa takie same wiersze. | SQLite Viewer (plik src/GzmApp/gzm.db): tabela Favorites |
| Brak tokenu antiforgery | Żądanie POST/DELETE dostaje status 400, a handler się nie uruchamia. | Network: nagłówek `RequestVerificationToken` |
