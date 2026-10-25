# ArcheoDąbrowa – prompty krok po kroku (s09)

Pilot kopiuje cały blok promptu do Kilo Code i akceptuje zmiany. Nawigator prowadzi pauzy. Po każdym kroku zamieniacie się rolami.
Aplikację uruchamiacie klawiszem F5 w VS Code. Adres: http://localhost:5080.

Pracujecie jak zawodowcy: najpierw **specyfikacja**, potem **testy**, na końcu **kod**. Specyfikacja to 4 pliki w `scenarios/s09/spec/` – po jednym na krok. Każde ponumerowane zdanie w sekcji „Reguły” to jeden test. Kilo pisze kod, a testy mówią, czy kod robi to, co obiecuje zdanie.

## Krok 0 – specyfikacja i czerwone testy

### ⏸ Pauza – zanim zaczniecie

1. Otwórzcie `src/GzmApp/appsettings.json` i wpiszcie swój scenariusz: `"Scenario": "s09"`. Zapiszcie (Ctrl+S).
2. Otwórzcie folder `scenarios/s09/spec/` i przeczytajcie plik kroku 1. Sekcja „Co się dzieje” opisuje krok słowami, a „Reguły” – co dokładnie ma działać.
3. Ikona kolby po lewej (Testing) → `GzmApp.Specs` → ▶ „Run Tests”. Każdy test nazywa się tak jak zdanie ze specyfikacji, np. „Reguła 1: …”. Wszystkie są czerwone – aplikacja jeszcze nic nie umie.
4. Wasz cel: zamienić czerwone testy w zielone, krok po kroku.

## Krok 1 – dane z pliku do bazy

~~~~text
CEL
Wczytaj plik CSV „Dąbrowa Górnicza: zabytki archeologiczne” do nowej tabeli `ArchaeologicalSites` w SQL Server przez Dapper. Import uruchamia się przy starcie aplikacji, gdy tabela jest pusta.

SPECYFIKACJA
Reguły tego kroku są w pliku `scenarios/s09/spec/s09-001-importuj-dane.md` (sekcja „Reguły”). Przeczytaj go najpierw. Każda reguła to jeden test w `tests/GzmApp.Specs`. Nie zmieniaj plików specyfikacji ani testów.

DANE
- Plik: `data/raw/s09_dabrowa-zabytki-archeologiczne.csv` w katalogu głównym repozytorium. Ścieżka z aplikacji: `Path.Combine(environment.ContentRootPath, "..", "..", "data/raw/s09_dabrowa-zabytki-archeologiczne.csv")` (`environment` to `IWebHostEnvironment`).
- Kodowanie: UTF-8. Separator: przecinek `,`. Nagłówek w pierwszej linii.
- Kolumny do importu (pozostałych kolumn pliku nie importuj):

| Kolumna w pliku | Pole w tabeli | Typ SQL | Kultura | Format | Może być puste |
|---|---|---|---|---|---|
| `archaeologicalSiteNumberInArea` | `AzpArea` | `nvarchar(10)` | pl-PL | – | nie |
| `archaeologicalSiteNumberInCity` | `SiteNumberInCity` | `int` | pl-PL | – | nie |
| `azpnumber` | `AzpNumber` | `int` | pl-PL | – | nie |
| `chronology` | `Chronology` | `nvarchar(200)` | pl-PL | – | tak |
| `comment` | `Comment` | `nvarchar(400)` | pl-PL | – | tak |
| `id` | `Id` | `int` | pl-PL | – | nie |
| `localName` | `LocalName` | `nvarchar(200)` | pl-PL | – | nie |

- Klucz naturalny (unikalny): `Id`. Wiersze zapisuj przez `MERGE` po kluczu: powtórzony klucz nadpisuje wcześniejszy wiersz, bez wyjątku.
- Utwórz plik `db/s09_create.sql` z tym skryptem i uruchamiaj go przy starcie (w `RunAtStartupAsync`) przed importem. W działającej aplikacji czytaj go z `Path.Combine(AppContext.BaseDirectory, "db", "s09_create.sql")` (tak jak `DatabaseBootstrapper`):

