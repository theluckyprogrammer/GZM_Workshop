# Etap 3 – widok HTMX (SportGliwice, s07)

> Pilot: skopiuj do Kilo Code wszystko od „CEL” do końca. Kontroler danych: przygotuj narzędzia z sekcji GOTOWE GDY.

## CEL
Strona główna aplikacji SportGliwice: wyszukiwarka działająca podczas pisania (HTMX 4), lista wyników i kafelek ze statystyką.

## DANE
- Wyszukiwarka wysyła `q` do endpointu z etapu 2: `?handler=Szukaj`.
- Statystyka: 5 najczęstszych wartości pola `Facility` z liczbą rekordów (puste pomijamy; przy równej liczbie – alfabetycznie).
- Zapytanie statystyki: `SELECT TOP 5 [Facility] AS Value, COUNT(*) AS Count FROM dbo.[SportsFacilities] WHERE [Facility] IS NOT NULL GROUP BY [Facility] ORDER BY COUNT(*) DESC, [Facility]`
- Atrybucja do stopki: „Źródło: Gliwice, otwartedane.metropoliagzm.pl, CC BY 4.0”.

## ZASADY
- W `src/GzmApp/Pages/Index.cshtml`: `<input type="search" name="q" hx-get="?handler=Szukaj" hx-trigger="keyup changed delay:300ms" hx-target="#wyniki">` i `<div id="wyniki">` z listą na start (pusta fraza).
- HTMX 4 jest już w `_Layout.cshtml` (plik lokalny). Nie dodawaj CDN ani innych bibliotek.
- Metoda `IsHtmx()` sprawdza nagłówek `HX-Request`: żądanie HTMX dostaje fragment `_Lista`, zwykłe – całą stronę.
- Statystykę licz w SQL przez `SqlRunner` (nowa metoda w `RecordRepository`). Pokaż ją w `<div id="statystyka">`.
- W `src/GzmApp/appsettings.json` ustaw `"DataAttribution"` na tekst atrybucji (stopka już go pokazuje).
- Metody do 20 linii, bez zagnieżdżonych if-ów.

## GOTOWE GDY
1. Wpisz w wyszukiwarkę frazę **nawierzchnią** (E2.phrase): bez przeładowania strony lista ma **3** (E2.count) wyników.
2. Zakładka Network: żądanie `?handler=Szukaj` ma nagłówek `HX-Request: true`, a odpowiedź nie zawiera `<html>`.
3. Kafelek statystyki pokazuje:
   - Hala sportowa: **11** (E3.group.1)
   - Boisko trawiaste: **5** (E3.group.2)
   - Korty tenisowe: **5** (E3.group.3)
   - Kryta pływalnia: **4** (E3.group.4)
   - Boisko wielofunkcyjne (ze sztuczną nawierzchnią): **3** (E3.group.5)
4. Log SQL w terminalu: widać zapytanie statystyki.
5. Stopka pokazuje atrybucję danych.

Nie zmieniaj plików poza: `src/GzmApp/Pages/Index.cshtml`, `src/GzmApp/Pages/Index.cshtml.cs`, `src/GzmApp/Pages/Shared/_Lista.cshtml`, `src/GzmApp/Data/RecordRepository.cs`, `src/GzmApp/appsettings.json`. Nie dodawaj pakietów.
