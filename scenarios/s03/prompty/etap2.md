# Etap 2 – endpoint z filtrowaniem (RowerStat, s03)

> Pilot: skopiuj do Kilo Code wszystko od „CEL” do końca. Kontroler danych: przygotuj narzędzia z sekcji GOTOWE GDY.

## CEL
Adres, który zwraca rekordy z tabeli `BikeAvailability` pasujące do wpisanej frazy. Odpowiedź to fragment HTML (lista), a nie cała strona.

## DANE
- Tabela `BikeAvailability`, szukamy w polu `Commune` (w pliku CSV: `Gmina`).
- Każdy wynik pokazuje numer wiersza i tytuł: „Nr `SourceRow`: `Commune`”.
- Adres: `/?handler=Szukaj&q=<fraza>`.

## ZASADY
- `IRecordRepository` i `RecordRepository` w `src/GzmApp/Data/RecordRepository.cs`, metoda `Search(string? q, int take)`. Zapytanie przez `SqlRunner`, fraza jako parametr `@q`.
- Warunek: `Commune LIKE '%' + @q + '%' COLLATE Latin1_General_CI_AI` – bez rozróżniania wielkości liter i ogonków.
- Pusta fraza: pierwsze 20 rekordów według `SourceRow`. Fraza dłuższa niż 100 znaków: odpowiedź 400.
- Handler `OnGetSzukaj(string? q)` w `src/GzmApp/Pages/Index.cshtml.cs` zwraca `Partial("_Lista", wyniki)`.
- Widok częściowy `src/GzmApp/Pages/Shared/_Lista.cshtml`: `<p id="licznik">Wyników: N</p>` i `<ul>` z jednym `<li>` na rekord. Bez `<html>`, bez layoutu.
- Rejestracja repozytorium w `Program.cs`. Metody do 20 linii, bez zagnieżdżonych if-ów.

## GOTOWE GDY
1. Przeglądarka: `http://localhost:5080/?handler=Szukaj&q=śląski` (fraza **śląski** (E2.phrase)) pokazuje **3** (E2.count) wyników:
   - Nr 6: **Chełm Śląski** (E2.result.1)
   - Nr 20: **Piekary Śląskie** (E2.result.2)
   - Nr 27: **Siemianowice Śląskie** (E2.result.3)
2. Zakładka Network (F12): status 200, odpowiedź zaczyna się od `<p id="licznik">` i nie zawiera `<html>`.
3. Log SQL w terminalu: zapytanie z parametrem `q` równym frazie – z ogonkami, bez „krzaków”.
4. Network: `?handler=Szukaj&q=' OR 1=1 --` daje „Wyników: 0”.

Nie zmieniaj plików poza: `src/GzmApp/Data/RecordRepository.cs`, `src/GzmApp/Pages/Index.cshtml.cs`, `src/GzmApp/Pages/Shared/_Lista.cshtml`, `src/GzmApp/Program.cs`. Nie dodawaj pakietów.