```sql
IF OBJECT_ID(N'dbo.ArchaeologicalSites') IS NULL
CREATE TABLE dbo.[ArchaeologicalSites] (
    [RecordId] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_ArchaeologicalSites] PRIMARY KEY,
    [SourceRow] int NOT NULL,
    [AzpArea] nvarchar(10) NOT NULL,
    [SiteNumberInCity] int NOT NULL,
    [AzpNumber] int NOT NULL,
    [Chronology] nvarchar(200) NULL,
    [Comment] nvarchar(400) NULL,
    [Id] int NOT NULL,
    [LocalName] nvarchar(200) NOT NULL,
    CONSTRAINT [UQ_ArchaeologicalSites_Key] UNIQUE ([Id])
);
```

ZASADY
- W `src/GzmApp/appsettings.json` ustaw `"Scenario": "s09"`.
- Plik czytaj pakietem CsvHelper (jest już w projekcie), `CultureInfo("pl-PL")`, separator jak wyżej. Nie dziel linii przez `Split`.
- Liczby i daty odczytuj z kulturą i formatem z tabeli kolumn. Puste pole albo wartość, której nie da się odczytać, w kolumnie z „Może być puste: tak” zapisz jako NULL. W kolumnie z „nie” taki wiersz pomiń i zapisz jego numer w logu. Bez wyjątku.
- `SourceRow` = numer wiersza danych w pliku, od 1, bez nagłówka. Tekst przytnij (`Trim`).
- Zapytania wysyłaj przez istniejącą klasę `SqlRunner`, parametry przez Dappera.
- Po imporcie zapisz w logu liczbę wierszy w tabeli i czas importu. Gdy tabela nie jest pusta, zapisz w logu „Import pominięty – w tabeli jest N wierszy”.
- Klasa `CsvImporter` z interfejsem `ICsvImporter`. Rejestracja w metodzie `AddAppServices`, a skrypt tabeli i import w `RunAtStartupAsync` – obie w `src/GzmApp/AppServices.cs`. `Program.cs` już je wywołuje, nie zmieniaj go.
- Krótkie metody, bez zagnieżdżonych if-ów.
- Na koniec uruchom `dotnet test tests/GzmApp.Specs --filter Capability=s09-001-importuj-dane`. Jeśli test nie przechodzi, przeczytaj jego komunikat i popraw kod (nie test) – najwyżej raz. Napisz, ile testów przeszło.
- Na koniec napisz w 3 punktach, co zrobiłeś.

Nie zmieniaj plików poza: `db/s09_create.sql`, `src/GzmApp/Data/CsvImporter.cs`, `src/GzmApp/AppServices.cs`, `src/GzmApp/appsettings.json`. Nie dodawaj pakietów.
~~~~

### ⏸ Pauza – zobaczcie, co powstało

1. Uruchomcie aplikację (F5). W terminalu pojawia się wpis z liczbą zaimportowanych wierszy.
2. Rozszerzenie mssql (ikona bazy po lewej): serwer `(localdb)\MSSQLLocalDB`, baza `GzmWorkshop`, tabela `ArchaeologicalSites` → prawy klik → „Select Top 1000”. To Wasze dane z pliku – już w bazie.
3. Zatrzymajcie aplikację (Shift+F5).
4. Testing → ▶ „Run Tests”. Testy kroku 1 (`s09-001-importuj-dane`) są zielone. Czerwony test? Kliknijcie go: komunikat mówi, czego brakuje. Wklejcie ten komunikat do Kilo.

### ⏸ Pauza – decyzja 1: po czym użytkownik będzie szukał?

**A – nazwa stanowiska**

~~~text
Wyszukiwarka szuka frazy w dowolnym miejscu pola `LocalName` (kolumna w pliku: `localName`).
~~~

