# RezerwacjaRadar – prompty krok po kroku (s08)

Pilot kopiuje cały blok promptu do Kilo Code i akceptuje zmiany. Nawigator prowadzi pauzy. Po każdym kroku zamieniacie się rolami.
Aplikację uruchamiacie klawiszem F5 w VS Code. Adres: http://localhost:5080.

Pracujecie jak zawodowcy: najpierw **specyfikacja**, potem **testy**, na końcu **kod**. Specyfikacja to 4 pliki w `scenarios/s08/spec/` – po jednym na krok. Każde ponumerowane zdanie w sekcji „Reguły” to jeden test. Kilo pisze kod, a testy mówią, czy kod robi to, co obiecuje zdanie.

## Krok 0 – specyfikacja i czerwone testy

### ⏸ Pauza – zanim zaczniecie

1. Otwórzcie `src/GzmApp/appsettings.json` i wpiszcie swój scenariusz: `"Scenario": "s08"`. Zapiszcie (Ctrl+S).
2. Otwórzcie folder `scenarios/s08/spec/` i przeczytajcie plik kroku 1. Sekcja „Co się dzieje” opisuje krok słowami, a „Reguły” – co dokładnie ma działać.
3. Ikona kolby po lewej (Testing) → `GzmApp.Specs` → ▶ „Run Tests”. Każdy test nazywa się tak jak zdanie ze specyfikacji, np. „Reguła 1: …”. Wszystkie są czerwone – aplikacja jeszcze nic nie umie.
4. Wasz cel: zamienić czerwone testy w zielone, krok po kroku.

## Krok 1 – dane z pliku do bazy

~~~~text
CEL
Wczytaj plik CSV „Metrorower: rezerwacje” do nowej tabeli `BikeReservations` w SQL Server przez Dapper. Import uruchamia się przy starcie aplikacji, gdy tabela jest pusta.

SPECYFIKACJA
Reguły tego kroku są w pliku `scenarios/s08/spec/s08-001-importuj-dane.md` (sekcja „Reguły”). Przeczytaj go najpierw. Każda reguła to jeden test w `tests/GzmApp.Specs`. Nie zmieniaj plików specyfikacji ani testów.

DANE
- Plik: `data/raw/s08_metrorower-rezerwacje.csv` w katalogu głównym repozytorium. Ścieżka z aplikacji: `Path.Combine(environment.ContentRootPath, "..", "..", "data/raw/s08_metrorower-rezerwacje.csv")` (`environment` to `IWebHostEnvironment`).
- Kodowanie: UTF-8. Separator: przecinek `,`. Nagłówek w pierwszej linii.
- Kolumny do importu (pozostałych kolumn pliku nie importuj):

| Kolumna w pliku | Pole w tabeli | Typ SQL | Kultura | Format | Może być puste |
|---|---|---|---|---|---|
| `Data` | `ReservedAt` | `datetime2(0)` | invariant | `yyyy-MM-dd'T'HH:mm:ss'Z'` | nie |
| `Wypożyczono` | `Rented` | `bit` | pl-PL | „true” = 1, „false” = 0 | nie |
| `Ilość wypożyczonych rowerów` | `RentedBikes` | `int` | pl-PL | – | nie |
| `Ilość zarezerwowanych rowerów` | `ReservedBikes` | `int` | pl-PL | – | nie |
| `Numer stacji rezerwacji` | `StationNumber` | `int` | pl-PL | – | tak |
| `Nazwa stacji rezerwacji` | `StationName` | `nvarchar(30)` | pl-PL | – | nie |
| `Kanał` | `Channel` | `nvarchar(30)` | pl-PL | – | nie |
| `Status` | `Status` | `nvarchar(20)` | pl-PL | – | nie |
| `Czas wygaśnięcia` | `ExpiresAt` | `datetime2(0)` | invariant | `yyyy-MM-dd'T'HH:mm:ss'Z'` | nie |

