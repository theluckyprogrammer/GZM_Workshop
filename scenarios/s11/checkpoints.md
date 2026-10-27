# Wartości kontrolne – s11: Katowice: miejsca pamięci narodowej

Plik wygenerował `tools/Expect`. Nie edytuj ręcznie: zmień `mapping.json` i uruchom narzędzie ponownie.

- Plik: `data/raw/s11_katowice-miejsca-pamieci.csv` (UTF-8, separator `,`, nagłówek w linii 1)
- Tabela: `KatowiceMemorials`, klucz: `RecordNumber`
- Atrybucja: Źródło: Katowice, otwartedane.metropoliagzm.pl, CC BY 4.0

Zasady: wiersze liczymy od 1 bez nagłówka (= `SourceRow`). Tekst jest przycięty (Trim), pusty tekst = NULL.
Filtr: fragment tekstu bez rozróżniania wielkości liter (`LIKE '%fraza%'`). „Dziwny” rekord: pole puste albo nieparsowalne.
Znak `|` w wartości jest zapisany jako `\|`, a nowa linia jako `\n`.

## E1 – import (dane → baza)

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E1.rows | Liczba wierszy w tabeli po imporcie | 391 |
| E1.fileRows | Wiersze danych w pliku (bez nagłówka) | 391 |
| E1.skipped | Pominięte wiersze (notatki pod tabelą) | brak |
| E1.duplicates | Duplikaty klucza (scalone przez upsert) | brak |
| E1.row7.raw | Wiersz 7, kolumna CSV `addressstreetName` – tekst w pliku | pl. Bolesława Chrobrego |
| E1.row7.value | Wiersz 7, pole `Street` – wartość w bazie | pl. Bolesława Chrobrego |
| E1.weird | „Dziwne” rekordy (co najmniej jedno pole puste albo nieparsowalne) | 11 |
| E1.weird.rows | Pierwsze „dziwne” rekordy (SourceRow) | 24, 25, 34, 43, 46 |
| E1.weird.Street | Pole `Street`: puste / nieparsowalne | 11 / 0 |
| E1.weird.Description | Pole `Description`: puste / nieparsowalne | 0 / 0 |
| E1.weird.RecordNumber | Pole `RecordNumber`: puste / nieparsowalne | 0 / 0 |
| E1.weird.ObjectType | Pole `ObjectType`: puste / nieparsowalne | 0 / 0 |
| E1.wrongFieldCount | Wiersze z inną liczbą pól niż nagłówek | 0 |

## E2 – endpoint z filtrowaniem

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E2.phrase | Fraza wpisana w wyszukiwarkę (pole `Description`) | kościoła |
| E2.count | Liczba wyników | 5 |
| E2.result.1 | Wynik 1: SourceRow 93 | Tablica upamiętniająca fundatorów Kościoła Garnizonowego w Katowicach |
| E2.result.2 | Wynik 2: SourceRow 121 | Tablica upamiętniająca księdza prałata Józefa Kubisa, pierwszego proboszcza w latach 1900-42 parafii rzymskokatolickiej pw św. Józefa. Tablica odsłonięta na 65 lecie kościoła i parafii |
| E2.result.3 | Wynik 3: SourceRow 187 | Tablica upamiętniająca Pasterzy Kościoła Katowickiego w latach 1925-2000. |
| E2.result.4 | Wynik 4: SourceRow 204 | Wotum wdzięczności za odzyskaną wolność wraz z tablicą upamiętniającą 85. rocznicę wzniesienia Kościoła Matki Boskiej Częstochowskiej |
| E2.result.5 | Wynik 5: SourceRow 308 | Grób Ludwika Musioła, nauczyciela, historyka górnośląskiego, prekursora krajoznawstwa, historyka kultury oraz Kościoła katolickiego, lingwisty, etnografa |

## E3 – statystyka

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E3.kind | Statystyka | count-by(ObjectType) |
| E3.nulls | Pominięte puste wartości (NULL) | 0 |
| E3.group.1 | Miejsce 1: tablica | 182 |
| E3.group.2 | Miejsce 2: upamiętnienie | 74 |
| E3.group.3 | Miejsce 3: grób | 72 |
| E3.group.4 | Miejsce 4: pomnik | 34 |
| E3.group.5 | Miejsce 5: grób wojenny | 28 |
| E3.group.6 | Miejsce 6: cmentarz wojenny | 1 |

## E4 – ulubione

| ID | Co sprawdzić | Wartość |
|---|---|---|
| E4.title | Tytuł rekordu z wiersza 7 (pole `Description`) | Pomnik Józefa Piłsudskiego |