**B – epoka**

~~~text
Wyszukiwarka szuka frazy w dowolnym miejscu pola `Chronology` (kolumna w pliku: `chronology`).
~~~

**C – opis znalezisk**

~~~text
Wyszukiwarka szuka frazy w dowolnym miejscu pola `Comment` (kolumna w pliku: `comment`).
~~~

Wariant A jest już w specyfikacji. Wybraliście inny? Skopiujcie jego zdanie i wklejcie w pliku `scenarios/s09/spec/s09-002-szukaj-rekordow.md` w miejsce zdania reguły 1 (zostawcie „1. ” na początku). Zapiszcie plik.

## Krok 2 – wyszukiwarka (endpoint)

~~~~text
CEL
Adres, który zwraca rekordy z tabeli `ArchaeologicalSites` pasujące do wpisanej frazy. Odpowiedź to fragment HTML (sama lista), a nie cała strona.

SPECYFIKACJA
Reguły tego kroku są w pliku `scenarios/s09/spec/s09-002-szukaj-rekordow.md` (sekcja „Reguły”). Przeczytaj go najpierw. Każda reguła to jeden test w `tests/GzmApp.Specs`. Nie zmieniaj plików specyfikacji ani testów.

DANE
- Adres: `/?handler=Szukaj&q=<fraza>`.
- Pole, w którym szukasz, podaje reguła 1. „W dowolnym miejscu pola” to `LIKE '%' + @q + '%'`, a „zaczyna się od frazy” to `LIKE @q + '%'`.
- Każdy wynik pokazuje „Nr `SourceRow`: `LocalName`”.

ZASADY
- `IRecordRepository` i `RecordRepository` w `src/GzmApp/Data/RecordRepository.cs`, metoda `Search(string? q, int take)`. Zapytanie przez `SqlRunner`, fraza jako parametr `@q` – bez sklejania tekstu SQL.
- Porównanie bez rozróżniania wielkości liter i ogonków: `COLLATE Latin1_General_CI_AI`.
- Wyniki według `SourceRow`, najwyżej 20 (`take` = 20), także dla pustej frazy. `Wyników: N` to liczba wszystkich pasujących rekordów (osobne `COUNT(*)`), lista pokazuje najwyżej 20.
- Fraza dłuższa niż 100 znaków: odpowiedź 400 z krótkim fragmentem `<p class="alert alert-warning">…</p>`.
- Handler `OnGetSzukaj(string? q)` w `src/GzmApp/Pages/Index.cshtml.cs` zwraca `Partial("_Lista", wyniki)`.
- Widok częściowy `src/GzmApp/Pages/Shared/_Lista.cshtml`: `<p id="licznik">Wyników: N</p>` i `<ul id="lista">` z jednym `<li data-record-id="@r.RecordId">Nr @r.SourceRow: <span class="tytul">…</span></li>` na rekord. Bez `<html>` i bez layoutu.
- Rejestracja repozytorium w `AddAppServices` w `src/GzmApp/AppServices.cs`. Krótkie metody, bez zagnieżdżonych if-ów.
- Na koniec uruchom `dotnet test tests/GzmApp.Specs --filter Capability=s09-002-szukaj-rekordow`. Jeśli test nie przechodzi, przeczytaj jego komunikat i popraw kod (nie test) – najwyżej raz. Napisz, ile testów przeszło.
- Na koniec napisz w 3 punktach, co zrobiłeś.

Nie zmieniaj plików poza: `src/GzmApp/Data/RecordRepository.cs`, `src/GzmApp/Pages/Index.cshtml.cs`, `src/GzmApp/Pages/Shared/_Lista.cshtml`, `src/GzmApp/AppServices.cs`. Nie dodawaj pakietów.
~~~~

### ⏸ Pauza – zobaczcie, co powstało