- Klucz: `RowHash` = SHA-256 z wartości wszystkich importowanych pól (po `Trim`) połączonych znakiem `\u001F`, zapisany jako `binary(32)`.
- W pliku są identyczne wiersze (drugie z par: 63, 74). Wstaw tylko pierwszy z nich – pomiń wiersz, którego `RowHash` już jest w tabeli.
- Utwórz plik `db/s08_create.sql` z tym skryptem i uruchamiaj go przy starcie (w `RunAtStartupAsync`) przed importem. W działającej aplikacji czytaj go z `Path.Combine(AppContext.BaseDirectory, "db", "s08_create.sql")` (tak jak `DatabaseBootstrapper`):

```sql
IF OBJECT_ID(N'dbo.BikeReservations') IS NULL
CREATE TABLE dbo.[BikeReservations] (
    [RecordId] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_BikeReservations] PRIMARY KEY,
    [SourceRow] int NOT NULL,
    [ReservedAt] datetime2(0) NOT NULL,
    [Rented] bit NOT NULL,
    [RentedBikes] int NOT NULL,
    [ReservedBikes] int NOT NULL,
    [StationNumber] int NULL,
    [StationName] nvarchar(30) NOT NULL,
    [Channel] nvarchar(30) NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [ExpiresAt] datetime2(0) NOT NULL,
    [RowHash] binary(32) NOT NULL,
    CONSTRAINT [UQ_BikeReservations_Key] UNIQUE ([RowHash])
);
```

ZASADY
- W `src/GzmApp/appsettings.json` ustaw `"Scenario": "s08"`.
- Plik czytaj pakietem CsvHelper (jest już w projekcie), `CultureInfo("pl-PL")`, separator jak wyżej. Nie dziel linii przez `Split`.
- Liczby i daty odczytuj z kulturą i formatem z tabeli kolumn. Puste pole albo wartość, której nie da się odczytać, w kolumnie z „Może być puste: tak” zapisz jako NULL. W kolumnie z „nie” taki wiersz pomiń i zapisz jego numer w logu. Bez wyjątku.
- `SourceRow` = numer wiersza danych w pliku, od 1, bez nagłówka. Tekst przytnij (`Trim`).
- Zapytania wysyłaj przez istniejącą klasę `SqlRunner`, parametry przez Dappera.
- Po imporcie zapisz w logu liczbę wierszy w tabeli i czas importu. Gdy tabela nie jest pusta, zapisz w logu „Import pominięty – w tabeli jest N wierszy”.
- Klasa `CsvImporter` z interfejsem `ICsvImporter`. Rejestracja w metodzie `AddAppServices`, a skrypt tabeli i import w `RunAtStartupAsync` – obie w `src/GzmApp/AppServices.cs`. `Program.cs` już je wywołuje, nie zmieniaj go.
- Krótkie metody, bez zagnieżdżonych if-ów.
- Na koniec uruchom `dotnet test tests/GzmApp.Specs --filter Capability=s08-001-importuj-dane`. Jeśli test nie przechodzi, przeczytaj jego komunikat i popraw kod (nie test) – najwyżej raz. Napisz, ile testów przeszło.
- Na koniec napisz w 3 punktach, co zrobiłeś.

Nie zmieniaj plików poza: `db/s08_create.sql`, `src/GzmApp/Data/CsvImporter.cs`, `src/GzmApp/AppServices.cs`, `src/GzmApp/appsettings.json`. Nie dodawaj pakietów.
~~~~

### ⏸ Pauza – zobaczcie, co powstało

1. Uruchomcie aplikację (F5). W terminalu pojawia się wpis z liczbą zaimportowanych wierszy.
2. Rozszerzenie mssql (ikona bazy po lewej): serwer `(localdb)\MSSQLLocalDB`, baza `GzmWorkshop`, tabela `BikeReservations` → prawy klik → „Select Top 1000”. To Wasze dane z pliku – już w bazie.
3. Zatrzymajcie aplikację (Shift+F5).
4. Testing → ▶ „Run Tests”. Testy kroku 1 (`s08-001-importuj-dane`) są zielone. Czerwony test? Kliknijcie go: komunikat mówi, czego brakuje. Wklejcie ten komunikat do Kilo.

### ⏸ Pauza – decyzja 1: po czym użytkownik będzie szukał?

