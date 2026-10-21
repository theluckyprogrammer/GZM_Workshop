# Wartości kontrolne – s07: Gliwice: obiekty sportowe

Plik wygenerował `tools/Expect`. Nie edytuj ręcznie: zmień `mapping.json` i uruchom narzędzie ponownie.

- Plik: `data/raw/s07_gliwice-obiekty-sportowe.csv` (UTF-8, separator `,`, nagłówek w linii 1)
- Tabela: `SportsFacilities`, klucz: `ObjectId`
- Atrybucja: Źródło: Gliwice, otwartedane.metropoliagzm.pl, CC BY 4.0

Zasady: wiersze liczymy od 1 bez nagłówka (= `SourceRow`). Tekst jest przycięty (Trim), pusty tekst = NULL.
Filtr: fragment tekstu bez rozróżniania wielkości liter (`LIKE '%fraza%'`). „Dziwny” rekord: pole puste albo nieparsowalne.
Znak `|` w wartości jest zapisany jako `\|`, a nowa linia jako `\n`.

## E1 – import (dane → baza)

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E1.rows | Liczba wierszy w tabeli po imporcie | 71 |
| E1.fileRows | Wiersze danych w pliku (bez nagłówka) | 71 |
| E1.skipped | Pominięte wiersze (notatki pod tabelą) | brak |
| E1.duplicates | Duplikaty klucza (scalone przez MERGE) | brak |
| E1.row7.raw | Wiersz 7, kolumna CSV `ADRES___ZARZĄDZAJĄCY` – tekst w pliku | ul. Okrzei 20 / GKS „PIAST” S.A. |
| E1.row7.value | Wiersz 7, pole `AddressManager` – wartość w bazie | ul. Okrzei 20 / GKS „PIAST” S.A. |
| E1.weird | „Dziwne” rekordy (co najmniej jedno pole puste albo nieparsowalne) | 0 |
| E1.weird.rows | Pierwsze „dziwne” rekordy (SourceRow) | brak |
| E1.weird.Facility | Pole `Facility`: puste / nieparsowalne | 0 / 0 |
| E1.weird.AddressManager | Pole `AddressManager`: puste / nieparsowalne | 0 / 0 |
| E1.weird.ObjectId | Pole `ObjectId`: puste / nieparsowalne | 0 / 0 |
| E1.wrongFieldCount | Wiersze z inną liczbą pól niż nagłówek | 0 |

## E2 – endpoint z filtrowaniem

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E2.phrase | Fraza wpisana w wyszukiwarkę (pole `Facility`) | nawierzchnią |
| E2.count | Liczba wyników | 3 |
| E2.result.1 | Wynik 1: SourceRow 20 | Boisko wielofunkcyjne (ze sztuczną nawierzchnią) |
| E2.result.2 | Wynik 2: SourceRow 21 | Boisko wielofunkcyjne (ze sztuczną nawierzchnią) |
| E2.result.3 | Wynik 3: SourceRow 22 | Boisko wielofunkcyjne (ze sztuczną nawierzchnią) |

## E3 – statystyka

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E3.kind | Statystyka | top5(Facility) |
| E3.nulls | Pominięte puste wartości (NULL) | 0 |
| E3.group.1 | Miejsce 1: Hala sportowa | 11 |
| E3.group.2 | Miejsce 2: Boisko trawiaste | 5 |
| E3.group.3 | Miejsce 3: Korty tenisowe | 5 |
| E3.group.4 | Miejsce 4: Kryta pływalnia | 4 |
| E3.group.5 | Miejsce 5: Boisko wielofunkcyjne (ze sztuczną nawierzchnią) | 3 |

## E4 – ulubione

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E4.title | Tytuł rekordu z wiersza 7 (pole `Facility`) | Boisko do minipiłki nożnej przy stadionie GKS „PIAST” |