1. Uruchomcie aplikację i otwórzcie adres `http://localhost:5080/?handler=Szukaj&q=` z dopisanym słowem z wybranej kolumny (dla wariantu A np. `gródki`). Widzicie samą listę – bez menu i stopki. To „fragment HTML”.
2. F12 → zakładka Network → odświeżcie stronę. Kliknijcie żądanie `?handler=Szukaj`: status 200, a w „Response” jest tylko lista. Polskie litery mogą tam wyglądać np. jak `&#x142;` (to „ł”) – to ten sam znak, zapisany kodem HTML.
3. W terminalu (log SQL) widać zapytanie z parametrem `q` – to Wasza fraza.
4. Testing → ▶ „Run Tests”. Zielone są testy kroków 1 i 2. Reguła 5 sprawdza, że fraza `' OR 1=1 --` nic nie znajduje – to ochrona przed atakiem SQL injection.

### ⏸ Pauza – decyzja 2: co pokazać na liście?

**A – epoka**

~~~text
Przy każdym rekordzie lista pokazuje tytuł `LocalName` i pola: `Chronology`.
~~~

**B – obszar i numer AZP**

~~~text
Przy każdym rekordzie lista pokazuje tytuł `LocalName` i pola: `AzpArea`, `AzpNumber`.
~~~

**C – opis znalezisk**

~~~text
Przy każdym rekordzie lista pokazuje tytuł `LocalName` i pola: `Comment`.
~~~

### ⏸ Pauza – decyzja 3: jaki kafelek ze statystyką?

**A – liczba stanowisk w każdym obszarze AZP**

~~~text
Kafelek ze statystyką pokazuje liczbę rekordów dla każdej wartości pola `AzpArea`.
~~~

**B – 5 najczęstszych opisów epoki**

~~~text
Kafelek ze statystyką pokazuje 5 najczęstszych wartości pola `Chronology` z liczbą rekordów.
~~~

**C – 5 najczęstszych nazw stanowisk**

~~~text
Kafelek ze statystyką pokazuje 5 najczęstszych wartości pola `LocalName` z liczbą rekordów.
~~~

Warianty A są już w specyfikacji. Wybraliście inne? Wklejcie ich zdania w pliku `scenarios/s09/spec/s09-003-pokaz-strone.md` w miejsce reguły 4 (lista) i reguły 5 (kafelek). Zapiszcie plik.

## Krok 3 – strona z wyszukiwarką i kafelkiem (HTMX)

~~~~text
CEL
Strona główna aplikacji ArcheoDąbrowa: wyszukiwarka działająca podczas pisania (HTMX 4), lista wyników i kafelek ze statystyką.

SPECYFIKACJA
Reguły tego kroku są w pliku `scenarios/s09/spec/s09-003-pokaz-strone.md` (sekcja „Reguły”). Przeczytaj go najpierw. Każda reguła to jeden test w `tests/GzmApp.Specs`. Nie zmieniaj plików specyfikacji ani testów.

ZASADY
- W `src/GzmApp/Pages/Index.cshtml`: `<input type="search" name="q" hx-get="?handler=Szukaj" hx-trigger="keyup changed delay:300ms" hx-target="#wyniki">` i `<div id="wyniki">` z listą na start (pusta fraza).
- HTMX 4 jest już w `_Layout.cshtml` (plik lokalny). Nie dodawaj CDN ani innych bibliotek.
- Metoda `IsHtmx()` sprawdza nagłówek `HX-Request`. W `OnGetSzukaj`: żądanie HTMX dostaje fragment `_Lista`, zwykłe wejście na ten sam adres – całą stronę z wynikami dla frazy `q`.
- Listę w `_Lista.cshtml` zmień według reguły 4: pola pokaż w tym samym `<li>`, za tytułem. Liczby w formacie pl-PL, daty `yyyy-MM-dd`, pola tak/nie jako „tak” albo „nie”.
- Statystykę z reguły 5 licz w SQL przez `SqlRunner` (nowa metoda w `RecordRepository`), zapytaniem z listy poniżej, z polem z reguły 5 w miejsce `[Pole]`:
  - suma: `SELECT SUM([Pole]) FROM dbo.[ArchaeologicalSites]`
  - średnia: `SELECT ROUND(AVG(CAST([Pole] AS decimal(18,2))), 2) FROM dbo.[ArchaeologicalSites]`
  - 5 najczęstszych wartości: `SELECT TOP 5 [Pole] AS Value, COUNT(*) AS Count FROM dbo.[ArchaeologicalSites] WHERE [Pole] IS NOT NULL GROUP BY [Pole] ORDER BY COUNT(*) DESC, [Pole]`
  - liczba rekordów dla każdej wartości: `SELECT [Pole] AS Value, COUNT(*) AS Count FROM dbo.[ArchaeologicalSites] WHERE [Pole] IS NOT NULL GROUP BY [Pole] ORDER BY COUNT(*) DESC, [Pole]`
