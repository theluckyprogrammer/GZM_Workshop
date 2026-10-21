# Wartości kontrolne – s05: Dąbrowa Górnicza: rzeczy znalezione

Plik wygenerował `tools/Expect`. Nie edytuj ręcznie: zmień `mapping.json` i uruchom narzędzie ponownie.

- Plik: `data/raw/s05_dabrowa-rzeczy-znalezione.csv` (UTF-8, separator `,`, nagłówek w linii 1)
- Tabela: `DabrowaLostItems`, klucz: `Id`
- Atrybucja: Źródło: Dąbrowa Górnicza, otwartedane.metropoliagzm.pl, CC BY 4.0

Zasady: wiersze liczymy od 1 bez nagłówka (= `SourceRow`). Tekst jest przycięty (Trim), pusty tekst = NULL.
Filtr: fragment tekstu bez rozróżniania wielkości liter (`LIKE '%fraza%'`). „Dziwny” rekord: pole puste albo nieparsowalne.
Znak `|` w wartości jest zapisany jako `\|`, a nowa linia jako `\n`.

## E1 – import (dane → baza)

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E1.rows | Liczba wierszy w tabeli po imporcie | 64 |
| E1.fileRows | Wiersze danych w pliku (bez nagłówka) | 64 |
| E1.skipped | Pominięte wiersze (notatki pod tabelą) | brak |
| E1.duplicates | Duplikaty klucza (scalone przez MERGE) | brak |
| E1.row7.raw | Wiersz 7, kolumna CSV `foundDate` – tekst w pliku | 1732147200000 |
| E1.row7.value | Wiersz 7, pole `FoundDate` – wartość w bazie | 2024-11-21 |
| E1.weird | „Dziwne” rekordy (co najmniej jedno pole puste albo nieparsowalne) | 0 |
| E1.weird.rows | Pierwsze „dziwne” rekordy (SourceRow) | brak |
| E1.weird.CollectionDate | Pole `CollectionDate`: puste / nieparsowalne | 0 / 0 |
| E1.weird.FoundDate | Pole `FoundDate`: puste / nieparsowalne | 0 / 0 |
| E1.weird.Id | Pole `Id`: puste / nieparsowalne | 0 / 0 |
| E1.weird.ItemName | Pole `ItemName`: puste / nieparsowalne | 0 / 0 |
| E1.wrongFieldCount | Wiersze z inną liczbą pól niż nagłówek | 0 |

## E2 – endpoint z filtrowaniem

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E2.phrase | Fraza wpisana w wyszukiwarkę (pole `ItemName`) | zawartością |
| E2.count | Liczba wyników | 4 |
| E2.result.1 | Wynik 1: SourceRow 5 | Torba z zawartością |
| E2.result.2 | Wynik 2: SourceRow 7 | Plecak z zawartością |
| E2.result.3 | Wynik 3: SourceRow 21 | Walizka z zawartością |
| E2.result.4 | Wynik 4: SourceRow 57 | Torebka z zawartością |

## E3 – statystyka

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E3.kind | Statystyka | top5(ItemName) |
| E3.nulls | Pominięte puste wartości (NULL) | 0 |
| E3.group.1 | Miejsce 1: Telefon komórkowy | 20 |
| E3.group.2 | Miejsce 2: Portfel | 5 |
| E3.group.3 | Miejsce 3: Pieniądze | 4 |
| E3.group.4 | Miejsce 4: Portfel + pieniądze | 3 |
| E3.group.5 | Miejsce 5: Klucze | 2 |

## E4 – ulubione

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E4.title | Tytuł rekordu z wiersza 7 (pole `ItemName`) | Plecak z zawartością |
