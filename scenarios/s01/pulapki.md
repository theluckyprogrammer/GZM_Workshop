# Pułapki w danych – PamięćZabrze (s01)

Źródło: `data/metadata.json` i `checkpoints.md`. Przy każdej pułapce: co się stanie i jakim narzędziem ją złapiecie. To prawdziwe cechy danych – nikt ich nie podłożył.

## Etap 1 – import

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Kodowanie UTF-8 | Plik jest dobry, ale zapisany ponownie w Excelu dostanie Windows-1250 i ogonki się zepsują. Nie zapisuj go w Excelu. | rozszerzenie mssql: `SELECT TOP 5 * FROM ZabrzeMemorials` |
| Kolumny, których nie importujemy: 18 | Nie dodawaj dla nich pól – tabela jest gotowa w prompcie. | rozszerzenie mssql: lista kolumn tabeli |
| Puste pola w `addresscity`: 6 | Muszą trafić do bazy jako NULL, nie jako pusty tekst ani 0. | rozszerzenie mssql: `SELECT COUNT(*) FROM ZabrzeMemorials WHERE City IS NULL` |
| Puste pola w `addressstreetName`: 6 | Muszą trafić do bazy jako NULL, nie jako pusty tekst ani 0. | rozszerzenie mssql: `SELECT COUNT(*) FROM ZabrzeMemorials WHERE Street IS NULL` |
| Puste pola w `inscription`: 3 | Muszą trafić do bazy jako NULL, nie jako pusty tekst ani 0. | rozszerzenie mssql: `SELECT COUNT(*) FROM ZabrzeMemorials WHERE Inscription IS NULL` |
| Puste pola w `locationName`: 11 | Muszą trafić do bazy jako NULL, nie jako pusty tekst ani 0. | rozszerzenie mssql: `SELECT COUNT(*) FROM ZabrzeMemorials WHERE LocationName IS NULL` |

## Etap 2 – endpoint z filtrowaniem

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Fraza z ogonkami: „którym” | Przy złym kodowaniu albo kulturze wyników jest 0. W adresie fraza jest zakodowana (`%C5%...`). | Network: adres żądania; log SQL: wartość parametru `q` |
| Wielkość liter i ogonki | Porównanie `=` albo LIKE z collation `CS`/`AS` nie znajdzie „KTÓRYM”. | log SQL: czy zapytanie ma `COLLATE ..._CI_AI` |
| Fraza `' OR 1=1 --` | Sklejany tekst SQL zwróci wszystkie rekordy. Z parametrem `@q` wyników jest 0. | Network + log SQL |

## Etap 3 – widok i statystyka

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Statystyka liczona z listy wyników | Lista pokazuje najwyżej 20 rekordów. Statystyka ma objąć całą tabelę – licz ją w SQL. | log SQL: zapytanie statystyki bez `TOP 20` |
| Wielkość liter i spacje w grupach | SQL (collation bez wielkości liter) łączy „Abc” i „abc” w jedną grupę, a `GroupBy` w C# nie. Licz w SQL. | Watch na liście grup + log SQL |
| Puste wartości w polu `City`: 6 | NULL nie wchodzi do statystyki. Zamiana pustych na 0 albo „” da inny wynik niż na karcie. | rozszerzenie mssql: `SELECT COUNT(*) FROM ZabrzeMemorials WHERE City IS NULL` |
| Ta sama dzielnica zapisana różnie: „Zabrze-Biskupice”, „Zabrze – Biskupice”, „Zabrze - Mikulczyce” | W top 5 to osobne grupy. Tak jest w danych – nie poprawiaj ich, porównuj z kartą. | rozszerzenie mssql: `SELECT DISTINCT City FROM ZabrzeMemorials` |

## Etap 4 – ulubione

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Rekordy bez tytułu (`LocationName` puste: 11) | Na liście ulubionych pojawi się pusta pozycja. Pokaż np. „(bez nazwy)”. | rozszerzenie mssql: `SELECT COUNT(*) FROM ZabrzeMemorials WHERE LocationName IS NULL` |
| Dwa kliknięcia ★ w dwóch kartach | Bez klucza `PK_Favorites` w tabeli są dwa takie same wiersze. | rozszerzenie mssql: `SELECT COUNT(*) FROM Favorites` |
| Brak tokenu antiforgery | Żądanie POST/DELETE dostaje status 400, a handler się nie uruchamia. | Network: nagłówek `RequestVerificationToken` |