- Pokaż ją w `<div id="statystyka">` nad listą: jedną liczbę w `<p data-stat-value>`, a listę wartości jako `<li data-stat>wartość: <strong>liczba</strong></li>`. Liczby w formacie pl-PL (przecinek dziesiętny).
- W `src/GzmApp/appsettings.json` ustaw `"DataAttribution": "Źródło: Dąbrowa Górnicza, otwartedane.metropoliagzm.pl, CC BY 4.0"` (stopka już go pokazuje).
- Krótkie metody, bez zagnieżdżonych if-ów.
- Na koniec uruchom `dotnet test tests/GzmApp.Specs --filter Capability=s09-003-pokaz-strone`. Jeśli test nie przechodzi, przeczytaj jego komunikat i popraw kod (nie test) – najwyżej raz. Napisz, ile testów przeszło.
- Na koniec napisz w 3 punktach, co zrobiłeś.

Nie zmieniaj plików poza: `src/GzmApp/Pages/Index.cshtml`, `src/GzmApp/Pages/Index.cshtml.cs`, `src/GzmApp/Pages/Shared/_Lista.cshtml`, `src/GzmApp/Data/RecordRepository.cs`, `src/GzmApp/appsettings.json`. Nie dodawaj pakietów.
~~~~

### ⏸ Pauza – zobaczcie, co powstało

1. Otwórzcie `http://localhost:5080` i zacznijcie pisać w wyszukiwarce. Lista zmienia się bez przeładowania strony.
2. F12 → Network: przy pisaniu pojawiają się żądania `?handler=Szukaj` z nagłówkiem `HX-Request: true`. Tylko lista wraca z serwera, reszta strony zostaje.
3. Nad listą jest kafelek ze statystyką, a w stopce – źródło danych.
4. Adres `?handler=Szukaj&q=…` otwarty wprost w przeglądarce pokazuje teraz całą stronę – sam fragment dostaje tylko HTMX.
5. Testing → ▶ „Run Tests”. Zielone są testy kroków 1–3.

### ⏸ Pauza – decyzja 4: co pokazać w „Moich ulubionych”?

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

Wariant A jest już w specyfikacji. Wybraliście inny? Wklejcie jego zdanie w pliku `scenarios/s09/spec/s09-004-zapisz-ulubione.md` w miejsce reguły 8. Zapiszcie plik.

## Krok 4 – logowanie i ulubione

~~~~text
CEL
Zalogowany użytkownik oznacza rekordy gwiazdką ★ i widzi je na stronie „Moje ulubione”. Ulubione w ArcheoDąbrowa to stanowiska, które para chce odwiedzić.

SPECYFIKACJA
Reguły tego kroku są w pliku `scenarios/s09/spec/s09-004-zapisz-ulubione.md` (sekcja „Reguły”). Przeczytaj go najpierw. Każda reguła to jeden test w `tests/GzmApp.Specs`. Nie zmieniaj plików specyfikacji ani testów.

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

