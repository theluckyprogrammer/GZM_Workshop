# Pułapki w danych – SekcjaSCK (s13)

Źródło: `data/metadata.json` i `checkpoints.md`. Przy każdej pułapce: co się stanie i jakim narzędziem ją złapiecie. To prawdziwe cechy danych – nikt ich nie podłożył.

## Etap 1 – import

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Kodowanie UTF-8 | Plik jest dobry, ale zapisany ponownie w Excelu dostanie Windows-1250 i ogonki się zepsują. Nie zapisuj go w Excelu. | rozszerzenie mssql: `SELECT TOP 5 * FROM CultureClasses` |
| Kolumny, których nie importujemy: 4 | Nie dodawaj dla nich pól – tabela jest gotowa w prompcie. | rozszerzenie mssql: lista kolumn tabeli |
| Kropka dziesiętna w `minimalAge` | Z kulturą pl-PL (przecinek) liczba się nie odczyta albo straci kropkę. | breakpoint w imporcie + Watch na wartości po odczycie |
| Dwa zapisy w `startHour`: `HH:mm` i `HH.mm` | Jeden wzorzec odrzuci część wartości – wzrośnie liczba „dziwnych” rekordów. | log importu: liczba „dziwnych” vs karta (E1.weird) |
| Puste pola w `day`: 4 | Muszą trafić do bazy jako NULL, nie jako pusty tekst ani 0. | rozszerzenie mssql: `SELECT COUNT(*) FROM CultureClasses WHERE Day IS NULL` |
| Puste pola w `minimalAge`: 12 | Muszą trafić do bazy jako NULL, nie jako pusty tekst ani 0. | rozszerzenie mssql: `SELECT COUNT(*) FROM CultureClasses WHERE MinAge IS NULL` |
| Puste pola w `room`: 5 | Muszą trafić do bazy jako NULL, nie jako pusty tekst ani 0. | rozszerzenie mssql: `SELECT COUNT(*) FROM CultureClasses WHERE Room IS NULL` |
| Puste pola w `startHour`: 10 | Muszą trafić do bazy jako NULL, nie jako pusty tekst ani 0. | rozszerzenie mssql: `SELECT COUNT(*) FROM CultureClasses WHERE StartHour IS NULL` |
| Wiek „0.5” w `minimalAge` (pół roku) | Typ int go odrzuci – pole `MinAge` ma typ `decimal(4,1)`. | breakpoint + Watch na `minimalAge` w wierszu 1 |

## Etap 2 – endpoint z filtrowaniem

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Fraza z ogonkami: „dziecięcy” | Przy złym kodowaniu albo kulturze wyników jest 0. W adresie fraza jest zakodowana (`%C5%...`). | Network: adres żądania; log SQL: wartość parametru `q` |
| Wielkość liter i ogonki | Porównanie `=` albo LIKE z collation `CS`/`AS` nie znajdzie „DZIECIĘCY”. | log SQL: czy zapytanie ma `COLLATE ..._CI_AI` |
| Fraza `' OR 1=1 --` | Sklejany tekst SQL zwróci wszystkie rekordy. Z parametrem `@q` wyników jest 0. | Network + log SQL |

## Etap 3 – widok i statystyka

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Statystyka liczona z listy wyników | Lista pokazuje najwyżej 20 rekordów. Statystyka ma objąć całą tabelę – licz ją w SQL. | log SQL: zapytanie statystyki bez `TOP 20` |
| Wielkość liter i spacje w grupach | SQL (collation bez wielkości liter) łączy „Abc” i „abc” w jedną grupę, a `GroupBy` w C# nie. Licz w SQL. | Watch na liście grup + log SQL |

## Etap 4 – ulubione

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Dwa kliknięcia ★ w dwóch kartach | Bez klucza `PK_Favorites` w tabeli są dwa takie same wiersze. | rozszerzenie mssql: `SELECT COUNT(*) FROM Favorites` |
| Brak tokenu antiforgery | Żądanie POST/DELETE dostaje status 400, a handler się nie uruchamia. | Network: nagłówek `RequestVerificationToken` |
