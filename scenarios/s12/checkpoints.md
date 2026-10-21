# Wartości kontrolne – s12: Bytom: zabytki

Plik wygenerował `tools/Expect`. Nie edytuj ręcznie: zmień `mapping.json` i uruchom narzędzie ponownie.

- Plik: `data/raw/s12_bytom-zabytki.csv` (UTF-8, separator `,`, nagłówek w linii 1)
- Tabela: `HistoricBuildings`, klucz: `Id`
- Atrybucja: Źródło: Bytom, otwartedane.metropoliagzm.pl, CC BY 4.0

Zasady: wiersze liczymy od 1 bez nagłówka (= `SourceRow`). Tekst jest przycięty (Trim), pusty tekst = NULL.
Filtr: fragment tekstu bez rozróżniania wielkości liter (`LIKE '%fraza%'`). „Dziwny” rekord: pole puste albo nieparsowalne.
Znak `|` w wartości jest zapisany jako `\|`, a nowa linia jako `\n`.

## E1 – import (dane → baza)

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E1.rows | Liczba wierszy w tabeli po imporcie | 2313 |
| E1.fileRows | Wiersze danych w pliku (bez nagłówka) | 2313 |
| E1.skipped | Pominięte wiersze (notatki pod tabelą) | brak |
| E1.duplicates | Duplikaty klucza (scalone przez MERGE) | brak |
| E1.row7.raw | Wiersz 7, kolumna CSV `style` – tekst w pliku | Modernizm |
| E1.row7.value | Wiersz 7, pole `Style` – wartość w bazie | Modernizm |
| E1.weird | „Dziwne” rekordy (co najmniej jedno pole puste albo nieparsowalne) | 176 |
| E1.weird.rows | Pierwsze „dziwne” rekordy (SourceRow) | 1, 95, 96, 104, 137 |
| E1.weird.BuildingNumber | Pole `BuildingNumber`: puste / nieparsowalne | 129 / 0 |
| E1.weird.Street | Pole `Street`: puste / nieparsowalne | 6 / 0 |
| E1.weird.Id | Pole `Id`: puste / nieparsowalne | 0 / 0 |
| E1.weird.Name | Pole `Name`: puste / nieparsowalne | 0 / 0 |
| E1.weird.Style | Pole `Style`: puste / nieparsowalne | 126 / 0 |
| E1.wrongFieldCount | Wiersze z inną liczbą pól niż nagłówek | 0 |

## E2 – endpoint z filtrowaniem

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E2.phrase | Fraza wpisana w wyszukiwarkę (pole `Name`) | garaż |
| E2.count | Liczba wyników | 5 |
| E2.result.1 | Wynik 1: SourceRow 319 | zespół domów w ogrodach, z garażami w głębi działek |
| E2.result.2 | Wynik 2: SourceRow 320 | zespół domów w ogrodach, z garażami  w głębi działek |
| E2.result.3 | Wynik 3: SourceRow 1248 | garaż drezyn z budynkiem socjalnym Zespołu Stacji Kolei Wąskotorowej Bytom-Karb |
| E2.result.4 | Wynik 4: SourceRow 1755 | willa i budynek garażowo-mieszkalny |
| E2.result.5 | Wynik 5: SourceRow 2234 | dom wielorodzinny, garaże |

## E3 – statystyka

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E3.kind | Statystyka | top5(Style) |
| E3.nulls | Pominięte puste wartości (NULL) | 126 |
| E3.group.1 | Miejsce 1: Modernizm | 512 |
| E3.group.2 | Miejsce 2: Funkcjonalizm | 225 |
| E3.group.3 | Miejsce 3: Historyzm | 199 |
| E3.group.4 | Miejsce 4: Historyzm ceglany prosty | 191 |
| E3.group.5 | Miejsce 5: Neorenesans | 153 |

## E4 – ulubione

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E4.title | Tytuł rekordu z wiersza 7 (pole `Name`) | kamienica-specjalny ośrodek wychowawczy |
