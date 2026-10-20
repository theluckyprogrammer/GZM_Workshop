# Pułapki w danych – RezerwacjaRadar (s08)

Źródło: `data/metadata.json` i `checkpoints.md`. Przy każdej pułapce: co się stanie i jakim narzędziem ją złapiecie. To prawdziwe cechy danych – nikt ich nie podłożył.

## Etap 1 – import

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Kodowanie UTF-8 | Plik jest dobry, ale zapisany ponownie w Excelu dostanie Windows-1250 i ogonki się zepsują. Nie zapisuj go w Excelu. | rozszerzenie mssql: `SELECT TOP 5 * FROM BikeReservations` |
| Identyczne wiersze w pliku (drugie z par: 63, 74) | Drugi taki sam wiersz łamie unikalny `RowHash` i import przerywa się błędem. | komunikat błędu w terminalu + log SQL |
| Czas w `Data` jest w UTC (litera Z) | Godziny są o 1–2 h wcześniejsze niż czas polski. Nie przeliczaj – porównuj z kartą. | breakpoint w imporcie + Watch na wartości `DateTime` |
| Czas w `Czas wygaśnięcia` jest w UTC (litera Z) | Godziny są o 1–2 h wcześniejsze niż czas polski. Nie przeliczaj – porównuj z kartą. | breakpoint w imporcie + Watch na wartości `DateTime` |
| Puste pola w `Numer stacji rezerwacji`: 12 | Muszą trafić do bazy jako NULL, nie jako pusty tekst ani 0. | rozszerzenie mssql: `SELECT COUNT(*) FROM BikeReservations WHERE StationNumber IS NULL` |

## Etap 2 – endpoint z filtrowaniem

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Wielkość liter i ogonki | Porównanie `=` albo LIKE z collation `CS`/`AS` nie znajdzie „27402”. | log SQL: czy zapytanie ma `COLLATE ..._CI_AI` |
| Fraza `' OR 1=1 --` | Sklejany tekst SQL zwróci wszystkie rekordy. Z parametrem `@q` wyników jest 0. | Network + log SQL |
| „Nazwa stacji” to zwykle numer (np. 27402), czasem „BIKE 582980” | Wyszukiwarka szuka w tekście – fraza z cyframi jest tu normalna. | Network: odpowiedź `?handler=Szukaj` |

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
