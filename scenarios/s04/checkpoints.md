# Wartości kontrolne – s04: Bytom: rzeczy znalezione

Plik wygenerował `tools/Expect`. Nie edytuj ręcznie: zmień `mapping.json` i uruchom narzędzie ponownie.

- Plik: `data/raw/s04_bytom-rzeczy-znalezione.csv` (UTF-8, separator `,`, nagłówek w linii 1)
- Tabela: `BytomLostItems`, klucz: `Id`
- Atrybucja: Źródło: Bytom, otwartedane.metropoliagzm.pl, CC BY 4.0

Zasady: wiersze liczymy od 1 bez nagłówka (= `SourceRow`). Tekst jest przycięty (Trim), pusty tekst = NULL.
Filtr: fragment tekstu bez rozróżniania wielkości liter (`LIKE '%fraza%'`). „Dziwny” rekord: pole puste albo nieparsowalne.
Znak `|` w wartości jest zapisany jako `\|`, a nowa linia jako `\n`.

## E1 – import (dane → baza)

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E1.rows | Liczba wierszy w tabeli po imporcie | 451 |
| E1.fileRows | Wiersze danych w pliku (bez nagłówka) | 451 |
| E1.skipped | Pominięte wiersze (notatki pod tabelą) | brak |
| E1.duplicates | Duplikaty klucza (scalone przez upsert) | brak |
| E1.row7.raw | Wiersz 7, kolumna CSV `entryDate` – tekst w pliku | 1352851200000 |
| E1.row7.value | Wiersz 7, pole `EntryDate` – wartość w bazie | 2012-11-14 |
| E1.weird | „Dziwne” rekordy (co najmniej jedno pole puste albo nieparsowalne) | 7 |
| E1.weird.rows | Pierwsze „dziwne” rekordy (SourceRow) | 199, 208, 231, 259, 335 |
| E1.weird.EntryDate | Pole `EntryDate`: puste / nieparsowalne | 7 / 0 |
| E1.weird.Id | Pole `Id`: puste / nieparsowalne | 0 / 0 |
| E1.weird.ItemName | Pole `ItemName`: puste / nieparsowalne | 0 / 0 |
| E1.weird.Status | Pole `Status`: puste / nieparsowalne | 0 / 0 |
| E1.weird.Year | Pole `Year`: puste / nieparsowalne | 0 / 0 |
| E1.wrongFieldCount | Wiersze z inną liczbą pól niż nagłówek | 0 |

## E2 – endpoint z filtrowaniem

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E2.phrase | Fraza wpisana w wyszukiwarkę (pole `ItemName`) | wózek |
| E2.count | Liczba wyników | 5 |
| E2.result.1 | Wynik 1: SourceRow 88 | Wózek |
| E2.result.2 | Wynik 2: SourceRow 128 | Wózek |
| E2.result.3 | Wynik 3: SourceRow 237 | Wózek dzięcięcy |
| E2.result.4 | Wynik 4: SourceRow 256 | Wózek dzięcięcy |
| E2.result.5 | Wynik 5: SourceRow 439 | Wózek inwalidzki |

## E3 – statystyka

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E3.kind | Statystyka | count-by(Status) |
| E3.nulls | Pominięte puste wartości (NULL) | 0 |
| E3.group.1 | Miejsce 1: FINISHED | 402 |
| E3.group.2 | Miejsce 2: UNFINISHED | 49 |

## E4 – ulubione

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E4.title | Tytuł rekordu z wiersza 7 (pole `ItemName`) | Umowa |
