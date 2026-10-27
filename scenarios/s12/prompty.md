# ZabytkiBytom – prompty krok po kroku (s12)

Pilot kopiuje cały blok promptu do Kilo Code i akceptuje zmiany. Nawigator prowadzi pauzy. Po każdym kroku zamieniacie się rolami.
Aplikację uruchamiacie klawiszem **F5** w VS Code albo poleceniem `dotnet run --project src/GzmApp` (adres: http://localhost:5080). Testy – ikoną kolby albo `dotnet test`. Każde „jak to zrobić” jest w **docs/PORADNIK-UCZNIA.md** – zaglądajcie tam w każdej pauzie.

Pracujecie jak zawodowcy: najpierw **specyfikacja**, potem **testy**, na końcu **kod**. Specyfikacja to pliki w `scenarios/s12/spec/` – po jednym na krok. Każde ponumerowane zdanie w sekcji „Reguły” to jeden test o tej samej nazwie. Kilo pisze kod, a testy mówią, czy kod robi to, co obiecuje zdanie. W pauzach „decyzja” pytacie Kilo o propozycje i wybieracie, jak ma działać Wasz portal – każda decyzja ląduje w `LOG-DECYZJI.md`.

## Krok 0 – powstaje Wasz portal (szkielet i dane)

Pierwszy prompt stawia fundament: aplikację (serwer, strona startowa, logowanie, baza) i plik z danymi „Bytom: zabytki” pobrany prosto z portalu otwartedane.metropoliagzm.pl. Wy jeszcze niczego nie budujecie – najpierw sprawdzacie fundament.

~~~~text
CEL
Wasz portal ZabytkiBytom zaczyna istnieć. Po tym prompcie na dysku jest aplikacja w src/GzmApp, projekt testów w tests/GzmApp.Specs i plik z danymi data/raw/s12_bytom-zabytki.csv. Scenariusz s12 jest już wpisany w konfigurację – pary nigdy nie wpisują go ręcznie.

WYKONAJ
1. Uruchom w terminalu (VS Code: Ctrl + `) polecenie:
   powershell -ExecutionPolicy Bypass -File tools/nowy-portal.ps1 -Scenariusz s12
   Skrypt tworzy src/, tests/ i db/, pobiera dane z portalu GZM i zakłada LOG-DECYZJI.md. Poczekaj na napis „GOTOWE”.
2. Zbuduj aplikację: dotnet build src/GzmApp
3. Sprawdź dane: dotnet test tests/GzmApp.Specs --filter Capability=s12-000-dane
4. Sprawdź całość: dotnet test tests/GzmApp.Specs
   Zielone mają być testy kroku 0 (dane) i test kontrolny; reguły kroków 1–4 są czerwone – tak ma być, to lista zadań pary.

SPECYFIKACJA
Ten krok nie buduje funkcji, ale ma swoją specyfikację: reguły pliku `scenarios/s12/spec/s12-000-dane.md` sprawdzają, że dane naprawdę są i są kompletne.

ZASADY
- Nie zmieniaj plików – wszystko tworzy skrypt. Gdy polecenie z punktu 1 się nie powiedzie, przeczytaj komunikat skryptu i powiedz parze, co zrobić dalej (np. brak internetu: poproś o plik z danych od prowadzącego i uruchom polecenie ponownie – skrypt pomija pobieranie, gdy plik już jest).
- Po każdym poleceniu krótko napisz, co z niego wynikło.
- Na koniec napisz w 3 punktach, co powstało.

Nie zmieniaj plików poza: niczym – w tym kroku wszystko tworzy skrypt. Nie dodawaj pakietów.
~~~~

### ⏸ Pauza – zobaczcie, co powstało: fundament

1. **Dane**: w Eksploratorze VS Code (Ctrl+Shift+E) znajdźcie `data/raw/s12_bytom-zabytki.csv` i otwórzcie. Pierwsza linia to nazwy kolumn – porównajcie je z tabelą „Kolumny do importu” w prompcie kroku 1. Kliknijcie test reguły 2 kroku 0 w panelu Testing: komunikat mówi, ile wierszy ma plik. Zamknijcie plik bez zapisywania.
2. **Aplikacja**: naciśnijcie F5. Edge otworzy http://localhost:5080 – strona „Tu pojawi się Twoja aplikacja” z Waszym scenariuszem s12. W terminalu (zakładka z `dotnet`) widać wpisy startowe. Zatrzymajcie: Shift+F5 (poradnik, §2–3).
3. **Logowanie**: na stronie kliknijcie „Zaloguj się” – Wasz portal ma już konta (przydadzą się w „Co dalej?”). Wróćcie na stronę główną.
4. **Testy**: ikona kolby → ▶ „Run Tests”. Zielone: kontrola i kroku 0. Czerwone: kroki 1–4. Kliknijcie czerwony test – komunikat po polsku mówi, czego brakuje. To właśnie będziecie budować (poradnik §4–5).
5. **Baza**: ikona bazy danych (SQL Server) → `GzmWorkshop` → `Tables` – są już tabele logowania (AspNet…). Wasza tabela z danymi powstanie w kroku 1 (poradnik §8).
6. Przeczytajcie specyfikację kroku 1: `scenarios/s12/spec/s12-001-importuj-dane.md`. „Co się dzieje” opisuje krok słowami, a „Reguły” – co dokładnie ma działać.

Wasz cel: zamienić czerwone testy w zielone, krok po kroku.

## Krok 1 – dane z pliku do bazy

~~~~text
CEL
Wczytaj plik CSV „Bytom: zabytki” do nowej tabeli `HistoricBuildings` w SQL Server przez Dapper. Import uruchamia się przy starcie aplikacji, gdy tabela jest pusta.

SPECYFIKACJA
Reguły tego kroku są w pliku `scenarios/s12/spec/s12-001-importuj-dane.md` (sekcja „Reguły”). Przeczytaj go najpierw. Każda reguła to jeden test w `tests/GzmApp.Specs`. Nie zmieniaj plików specyfikacji ani testów.

DANE
- Plik: `data/raw/s12_bytom-zabytki.csv` w katalogu głównym repozytorium. Ścieżka z aplikacji: `Path.Combine(environment.ContentRootPath, "..", "..", "data/raw/s12_bytom-zabytki.csv")` (`environment` to `IWebHostEnvironment`).
- Kodowanie: UTF-8. Separator: przecinek `,`. Nagłówek w pierwszej linii.
- Kolumny do importu (pozostałych kolumn pliku nie importuj):

| Kolumna w pliku | Pole w tabeli | Typ SQL | Kultura | Format | Może być puste |
|---|---|---|---|---|---|
| `addressbuildingNumber` | `BuildingNumber` | `nvarchar(10)` | pl-PL | – | tak |
| `addressstreetName` | `Street` | `nvarchar(100)` | pl-PL | – | tak |
| `id` | `Id` | `int` | pl-PL | – | nie |
| `name` | `Name` | `nvarchar(1000)` | pl-PL | – | nie |
| `style` | `Style` | `nvarchar(80)` | pl-PL | – | tak |

- Klucz naturalny (unikalny): `Id`. Wiersze zapisuj przez `MERGE` po kluczu: powtórzony klucz nadpisuje wcześniejszy wiersz, bez wyjątku.
- Utwórz plik `db/s12_create.sql` z tym skryptem i uruchamiaj go przy starcie (w `RunAtStartupAsync`) przed importem. W działającej aplikacji czytaj go z `Path.Combine(AppContext.BaseDirectory, "db", "s12_create.sql")` (tak jak `DatabaseBootstrapper`):

```sql
IF OBJECT_ID(N'dbo.HistoricBuildings') IS NULL
CREATE TABLE dbo.[HistoricBuildings] (
    [RecordId] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_HistoricBuildings] PRIMARY KEY,
    [SourceRow] int NOT NULL,
    [BuildingNumber] nvarchar(10) NULL,
    [Street] nvarchar(100) NULL,
    [Id] int NOT NULL,
    [Name] nvarchar(1000) NOT NULL,
    [Style] nvarchar(80) NULL,
    CONSTRAINT [UQ_HistoricBuildings_Key] UNIQUE ([Id])
);
```

ZASADY
- Plik czytaj pakietem CsvHelper (jest już w projekcie) z `CultureInfo("pl-PL")`. Uwaga: ta kultura sama ustawia domyślny separator „;” – jeśli DANE mówią o innym, wskaż go jawnie w konfiguracji: `new CsvConfiguration(CultureInfo("pl-PL")) { Delimiter = "," }`. Nie dziel linii przez `Split`.
- Pola odczytuj po nazwie kolumny z pliku: najpierw `csv.Read()` i `csv.ReadHeader()` (w CsvHelper 33 nie ma `ReadHeaderAsync`), potem `while (csv.Read())` i `csv.GetField("Kolumna")`. Nie mapuj wiersza na klasę – nazwy w pliku różnią się od nazw pól w tabeli.
- Liczby i daty odczytuj z kulturą i formatem z tabeli kolumn. Puste pole albo wartość, której nie da się odczytać, w kolumnie z „Może być puste: tak” zapisz jako NULL. W kolumnie z „nie” taki wiersz pomiń i zapisz jego numer w logu. Bez wyjątku.
- `SourceRow` = numer wiersza danych w pliku, od 1, bez nagłówka. Licz go sam (licznik zwiększany przy każdym wierszu danych, także pominiętym) – `csv.Parser.Row` liczy razem z nagłówkiem. Tekst przytnij (`Trim`).
- Bez `if` wewnątrz pętli: jeden wiersz odczytuje i sprawdza osobna metoda, która zwraca gotowy wiersz albo `null`; pętla tylko zbiera wyniki, a pominięte wiersze odfiltruj LINQ (`Where`).
- Zapytania wysyłaj przez istniejącą klasę `SqlRunner`, parametry przez Dappera.
- Po imporcie zapisz w logu liczbę wierszy w tabeli i czas importu. Gdy tabela nie jest pusta, zapisz w logu „Import pominięty – w tabeli jest N wierszy”.
- Klasa `CsvImporter` z interfejsem `ICsvImporter`. Rejestracja w metodzie `AddAppServices`, a skrypt tabeli i import w `RunAtStartupAsync` – obie w `src/GzmApp/AppServices.cs`. `Program.cs` już je wywołuje, nie zmieniaj go.
- Krótkie metody, bez zagnieżdżonych if-ów.
- Na koniec uruchom `dotnet test tests/GzmApp.Specs --filter Capability=s12-001-importuj-dane`. Jeśli test nie przechodzi, przeczytaj jego komunikat i popraw kod (nie test) – najwyżej raz. Napisz, ile testów przeszło.
- Na koniec napisz w 3 punktach, co zrobiłeś.

Nie zmieniaj plików poza: `db/s12_create.sql`, `src/GzmApp/Data/CsvImporter.cs`, `src/GzmApp/AppServices.cs`. Nie dodawaj pakietów.
~~~~

### ⏸ Pauza – zobaczcie, co powstało

1. **Co powstało**: nowe pliki `db/s12_create.sql` (tabela w bazie) i `src/GzmApp/Data/CsvImporter.cs` (czytanie pliku) oraz wpisy w `src/GzmApp/AppServices.cs` – tam rejestruje się klasę i uruchamia import.
2. Uruchomcie aplikację (F5). W terminalu pojawia się wpis z liczbą zaimportowanych wierszy – to log Waszego importu.
3. Rozszerzenie mssql (ikona bazy po lewej): serwer `(localdb)\MSSQLLocalDB`, baza `GzmWorkshop`, tabela `HistoricBuildings` → prawy klik → „Select Top 1000”. To Wasze dane z pliku – już w bazie (poradnik §8).
4. **Debuger, pierwsze podejście** (poradnik §7): postawcie czerwoną pułapkę obok linii, w której `CsvImporter` odczytuje pole wiersza, uruchomcie F5 i odświeżcie stronę. Aplikacja zatrzyma się na pułapce – w panelu Variables zobaczcie wartości pierwszego wiersza z pliku.
5. Zatrzymajcie aplikację (Shift+F5).
6. Testing → ▶ „Run Tests”. Testy kroku 1 (`s12-001-importuj-dane`) są zielone. To samo z terminala: `dotnet test tests/GzmApp.Specs --filter Capability=s12-001-importuj-dane`. Czerwony test? Kliknijcie go: komunikat mówi, czego brakuje. Wklejcie ten komunikat do Kilo.

### ⏸ Pauza – decyzja 1: po czym użytkownik będzie szukał?

Nie ma gotowej listy do odhaczenia – o warianty pyta się **Kilo**, bo zna Wasze dane i wcześniejsze decyzje. Wy wybieracie.

1. Wklejcie Kilo do rozmowy (to nie prompt kroku – zwykła prośba):

~~~text
Zaproponuj nam 3 różne warianty decyzji 1: „po czym użytkownik będzie szukał” w ZabytkiBytom.
Popatrz na kolumny naszych danych i na nasze dotychczasowe decyzje z LOG-DECYZJI.md.
Każdy wariant to jedno zdanie według któregoś wzorca poniżej – w miejsce {Pole} i {kolumna} podstaw różne pola z naszych kolumn:
Wyszukiwarka szuka frazy w dowolnym miejscu pola `{Pole}` (kolumna w pliku: `{kolumna}`).
Wyszukiwarka pokazuje rekordy, w których pole `{Pole}` (kolumna w pliku: `{kolumna}`) zaczyna się od frazy.
Przedstaw warianty po polsku, powiedz do czego się nadają, zapytaj nas, który wybieramy, i czekaj na odpowiedź.
~~~

2. Porozmawiajcie i odpowiedzcie Kilo, który wariant wybieracie. Nie ma złych decyzji.
3. Wybrane zdanie wklejcie do specyfikacji: plik `scenarios/s12/spec/s12-002-szukaj-rekordow.md`, w miejsce zdania reguły 1 (zostawcie „1. ” na początku). Zapiszcie (Ctrl+S). Potem Ctrl+Shift+P → „.NET: Build”, a w panelu Testing kliknijcie ⟳ – pojawi się test Waszego wariantu.
4. Napiszcie Kilo: „Zapisz tę decyzję w LOG-DECYZJI.md”.

Bez decyzji też dobrze: w specyfikacji jest już zdanie domyślne i jego test działa.

## Krok 2 – wyszukiwarka (endpoint)

~~~~text
CEL
Adres, który zwraca rekordy z tabeli `HistoricBuildings` pasujące do wpisanej frazy. Odpowiedź to fragment HTML (sama lista), a nie cała strona.

SPECYFIKACJA
Reguły tego kroku są w pliku `scenarios/s12/spec/s12-002-szukaj-rekordow.md` (sekcja „Reguły”). Przeczytaj go najpierw. Każda reguła to jeden test w `tests/GzmApp.Specs`. Nie zmieniaj plików specyfikacji ani testów.

DANE
- Adres: `/?handler=Szukaj&q=<fraza>`.
- Pole, w którym szukasz, podaje reguła 1. „W dowolnym miejscu pola” to `LIKE '%' + @q + '%'`, a „zaczyna się od frazy” to `LIKE @q + '%'`.
- Każdy wynik pokazuje „Nr `SourceRow`: `Name`”.

ZASADY
- `IRecordRepository` i `RecordRepository` w `src/GzmApp/Data/RecordRepository.cs`, metoda `Search(string? q, int take)` (może być asynchroniczna, jak metody `SqlRunner`). Zapytanie przez `SqlRunner`, fraza jako parametr `@q` – bez sklejania tekstu SQL.
- `Search` zwraca cały wiersz: `RecordId`, `SourceRow` i wszystkie zaimportowane pola (`BuildingNumber`, `Street`, `Id`, `Name`, `Style`) – przydadzą się w krokach 3 i 4. Wiersz to klasa z właściwościami (`public int SourceRow { get; init; }`): Dapper dopasowuje je po nazwach kolumn. Pozycyjny `record` działa tylko wtedy, gdy kolejność kolumn w `SELECT` jest dokładnie taka jak kolejność jego parametrów.
- Porównanie bez rozróżniania wielkości liter i ogonków: `COLLATE Latin1_General_CI_AI`.
- Wyniki według `SourceRow`, najwyżej 20 (`take` = 20), także dla pustej frazy. `Wyników: N` to liczba wszystkich pasujących rekordów (osobne `COUNT(*)`), lista pokazuje najwyżej 20.
- Fraza dłuższa niż 100 znaków: odpowiedź 400 z krótkim fragmentem `<p class="alert alert-warning">…</p>`.
- Handler `OnGetSzukaj(string? q)` w `src/GzmApp/Pages/Index.cshtml.cs` zwraca `Partial("_Lista", wyniki)`. Liczbę „Wyników: N” przekaż razem z listą jako jeden model (np. rekord `RecordList(int Count, …)`), bo `ViewData` nie przechodzi przez `Partial(...)`.
- W `IndexModel` zostaw właściwość `Scenario` i metodę `OnGet()` – używa ich obecny `Index.cshtml`, który zmienicie dopiero w kroku 3.
- Widok częściowy `src/GzmApp/Pages/Shared/_Lista.cshtml` (bez `@page` w pierwszej linii – to nie jest strona; klasy CSS z biblioteki Tabler, która jest już w `_Layout.cshtml`): `<p id="licznik" class="text-secondary px-3 pt-3 mb-2">Wyników: N</p>` i `<ul id="lista" class="list-group list-group-flush">` z jednym `<li class="list-group-item" data-record-id="@r.RecordId"><span class="badge bg-blue-lt me-2">Nr @r.SourceRow:</span> <span class="tytul fw-bold">…</span></li>` na rekord. Bez `<html>` i bez layoutu.
- Rejestracja repozytorium w `AddAppServices` w `src/GzmApp/AppServices.cs`. Krótkie metody, bez zagnieżdżonych if-ów.
- Na koniec uruchom `dotnet test tests/GzmApp.Specs --filter Capability=s12-002-szukaj-rekordow`. Jeśli test nie przechodzi, przeczytaj jego komunikat i popraw kod (nie test) – najwyżej raz. Napisz, ile testów przeszło.
- Na koniec napisz w 3 punktach, co zrobiłeś.

Nie zmieniaj plików poza: `src/GzmApp/Data/RecordRepository.cs`, `src/GzmApp/Pages/Index.cshtml.cs`, `src/GzmApp/Pages/Shared/_Lista.cshtml`, `src/GzmApp/AppServices.cs`. Nie dodawaj pakietów.
~~~~

### ⏸ Pauza – zobaczcie, co powstało

1. **Co powstało**: `src/GzmApp/Data/RecordRepository.cs` (zapytania do bazy z parametrem `@q`) i `src/GzmApp/Pages/Shared/_Lista.cshtml` (fragment listy). Handler `OnGetSzukaj` jest w `src/GzmApp/Pages/Index.cshtml.cs`.
2. Uruchomcie aplikację i otwórzcie adres `http://localhost:5080/?handler=Szukaj&q=` z dopisanym słowem z wybranej kolumny (dla wariantu domyślnego np. `garaż`). Widzicie samą listę – bez menu i stopki. To „fragment HTML”.
3. **Narzędzia deweloperskie Edge** (poradnik §6): F12 → zakładka Network → odświeżcie stronę (Ctrl+F5). Kliknijcie żądanie `?handler=Szukaj`: status 200, w „Response” jest tylko lista, a w „Headers” parametr `q`. W zakładce Elements kliknijcie `<ul id="lista">` – element podświetla się na stronie.
4. **Debuger** (poradnik §7): postawcie pułapkę w `OnGetSzukaj` w `Index.cshtml.cs`, uruchomcie F5 i otwórzcie adres z punktu 2. Aplikacja zatrzyma się: w panelu Variables zobaczcie `q` – Waszą frazę tak, jak widzi ją serwer. Shift+F5, żeby skończyć.
5. W terminalu (log SQL) widać zapytanie z parametrem `q` – to Wasza fraza.
6. Testing → ▶ „Run Tests”. Zielone są testy kroków 1 i 2. Reguła 5 sprawdza, że fraza `' OR 1=1 --` nic nie znajduje – to ochrona przed atakiem SQL injection.

### ⏸ Pauza – decyzja 2: co pokazać na liście?

1. Wklejcie Kilo do rozmowy:

~~~text
Zaproponuj nam 3 różne warianty decyzji 2: „co pokazać na liście wyników” w ZabytkiBytom.
Popatrz na kolumny naszych danych i na nasze dotychczasowe decyzje z LOG-DECYZJI.md.
Każdy wariant to jedno zdanie według wzorca poniżej – w miejsce {Pole} podstaw pole z tytułem, a w miejsce {pola} różne zestawy pól (1–3 pola):
Przy każdym rekordzie lista pokazuje tytuł `{Pole}` i pola: {pola}.
Przedstaw warianty po polsku, powiedz, co parę zobaczy dzięki każdemu, zapytaj nas, który wybieramy, i czekaj na odpowiedź.
~~~

2. Wybierzcie wariant i odpowiedzcie Kilo.
3. Wybrane zdanie wklejcie w pliku `scenarios/s12/spec/s12-003-pokaz-strone.md` w miejsce zdania reguły 4 (zostawcie „4. ”). Zapiszcie, potem Ctrl+Shift+P → „.NET: Build” i ⟳ w panelu Testing.
4. Napiszcie Kilo: „Zapisz tę decyzję w LOG-DECYZJI.md”.

### ⏸ Pauza – decyzja 3: jaki kafelek ze statystyką?

1. Wklejcie Kilo do rozmowy:

~~~text
Zaproponuj nam 3 różne warianty decyzji 3: „jaki kafelek ze statystyką” w ZabytkiBytom.
Popatrz na kolumny naszych danych i na nasze dotychczasowe decyzje z LOG-DECYZJI.md.
Każdy wariant to jedno zdanie według któregoś wzorca poniżej – w miejsce {Pole} podstaw różne pola (sumę i średnią tylko dla pól liczbowych):
Kafelek ze statystyką pokazuje sumę pola `{Pole}`.
Kafelek ze statystyką pokazuje średnią pola `{Pole}` z 2 miejscami po przecinku.
Kafelek ze statystyką pokazuje 5 najczęstszych wartości pola `{Pole}` z liczbą rekordów.
Kafelek ze statystyką pokazuje liczbę rekordów dla każdej wartości pola `{Pole}`.
Przedstaw warianty po polsku, powiedz, co każda statystyka powie o danych, zapytaj nas, który wybieramy, i czekaj na odpowiedź.
~~~

2. Wybierzcie wariant i odpowiedzcie Kilo.
3. Wybrane zdanie wklejcie w pliku `scenarios/s12/spec/s12-003-pokaz-strone.md` w miejsce zdania reguły 5 (zostawcie „5. ”). Zapiszcie, potem Ctrl+Shift+P → „.NET: Build” i ⟳ w panelu Testing.
4. Napiszcie Kilo: „Zapisz tę decyzję w LOG-DECYZJI.md”.

W specyfikacji są już zdania domyślne (decyzje 2 i 3) – jeśli nie wybierzecie, testy działają dla nich.

## Krok 3 – strona z wyszukiwarką i kafelkiem (HTMX)

~~~~text
CEL
Strona główna aplikacji ZabytkiBytom: wyszukiwarka działająca podczas pisania (HTMX 4), lista wyników i kafelek ze statystyką.

SPECYFIKACJA
Reguły tego kroku są w pliku `scenarios/s12/spec/s12-003-pokaz-strone.md` (sekcja „Reguły”). Przeczytaj go najpierw. Każda reguła to jeden test w `tests/GzmApp.Specs`. Nie zmieniaj plików specyfikacji ani testów.

ZASADY
- `src/GzmApp/Pages/Index.cshtml` – układ z klasami Tablera (biblioteka CSS jest już w `_Layout.cshtml`), który dobrze wygląda też na telefonie:

```html
<div class="page-header mb-4">
  <h1 class="page-title">ZabytkiBytom</h1>
  <div class="text-secondary">Zabytki Bytomia: wyszukaj budynek i sprawdź jego styl.</div>
</div>
<div class="row g-3">
  <div class="col-12 col-lg-4">
    <div id="statystyka" class="card"><div class="card-body"><!-- kafelek: tytuł w <div class="subheader">, wartość niżej --></div></div>
  </div>
  <div class="col-12 col-lg-8">
    <div class="card">
      <div class="card-body">
        <input type="search" name="q" class="form-control" placeholder="Szukaj…" autocomplete="off"
               hx-get="?handler=Szukaj" hx-trigger="keyup changed delay:300ms" hx-target="#wyniki">
      </div>
      <div id="wyniki"><!-- lista na start (pusta fraza) --></div>
    </div>
  </div>
</div>
```

- HTMX 4 i Tabler są już w `_Layout.cshtml` (pliki lokalne). Nie dodawaj CDN ani innych bibliotek. Teksty na stronie po polsku.
- Dane na start strony ładuj w istniejącej metodzie `OnGet` – zmień ją na `public async Task OnGetAsync()` w tym samym miejscu, nie dodawaj drugiej obok (dwie metody GET bez nazwy handlera to błąd 500 „Multiple handlers matched”).
- Prywatna metoda `IsHtmx()` w `Index.cshtml.cs` sprawdza nagłówek: `Request.Headers["HX-Request"] == "true"`. W `OnGetSzukaj`: żądanie HTMX dostaje fragment `_Lista`, zwykłe wejście na ten sam adres – całą stronę z wynikami dla frazy `q`.
- Listę w `_Lista.cshtml` zmień według reguły 4: pola pokaż w tym samym `<li>`, za tytułem, w `<div class="text-secondary small">` jako „nazwa: wartość · nazwa: wartość” z polskimi nazwami pól: `BuildingNumber` – nr budynku, `Street` – ulica, `Id` – nr, `Name` – nazwa, `Style` – styl. Liczby w formacie pl-PL (rok i numery/identyfikatory bez separatora tysięcy: „2012”, nie „2 012”), daty `yyyy-MM-dd` (gdy w danych jest też godzina: `yyyy-MM-dd HH:mm`), pola tak/nie jako „tak” albo „nie”.
- Statystykę z reguły 5 licz w SQL przez `SqlRunner` (nowa metoda w `RecordRepository`), zapytaniem z listy poniżej, z polem z reguły 5 w miejsce `[Pole]`:
  - suma: `SELECT SUM([Pole]) FROM dbo.[HistoricBuildings]`
  - średnia: `SELECT ROUND(AVG(CAST([Pole] AS decimal(18,2))), 2) FROM dbo.[HistoricBuildings]`
  - 5 najczęstszych wartości: `SELECT TOP 5 [Pole] AS Value, COUNT(*) AS Count FROM dbo.[HistoricBuildings] WHERE [Pole] IS NOT NULL GROUP BY [Pole] ORDER BY COUNT(*) DESC, [Pole]`
  - liczba rekordów dla każdej wartości: `SELECT [Pole] AS Value, COUNT(*) AS Count FROM dbo.[HistoricBuildings] WHERE [Pole] IS NOT NULL GROUP BY [Pole] ORDER BY COUNT(*) DESC, [Pole]`
- Pokaż ją w karcie `<div id="statystyka">`: opis po polsku (np. „Średnia: obwód (cm)”) w `<div class="subheader">`, jedną liczbę w `<p data-stat-value class="h1 mb-0">`, a listę wartości jako `<ul class="list-unstyled mb-0">` z `<li data-stat class="d-flex justify-content-between">wartość: <strong>liczba</strong></li>`. Liczby w formacie pl-PL (przecinek dziesiętny).
- W `src/GzmApp/appsettings.json` ustaw `"DataAttribution": "Źródło: Bytom, otwartedane.metropoliagzm.pl, CC BY 4.0"` (stopka już go pokazuje).
- Krótkie metody, bez zagnieżdżonych if-ów.
- Na koniec uruchom `dotnet test tests/GzmApp.Specs --filter Capability=s12-003-pokaz-strone`. Jeśli test nie przechodzi, przeczytaj jego komunikat i popraw kod (nie test) – najwyżej raz. Napisz, ile testów przeszło.
- Na koniec napisz w 3 punktach, co zrobiłeś.

Nie zmieniaj plików poza: `src/GzmApp/Pages/Index.cshtml`, `src/GzmApp/Pages/Index.cshtml.cs`, `src/GzmApp/Pages/Shared/_Lista.cshtml`, `src/GzmApp/Data/RecordRepository.cs`, `src/GzmApp/appsettings.json`. Nie dodawaj pakietów.
~~~~

### ⏸ Pauza – zobaczcie, co powstało

1. **Co powstało**: `src/GzmApp/Pages/Index.cshtml` to teraz cała strona ZabytkiBytom (wyszukiwarka, kafelek, stopka ze źródłem danych); `_Lista.cshtml` pokazuje pola z Waszej decyzji 2, a statystykę z decyzji 3 liczy nowa metoda w `RecordRepository.cs`.
2. Otwórzcie http://localhost:5080 i zacznijcie pisać w wyszukiwarce. Lista zmienia się bez przeładowania strony.
3. F12 → Network (poradnik §6): przy pisaniu co 300 ms pojawiają się żądania `?handler=Szukaj` z nagłówkiem `HX-Request: true`. Kliknijcie jedno: w „Response” jest tylko fragment listy – reszta strony zostaje na miejscu. W Elements zobaczcie, że podmienia się tylko `<div id="wyniki">`.
4. F12 → ikona telefonu (tryb mobilny): zwęźcie okno – kafelek przesuwa się nad listę, strona działa też na telefonie.
5. W stopce jest źródło danych z licencją CC BY 4.0 – dane są otwarte, ale autor zbioru chce, żeby było widać, skąd pochodzą.
6. Adres `?handler=Szukaj&q=…` otwarty wprost w przeglądarce pokazuje teraz całą stronę – sam fragment dostaje tylko HTMX.
7. Testing → ▶ „Run Tests”. Zielone są testy kroków 1–3. W terminalu to samo: `dotnet test tests/GzmApp.Specs --filter Capability=s12-003-pokaz-strone`.

### ⏸ Pauza – decyzja 4: co ma przeanalizować AI?

To najciekawsza decyzja – Kilo proponuje zadania dla AI **na podstawie Waszych danych i wszystkich wcześniejszych decyzji**, a nie z gotowej listy.

1. Wklejcie Kilo do rozmowy:

~~~text
Zaproponuj nam 3 różne warianty decyzji 4: „co ma przeanalizować AI” w ZabytkiBytom.
Popatrz na kolumny naszych danych, na to, co już zbudowaliśmy, i na nasze dotychczasowe decyzje z LOG-DECYZJI.md – propozycje mają pasować do tego portalu.
Każdy wariant to jedno zdanie według wzorca poniżej – w miejsce {zadanie} wstaw konkretne, pożyteczne zadanie analityczne dla naszych danych (nie powtarzaj tych samych pomysłów, jakie już zapisaliśmy w LOG-DECYZJI.md):
Model dostaje zadanie: {zadanie}
Przedstaw warianty po polsku, powiedz, czego parę się z nich dowie, zapytaj nas, który wybieramy, i czekaj na odpowiedź.
~~~

2. Wybierzcie wariant i odpowiedzcie Kilo.
3. Wybrane zdanie wklejcie w pliku `scenarios/s12/spec/s12-004-analiza-ai.md` w miejsce zdania reguły 3 (zostawcie „3. ”). Zapiszcie, potem Ctrl+Shift+P → „.NET: Build” i ⟳ w panelu Testing.
4. Napiszcie Kilo: „Zapisz tę decyzję w LOG-DECYZJI.md”.

W specyfikacji jest już zdanie domyślne – bez decyzji testy działają dla niego.

## Krok 4 – analiza AI

~~~~text
CEL
Przycisk „✨ Analizuj z AI”: rekordy z listy wyników idą do modelu sztucznej inteligencji GPT-6 Luna, a jego analiza pojawia się na stronie w karcie, która mówi wprost, że ten tekst napisała AI.

SPECYFIKACJA
Reguły tego kroku są w pliku `scenarios/s12/spec/s12-004-analiza-ai.md` (sekcja „Reguły”). Przeczytaj go najpierw. Każda reguła to jeden test w `tests/GzmApp.Specs`. Nie zmieniaj plików specyfikacji ani testów.

DANE
- Model AI (GPT-6 Luna) jest już podłączony w `Program.cs`: z DI bierzesz `IChatClient` (`Microsoft.Extensions.AI`) i `IOptions<Sol6Options>` (`GzmApp.Ai`, nazwa modelu w `Model`). Klucz jest w pliku `src/GzmApp/appsettings.Local.json` od prowadzącego. Nie twórz własnego klienta AI i nie wpisuj klucza w kod.
- Zadanie dla modelu to zdanie z reguły 3 specyfikacji, bez początku „Model dostaje zadanie: ”.
- Rekordy: te same co na liście dla frazy `q` – `Search(q, 20)` z `RecordRepository`. Do modelu wyślij pola: `BuildingNumber`, `Street`, `Id`, `Name`, `Style`. Jedna linia na rekord: `Nr <SourceRow>: Pole=wartość; Pole=wartość`. Wartości w tych samych formatach co na liście (liczby pl-PL, daty `yyyy-MM-dd`, tak/nie), puste pole jako „brak”. Jeśli `Search` nie zwraca któregoś pola, dopisz je w `RecordRepository.cs`.

ZASADY
- `IAiAnalyzer` i `AiAnalyzer` w `src/GzmApp/Ai/AiAnalyzer.cs`: metoda `AnalyzeAsync(string? q, CancellationToken ct)` zwraca `AiAnalysis(string Text, string Model, int RecordCount, string Prompt)`; `Prompt` to tekst wysłany do modelu.
- Dwie wiadomości: `new ChatMessage(ChatRole.System, "Jesteś analitykiem danych. Odpowiadaj po polsku, krótko (najwyżej 8 punktów zaczynających się od „- ”), tylko na podstawie przekazanych rekordów. Nie wymyślaj faktów. Podawaj numery rekordów (Nr). Pisz zwykłym tekstem, bez Markdown (bez ** i #).")` i `new ChatMessage(ChatRole.User, zadanie + "\n\nRekordy:\n" + linie)`. Jedno wywołanie na kliknięcie: `var response = await chat.GetResponseAsync(messages, cancellationToken: ct);`, odpowiedź to `response.Text`.
- Wyjątek `Sol6Exception` (brak klucza albo model nie odpowiada) złap w `OnGetAnalizuj` i zwróć krótki fragment ze statusem 200: `Content($"<div class=\"alert alert-warning\">{encoder.Encode(ex.Message)}</div>", "text/html; charset=utf-8")`, gdzie `encoder` to `HtmlEncoder` wstrzyknięty z DI (zostawia polskie znaki i półpauzę) – bez strony błędu.
- Odpowiedź modelu zawiera polskie znaki i znaki typograficzne (np. półpaużę „–”). Enkoder, który zostawia je dosłownie, jest już zarejestrowany w `Program.cs` (`AddWebEncoders` z `UnicodeRanges.All`) – nie rejestruj go drugi raz. Tekst pokazuj w widoku przez zwykłe `@` i nie koduj go ręcznie (`WebUtility.HtmlEncode`, `HtmlEncoder.Default`): one zamieniają te znaki na encje (`&#8211;`).
- Handler `OnGetAnalizuj(string? q)` w `src/GzmApp/Pages/Index.cshtml.cs` zwraca `Partial("_Analiza", analiza)`.
- W `src/GzmApp/Pages/Index.cshtml`, w tej samej karcie: między `card-body` z polem wyszukiwania a `<div id="wyniki">`:

```html
<div class="card-body border-top">
  <button type="button" class="btn btn-purple" hx-get="?handler=Analizuj" hx-include="[name=q]"
          hx-target="#analiza" hx-indicator="#analiza-czeka" hx-disable="this">✨ Analizuj z AI</button>
  <span id="analiza-czeka" class="htmx-indicator align-items-center text-secondary ms-2">
    <span class="spinner-border spinner-border-sm me-2"></span>AI analizuje dane…</span>
  <div id="analiza" class="mt-3"></div>
</div>
```

- Widok częściowy `src/GzmApp/Pages/Shared/_Analiza.cshtml`: `<div class="card card-ai" data-ai>` z nagłówkiem `<div class="card-header"><h3 class="card-title">✨ Analiza AI</h3><span class="badge bg-purple-lt ms-auto">model …</span></div>`, odpowiedzią w `<div class="card-body ai-text">@Model.Text</div>` (zwykłe `@`, nigdy `Html.Raw` – odpowiedź modelu to tekst, nie HTML) i stopką `<div class="card-footer text-secondary small">`: „Ten tekst wygenerowała sztuczna inteligencja (model …) na podstawie N rekordów z Waszej bazy. AI może się mylić – sprawdźcie w danych.” oraz `<details><summary>Co poszło do modelu?</summary><pre class="small">@Model.Prompt</pre></details>`. Bez `<html>` i bez layoutu.
- Rejestracja `AiAnalyzer` w `AddAppServices` w `src/GzmApp/AppServices.cs`. Krótkie metody, bez zagnieżdżonych if-ów.
- Na koniec uruchom `dotnet test tests/GzmApp.Specs --filter Capability=s12-004-analiza-ai`. Reguły 1–8 sprawdza udawany model (bez tokenów), a reguła 9 – jedno zapytanie do prawdziwego modelu. Jeśli test nie przechodzi, przeczytaj jego komunikat i popraw kod (nie test) – najwyżej raz. Napisz, ile testów przeszło.
- Na koniec napisz w 3 punktach, co zrobiłeś.

Nie zmieniaj plików poza: `src/GzmApp/Ai/AiAnalyzer.cs`, `src/GzmApp/Pages/Index.cshtml`, `src/GzmApp/Pages/Index.cshtml.cs`, `src/GzmApp/Pages/Shared/_Analiza.cshtml`, `src/GzmApp/Data/RecordRepository.cs`, `src/GzmApp/AppServices.cs`. Nie dodawaj pakietów.
~~~~

### ⏸ Pauza – zobaczcie, co powstało

1. **Co powstało**: `src/GzmApp/Ai/AiAnalyzer.cs` (rozmowa z modelem), przycisk i karta AI na stronie głównej oraz widok `src/GzmApp/Pages/Shared/_Analiza.cshtml`.
2. Otwórzcie http://localhost:5080, wpiszcie frazę i kliknijcie „✨ Analizuj z AI”. Po kilku sekundach pojawia się fioletowa karta. Ten tekst napisał model AI, a nie Wasz kod – i karta mówi o tym wprost.
3. Rozwińcie „Co poszło do modelu?”: zadanie z Waszej decyzji 4 i rekordy z listy. Model widzi tylko to – nic więcej o Waszej bazie nie wie.
4. Terminal: wpisy `AI (…): wysyłam do modelu … znaków` i `odpowiedź po … s, tokeny: …`. Tokeny to „waluta” AI – każde kliknięcie kosztuje.
5. Sprawdźcie AI: czy numery „Nr” i liczby zgadzają się z listą wyników? Kliknijcie jeszcze raz – odpowiedź może być trochę inna. AI może się mylić, dlatego karta to mówi.
6. F12 → Network: żądanie `?handler=Analizuj&q=…`. Z modelem rozmawia serwer – klucz nie trafia do przeglądarki.
7. Testing → ▶ „Run Tests”. Wszystkie testy kroków 0–4 są zielone. Reguły 1–8 kroku 4 sprawdza udawany model: zawsze odpowiada tak samo, więc test wie, czego szukać na stronie. Reguła 9 pyta prawdziwy model – sprawdza, że u Was AI naprawdę działa.

## Gotowe! Co dalej?

Macie całą aplikację: dane w bazie, wyszukiwarkę, stronę z HTMX i analizę AI. Jeśli zostało Wam trochę czasu, dodajcie coś od siebie – własnym promptem albo jednym z pomysłów:

1. Drugi kafelek ze statystyką – poproście Kilo o inne propozycje decyzji 3 i wklejcie wybrane zdanie do specyfikacji jako regułę 7 kroku 3: test będzie czerwony, dopóki Kilo go nie zrobi.
2. Drugie zadanie dla AI – poproście Kilo o inne propozycje decyzji 4 i dodajcie drugi przycisk (np. „🔎 Sprawdź jakość danych”).
3. Sortowanie listy po wybranym polu (przycisk przy nagłówku listy, `hx-get` z parametrem `sort`).
4. Przycisk „Pokaż więcej” pod listą: kolejne 20 rekordów doklejane na końcu (`hx-swap="beforeend"`).
5. Logowanie i ulubione: zalogowany użytkownik oznacza rekordy gwiazdką ★. Ulubione w ZabytkiBytom to zabytki na trasie spaceru po Bytomiu. Specyfikacja czeka w `scenarios/s12/spec/dodatkowe/s12-005-zapisz-ulubione.md` – przenieście ten plik do `scenarios/s12/spec/` i przebudujcie testy od zera (terminal: `dotnet build --no-incremental tests/GzmApp.Specs`): pojawi się 8 nowych czerwonych testów. Zwykłe „Build” nowego pliku nie zauważy.

### ⏸ Pauza – decyzja 5: co pokazać w „Moich ulubionych”?

1. Wklejcie Kilo do rozmowy:

~~~text
Zaproponuj nam 3 warianty decyzji 5: „co pokazać na stronie Moje ulubione” w ZabytkiBytom.
Każdy wariant to jedno zdanie według któregoś wzorca poniżej (bez zmian w treści – to trzy różne możliwości):
Strona „Moje ulubione” pokazuje tytuły ulubionych rekordów.
Strona „Moje ulubione” pokazuje tytuł i datę dodania każdego rekordu, od najnowszych.
Strona „Moje ulubione” pokazuje tytuł i przycisk „Usuń”, który usuwa rekord z listy bez przeładowania strony.
Przedstaw warianty po polsku, powiedz, czym się różnią, zapytaj nas, który wybieramy, i czekaj na odpowiedź.
~~~

2. Wybierzcie wariant i odpowiedzcie Kilo.
3. Wybrane zdanie wklejcie w pliku `scenarios/s12/spec/s12-005-zapisz-ulubione.md` w miejsce zdania reguły 8 (zostawcie „8. ”). Zapiszcie plik.
4. Napiszcie Kilo: „Zapisz tę decyzję w LOG-DECYZJI.md”.

~~~~text
CEL
Zalogowany użytkownik oznacza rekordy gwiazdką ★ i widzi je na stronie „Moje ulubione”. Ulubione w ZabytkiBytom to zabytki na trasie spaceru po Bytomiu.

SPECYFIKACJA
Reguły tego kroku są w pliku `scenarios/s12/spec/s12-005-zapisz-ulubione.md` (sekcja „Reguły”). Przeczytaj go najpierw. Każda reguła to jeden test w `tests/GzmApp.Specs`. Nie zmieniaj plików specyfikacji ani testów.

DANE
- Utwórz plik `db/900_favorites.sql` z tym skryptem i uruchamiaj go przy starcie (w `RunAtStartupAsync` w `src/GzmApp/AppServices.cs`), czytając go z `Path.Combine(AppContext.BaseDirectory, "db", "900_favorites.sql")`:

```sql
IF OBJECT_ID(N'dbo.Favorites') IS NULL
CREATE TABLE dbo.Favorites (
    UserId nvarchar(450) NOT NULL,
    Scenario nvarchar(10) NOT NULL,
    RecordId int NOT NULL,
    CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_Favorites_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Favorites PRIMARY KEY (UserId, Scenario, RecordId)
);
```

- `RecordId` to kolumna `RecordId` z tabeli `HistoricBuildings`. Tytuł rekordu: pole `Name`.
- `Scenario` = `s12` (klucz `Scenario` w `appsettings.json`). `UserId` = identyfikator zalogowanego użytkownika.

ZASADY
- `IFavoritesRepository` i `FavoritesRepository` w `src/GzmApp/Data/FavoritesRepository.cs`: `Add`, `Remove`, `ListFor(userId)` przez `SqlRunner`. `Add` i `Remove` wykonane dwa razy dają ten sam stan (`IF NOT EXISTS` albo `MERGE` w jednym zapytaniu). Każda publiczna metoda z `userId` zaczyna się od `ArgumentException.ThrowIfNullOrWhiteSpace(userId);`. Klasy pomocnicze (np. wiersz ulubionego: `RecordId`, tytuł, `CreatedAt`) trzymaj w tym samym pliku, jako klasy z właściwościami (Dapper dopasowuje po nazwach kolumn).
- `UserId` zalogowanego: `User.FindFirstValue(ClaimTypes.NameIdentifier)` w PageModel (`using System.Security.Claims;`), a w widoku `User.FindFirst(ClaimTypes.NameIdentifier)?.Value`.
- Przycisk ★ to widok częściowy `src/GzmApp/Pages/Shared/_Gwiazdka.cshtml` (model: `public record StarModel(int RecordId, bool IsFavorite);` w `FavoritesRepository.cs`): w ulubionych – „★” z `data-favorite="true"` i `hx-delete="?handler=Ulubione&id=..."`, poza nimi – „☆” z `data-favorite="false"` i `hx-post="?handler=Ulubione&id=..."`, oba z `hx-swap="outerHTML"`. Używa go `_Lista.cshtml` przy każdym rekordzie.
- Stan ★ `_Lista.cshtml` sprawdza sama: `@inject IFavoritesRepository` i ulubione zalogowanego użytkownika (niezalogowany: wszystkie „☆”). Nie przekazuj tego przez `ViewData` – `Partial(...)` w PageModel go nie kopiuje.
- Handlery `OnPostUlubione(int id)` i `OnDeleteUlubione(int id)` w `Index.cshtml.cs` zwracają `Partial("_Gwiazdka", …)` z nowym stanem. Niezalogowany: status 401 i fragment „Zaloguj się, aby dodać do ulubionych” – sprawdź `User.Identity?.IsAuthenticated` w handlerze i zwróć `new ContentResult { StatusCode = 401, ContentType = "text/html; charset=utf-8", Content = "<p class=\"alert alert-warning\">Zaloguj się, aby dodać do ulubionych</p>" }`. Nie dawaj `[Authorize]` na `IndexModel` ani na handlerach: wtedy niezalogowany dostaje przekierowanie 302 do logowania zamiast 401.
- Token antiforgery wysyła już `<body hx-headers:inherited='{"RequestVerificationToken":"..."}'>` w `_Layout.cshtml`. Nie wyłączaj walidacji antiforgery.
- Strona `src/GzmApp/Pages/Ulubione.cshtml` z `[Authorize]` według reguły 8 (strona „Moje ulubione”): `<ul id="ulubione">`, jeden `<li>` na rekord, tytuł w `<span class="tytul">`. Pokazuj tylko to, co obejmuje wybrany wariant reguły 8: same tytuły; z datą dodania: `CreatedAt` (w bazie UTC), czas polski `dd.MM.yyyy HH:mm`; z przyciskiem „Usuń”: `hx-delete="?handler=Usun&id=..."` z `hx-target="closest li"` i `hx-swap="outerHTML"`, a handler `OnDeleteUsun(int id)` w `Ulubione.cshtml.cs` zwraca pustą odpowiedź 200.
- Link „Moje ulubione” w menu w `_Layout.cshtml`, widoczny zawsze (niezalogowanego strona przekieruje do logowania).
- Rejestracja repozytorium w `AddAppServices` w `src/GzmApp/AppServices.cs`. Krótkie metody, bez zagnieżdżonych if-ów.
- Na koniec uruchom `dotnet test tests/GzmApp.Specs --filter Capability=s12-005-zapisz-ulubione`. Jeśli test nie przechodzi, przeczytaj jego komunikat i popraw kod (nie test) – najwyżej raz. Napisz, ile testów przeszło.
- Na koniec napisz w 3 punktach, co zrobiłeś.

Nie zmieniaj plików poza: `db/900_favorites.sql`, `src/GzmApp/Data/FavoritesRepository.cs`, `src/GzmApp/Pages/Index.cshtml.cs`, `src/GzmApp/Pages/Shared/_Lista.cshtml`, `src/GzmApp/Pages/Shared/_Gwiazdka.cshtml`, `src/GzmApp/Pages/Ulubione.cshtml`, `src/GzmApp/Pages/Ulubione.cshtml.cs`, `src/GzmApp/Pages/Shared/_Layout.cshtml`, `src/GzmApp/AppServices.cs`. Nie dodawaj pakietów.
~~~~

### ⏸ Pauza – zobaczcie, co powstało

1. **Co powstało**: tabela `Favorites` w bazie, `src/GzmApp/Data/FavoritesRepository.cs` (zapisywanie ulubionych), gwiazdki na liście i strona „Moje ulubione” (`src/GzmApp/Pages/Ulubione.cshtml`).
2. Zarejestrujcie się (przycisk „Zarejestruj się”) i zalogujcie. Kliknijcie ☆ przy rekordzie – zamienia się w ★ bez przeładowania strony.
3. Strona „Moje ulubione” pokazuje Wasz wybór tak, jak zdecydowaliście w decyzji 5. Wylogujcie się i wróćcie na stronę główną: wszystkie gwiazdki są z powrotem „☆” – ulubione są przypisane do konta.
4. F12 → Network: kliknięcie ★ to żądanie POST z nagłówkiem `RequestVerificationToken` (token antiforgery – ochrona przed fałszerstwem żądań).
5. Testing → ▶ „Run Tests”. Zielone są wszystkie testy, także kroku 5.
