# Wartości kontrolne – s03: Dzienna dostępność rowerów w gminach

Plik wygenerował `tools/Expect`. Nie edytuj ręcznie: zmień `mapping.json` i uruchom narzędzie ponownie.

- Plik: `data/raw/s03_rowery-dostepnosc-gminy.csv` (UTF-8, separator `,`, nagłówek w linii 1)
- Tabela: `BikeAvailability`, klucz: `Day` + `Commune`
- Atrybucja: Źródło: Urząd Metropolitalny Górnośląsko-Zagłębiowskiej Metropolii, otwartedane.metropoliagzm.pl, CC BY 4.0

Zasady: wiersze liczymy od 1 bez nagłówka (= `SourceRow`). Tekst jest przycięty (Trim), pusty tekst = NULL.
Filtr: fragment tekstu bez rozróżniania wielkości liter (`LIKE '%fraza%'`). „Dziwny” rekord: pole puste albo nieparsowalne.
Znak `|` w wartości jest zapisany jako `\|`, a nowa linia jako `\n`.

## E1 – import (dane → baza)

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E1.rows | Liczba wierszy w tabeli po imporcie | 40 |
| E1.fileRows | Wiersze danych w pliku (bez nagłówka) | 40 |
| E1.skipped | Pominięte wiersze (notatki pod tabelą) | brak |
| E1.duplicates | Duplikaty klucza (scalone przez upsert) | brak |
| E1.row7.raw | Wiersz 7, kolumna CSV `Liczba dostępnych rowerów` – tekst w pliku | 400 |
| E1.row7.value | Wiersz 7, pole `AvailableBikes` – wartość w bazie | 400 |
| E1.weird | „Dziwne” rekordy (co najmniej jedno pole puste albo nieparsowalne) | 0 |
| E1.weird.rows | Pierwsze „dziwne” rekordy (SourceRow) | brak |
| E1.weird.Day | Pole `Day`: puste / nieparsowalne | 0 / 0 |
| E1.weird.Commune | Pole `Commune`: puste / nieparsowalne | 0 / 0 |
| E1.weird.AvailableBikes | Pole `AvailableBikes`: puste / nieparsowalne | 0 / 0 |
| E1.weird.RequiredBikes | Pole `RequiredBikes`: puste / nieparsowalne | 0 / 0 |
| E1.weird.Exceeded | Pole `Exceeded`: puste / nieparsowalne | 0 / 0 |
| E1.wrongFieldCount | Wiersze z inną liczbą pól niż nagłówek | 0 |

## E2 – endpoint z filtrowaniem

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E2.phrase | Fraza wpisana w wyszukiwarkę (pole `Commune`) | śląski |
| E2.count | Liczba wyników | 3 |
| E2.result.1 | Wynik 1: SourceRow 6 | Chełm Śląski |
| E2.result.2 | Wynik 2: SourceRow 20 | Piekary Śląskie |
| E2.result.3 | Wynik 3: SourceRow 27 | Siemianowice Śląskie |

## E3 – statystyka

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E3.kind | Statystyka | sum(AvailableBikes) |
| E3.nulls | Pominięte puste wartości (NULL) | 0 |
| E3.value | Suma | 7449 |

## E4 – ulubione

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E4.title | Tytuł rekordu z wiersza 7 (pole `Commune`) | Chorzów |
