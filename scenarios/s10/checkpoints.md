# Wartości kontrolne – s10: Gliwice: boiska i stadiony

Plik wygenerował `tools/Expect`. Nie edytuj ręcznie: zmień `mapping.json` i uruchom narzędzie ponownie.

- Plik: `data/raw/s10_gliwice-boiska-stadiony.csv` (UTF-8, separator `,`, nagłówek w linii 1)
- Tabela: `SportsFields`, klucz: `Fid`
- Atrybucja: Źródło: Gliwice, otwartedane.metropoliagzm.pl, CC BY 4.0

Zasady: wiersze liczymy od 1 bez nagłówka (= `SourceRow`). Tekst jest przycięty (Trim), pusty tekst = NULL.
Filtr: fragment tekstu bez rozróżniania wielkości liter (`LIKE '%fraza%'`). „Dziwny” rekord: pole puste albo nieparsowalne.
Znak `|` w wartości jest zapisany jako `\|`, a nowa linia jako `\n`.

## E1 – import (dane → baza)

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E1.rows | Liczba wierszy w tabeli po imporcie | 121 |
| E1.fileRows | Wiersze danych w pliku (bez nagłówka) | 121 |
| E1.skipped | Pominięte wiersze (notatki pod tabelą) | brak |
| E1.duplicates | Duplikaty klucza (scalone przez upsert) | brak |
| E1.row7.raw | Wiersz 7, kolumna CSV `x` – tekst w pliku | 2077273.6463570932 |
| E1.row7.value | Wiersz 7, pole `X` – wartość w bazie | 2077273.6463570932 |
| E1.weird | „Dziwne” rekordy (co najmniej jedno pole puste albo nieparsowalne) | 2 |
| E1.weird.rows | Pierwsze „dziwne” rekordy (SourceRow) | 64, 115 |
| E1.weird.Fid | Pole `Fid`: puste / nieparsowalne | 0 / 0 |
| E1.weird.Kind | Pole `Kind`: puste / nieparsowalne | 0 / 0 |
| E1.weird.Name | Pole `Name`: puste / nieparsowalne | 2 / 0 |
| E1.weird.Address | Pole `Address`: puste / nieparsowalne | 0 / 0 |
| E1.weird.X | Pole `X`: puste / nieparsowalne | 0 / 0 |
| E1.weird.Y | Pole `Y`: puste / nieparsowalne | 0 / 0 |
| E1.wrongFieldCount | Wiersze z inną liczbą pól niż nagłówek | 0 |

## E2 – endpoint z filtrowaniem

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E2.phrase | Fraza wpisana w wyszukiwarkę (pole `Name`) | śląska |
| E2.count | Liczba wyników | 5 |
| E2.result.1 | Wynik 1: SourceRow 63 | Politechnika Śląska |
| E2.result.2 | Wynik 2: SourceRow 101 | Politechnika Śląska |
| E2.result.3 | Wynik 3: SourceRow 102 | Politechnika Śląska |
| E2.result.4 | Wynik 4: SourceRow 103 | Politechnika Śląska |
| E2.result.5 | Wynik 5: SourceRow 105 | Politechnika Śląska |

## E3 – statystyka

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E3.kind | Statystyka | top5(Kind) |
| E3.nulls | Pominięte puste wartości (NULL) | 0 |
| E3.group.1 | Miejsce 1: boisko do piłki nożnej i koszykówki | 26 |
| E3.group.2 | Miejsce 2: boisko do piłki nożnej | 19 |
| E3.group.3 | Miejsce 3: boisko do koszykówki | 10 |
| E3.group.4 | Miejsce 4: boisko do koszykówki i siatkówki | 8 |
| E3.group.5 | Miejsce 5: boisko trawiaste do piłki nożnej | 8 |

## E4 – ulubione

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E4.title | Tytuł rekordu z wiersza 7 (pole `Name`) | Szkoła Podstawowa Nr 6 |
