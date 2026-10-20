# Etap 2 – endpoint z filtrowaniem (PamięćKatowic, s11)

> Pilot: skopiuj do Kilo Code wszystko od „CEL” do końca. Kontroler danych: przygotuj narzędzia z sekcji GOTOWE GDY.

## CEL
Adres, który zwraca rekordy z tabeli `KatowiceMemorials` pasujące do wpisanej frazy. Odpowiedź to fragment HTML (lista), a nie cała strona.

## DANE
- Tabela `KatowiceMemorials`, szukamy w polu `Description` (w pliku CSV: `description`).
- Każdy wynik pokazuje numer wiersza i tytuł: „Nr `SourceRow`: `Description`”.
- Adres: `/?handler=Szukaj&q=<fraza>`.

## ZASADY
- `IRecordRepository` i `RecordRepository` w `src/GzmApp/Data/RecordRepository.cs`, metoda `Search(string? q, int take)`. Zapytanie przez `SqlRunner`, fraza jako parametr `@q`.
- Warunek: `Description LIKE '%' + @q + '%' COLLATE Latin1_General_CI_AI` – bez rozróżniania wielkości liter i ogonków.
- Pusta fraza: pierwsze 20 rekordów według `SourceRow`. Fraza dłuższa niż 100 znaków: odpowiedź 400.
- Handler `OnGetSzukaj(string? q)` w `src/GzmApp/Pages/Index.cshtml.cs` zwraca `Partial("_Lista", wyniki)`.
- Widok częściowy `src/GzmApp/Pages/Shared/_Lista.cshtml`: `<p id="licznik">Wyników: N</p>` i `<ul>` z jednym `<li>` na rekord. Bez `<html>`, bez layoutu.
- Rejestracja repozytorium w `Program.cs`. Metody do 20 linii, bez zagnieżdżonych if-ów.

## GOTOWE GDY
1. Przeglądarka: `http://localhost:5080/?handler=Szukaj&q=kościoła` (fraza **kościoła** (E2.phrase)) pokazuje **5** (E2.count) wyników:
   - Nr 93: **Tablica upamiętniająca fundatorów Kościoła Garnizonowego w Katowicach** (E2.result.1)
   - Nr 121: **Tablica upamiętniająca księdza prałata Józefa Kubisa, pierwszego proboszcza w latach 1900-42 parafii rzymskokatolickiej pw św. Józefa. Tablica odsłonięta na 65 lecie kościoła i parafii** (E2.result.2)
   - Nr 187: **Tablica upamiętniająca Pasterzy Kościoła Katowickiego w latach 1925-2000.** (E2.result.3)
   - Nr 204: **Wotum wdzięczności za odzyskaną wolność wraz z tablicą upamiętniającą 85. rocznicę wzniesienia Kościoła Matki Boskiej Częstochowskiej** (E2.result.4)
   - Nr 308: **Grób Ludwika Musioła, nauczyciela, historyka górnośląskiego, prekursora krajoznawstwa, historyka kultury oraz Kościoła katolickiego, lingwisty, etnografa** (E2.result.5)
2. Zakładka Network (F12): status 200, odpowiedź zaczyna się od `<p id="licznik">` i nie zawiera `<html>`.
3. Log SQL w terminalu: zapytanie z parametrem `q` równym frazie – z ogonkami, bez „krzaków”.
4. Network: `?handler=Szukaj&q=' OR 1=1 --` daje „Wyników: 0”.

Nie zmieniaj plików poza: `src/GzmApp/Data/RecordRepository.cs`, `src/GzmApp/Pages/Index.cshtml.cs`, `src/GzmApp/Pages/Shared/_Lista.cshtml`, `src/GzmApp/Program.cs`. Nie dodawaj pakietów.
