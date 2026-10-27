# Wartości kontrolne – s06: Dąbrowa Górnicza: pomniki przyrody

Plik wygenerował `tools/Expect`. Nie edytuj ręcznie: zmień `mapping.json` i uruchom narzędzie ponownie.

- Plik: `data/raw/s06_dabrowa-pomniki-przyrody.csv` (UTF-8, separator `,`, nagłówek w linii 1)
- Tabela: `NatureMonuments`, klucz: `Id`
- Atrybucja: Źródło: Dąbrowa Górnicza, otwartedane.metropoliagzm.pl, CC BY 4.0

Zasady: wiersze liczymy od 1 bez nagłówka (= `SourceRow`). Tekst jest przycięty (Trim), pusty tekst = NULL.
Filtr: fragment tekstu bez rozróżniania wielkości liter (`LIKE '%fraza%'`). „Dziwny” rekord: pole puste albo nieparsowalne.
Znak `|` w wartości jest zapisany jako `\|`, a nowa linia jako `\n`.

## E1 – import (dane → baza)

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E1.rows | Liczba wierszy w tabeli po imporcie | 31 |
| E1.fileRows | Wiersze danych w pliku (bez nagłówka) | 31 |
| E1.skipped | Pominięte wiersze (notatki pod tabelą) | brak |
| E1.duplicates | Duplikaty klucza (scalone przez upsert) | brak |
| E1.row7.raw | Wiersz 7, kolumna CSV `circumference` – tekst w pliku | 290 |
| E1.row7.value | Wiersz 7, pole `Circumference` – wartość w bazie | 290 |
| E1.weird | „Dziwne” rekordy (co najmniej jedno pole puste albo nieparsowalne) | 2 |
| E1.weird.rows | Pierwsze „dziwne” rekordy (SourceRow) | 7, 31 |
| E1.weird.CadastralDistrict | Pole `CadastralDistrict`: puste / nieparsowalne | 0 / 0 |
| E1.weird.Circumference | Pole `Circumference`: puste / nieparsowalne | 1 / 0 |
| E1.weird.Coordinates | Pole `Coordinates`: puste / nieparsowalne | 1 / 0 |
| E1.weird.Description | Pole `Description`: puste / nieparsowalne | 0 / 0 |
| E1.weird.Id | Pole `Id`: puste / nieparsowalne | 0 / 0 |
| E1.weird.Name | Pole `Name`: puste / nieparsowalne | 0 / 0 |
| E1.wrongFieldCount | Wiersze z inną liczbą pól niż nagłówek | 0 |

## E2 – endpoint z filtrowaniem

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E2.phrase | Fraza wpisana w wyszukiwarkę (pole `Name`) | wyniosły |
| E2.count | Liczba wyników | 3 |
| E2.result.1 | Wynik 1: SourceRow 5 | Jesion wyniosły (Fraxinus excelsior) |
| E2.result.2 | Wynik 2: SourceRow 14 | Jesion wyniosły (Fraxinus excelsior) |
| E2.result.3 | Wynik 3: SourceRow 28 | Jesion wyniosły (Fraxinus excelsior) |

## E3 – statystyka

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E3.kind | Statystyka | avg(Circumference) |
| E3.nulls | Pominięte puste wartości (NULL) | 1 |
| E3.value | Średnia (2 miejsca po przecinku) | 311.40 |

## E4 – ulubione

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E4.title | Tytuł rekordu z wiersza 7 (pole `Name`) | Buk pospolity (Fagus sylvatica) |
