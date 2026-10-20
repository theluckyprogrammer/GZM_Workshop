# Etap 3 – widok HTMX (SekcjaSCK, s13)

> Pilot: skopiuj do Kilo Code wszystko od „CEL” do końca. Kontroler danych: przygotuj narzędzia z sekcji GOTOWE GDY.

## CEL
Strona główna aplikacji SekcjaSCK: wyszukiwarka działająca podczas pisania (HTMX 4), lista wyników i kafelek ze statystyką.

## DANE
- Wyszukiwarka wysyła `q` do endpointu z etapu 2: `?handler=Szukaj`.
- Statystyka: liczba rekordów dla każdej wartości pola `ClassType` (puste pomijamy; od największej liczby).
- Zapytanie statystyki: `SELECT [ClassType] AS Value, COUNT(*) AS Count FROM dbo.[CultureClasses] WHERE [ClassType] IS NOT NULL GROUP BY [ClassType] ORDER BY COUNT(*) DESC, [ClassType]`
- Atrybucja do stopki: „Źródło: Siemianowickie Centrum Kultury, otwartedane.metropoliagzm.pl, CC BY 4.0”.

## ZASADY
- W `src/GzmApp/Pages/Index.cshtml`: `<input type="search" name="q" hx-get="?handler=Szukaj" hx-trigger="keyup changed delay:300ms" hx-target="#wyniki">` i `<div id="wyniki">` z listą na start (pusta fraza).
- HTMX 4 jest już w `_Layout.cshtml` (plik lokalny). Nie dodawaj CDN ani innych bibliotek.
- Metoda `IsHtmx()` sprawdza nagłówek `HX-Request`: żądanie HTMX dostaje fragment `_Lista`, zwykłe – całą stronę.
- Statystykę licz w SQL przez `SqlRunner` (nowa metoda w `RecordRepository`). Pokaż ją w `<div id="statystyka">`.
- W `src/GzmApp/appsettings.json` ustaw `"DataAttribution"` na tekst atrybucji (stopka już go pokazuje).
- Metody do 20 linii, bez zagnieżdżonych if-ów.

## GOTOWE GDY
1. Wpisz w wyszukiwarkę frazę **dziecięcy** (E2.phrase): bez przeładowania strony lista ma **4** (E2.count) wyników.
2. Zakładka Network: żądanie `?handler=Szukaj` ma nagłówek `HX-Request: true`, a odpowiedź nie zawiera `<html>`.
3. Kafelek statystyki pokazuje:
   - taneczne: **15** (E3.group.1)
   - muzyczne: **6** (E3.group.2)
   - rękodzieło: **6** (E3.group.3)
   - teatralne: **5** (E3.group.4)
   - plastyczne: **2** (E3.group.5)
   - taneczne, wokalne: **2** (E3.group.6)
   - Indywidualne lekcje śpiewu: **1** (E3.group.7)
   - wokalne: **1** (E3.group.8)
4. Log SQL w terminalu: widać zapytanie statystyki.
5. Stopka pokazuje atrybucję danych.

Nie zmieniaj plików poza: `src/GzmApp/Pages/Index.cshtml`, `src/GzmApp/Pages/Index.cshtml.cs`, `src/GzmApp/Pages/Shared/_Lista.cshtml`, `src/GzmApp/Data/RecordRepository.cs`, `src/GzmApp/appsettings.json`. Nie dodawaj pakietów.
