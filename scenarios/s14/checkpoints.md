# Wartości kontrolne – s14: ZTM: automaty biletowe

Plik wygenerował `tools/Expect`. Nie edytuj ręcznie: zmień `mapping.json` i uruchom narzędzie ponownie.

- Plik: `data/raw/s14_ztm-automaty-biletowe.csv` (Windows-1250, separator `;`, nagłówek w linii 5)
- Tabela: `TicketMachines`, klucz: `MachineCode`
- Atrybucja: Źródło: Zarząd Transportu Metropolitalnego, otwartedane.metropoliagzm.pl, Creative Commons Attribution

Zasady: wiersze liczymy od 1 bez nagłówka (= `SourceRow`). Tekst jest przycięty (Trim), pusty tekst = NULL.
Filtr: fragment tekstu bez rozróżniania wielkości liter (`LIKE '%fraza%'`). „Dziwny” rekord: pole puste albo nieparsowalne.
Znak `|` w wartości jest zapisany jako `\|`, a nowa linia jako `\n`.

## E1 – import (dane → baza)

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E1.rows | Liczba wierszy w tabeli po imporcie | 266 |
| E1.fileRows | Wiersze danych w pliku (bez nagłówka) | 272 |
| E1.skipped | Pominięte wiersze (notatki pod tabelą) | 267, 268, 269, 270, 271, 272 |
| E1.duplicates | Duplikaty klucza (scalone przez upsert) | brak |
| E1.row7.raw | Wiersz 7, kolumna CSV `Współrzędne geograficzne` – tekst w pliku | 50.39760177303035 18.877324014902115 |
| E1.row7.value | Wiersz 7, pole `Coordinates` – wartość w bazie | 50.39760177303035 18.877324014902115 |
| E1.weird | „Dziwne” rekordy (co najmniej jedno pole puste albo nieparsowalne) | 25 |
| E1.weird.rows | Pierwsze „dziwne” rekordy (SourceRow) | 45, 85, 94, 99, 100 |
| E1.weird.No | Pole `No`: puste / nieparsowalne | 0 / 0 |
| E1.weird.Commune | Pole `Commune`: puste / nieparsowalne | 0 / 0 |
| E1.weird.MachineCode | Pole `MachineCode`: puste / nieparsowalne | 0 / 0 |
| E1.weird.StopNumber | Pole `StopNumber`: puste / nieparsowalne | 14 / 0 |
| E1.weird.StopName | Pole `StopName`: puste / nieparsowalne | 0 / 0 |
| E1.weird.LocationDescription | Pole `LocationDescription`: puste / nieparsowalne | 1 / 0 |
| E1.weird.Platform | Pole `Platform`: puste / nieparsowalne | 11 / 0 |
| E1.weird.Coordinates | Pole `Coordinates`: puste / nieparsowalne | 1 / 0 |
| E1.wrongFieldCount | Wiersze z inną liczbą pól niż nagłówek | 0 |

## E2 – endpoint z filtrowaniem

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E2.phrase | Fraza wpisana w wyszukiwarkę (pole `StopName`) | brynów |
| E2.count | Liczba wyników | 5 |
| E2.result.1 | Wynik 1: SourceRow 96 | Brynów Centrum Przesiadkowe |
| E2.result.2 | Wynik 2: SourceRow 97 | Brynów Centrum Przesiadkowe |
| E2.result.3 | Wynik 3: SourceRow 98 | Brynów Centrum Przesiadkowe |
| E2.result.4 | Wynik 4: SourceRow 99 | Brynów Centrum Przesiadkowe |
| E2.result.5 | Wynik 5: SourceRow 100 | Brynów Centrum Przesiadkowe |

## E3 – statystyka

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E3.kind | Statystyka | top5(Commune) |
| E3.nulls | Pominięte puste wartości (NULL) | 0 |
| E3.group.1 | Miejsce 1: KATOWICE | 47 |
| E3.group.2 | Miejsce 2: SOSNOWIEC | 29 |
| E3.group.3 | Miejsce 3: GLIWICE | 18 |
| E3.group.4 | Miejsce 4: BYTOM | 13 |
| E3.group.5 | Miejsce 5: ZABRZE | 13 |

## E4 – ulubione

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E4.title | Tytuł rekordu z wiersza 7 (pole `StopName`) | Stroszek Osiedle |