**A – stacja**

~~~text
Wyszukiwarka szuka frazy w dowolnym miejscu pola `StationName` (kolumna w pliku: `Nazwa stacji rezerwacji`).
~~~

**B – kanał (aplikacja iOS / Android)**

~~~text
Wyszukiwarka szuka frazy w dowolnym miejscu pola `Channel` (kolumna w pliku: `Kanał`).
~~~

**C – status rezerwacji**

~~~text
Wyszukiwarka szuka frazy w dowolnym miejscu pola `Status` (kolumna w pliku: `Status`).
~~~

Wariant A jest już w specyfikacji. Wybraliście inny? Skopiujcie jego zdanie i wklejcie w pliku `scenarios/s08/spec/s08-002-szukaj-rekordow.md` w miejsce zdania reguły 1 (zostawcie „1. ” na początku). Zapiszcie plik.

## Krok 2 – wyszukiwarka (endpoint)

~~~~text
CEL
Adres, który zwraca rekordy z tabeli `BikeReservations` pasujące do wpisanej frazy. Odpowiedź to fragment HTML (sama lista), a nie cała strona.

SPECYFIKACJA
Reguły tego kroku są w pliku `scenarios/s08/spec/s08-002-szukaj-rekordow.md` (sekcja „Reguły”). Przeczytaj go najpierw. Każda reguła to jeden test w `tests/GzmApp.Specs`. Nie zmieniaj plików specyfikacji ani testów.

DANE
- Adres: `/?handler=Szukaj&q=<fraza>`.
- Pole, w którym szukasz, podaje reguła 1. „W dowolnym miejscu pola” to `LIKE '%' + @q + '%'`, a „zaczyna się od frazy” to `LIKE @q + '%'`.
- Każdy wynik pokazuje „Nr `SourceRow`: `StationName`”.

ZASADY
- `IRecordRepository` i `RecordRepository` w `src/GzmApp/Data/RecordRepository.cs`, metoda `Search(string? q, int take)`. Zapytanie przez `SqlRunner`, fraza jako parametr `@q` – bez sklejania tekstu SQL.
- Porównanie bez rozróżniania wielkości liter i ogonków: `COLLATE Latin1_General_CI_AI`.
- Wyniki według `SourceRow`, najwyżej 20 (`take` = 20), także dla pustej frazy. `Wyników: N` to liczba wszystkich pasujących rekordów (osobne `COUNT(*)`), lista pokazuje najwyżej 20.
- Fraza dłuższa niż 100 znaków: odpowiedź 400 z krótkim fragmentem `<p class="alert alert-warning">…</p>`.
- Handler `OnGetSzukaj(string? q)` w `src/GzmApp/Pages/Index.cshtml.cs` zwraca `Partial("_Lista", wyniki)`.
- Widok częściowy `src/GzmApp/Pages/Shared/_Lista.cshtml` (klasy CSS z biblioteki Tabler, która jest już w `_Layout.cshtml`): `<p id="licznik" class="text-secondary px-3 pt-3 mb-2">Wyników: N</p>` i `<ul id="lista" class="list-group list-group-flush">` z jednym `<li class="list-group-item" data-record-id="@r.RecordId"><span class="badge bg-blue-lt me-2">Nr @r.SourceRow:</span> <span class="tytul fw-bold">…</span></li>` na rekord. Bez `<html>` i bez layoutu.
- Rejestracja repozytorium w `AddAppServices` w `src/GzmApp/AppServices.cs`. Krótkie metody, bez zagnieżdżonych if-ów.
- Na koniec uruchom `dotnet test tests/GzmApp.Specs --filter Capability=s08-002-szukaj-rekordow`. Jeśli test nie przechodzi, przeczytaj jego komunikat i popraw kod (nie test) – najwyżej raz. Napisz, ile testów przeszło.
- Na koniec napisz w 3 punktach, co zrobiłeś.

Nie zmieniaj plików poza: `src/GzmApp/Data/RecordRepository.cs`, `src/GzmApp/Pages/Index.cshtml.cs`, `src/GzmApp/Pages/Shared/_Lista.cshtml`, `src/GzmApp/AppServices.cs`. Nie dodawaj pakietów.
~~~~

