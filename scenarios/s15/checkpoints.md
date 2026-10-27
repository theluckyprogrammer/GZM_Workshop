# Wartości kontrolne – s15: Katowice: uczniowskie kluby sportowe

Plik wygenerował `tools/Expect`. Nie edytuj ręcznie: zmień `mapping.json` i uruchom narzędzie ponownie.

- Plik: `data/raw/s15_katowice-uczniowskie-kluby-sportowe.csv` (UTF-8, separator `,`, nagłówek w linii 1)
- Tabela: `SchoolSportsClubs`, klucz: `Id`
- Atrybucja: Źródło: Katowice, otwartedane.metropoliagzm.pl, CC BY 4.0

Zasady: wiersze liczymy od 1 bez nagłówka (= `SourceRow`). Tekst jest przycięty (Trim), pusty tekst = NULL.
Filtr: fragment tekstu bez rozróżniania wielkości liter (`LIKE '%fraza%'`). „Dziwny” rekord: pole puste albo nieparsowalne.
Znak `|` w wartości jest zapisany jako `\|`, a nowa linia jako `\n`.

## E1 – import (dane → baza)

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E1.rows | Liczba wierszy w tabeli po imporcie | 94 |
| E1.fileRows | Wiersze danych w pliku (bez nagłówka) | 94 |
| E1.skipped | Pominięte wiersze (notatki pod tabelą) | brak |
| E1.duplicates | Duplikaty klucza (scalone przez upsert) | brak |
| E1.row7.raw | Wiersz 7, kolumna CSV `registrationDate` – tekst w pliku | 995846400000 |
| E1.row7.value | Wiersz 7, pole `RegisteredOn` – wartość w bazie | 2001-07-23 |
| E1.weird | „Dziwne” rekordy (co najmniej jedno pole puste albo nieparsowalne) | 2 |
| E1.weird.rows | Pierwsze „dziwne” rekordy (SourceRow) | 41, 58 |
| E1.weird.Id | Pole `Id`: puste / nieparsowalne | 0 / 0 |
| E1.weird.Name | Pole `Name`: puste / nieparsowalne | 0 / 0 |
| E1.weird.RegisteredOn | Pole `RegisteredOn`: puste / nieparsowalne | 2 / 0 |
| E1.weird.Street | Pole `Street`: puste / nieparsowalne | 0 / 0 |
| E1.weird.Status | Pole `Status`: puste / nieparsowalne | 0 / 0 |
| E1.wrongFieldCount | Wiersze z inną liczbą pól niż nagłówek | 0 |

## E2 – endpoint z filtrowaniem

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E2.phrase | Fraza wpisana w wyszukiwarkę (pole `Name`) | naprzód |
| E2.count | Liczba wyników | 4 |
| E2.result.1 | Wynik 1: SourceRow 8 | Uczniowski Klub Sportowy\n„Naprzód” Katowice |
| E2.result.2 | Wynik 2: SourceRow 10 | Hokejowy Uczniowski Klub Sportowy „Naprzód Pawłowski Janów” |
| E2.result.3 | Wynik 3: SourceRow 19 | Międzyszkolny Uczniowski Klub Sportowy „ Naprzód Janów” |
| E2.result.4 | Wynik 4: SourceRow 85 | Uczniowski Klub Sportowy Katowice Naprzód Janów |

## E3 – statystyka

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E3.kind | Statystyka | count-by(Status) |
| E3.nulls | Pominięte puste wartości (NULL) | 0 |
| E3.group.1 | Miejsce 1: ACTIVE | 58 |
| E3.group.2 | Miejsce 2: UNKNOWN | 22 |
| E3.group.3 | Miejsce 3: EXPUNGED | 14 |

## E4 – ulubione

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E4.title | Tytuł rekordu z wiersza 7 (pole `Name`) | Międzyszkolny Uczniowski \nKlub Sportowy „Tornado” |
