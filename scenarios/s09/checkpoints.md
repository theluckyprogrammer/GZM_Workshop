# Wartości kontrolne – s09: Dąbrowa Górnicza: zabytki archeologiczne

Plik wygenerował `tools/Expect`. Nie edytuj ręcznie: zmień `mapping.json` i uruchom narzędzie ponownie.

- Plik: `data/raw/s09_dabrowa-zabytki-archeologiczne.csv` (UTF-8, separator `,`, nagłówek w linii 1)
- Tabela: `ArchaeologicalSites`, klucz: `Id`
- Atrybucja: Źródło: Dąbrowa Górnicza, otwartedane.metropoliagzm.pl, CC BY 4.0

Zasady: wiersze liczymy od 1 bez nagłówka (= `SourceRow`). Tekst jest przycięty (Trim), pusty tekst = NULL.
Filtr: fragment tekstu bez rozróżniania wielkości liter (`LIKE '%fraza%'`). „Dziwny” rekord: pole puste albo nieparsowalne.
Znak `|` w wartości jest zapisany jako `\|`, a nowa linia jako `\n`.

## E1 – import (dane → baza)

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E1.rows | Liczba wierszy w tabeli po imporcie | 135 |
| E1.fileRows | Wiersze danych w pliku (bez nagłówka) | 135 |
| E1.skipped | Pominięte wiersze (notatki pod tabelą) | brak |
| E1.duplicates | Duplikaty klucza (scalone przez upsert) | brak |
| E1.row7.raw | Wiersz 7, kolumna CSV `chronology` – tekst w pliku | Wczesne średniowiecze - XI-XII/XIIIw.; Okres nowożytny - XVII-XIXw. |
| E1.row7.value | Wiersz 7, pole `Chronology` – wartość w bazie | Wczesne średniowiecze - XI-XII/XIIIw.; Okres nowożytny - XVII-XIXw. |
| E1.weird | „Dziwne” rekordy (co najmniej jedno pole puste albo nieparsowalne) | 8 |
| E1.weird.rows | Pierwsze „dziwne” rekordy (SourceRow) | 1, 12, 25, 34, 35 |
| E1.weird.AzpArea | Pole `AzpArea`: puste / nieparsowalne | 0 / 0 |
| E1.weird.SiteNumberInCity | Pole `SiteNumberInCity`: puste / nieparsowalne | 0 / 0 |
| E1.weird.AzpNumber | Pole `AzpNumber`: puste / nieparsowalne | 0 / 0 |
| E1.weird.Chronology | Pole `Chronology`: puste / nieparsowalne | 4 / 0 |
| E1.weird.Comment | Pole `Comment`: puste / nieparsowalne | 8 / 0 |
| E1.weird.Id | Pole `Id`: puste / nieparsowalne | 0 / 0 |
| E1.weird.LocalName | Pole `LocalName`: puste / nieparsowalne | 0 / 0 |
| E1.wrongFieldCount | Wiersze z inną liczbą pól niż nagłówek | 0 |

## E2 – endpoint z filtrowaniem

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E2.phrase | Fraza wpisana w wyszukiwarkę (pole `LocalName`) | gródki |
| E2.count | Liczba wyników | 3 |
| E2.result.1 | Wynik 1: SourceRow 69 | Ślad osadnictwa - Gródki; Ślad osadnictwa - Gródki; Ślad osadnictwa - Gródki |
| E2.result.2 | Wynik 2: SourceRow 86 | Ślad osadnictwa - Gródki |
| E2.result.3 | Wynik 3: SourceRow 96 | Osada - Gródki |

## E3 – statystyka

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E3.kind | Statystyka | count-by(AzpArea) |
| E3.nulls | Pominięte puste wartości (NULL) | 0 |
| E3.group.1 | Miejsce 1: 95-50 | 61 |
| E3.group.2 | Miejsce 2: 96-50 | 21 |
| E3.group.3 | Miejsce 3: 96-51 | 21 |
| E3.group.4 | Miejsce 4: 97-50 | 14 |
| E3.group.5 | Miejsce 5: 96-52 | 12 |
| E3.group.6 | Miejsce 6: 97-51 | 5 |
| E3.group.7 | Miejsce 7: 96-49 | 1 |

## E4 – ulubione

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E4.title | Tytuł rekordu z wiersza 7 (pole `LocalName`) | Osada; Osada |