### ⏸ Pauza – zobaczcie, co powstało

1. Uruchomcie aplikację i otwórzcie adres `http://localhost:5080/?handler=Szukaj&q=` z dopisanym słowem z wybranej kolumny (dla wariantu A np. `27402`). Widzicie samą listę – bez menu i stopki. To „fragment HTML”.
2. F12 → zakładka Network → odświeżcie stronę. Kliknijcie żądanie `?handler=Szukaj`: status 200, a w „Response” jest tylko lista.
3. W terminalu (log SQL) widać zapytanie z parametrem `q` – to Wasza fraza.
4. Testing → ▶ „Run Tests”. Zielone są testy kroków 1 i 2. Reguła 5 sprawdza, że fraza `' OR 1=1 --` nic nie znajduje – to ochrona przed atakiem SQL injection.

### ⏸ Pauza – decyzja 2: co pokazać na liście?

**A – czas rezerwacji (UTC) i status**

~~~text
Przy każdym rekordzie lista pokazuje tytuł `StationName` i pola: `ReservedAt`, `Status`.
~~~

**B – kanał i czy wypożyczono**

~~~text
Przy każdym rekordzie lista pokazuje tytuł `StationName` i pola: `Channel`, `Rented`.
~~~

**C – czas wygaśnięcia (UTC)**

~~~text
Przy każdym rekordzie lista pokazuje tytuł `StationName` i pola: `ExpiresAt`.
~~~

### ⏸ Pauza – decyzja 3: jaki kafelek ze statystyką?

**A – ile rezerwacji zamknięto, anulowano i wygasło**

~~~text
Kafelek ze statystyką pokazuje liczbę rekordów dla każdej wartości pola `Status`.
~~~

**B – ile rezerwacji z każdego kanału**

~~~text
Kafelek ze statystyką pokazuje liczbę rekordów dla każdej wartości pola `Channel`.
~~~

**C – łączna liczba zarezerwowanych rowerów**

~~~text
Kafelek ze statystyką pokazuje sumę pola `ReservedBikes`.
~~~

Warianty A są już w specyfikacji. Wybraliście inne? Wklejcie ich zdania w pliku `scenarios/s08/spec/s08-003-pokaz-strone.md` w miejsce reguły 4 (lista) i reguły 5 (kafelek). Zapiszcie plik.

## Krok 3 – strona z wyszukiwarką i kafelkiem (HTMX)

~~~~text
CEL
Strona główna aplikacji RezerwacjaRadar: wyszukiwarka działająca podczas pisania (HTMX 4), lista wyników i kafelek ze statystyką.

SPECYFIKACJA
Reguły tego kroku są w pliku `scenarios/s08/spec/s08-003-pokaz-strone.md` (sekcja „Reguły”). Przeczytaj go najpierw. Każda reguła to jeden test w `tests/GzmApp.Specs`. Nie zmieniaj plików specyfikacji ani testów.

ZASADY
- `src/GzmApp/Pages/Index.cshtml` – układ z klasami Tablera (biblioteka CSS jest już w `_Layout.cshtml`), który dobrze wygląda też na telefonie:

