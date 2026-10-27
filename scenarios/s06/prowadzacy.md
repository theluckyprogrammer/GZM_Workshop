# DrzewoRekord (s06) – materiał dla prowadzącego

Nie rozdawaj uczniom. Uczniowie mają `karta.md` (opis portalu, zasady i prompty) i specyfikację `spec/` (5 plików, po jednym na krok, oraz `spec/dodatkowe/` z ulubionymi do „Co dalej?”).

Para **losuje** scenariusz (wylosowany wydruk `print/opis-scenariusza-s06.html`), czyta `scenarios/s06/karta.md` i zaczyna od kroku 0 w promptych z tej samej karty. Pierwszy prompt uruchamia `tools/nowy-portal.ps1`: tworzy oddzielny branch pary (s06-<losowy numer>, od main) i przechodzi na niego, kopiuje szkielet (`szkielet/` → `src`, `tests`, `db`), wpisuje `"Scenario": "s06"` w `src/GzmApp/appsettings.json` (pary NIE wpisują scenariusza ręcznie), pobiera plik danych z portalu GZM i zakłada `LOG-DECYZJI.md`. Potem Kilo buduje i uruchamia testy.

Decyzje 1–5 nie mają wydrukowanych wariantów: para wkleja Kilo prośbę o propozycje, Kilo proponuje 3 warianty według wzorców zdań (z `karta.md`), bierze pod uwagę dane i `LOG-DECYZJI.md`, pyta parę i czeka. Para wkleja wybrane zdanie do specyfikacji (poza tym plikiem `scenarios/` są dla Kilo zablokowane). Bez decyzji działają zdania domyślne.

- **Pomysł:** Pomniki przyrody Dąbrowy Górniczej: jakie to drzewa i jaki mają obwód.
- **Dane:** „Dąbrowa Górnicza: pomniki przyrody”, plik `data/raw/s06_dabrowa-pomniki-przyrody.csv`, tabela `NatureMonuments`. Gdy pobieranie w kroku 0 się nie powiedzie (sieć), połóż plik z pendrive'a w `data/raw/` i poproś parę o ponowne uruchomienie skryptu – pominie pobieranie.

## Co powinno wyjść przy zdaniach domyślnych

Zdania domyślne (wpisane w specyfikacji) dają dokładnie te wartości. Przy innych decyzjach par liczby z kroków 2–4 będą inne – to w porządku.

- Krok 0: skrypt pisze „GOTOWE”, testy `s06-000-dane` zielone (plik, liczba wierszy **31** (E1.fileRows), nagłówki).
- Krok 1: wierszy w tabeli **31** (E1.rows); wiersz 7, pole `Circumference`: **290** (E1.row7.value); rekordy z pustym albo nieczytelnym polem: **2** (E1.weird).
- Krok 2: fraza **wyniosły** (E2.phrase) daje **3** (E2.count) wyników:
   - Nr 5: **Jesion wyniosły (Fraxinus excelsior)** (E2.result.1)
   - Nr 14: **Jesion wyniosły (Fraxinus excelsior)** (E2.result.2)
   - Nr 28: **Jesion wyniosły (Fraxinus excelsior)** (E2.result.3)
- Krok 3: kafelek – średnia pola `Circumference` z 2 miejscami po przecinku (puste pomijamy).
   - **311.40** (E3.value)
- Krok 4: karta „✨ Analizuj z AI” z odpowiedzią modelu AI GPT-6 Luna (treść za każdym razem trochę inna – nie ma wartości kontrolnej). Zadanie zależy od decyzji 4 pary (pytają Kilo o propozycje). Bez pliku `src/GzmApp/appsettings.Local.json` karta pokazuje „Brak klucza do modelu AI – poproś prowadzącego.”.
- Dodatkowo (ulubione): tytuł rekordu z wiersza 7 (np. do sprawdzenia „Moich ulubionych”): **Buk pospolity (Fagus sylvatica)** (E4.title).

Pełna lista wartości: `checkpoints.md` (generuje `tools/Expect`).

## Pułapki w danych

Co może pójść nie tak w tym zbiorze i jak to szybko zobaczyć.

### Krok 1 – import

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Kodowanie UTF-8 | Plik jest dobry, ale zapisany ponownie w Excelu dostanie Windows-1250 i ogonki się zepsują. Nie zapisuj go w Excelu. | SQLite Viewer: tabela NatureMonuments, pierwsze wiersze |
| Kolumny, których nie importujemy: 2 | Nie dodawaj dla nich pól – tabela jest gotowa w prompcie. | SQLite Viewer: lista kolumn tabeli |
| Puste pola w `circumference`: 1 | Muszą trafić do bazy jako NULL, nie jako pusty tekst ani 0. | SQLite Viewer: kolumna Circumference – puste komórki mają być NULL |
| Puste pola w `coordinateData`: 1 | Muszą trafić do bazy jako NULL, nie jako pusty tekst ani 0. | SQLite Viewer: kolumna Coordinates – puste komórki mają być NULL |
| Współrzędne w `coordinateData` mają stopnie, minuty i cudzysłów („N50° 23' 29.9"”) | Cudzysłów w środku pola psuje dzielenie przez `Split`. Zostają tekstem. | breakpoint + Watch na polu `coordinateData` |

### Krok 2 – wyszukiwarka

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Fraza z ogonkami: „wyniosły” | Przy złym kodowaniu albo kulturze wyników jest 0. W adresie fraza jest zakodowana (`%C5%...`). | Network: adres żądania; log SQL: wartość parametru `q` |
| Wielkość liter i ogonki | Zwykłe `LIKE` w SQLite pomija wielkość liter tylko w literach bez ogonków – samo „WYNIOSŁY” znajdzie 0 wyników. | log SQL: czy zapytanie używa funkcji plain() |
| Fraza `' OR 1=1 --` | Sklejany tekst SQL zwróci wszystkie rekordy. Z parametrem `@q` wyników jest 0. | Network + log SQL |
| Ten sam gatunek zapisany różnie: „Fagus silvatica” i „Fagus sylvatica” | Fraza „sylvatica” nie znajdzie wszystkich buków. | SQLite Viewer: posortuj kolumnę Name w NatureMonuments |

### Krok 3 – strona i statystyka

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Statystyka liczona z listy wyników | Lista pokazuje najwyżej 20 rekordów. Statystyka ma objąć całą tabelę – licz ją w SQL. | log SQL: zapytanie statystyki bez `TOP 20` |
| Średnia z liczb zapisanych jako tekst | Bez rzutowania SQLite policzy średnią po swojej drodze i wynik nie zgadza się z kartą. Rzutuj: `CAST(pole AS REAL)`. | log SQL + wartość E3 |
| Puste wartości w polu `Circumference`: 1 | NULL nie wchodzi do statystyki. Zamiana pustych na 0 albo „” da inny wynik niż na karcie. | SQLite Viewer (plik src/GzmApp/gzm.db): kolumna Circumference – puste komórki mają być NULL |

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
| Dwa kliknięcia ★ w dwóch kartach | Bez klucza `PK_Favorites` w tabeli są dwa takie same wiersze. | SQLite Viewer (plik src/GzmApp/gzm.db): tabela Favorites |
| Brak tokenu antiforgery | Żądanie POST/DELETE dostaje status 400, a handler się nie uruchamia. | Network: nagłówek `RequestVerificationToken` |
