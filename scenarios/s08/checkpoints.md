# Wartości kontrolne – s08: Metrorower: rezerwacje

Plik wygenerował `tools/Expect`. Nie edytuj ręcznie: zmień `mapping.json` i uruchom narzędzie ponownie.

- Plik: `data/raw/s08_metrorower-rezerwacje.csv` (UTF-8, separator `,`, nagłówek w linii 1)
- Tabela: `BikeReservations`, klucz: `RowHash` (skrót wszystkich importowanych pól)
- Atrybucja: Źródło: Urząd Metropolitalny Górnośląsko-Zagłębiowskiej Metropolii, otwartedane.metropoliagzm.pl, CC BY 4.0

Zasady: wiersze liczymy od 1 bez nagłówka (= `SourceRow`). Tekst jest przycięty (Trim), pusty tekst = NULL.
Filtr: fragment tekstu bez rozróżniania wielkości liter (`LIKE '%fraza%'`). „Dziwny” rekord: pole puste albo nieparsowalne.
Znak `|` w wartości jest zapisany jako `\|`, a nowa linia jako `\n`.

## E1 – import (dane → baza)

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E1.rows | Liczba wierszy w tabeli po imporcie | 98 |
| E1.fileRows | Wiersze danych w pliku (bez nagłówka) | 100 |
| E1.skipped | Pominięte wiersze (notatki pod tabelą) | brak |
| E1.duplicates | Duplikaty klucza (scalone przez upsert) | 63, 74 |
| E1.row7.raw | Wiersz 7, kolumna CSV `Data` – tekst w pliku | 2026-10-06T14:55:37Z |
| E1.row7.value | Wiersz 7, pole `ReservedAt` – wartość w bazie | 2026-10-06 14:55:37 |
| E1.weird | „Dziwne” rekordy (co najmniej jedno pole puste albo nieparsowalne) | 12 |
| E1.weird.rows | Pierwsze „dziwne” rekordy (SourceRow) | 19, 48, 50, 52, 66 |
| E1.weird.ReservedAt | Pole `ReservedAt`: puste / nieparsowalne | 0 / 0 |
| E1.weird.Rented | Pole `Rented`: puste / nieparsowalne | 0 / 0 |
| E1.weird.RentedBikes | Pole `RentedBikes`: puste / nieparsowalne | 0 / 0 |
| E1.weird.ReservedBikes | Pole `ReservedBikes`: puste / nieparsowalne | 0 / 0 |
| E1.weird.StationNumber | Pole `StationNumber`: puste / nieparsowalne | 12 / 0 |
| E1.weird.StationName | Pole `StationName`: puste / nieparsowalne | 0 / 0 |
| E1.weird.Channel | Pole `Channel`: puste / nieparsowalne | 0 / 0 |
| E1.weird.Status | Pole `Status`: puste / nieparsowalne | 0 / 0 |
| E1.weird.ExpiresAt | Pole `ExpiresAt`: puste / nieparsowalne | 0 / 0 |
| E1.wrongFieldCount | Wiersze z inną liczbą pól niż nagłówek | 0 |

## E2 – endpoint z filtrowaniem

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E2.phrase | Fraza wpisana w wyszukiwarkę (pole `StationName`) | 27402 |
| E2.count | Liczba wyników | 5 |
| E2.result.1 | Wynik 1: SourceRow 67 | 27402 |
| E2.result.2 | Wynik 2: SourceRow 72 | 27402 |
| E2.result.3 | Wynik 3: SourceRow 73 | 27402 |
| E2.result.4 | Wynik 4: SourceRow 86 | 27402 |
| E2.result.5 | Wynik 5: SourceRow 97 | 27402 |

## E3 – statystyka

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E3.kind | Statystyka | count-by(Status) |
| E3.nulls | Pominięte puste wartości (NULL) | 0 |
| E3.group.1 | Miejsce 1: CLOSED | 54 |
| E3.group.2 | Miejsce 2: CANCELLED | 26 |
| E3.group.3 | Miejsce 3: EXPIRED | 18 |

## E4 – ulubione

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E4.title | Tytuł rekordu z wiersza 7 (pole `StationName`) | 27681 |