```html
<div class="page-header mb-4">
  <h1 class="page-title">RezerwacjaRadar</h1>
  <div class="text-secondary">Ostatnie rezerwacje Metroroweru: które kończą się wypożyczeniem, a które wygasają albo są anulowane.</div>
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
- Metoda `IsHtmx()` sprawdza nagłówek `HX-Request`. W `OnGetSzukaj`: żądanie HTMX dostaje fragment `_Lista`, zwykłe wejście na ten sam adres – całą stronę z wynikami dla frazy `q`.
- Listę w `_Lista.cshtml` zmień według reguły 4: pola pokaż w tym samym `<li>`, za tytułem, w `<div class="text-secondary small">` jako „nazwa: wartość · nazwa: wartość” z polskimi nazwami pól: `ReservedAt` – data rezerwacji, `Rented` – wypożyczono, `RentedBikes` – wypożyczone rowery, `ReservedBikes` – zarezerwowane rowery, `StationNumber` – nr stacji, `StationName` – stacja, `Channel` – kanał, `Status` – status, `ExpiresAt` – wygasa. Liczby w formacie pl-PL, daty `yyyy-MM-dd`, pola tak/nie jako „tak” albo „nie”.
- Statystykę z reguły 5 licz w SQL przez `SqlRunner` (nowa metoda w `RecordRepository`), zapytaniem z listy poniżej, z polem z reguły 5 w miejsce `[Pole]`:
  - suma: `SELECT SUM([Pole]) FROM dbo.[BikeReservations]`
  - średnia: `SELECT ROUND(AVG(CAST([Pole] AS decimal(18,2))), 2) FROM dbo.[BikeReservations]`
  - 5 najczęstszych wartości: `SELECT TOP 5 [Pole] AS Value, COUNT(*) AS Count FROM dbo.[BikeReservations] WHERE [Pole] IS NOT NULL GROUP BY [Pole] ORDER BY COUNT(*) DESC, [Pole]`
  - liczba rekordów dla każdej wartości: `SELECT [Pole] AS Value, COUNT(*) AS Count FROM dbo.[BikeReservations] WHERE [Pole] IS NOT NULL GROUP BY [Pole] ORDER BY COUNT(*) DESC, [Pole]`
- Pokaż ją w karcie `<div id="statystyka">`: opis po polsku (np. „Średnia: obwód (cm)”) w `<div class="subheader">`, jedną liczbę w `<p data-stat-value class="h1 mb-0">`, a listę wartości jako `<ul class="list-unstyled mb-0">` z `<li data-stat class="d-flex justify-content-between">wartość: <strong>liczba</strong></li>`. Liczby w formacie pl-PL (przecinek dziesiętny).
- W `src/GzmApp/appsettings.json` ustaw `"DataAttribution": "Źródło: Urząd Metropolitalny Górnośląsko-Zagłębiowskiej Metropolii, otwartedane.metropoliagzm.pl, CC BY 4.0"` (stopka już go pokazuje).
- Krótkie metody, bez zagnieżdżonych if-ów.
- Na koniec uruchom `dotnet test tests/GzmApp.Specs --filter Capability=s08-003-pokaz-strone`. Jeśli test nie przechodzi, przeczytaj jego komunikat i popraw kod (nie test) – najwyżej raz. Napisz, ile testów przeszło.
- Na koniec napisz w 3 punktach, co zrobiłeś.

Nie zmieniaj plików poza: `src/GzmApp/Pages/Index.cshtml`, `src/GzmApp/Pages/Index.cshtml.cs`, `src/GzmApp/Pages/Shared/_Lista.cshtml`, `src/GzmApp/Data/RecordRepository.cs`, `src/GzmApp/appsettings.json`. Nie dodawaj pakietów.
~~~~

### ⏸ Pauza – zobaczcie, co powstało

1. Otwórzcie `http://localhost:5080` i zacznijcie pisać w wyszukiwarce. Lista zmienia się bez przeładowania strony.
2. F12 → Network: przy pisaniu pojawiają się żądania `?handler=Szukaj` z nagłówkiem `HX-Request: true`. Tylko lista wraca z serwera, reszta strony zostaje.
3. Obok listy jest kafelek ze statystyką, a w stopce – źródło danych. Zwęźcie okno przeglądarki (albo F12 → ikona telefonu): kafelek przesuwa się nad listę – strona działa też na telefonie.
4. Adres `?handler=Szukaj&q=…` otwarty wprost w przeglądarce pokazuje teraz całą stronę – sam fragment dostaje tylko HTMX.
5. Testing → ▶ „Run Tests”. Zielone są testy kroków 1–3.

### ⏸ Pauza – decyzja 4: co ma przeanalizować AI?

**A – jak kończą się rezerwacje**

~~~text
Model dostaje zadanie: Policz, ile rezerwacji zakończyło się wypożyczeniem, a ile wygasło albo zostało anulowanych, i wskaż stację z największą liczbą rezerwacji.
~~~

**B – 3 ciekawostki**

