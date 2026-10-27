# Wartości kontrolne – s02: Metrorower: stacje aktualne

Plik wygenerował `tools/Expect`. Nie edytuj ręcznie: zmień `mapping.json` i uruchom narzędzie ponownie.

- Plik: `data/raw/s02_metrorower-stacje.csv` (UTF-8, separator `,`, nagłówek w linii 1)
- Tabela: `BikeStations`, klucz: `Number`
- Atrybucja: Źródło: Urząd Metropolitalny Górnośląsko-Zagłębiowskiej Metropolii, otwartedane.metropoliagzm.pl, CC BY 4.0

Zasady: wiersze liczymy od 1 bez nagłówka (= `SourceRow`). Tekst jest przycięty (Trim), pusty tekst = NULL.
Filtr: fragment tekstu bez rozróżniania wielkości liter (`LIKE '%fraza%'`). „Dziwny” rekord: pole puste albo nieparsowalne.
Znak `|` w wartości jest zapisany jako `\|`, a nowa linia jako `\n`.

## E1 – import (dane → baza)

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E1.rows | Liczba wierszy w tabeli po imporcie | 945 |
| E1.fileRows | Wiersze danych w pliku (bez nagłówka) | 945 |
| E1.skipped | Pominięte wiersze (notatki pod tabelą) | brak |
| E1.duplicates | Duplikaty klucza (scalone przez upsert) | brak |
| E1.row7.raw | Wiersz 7, kolumna CSV `Lat` – tekst w pliku | 50.275952 |
| E1.row7.value | Wiersz 7, pole `Lat` – wartość w bazie | 50.275952000 |
| E1.weird | „Dziwne” rekordy (co najmniej jedno pole puste albo nieparsowalne) | 0 |
| E1.weird.rows | Pierwsze „dziwne” rekordy (SourceRow) | brak |
| E1.weird.Name | Pole `Name`: puste / nieparsowalne | 0 / 0 |
| E1.weird.Number | Pole `Number`: puste / nieparsowalne | 0 / 0 |
| E1.weird.Area | Pole `Area`: puste / nieparsowalne | 0 / 0 |
| E1.weird.BaseBikes | Pole `BaseBikes`: puste / nieparsowalne | 0 / 0 |
| E1.weird.Racks | Pole `Racks`: puste / nieparsowalne | 0 / 0 |
| E1.weird.Available | Pole `Available`: puste / nieparsowalne | 0 / 0 |
| E1.weird.Lat | Pole `Lat`: puste / nieparsowalne | 0 / 0 |
| E1.weird.Lng | Pole `Lng`: puste / nieparsowalne | 0 / 0 |
| E1.wrongFieldCount | Wiersze z inną liczbą pól niż nagłówek | 0 |

## E2 – endpoint z filtrowaniem

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E2.phrase | Fraza wpisana w wyszukiwarkę (pole `Area`) | bieruń |
| E2.count | Liczba wyników | 6 |
| E2.result.1 | Wynik 1: SourceRow 25 | 27025 |
| E2.result.2 | Wynik 2: SourceRow 231 | 27231 |
| E2.result.3 | Wynik 3: SourceRow 254 | 27254 |
| E2.result.4 | Wynik 4: SourceRow 259 | 27259 |
| E2.result.5 | Wynik 5: SourceRow 543 | 27544 |
| E2.result.6 | Wynik 6: SourceRow 902 | 27909 |

## E3 – statystyka

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E3.kind | Statystyka | sum(Racks) |
| E3.nulls | Pominięte puste wartości (NULL) | 0 |
| E3.value | Suma | 4750 |

## E4 – ulubione

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E4.title | Tytuł rekordu z wiersza 7 (pole `Name`) | 27007 |
