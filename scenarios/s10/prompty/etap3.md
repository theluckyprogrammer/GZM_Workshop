# Etap 3 – widok HTMX (BoiskoMap, s10)

> Pilot: skopiuj do Kilo Code wszystko od „CEL” do końca. Kontroler danych: przygotuj narzędzia z sekcji GOTOWE GDY.

## CEL
Strona główna aplikacji BoiskoMap: wyszukiwarka działająca podczas pisania (HTMX 4), lista wyników i kafelek ze statystyką.

## DANE
- Wyszukiwarka wysyła `q` do endpointu z etapu 2: `?handler=Szukaj`.
- Statystyka: 5 najczęstszych wartości pola `Kind` z liczbą rekordów (puste pomijamy; przy równej liczbie – alfabetycznie).
- Zapytanie statystyki: `SELECT TOP 5 [Kind] AS Value, COUNT(*) AS Count FROM dbo.[SportsFields] WHERE [Kind] IS NOT NULL GROUP BY [Kind] ORDER BY COUNT(*) DESC, [Kind]`
- Atrybucja do stopki: „Źródło: Gliwice, otwartedane.metropoliagzm.pl, CC BY 4.0”.

## ZASADY
- W `src/GzmApp/Pages/Index.cshtml`: `<input type="search" name="q" hx-get="?handler=Szukaj" hx-trigger="keyup changed delay:300ms" hx-target="#wyniki">` i `<div id="wyniki">` z listą na start (pusta fraza).
- HTMX 4 jest już w `_Layout.cshtml` (plik lokalny). Nie dodawaj CDN ani innych bibliotek.
- Metoda `IsHtmx()` sprawdza nagłówek `HX-Request`: żądanie HTMX dostaje fragment `_Lista`, zwykłe – całą stronę.
- Statystykę licz w SQL przez `SqlRunner` (nowa metoda w `RecordRepository`). Pokaż ją w `<div id="statystyka">`.
- W `src/GzmApp/appsettings.json` ustaw `"DataAttribution"` na tekst atrybucji (stopka już go pokazuje).
- Metody do 20 linii, bez zagnieżdżonych if-ów.

## GOTOWE GDY
1. Wpisz w wyszukiwarkę frazę **śląska** (E2.phrase): bez przeładowania strony lista ma **5** (E2.count) wyników.
2. Zakładka Network: żądanie `?handler=Szukaj` ma nagłówek `HX-Request: true`, a odpowiedź nie zawiera `<html>`.
3. Kafelek statystyki pokazuje:
   - boisko do piłki nożnej i koszykówki: **26** (E3.group.1)
   - boisko do piłki nożnej: **19** (E3.group.2)
   - boisko do koszykówki: **10** (E3.group.3)
   - boisko do koszykówki i siatkówki: **8** (E3.group.4)
   - boisko trawiaste do piłki nożnej: **8** (E3.group.5)
4. Log SQL w terminalu: widać zapytanie statystyki.
5. Stopka pokazuje atrybucję danych.

Nie zmieniaj plików poza: `src/GzmApp/Pages/Index.cshtml`, `src/GzmApp/Pages/Index.cshtml.cs`, `src/GzmApp/Pages/Shared/_Lista.cshtml`, `src/GzmApp/Data/RecordRepository.cs`, `src/GzmApp/appsettings.json`. Nie dodawaj pakietów.