~~~text
Model dostaje zadanie: Znajdź w tych rekordach 3 ciekawostki i krótko je opisz.
~~~

**C – kontrola jakości danych**

~~~text
Model dostaje zadanie: Sprawdź jakość tych danych: wskaż puste pola, literówki i różne zapisy tej samej wartości.
~~~

Wariant A jest już w specyfikacji. Wybraliście inny? Wklejcie jego zdanie w pliku `scenarios/s08/spec/s08-004-analiza-ai.md` w miejsce reguły 3. Zapiszcie plik.

## Krok 4 – analiza AI

~~~~text
CEL
Przycisk „✨ Analizuj z AI”: rekordy z listy wyników idą do modelu sztucznej inteligencji SOL-6, a jego analiza pojawia się na stronie w karcie, która mówi wprost, że ten tekst napisała AI.

SPECYFIKACJA
Reguły tego kroku są w pliku `scenarios/s08/spec/s08-004-analiza-ai.md` (sekcja „Reguły”). Przeczytaj go najpierw. Każda reguła to jeden test w `tests/GzmApp.Specs`. Nie zmieniaj plików specyfikacji ani testów.

DANE
- Model SOL-6 jest już podłączony w `Program.cs`: z DI bierzesz `IChatClient` (`Microsoft.Extensions.AI`) i `IOptions<Sol6Options>` (`GzmApp.Ai`, nazwa modelu w `Model`). Klucz jest w pliku `src/GzmApp/appsettings.Local.json` od prowadzącego. Nie twórz własnego klienta AI i nie wpisuj klucza w kod.
- Zadanie dla modelu to zdanie z reguły 3 specyfikacji, bez początku „Model dostaje zadanie: ”.
- Rekordy: te same co na liście dla frazy `q` – `Search(q, 20)` z `RecordRepository`. Do modelu wyślij pola: `ReservedAt`, `Rented`, `RentedBikes`, `ReservedBikes`, `StationNumber`, `StationName`, `Channel`, `Status`, `ExpiresAt`. Jedna linia na rekord: `Nr <SourceRow>: Pole=wartość; Pole=wartość`.

ZASADY
- `IAiAnalyzer` i `AiAnalyzer` w `src/GzmApp/Ai/AiAnalyzer.cs`: metoda `AnalyzeAsync(string? q, CancellationToken ct)` zwraca `AiAnalysis(string Text, string Model, int RecordCount, string Prompt)`; `Prompt` to tekst wysłany do modelu.
- Dwie wiadomości: `new ChatMessage(ChatRole.System, "Jesteś analitykiem danych. Odpowiadaj po polsku, krótko (najwyżej 8 punktów zaczynających się od „- ”), tylko na podstawie przekazanych rekordów. Nie wymyślaj faktów. Podawaj numery rekordów (Nr). Pisz zwykłym tekstem, bez Markdown (bez ** i #).")` i `new ChatMessage(ChatRole.User, zadanie + "\n\nRekordy:\n" + linie)`. Jedno wywołanie na kliknięcie: `var response = await chat.GetResponseAsync(messages, cancellationToken: ct);`, odpowiedź to `response.Text`.
- Wyjątek `Sol6Exception` (brak klucza albo model nie odpowiada) złap i pokaż jego `Message` w `<div class="alert alert-warning">` – bez strony błędu.
- Handler `OnGetAnalizuj(string? q)` w `src/GzmApp/Pages/Index.cshtml.cs` zwraca `Partial("_Analiza", analiza)`.
- W `src/GzmApp/Pages/Index.cshtml`, w karcie pod polem wyszukiwania:

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
- Na koniec uruchom `dotnet test tests/GzmApp.Specs --filter Capability=s08-004-analiza-ai`. Testy używają udawanego modelu (bez klucza i bez tokenów). Jeśli test nie przechodzi, przeczytaj jego komunikat i popraw kod (nie test) – najwyżej raz. Napisz, ile testów przeszło.
- Na koniec napisz w 3 punktach, co zrobiłeś.