- `RecordId` to kolumna `RecordId` z tabeli `ArchaeologicalSites`. Tytuł rekordu: pole `LocalName`.
- `Scenario` = `s09` (klucz `Scenario` w `appsettings.json`). `UserId` = identyfikator zalogowanego użytkownika.

ZASADY
- `IFavoritesRepository` i `FavoritesRepository` w `src/GzmApp/Data/FavoritesRepository.cs`: `Add`, `Remove`, `ListFor(userId)` przez `SqlRunner`. `Add` i `Remove` wykonane dwa razy dają ten sam stan (`IF NOT EXISTS` albo `MERGE` w jednym zapytaniu).
- Przycisk ★ to widok częściowy `src/GzmApp/Pages/Shared/_Gwiazdka.cshtml` (model: `RecordId` i czy rekord jest w ulubionych): w ulubionych – „★” z `data-favorite="true"` i `hx-delete="?handler=Ulubione&id=..."`, poza nimi – „☆” z `data-favorite="false"` i `hx-post="?handler=Ulubione&id=..."`, oba z `hx-swap="outerHTML"`. Używa go `_Lista.cshtml` przy każdym rekordzie.
- Stan ★ `_Lista.cshtml` sprawdza sama: `@inject IFavoritesRepository` i ulubione zalogowanego użytkownika (niezalogowany: wszystkie „☆”). Nie przekazuj tego przez `ViewData` – `Partial(...)` w PageModel go nie kopiuje.
- Handlery `OnPostUlubione(int id)` i `OnDeleteUlubione(int id)` w `Index.cshtml.cs` zwracają `Partial("_Gwiazdka", …)` z nowym stanem. Niezalogowany: status 401 i fragment „Zaloguj się, aby dodać do ulubionych”.
- Token antiforgery wysyła już `<body hx-headers:inherited='{"RequestVerificationToken":"..."}'>` w `_Layout.cshtml`. Nie wyłączaj walidacji antiforgery.
- Strona `src/GzmApp/Pages/Ulubione.cshtml` z `[Authorize]` według reguły 8: `<ul id="ulubione">`, jeden `<li>` na rekord, tytuł w `<span class="tytul">`. Data dodania to `CreatedAt` (w bazie UTC), pokazana jako czas polski `dd.MM.yyyy HH:mm`. Przycisk „Usuń”: `hx-delete="?handler=Usun&id=..."` z `hx-target="closest li"` i `hx-swap="outerHTML"`, handler `OnDeleteUsun(int id)` w `Ulubione.cshtml.cs` zwraca pustą odpowiedź 200.
- Link „Moje ulubione” w menu w `_Layout.cshtml`, widoczny zawsze (niezalogowanego strona przekieruje do logowania).
- Rejestracja repozytorium w `AddAppServices` w `src/GzmApp/AppServices.cs`. Krótkie metody, bez zagnieżdżonych if-ów.
- Na koniec uruchom `dotnet test tests/GzmApp.Specs --filter Capability=s09-004-zapisz-ulubione`. Jeśli test nie przechodzi, przeczytaj jego komunikat i popraw kod (nie test) – najwyżej raz. Napisz, ile testów przeszło.
- Na koniec napisz w 3 punktach, co zrobiłeś.

Nie zmieniaj plików poza: `db/900_favorites.sql`, `src/GzmApp/Data/FavoritesRepository.cs`, `src/GzmApp/Pages/Index.cshtml.cs`, `src/GzmApp/Pages/Shared/_Lista.cshtml`, `src/GzmApp/Pages/Shared/_Gwiazdka.cshtml`, `src/GzmApp/Pages/Ulubione.cshtml`, `src/GzmApp/Pages/Ulubione.cshtml.cs`, `src/GzmApp/Pages/Shared/_Layout.cshtml`, `src/GzmApp/AppServices.cs`. Nie dodawaj pakietów.
~~~~

### ⏸ Pauza – zobaczcie, co powstało

