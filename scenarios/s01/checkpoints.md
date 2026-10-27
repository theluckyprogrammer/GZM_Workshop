# Wartości kontrolne – s01: Zabrze: miejsca pamięci

Plik wygenerował `tools/Expect`. Nie edytuj ręcznie: zmień `mapping.json` i uruchom narzędzie ponownie.

- Plik: `data/raw/s01_zabrze-miejsca-pamieci.csv` (UTF-8, separator `,`, nagłówek w linii 1)
- Tabela: `ZabrzeMemorials`, klucz: `RecordNumber`
- Atrybucja: Źródło: Zabrze, otwartedane.metropoliagzm.pl, CC BY 4.0

Zasady: wiersze liczymy od 1 bez nagłówka (= `SourceRow`). Tekst jest przycięty (Trim), pusty tekst = NULL.
Filtr: fragment tekstu bez rozróżniania wielkości liter (`LIKE '%fraza%'`). „Dziwny” rekord: pole puste albo nieparsowalne.
Znak `|` w wartości jest zapisany jako `\|`, a nowa linia jako `\n`.

## E1 – import (dane → baza)

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E1.rows | Liczba wierszy w tabeli po imporcie | 58 |
| E1.fileRows | Wiersze danych w pliku (bez nagłówka) | 58 |
| E1.skipped | Pominięte wiersze (notatki pod tabelą) | brak |
| E1.duplicates | Duplikaty klucza (scalone przez upsert) | brak |
| E1.row7.raw | Wiersz 7, kolumna CSV `addresscity` – tekst w pliku | Zabrze |
| E1.row7.value | Wiersz 7, pole `City` – wartość w bazie | Zabrze |
| E1.weird | „Dziwne” rekordy (co najmniej jedno pole puste albo nieparsowalne) | 20 |
| E1.weird.rows | Pierwsze „dziwne” rekordy (SourceRow) | 3, 4, 9, 10, 11 |
| E1.weird.City | Pole `City`: puste / nieparsowalne | 6 / 0 |
| E1.weird.Street | Pole `Street`: puste / nieparsowalne | 6 / 0 |
| E1.weird.Description | Pole `Description`: puste / nieparsowalne | 0 / 0 |
| E1.weird.Inscription | Pole `Inscription`: puste / nieparsowalne | 3 / 0 |
| E1.weird.LocationName | Pole `LocationName`: puste / nieparsowalne | 11 / 0 |
| E1.weird.RecordNumber | Pole `RecordNumber`: puste / nieparsowalne | 0 / 0 |
| E1.wrongFieldCount | Wiersze z inną liczbą pól niż nagłówek | 0 |

## E2 – endpoint z filtrowaniem

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E2.phrase | Fraza wpisana w wyszukiwarkę (pole `Description`) | którym |
| E2.count | Liczba wyników | 5 |
| E2.result.1 | Wynik 1: SourceRow 1 | Szkoła Podstawowa Nr 26 |
| E2.result.2 | Wynik 2: SourceRow 2 | ul. Kasprowicza i róg Hagera |
| E2.result.3 | Wynik 3: SourceRow 8 | róg ul. Wolności |
| E2.result.4 | Wynik 4: SourceRow 29 | NULL |
| E2.result.5 | Wynik 5: SourceRow 32 | Budynek Teatru Nowego |

## E3 – statystyka

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E3.kind | Statystyka | top5(City) |
| E3.nulls | Pominięte puste wartości (NULL) | 6 |
| E3.group.1 | Miejsce 1: Zabrze | 37 |
| E3.group.2 | Miejsce 2: Zabrze – Biskupice | 3 |
| E3.group.3 | Miejsce 3: Zabrze – Kończyce | 2 |
| E3.group.4 | Miejsce 4: Zabrze-Mikulczyce | 2 |
| E3.group.5 | Miejsce 5: Zabrze-Rokitnica | 2 |

## E4 – ulubione

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E4.title | Tytuł rekordu z wiersza 7 (pole `LocationName`) | budynek MPWiK |