Nie zmieniaj plików poza: `src/GzmApp/Ai/AiAnalyzer.cs`, `src/GzmApp/Pages/Index.cshtml`, `src/GzmApp/Pages/Index.cshtml.cs`, `src/GzmApp/Pages/Shared/_Analiza.cshtml`, `src/GzmApp/AppServices.cs`. Nie dodawaj pakietów.
~~~~

### ⏸ Pauza – zobaczcie, co powstało

1. Otwórzcie `http://localhost:5080`, wpiszcie frazę i kliknijcie „✨ Analizuj z AI”. Po kilku sekundach pojawia się fioletowa karta. Ten tekst napisał model AI, a nie Wasz kod.
2. Rozwińcie „Co poszło do modelu?”: zadanie i Wasze rekordy. Model widzi tylko to – nic więcej o Waszej bazie nie wie.
3. Terminal: wpisy `SOL-6 (…): wysyłam do modelu … znaków` i `odpowiedź po … s, tokeny: …`. Tokeny to „waluta” AI – każde kliknięcie kosztuje.
4. Sprawdźcie AI: czy numery „Nr” i liczby zgadzają się z listą wyników? Kliknijcie jeszcze raz – odpowiedź może być trochę inna. AI może się mylić, dlatego karta to mówi.
5. F12 → Network: żądanie `?handler=Analizuj&q=…`. Z modelem rozmawia serwer – klucz nie trafia do przeglądarki.
6. Testing → ▶ „Run Tests”. Wszystkie testy kroków 1–4 są zielone. Testy kroku 4 używają udawanego modelu: nie zużywają tokenów i zawsze dostają tę samą odpowiedź.

## Gotowe! Co dalej?

Macie całą aplikację: dane w bazie, wyszukiwarkę, stronę z HTMX i analizę AI. Jeśli zostało Wam trochę czasu, dodajcie coś od siebie – własnym promptem albo jednym z pomysłów:

1. Drugi kafelek ze statystyką – weźcie inny wariant z decyzji 3. Najpierw dopiszcie go do specyfikacji jako regułę 7 kroku 3 (zdanie z decyzji 3): test będzie czerwony, dopóki Kilo go nie zrobi.
2. Drugie zadanie dla AI – weźcie inny wariant z decyzji 4 i dodajcie drugi przycisk (np. „🔎 Sprawdź jakość danych”).
3. Sortowanie listy po wybranym polu (przycisk przy nagłówku listy, `hx-get` z parametrem `sort`).
4. Przycisk „Pokaż więcej” pod listą: kolejne 20 rekordów doklejane na końcu (`hx-swap="beforeend"`).
5. Logowanie i ulubione: zalogowany użytkownik oznacza rekordy gwiazdką ★. Ulubione w RezerwacjaRadar to stacje, których rezerwacje para obserwuje. Specyfikacja czeka w `scenarios/s08/spec/dodatkowe/s08-005-zapisz-ulubione.md` – przenieście ten plik do `scenarios/s08/spec/` i przebudujcie projekt: pojawi się 8 nowych czerwonych testów.

### ⏸ Pauza – decyzja 5: co pokazać w „Moich ulubionych”?

**A – lista tytułów**

~~~text
Strona „Moje ulubione” pokazuje tytuły ulubionych rekordów.
~~~

**B – tytuły z datą dodania**

~~~text
Strona „Moje ulubione” pokazuje tytuł i datę dodania każdego rekordu, od najnowszych.
~~~

**C – tytuły z przyciskiem „Usuń”**

~~~text
Strona „Moje ulubione” pokazuje tytuł i przycisk „Usuń”, który usuwa rekord z listy bez przeładowania strony.
~~~

Wariant A jest już w specyfikacji. Wybraliście inny? Wklejcie jego zdanie w pliku `s08-005-zapisz-ulubione.md` w miejsce reguły 8. Zapiszcie plik.

~~~~text
CEL
Zalogowany użytkownik oznacza rekordy gwiazdką ★ i widzi je na stronie „Moje ulubione”. Ulubione w RezerwacjaRadar to stacje, których rezerwacje para obserwuje.

