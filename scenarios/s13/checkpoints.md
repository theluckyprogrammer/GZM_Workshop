# Wartości kontrolne – s13: SCK Siemianowice: zajęcia sekcyjne

Plik wygenerował `tools/Expect`. Nie edytuj ręcznie: zmień `mapping.json` i uruchom narzędzie ponownie.

- Plik: `data/raw/s13_sck-zajecia-sekcyjne.csv` (UTF-8, separator `,`, nagłówek w linii 1)
- Tabela: `CultureClasses`, klucz: `Id`
- Atrybucja: Źródło: Siemianowickie Centrum Kultury, otwartedane.metropoliagzm.pl, CC BY 4.0

Zasady: wiersze liczymy od 1 bez nagłówka (= `SourceRow`). Tekst jest przycięty (Trim), pusty tekst = NULL.
Filtr: fragment tekstu bez rozróżniania wielkości liter (`LIKE '%fraza%'`). „Dziwny” rekord: pole puste albo nieparsowalne.
Znak `|` w wartości jest zapisany jako `\|`, a nowa linia jako `\n`.

## E1 – import (dane → baza)

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E1.rows | Liczba wierszy w tabeli po imporcie | 38 |
| E1.fileRows | Wiersze danych w pliku (bez nagłówka) | 38 |
| E1.skipped | Pominięte wiersze (notatki pod tabelą) | brak |
| E1.duplicates | Duplikaty klucza (scalone przez upsert) | brak |
| E1.row7.raw | Wiersz 7, kolumna CSV `minimalAge` – tekst w pliku | 5 |
| E1.row7.value | Wiersz 7, pole `MinAge` – wartość w bazie | 5.0 |
| E1.weird | „Dziwne” rekordy (co najmniej jedno pole puste albo nieparsowalne) | 23 |
| E1.weird.rows | Pierwsze „dziwne” rekordy (SourceRow) | 4, 6, 7, 8, 9 |
| E1.weird.ClassType | Pole `ClassType`: puste / nieparsowalne | 0 / 0 |
| E1.weird.Day | Pole `Day`: puste / nieparsowalne | 4 / 0 |
| E1.weird.Id | Pole `Id`: puste / nieparsowalne | 0 / 0 |
| E1.weird.MinAge | Pole `MinAge`: puste / nieparsowalne | 12 / 0 |
| E1.weird.Room | Pole `Room`: puste / nieparsowalne | 5 / 0 |
| E1.weird.Section | Pole `Section`: puste / nieparsowalne | 0 / 0 |
| E1.weird.StartHour | Pole `StartHour`: puste / nieparsowalne | 10 / 0 |
| E1.weird.Unit | Pole `Unit`: puste / nieparsowalne | 0 / 0 |
| E1.wrongFieldCount | Wiersze z inną liczbą pól niż nagłówek | 0 |

## E2 – endpoint z filtrowaniem

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E2.phrase | Fraza wpisana w wyszukiwarkę (pole `Section`) | dziecięcy |
| E2.count | Liczba wyników | 4 |
| E2.result.1 | Wynik 1: SourceRow 13 | Dziecięcy ZPiT Siemianowice |
| E2.result.2 | Wynik 2: SourceRow 14 | Dziecięcy ZPiT Siemianowice |
| E2.result.3 | Wynik 3: SourceRow 15 | Dziecięcy ZPiT Siemianowice |
| E2.result.4 | Wynik 4: SourceRow 16 | Dziecięcy ZPiT Siemianowice |

## E3 – statystyka

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E3.kind | Statystyka | count-by(ClassType) |
| E3.nulls | Pominięte puste wartości (NULL) | 0 |
| E3.group.1 | Miejsce 1: taneczne | 15 |
| E3.group.2 | Miejsce 2: muzyczne | 6 |
| E3.group.3 | Miejsce 3: rękodzieło | 6 |
| E3.group.4 | Miejsce 4: teatralne | 5 |
| E3.group.5 | Miejsce 5: plastyczne | 2 |
| E3.group.6 | Miejsce 6: taneczne, wokalne | 2 |
| E3.group.7 | Miejsce 7: Indywidualne lekcje śpiewu | 1 |
| E3.group.8 | Miejsce 8: wokalne | 1 |

## E4 – ulubione

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E4.title | Tytuł rekordu z wiersza 7 (pole `Section`) | Pracownia Plastyczna „Okienko” |
