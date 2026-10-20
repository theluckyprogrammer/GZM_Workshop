# Etap 2 – endpoint z filtrowaniem (StacjaFinder, s02)

> Pilot: skopiuj do Kilo Code wszystko od „CEL” do końca. Kontroler danych: przygotuj narzędzia z sekcji GOTOWE GDY.

## CEL
Adres, który zwraca rekordy z tabeli `BikeStations` pasujące do wpisanej frazy. Odpowiedź to fragment HTML (lista), a nie cała strona.

## DANE
- Tabela `BikeStations`, szukamy w polu `Area` (w pliku CSV: `Obszar`).
- Każdy wynik pokazuje numer wiersza i tytuł: „Nr `SourceRow`: `Name`”.
- Adres: `/?handler=Szukaj&q=<fraza>`.

## ZASADY
- `IRecordRepository` i `RecordRepository` w `src/GzmApp/Data/RecordRepository.cs`, metoda `Search(string? q, int take)`. Zapytanie przez `SqlRunner`, fraza jako parametr `@q`.
- Warunek: `Area LIKE '%' + @q + '%' COLLATE Latin1_General_CI_AI` – bez rozróżniania wielkości liter i ogonków.
- Pusta fraza: pierwsze 20 rekordów według `SourceRow`. Fraza dłuższa niż 100 znaków: odpowiedź 400.
- Handler `OnGetSzukaj(string? q)` w `src/GzmApp/Pages/Index.cshtml.cs` zwraca `Partial("_Lista", wyniki)`.
- Widok częściowy `src/GzmApp/Pages/Shared/_Lista.cshtml`: `<p id="licznik">Wyników: N</p>` i `<ul>` z jednym `<li>` na rekord. Bez `<html>`, bez layoutu.
- Rejestracja repozytorium w `Program.cs`. Metody do 20 linii, bez zagnieżdżonych if-ów.

## GOTOWE GDY
1. Przeglądarka: `http://localhost:5080/?handler=Szukaj&q=bieruń` (fraza **bieruń** (E2.phrase)) pokazuje **6** (E2.count) wyników:
   - Nr 25: **27025** (E2.result.1)
   - Nr 231: **27231** (E2.result.2)
   - Nr 254: **27254** (E2.result.3)
   - Nr 259: **27259** (E2.result.4)
   - Nr 543: **27544** (E2.result.5)
   - Nr 902: **27909** (E2.result.6)
2. Zakładka Network (F12): status 200, odpowiedź zaczyna się od `<p id="licznik">` i nie zawiera `<html>`.
3. Log SQL w terminalu: zapytanie z parametrem `q` równym frazie – z ogonkami, bez „krzaków”.
4. Network: `?handler=Szukaj&q=' OR 1=1 --` daje „Wyników: 0”.

Nie zmieniaj plików poza: `src/GzmApp/Data/RecordRepository.cs`, `src/GzmApp/Pages/Index.cshtml.cs`, `src/GzmApp/Pages/Shared/_Lista.cshtml`, `src/GzmApp/Program.cs`. Nie dodawaj pakietów.
