# Pułapki w danych – BiletBot (s14)

Źródło: `data/metadata.json` i `checkpoints.md`. Przy każdej pułapce: co się stanie i jakim narzędziem ją złapiecie. To prawdziwe cechy danych – nikt ich nie podłożył.

## Etap 1 – import

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Kodowanie Windows-1250 | Czytany jako UTF-8 plik zamienia ogonki w „�”. | breakpoint w imporcie + Watch na tekście z ogonkiem; rozszerzenie mssql: `SELECT TOP 5 * FROM TicketMachines` |
| Separator to `;`, nie przecinek | Parser ustawiony na przecinek widzi jedną kolumnę w każdym wierszu. | breakpoint w imporcie + Watch na liczbie pól w wierszu |
| Nagłówek dopiero w linii 5 | Linie tytułu zostaną wzięte za nagłówek i nazwy kolumn się nie zgodzą. | breakpoint w imporcie + Watch na nazwach kolumn |
| Notatki pod tabelą (wiersze 267, 268, 269, 270, 271, 272) | Bez pomijania w tabeli będzie 272 zamiast 266 wierszy. | rozszerzenie mssql: `SELECT COUNT(*)` i karta (E1.rows) |
| Kolumny, których nie importujemy: 1 | Nie dodawaj dla nich pól – tabela jest gotowa w prompcie. | rozszerzenie mssql: lista kolumn tabeli |
| Puste pola w `NR`: 14 | Muszą trafić do bazy jako NULL, nie jako pusty tekst ani 0. | rozszerzenie mssql: `SELECT COUNT(*) FROM TicketMachines WHERE StopNumber IS NULL` |
| Puste pola w `Opis lokalizacji`: 1 | Muszą trafić do bazy jako NULL, nie jako pusty tekst ani 0. | rozszerzenie mssql: `SELECT COUNT(*) FROM TicketMachines WHERE LocationDescription IS NULL` |
| Puste pola w `Nr stanowiska`: 11 | Muszą trafić do bazy jako NULL, nie jako pusty tekst ani 0. | rozszerzenie mssql: `SELECT COUNT(*) FROM TicketMachines WHERE Platform IS NULL` |
| Puste pola w `Współrzędne geograficzne`: 1 | Muszą trafić do bazy jako NULL, nie jako pusty tekst ani 0. | rozszerzenie mssql: `SELECT COUNT(*) FROM TicketMachines WHERE Coordinates IS NULL` |
| Współrzędne „szerokość długość” w jednym polu, oddzielone spacją | Zostają tekstem (`Coordinates`). Nie rozdzielaj ich. | Watch na polu `Współrzędne geograficzne` |
| Ostatnia kolumna nie ma nazwy i jest pusta (średnik na końcu linii) | Nie importujemy jej; CsvHelper zobaczy 9 kolumn. | breakpoint + Watch na nagłówkach |

## Etap 2 – endpoint z filtrowaniem

| Pułapka | Co się stanie | Narzędzie |
|---|---|---|
| Fraza z ogonkami: „brynów” | Przy złym kodowaniu albo kulturze wyników jest 0. W adresie fraza jest zakodowana (`%C5%...`). | Network: adres żądania; log SQL: wartość parametru `q` |
| Wielkość liter i ogonki | Porównanie `=` albo LIKE z collation `CS`/`AS` nie znajdzie „BRYNÓW”. | log SQL: czy zapytanie ma `COLLATE ..._CI_AI` |
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