SPECYFIKACJA
Reguły tego kroku są w pliku `scenarios/s08/spec/s08-005-zapisz-ulubione.md` (sekcja „Reguły”). Przeczytaj go najpierw. Każda reguła to jeden test w `tests/GzmApp.Specs`. Nie zmieniaj plików specyfikacji ani testów.

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

- `RecordId` to kolumna `RecordId` z tabeli `BikeReservations`. Tytuł rekordu: pole `StationName`.
- `Scenario` = `s08` (klucz `Scenario` w `appsettings.json`). `UserId` = identyfikator zalogowanego użytkownika.

ZASADY
- `IFavoritesRepository` i `FavoritesRepository` w `src/GzmApp/Data/FavoritesRepository.cs`: `Add`, `Remove`, `ListFor(userId)` przez `SqlRunner`. `Add` i `Remove` wykonane dwa razy dają ten sam stan (`IF NOT EXISTS` albo `MERGE` w jednym zapytaniu).
- Przycisk ★ to widok częściowy `src/GzmApp/Pages/Shared/_Gwiazdka.cshtml` (model: `RecordId` i czy rekord jest w ulubionych): w ulubionych – „★” z `data-favorite="true"` i `hx-delete="?handler=Ulubione&id=..."`, poza nimi – „☆” z `data-favorite="false"` i `hx-post="?handler=Ulubione&id=..."`, oba z `hx-swap="outerHTML"`. Używa go `_Lista.cshtml` przy każdym rekordzie.
- Stan ★ `_Lista.cshtml` sprawdza sama: `@inject IFavoritesRepository` i ulubione zalogowanego użytkownika (niezalogowany: wszystkie „☆”). Nie przekazuj tego przez `ViewData` – `Partial(...)` w PageModel go nie kopiuje.
- Handlery `OnPostUlubione(int id)` i `OnDeleteUlubione(int id)` w `Index.cshtml.cs` zwracają `Partial("_Gwiazdka", …)` z nowym stanem. Niezalogowany: status 401 i fragment „Zaloguj się, aby dodać do ulubionych”.
- Token antiforgery wysyła już `<body hx-headers:inherited='{"RequestVerificationToken":"..."}'>` w `_Layout.cshtml`. Nie wyłączaj walidacji antiforgery.
- Strona `src/GzmApp/Pages/Ulubione.cshtml` z `[Authorize]` według reguły 8 (strona „Moje ulubione”): `<ul id="ulubione">`, jeden `<li>` na rekord, tytuł w `<span class="tytul">`. Data dodania to `CreatedAt` (w bazie UTC), pokazana jako czas polski `dd.MM.yyyy HH:mm`. Przycisk „Usuń”: `hx-delete="?handler=Usun&id=..."` z `hx-target="closest li"` i `hx-swap="outerHTML"`, handler `OnDeleteUsun(int id)` w `Ulubione.cshtml.cs` zwraca pustą odpowiedź 200.
- Link „Moje ulubione” w menu w `_Layout.cshtml`, widoczny zawsze (niezalogowanego strona przekieruje do logowania).
- Rejestracja repozytorium w `AddAppServices` w `src/GzmApp/AppServices.cs`. Krótkie metody, bez zagnieżdżonych if-ów.
- Na koniec uruchom `dotnet test tests/GzmApp.Specs --filter Capability=s08-005-zapisz-ulubione`. Jeśli test nie przechodzi, przeczytaj jego komunikat i popraw kod (nie test) – najwyżej raz. Napisz, ile testów przeszło.
- Na koniec napisz w 3 punktach, co zrobiłeś.

Nie zmieniaj plików poza: `db/900_favorites.sql`, `src/GzmApp/Data/FavoritesRepository.cs`, `src/GzmApp/Pages/Index.cshtml.cs`, `src/GzmApp/Pages/Shared/_Lista.cshtml`, `src/GzmApp/Pages/Shared/_Gwiazdka.cshtml`, `src/GzmApp/Pages/Ulubione.cshtml`, `src/GzmApp/Pages/Ulubione.cshtml.cs`, `src/GzmApp/Pages/Shared/_Layout.cshtml`, `src/GzmApp/AppServices.cs`. Nie dodawaj pakietów.
~~~~