1. Kliknijcie ☆ bez logowania: pojawia się „Zaloguj się, aby dodać do ulubionych”.
2. Zarejestrujcie się (Register) i zalogujcie. Kliknijcie ☆ przy kilku rekordach – zmienią się w ★.
3. F12 → Network: żądanie POST `?handler=Ulubione` ma nagłówek `RequestVerificationToken` – to ochrona przed podszywaniem się pod formularz.
4. Otwórzcie „Moje ulubione”. W mssql zajrzyjcie do tabeli `Favorites` – tam są Wasze gwiazdki.
5. Testing → ▶ „Run Tests”. Wszystkie testy są zielone – aplikacja robi wszystko, co obiecuje specyfikacja.

## Gotowe! Co dalej?

Macie całą aplikację: dane w bazie, wyszukiwarkę, stronę z HTMX, logowanie i ulubione. Jeśli zostało Wam trochę czasu, dodajcie coś od siebie – własnym promptem albo jednym z pomysłów:

1. Drugi kafelek ze statystyką – weźcie inny wariant z decyzji 3. Najpierw dopiszcie go do specyfikacji jako regułę 7 kroku 3 (zdanie z decyzji 3): test będzie czerwony, dopóki Kilo go nie zrobi.
2. Sortowanie listy po wybranym polu (przycisk przy nagłówku listy, `hx-get` z parametrem `sort`).
3. Przycisk „Pokaż więcej” pod listą: kolejne 20 rekordów doklejane na końcu (`hx-swap="beforeend"`).
4. Opis rekordu przez model SOL-6. Zadanie dla modelu: Opisz w jednym zdaniu to stanowisko i jego chronologię. Korzystaj tylko z rekordu. Prompt na start:

~~~~text
CEL
Przycisk „✨ Opisz” przy rekordzie na liście. Po kliknięciu pod rekordem pojawia się opis od modelu SOL-6: Opisz w jednym zdaniu to stanowisko i jego chronologię. Korzystaj tylko z rekordu.

DANE
- Rekord z tabeli `ArchaeologicalSites` wybrany po `RecordId`. Do modelu wyślij pola: `AzpArea`, `SiteNumberInCity`, `AzpNumber`, `Chronology`, `Comment`, `Id`, `LocalName`.
- Konfiguracja w `src/GzmApp/appsettings.json`: `Sol6:Endpoint`, `Sol6:Model`, `Sol6:Kind` (`openai` albo `azure`) – wartości poda prowadzący. Klucz `Sol6:ApiKey` tylko w `dotnet user-secrets`, nigdy w pliku w repozytorium.

ZASADY
- `IAiSummarizer` i `AiSummarizer` w `src/GzmApp/Ai/AiSummarizer.cs` na `IChatClient`: pakiety `OpenAI` i `Microsoft.Extensions.AI.OpenAI` (`AsIChatClient()`), dla `azure` – `AzureOpenAIClient` z `AzureKeyCredential`. Pakiety są już w projekcie.
- Jedno wywołanie modelu na kliknięcie. Prompt systemowy: „Odpowiadaj tylko na podstawie przekazanych rekordów, nie wymyślaj faktów.”
- Handler `OnGetPodsumuj(int id)` w `Index.cshtml.cs` zwraca fragment `<p>` z opisem. Przycisk w `_Lista.cshtml`: `hx-get="?handler=Podsumuj&id=@r.RecordId" hx-target="#opis-@r.RecordId"`.
- Brak klucza: fragment „Brak klucza SOL-6 – poproś prowadzącego”, bez wyjątku.
- Rejestracja i konfiguracja `Sol6` w `AddAppServices` w `src/GzmApp/AppServices.cs`.

Nie zmieniaj plików poza: `src/GzmApp/Ai/AiSummarizer.cs`, `src/GzmApp/Pages/Index.cshtml.cs`, `src/GzmApp/Pages/Shared/_Lista.cshtml`, `src/GzmApp/AppServices.cs`, `src/GzmApp/appsettings.json`. Nie dodawaj pakietów.
~~~~
