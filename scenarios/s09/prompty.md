# ArcheoDąbrowa – prompty krok po kroku (s09)

Pilot kopiuje cały blok promptu do Kilo Code i akceptuje zmiany. Nawigator prowadzi pauzy. Po każdym kroku zamieniacie się rolami.
Aplikację uruchamiacie klawiszem F5 w VS Code. Adres: http://localhost:5080.

## Krok 1 – dane z pliku do bazy

~~~~text
CEL
Wczytaj plik CSV „Dąbrowa Górnicza: zabytki archeologiczne” do nowej tabeli `ArchaeologicalSites` w SQL Server przez Dapper. Import uruchamia się przy starcie aplikacji, gdy tabela jest pusta.

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
- Na koniec napisz w 3 punktach, co zrobiłeś.

Nie zmieniaj plików poza: `db/s09_create.sql`, `src/GzmApp/Data/CsvImporter.cs`, `src/GzmApp/AppServices.cs`, `src/GzmApp/appsettings.json`. Nie dodawaj pakietów.
~~~~

### ⏸ Pauza – zobaczcie, co powstało

1. Uruchomcie aplikację (F5). W terminalu pojawia się wpis z liczbą zaimportowanych wierszy.
2. Rozszerzenie mssql (ikona bazy po lewej): serwer `(localdb)\MSSQLLocalDB`, baza `GzmWorkshop`, tabela `ArchaeologicalSites` → prawy klik → „Select Top 1000”. To Wasze dane z pliku – już w bazie.
3. Zatrzymajcie aplikację (Shift+F5).

### ⏸ Pauza – decyzja 1: po czym użytkownik będzie szukał?

**A – nazwa stanowiska**

~~~text
Szukaj w polu `LocalName` (kolumna w pliku: `localName`), fraza w dowolnym miejscu: `LIKE '%' + @q + '%'`.
~~~

**B – epoka**

~~~text
Szukaj w polu `Chronology` (kolumna w pliku: `chronology`), fraza w dowolnym miejscu: `LIKE '%' + @q + '%'`.
~~~

**C – opis znalezisk**

~~~text
Szukaj w polu `Comment` (kolumna w pliku: `comment`), fraza w dowolnym miejscu: `LIKE '%' + @q + '%'`.
~~~

Skopiujcie linię wybranego wariantu i wklejcie ją w prompcie kroku 2 zamiast `[DECYZJA 1]`.

## Krok 2 – wyszukiwarka (endpoint)

~~~~text
CEL
Adres, który zwraca rekordy z tabeli `ArchaeologicalSites` pasujące do wpisanej frazy. Odpowiedź to fragment HTML (sama lista), a nie cała strona.

DECYZJA 1: [DECYZJA 1]
(Jeśli wyżej nadal jest napis w nawiasie kwadratowym, użyj: Szukaj w polu `LocalName` (kolumna w pliku: `localName`), fraza w dowolnym miejscu: `LIKE '%' + @q + '%'`.)

DANE
- Adres: `/?handler=Szukaj&q=<fraza>`.
- Każdy wynik pokazuje „Nr `SourceRow`: `LocalName`”.

ZASADY
- `IRecordRepository` i `RecordRepository` w `src/GzmApp/Data/RecordRepository.cs`, metoda `Search(string? q, int take)`. Zapytanie przez `SqlRunner`, fraza jako parametr `@q` – bez sklejania tekstu SQL.
- Porównanie bez rozróżniania wielkości liter i ogonków: `COLLATE Latin1_General_CI_AI`.
- Wyniki według `SourceRow`, najwyżej 20 (`take` = 20), także dla pustej frazy. `Wyników: N` to liczba wszystkich pasujących rekordów (osobne `COUNT(*)`), lista pokazuje najwyżej 20.
- Fraza dłuższa niż 100 znaków: odpowiedź 400 z krótkim fragmentem `<p class="alert alert-warning">…</p>`.
- Handler `OnGetSzukaj(string? q)` w `src/GzmApp/Pages/Index.cshtml.cs` zwraca `Partial("_Lista", wyniki)`.
- Widok częściowy `src/GzmApp/Pages/Shared/_Lista.cshtml`: `<p id="licznik">Wyników: N</p>` i `<ul>` z jednym `<li>` na rekord. Bez `<html>` i bez layoutu.
- Rejestracja repozytorium w `AddAppServices` w `src/GzmApp/AppServices.cs`. Krótkie metody, bez zagnieżdżonych if-ów.
- Na koniec napisz w 3 punktach, co zrobiłeś.

Nie zmieniaj plików poza: `src/GzmApp/Data/RecordRepository.cs`, `src/GzmApp/Pages/Index.cshtml.cs`, `src/GzmApp/Pages/Shared/_Lista.cshtml`, `src/GzmApp/AppServices.cs`. Nie dodawaj pakietów.
~~~~

### ⏸ Pauza – zobaczcie, co powstało

1. Uruchomcie aplikację i otwórzcie adres `http://localhost:5080/?handler=Szukaj&q=` z dopisanym słowem z wybranej kolumny (dla wariantu A np. `gródki`). Widzicie samą listę – bez menu i stopki. To „fragment HTML”.
2. F12 → zakładka Network → odświeżcie stronę. Kliknijcie żądanie `?handler=Szukaj`: status 200, a w „Response” jest tylko lista. Polskie litery mogą tam wyglądać jak `&#x144;` – to ten sam znak, zapisany kodem HTML.
3. W terminalu (log SQL) widać zapytanie z parametrem `q` – to Wasza fraza.

### ⏸ Pauza – decyzja 2: co pokazać na liście?

**A – epoka**

~~~text
Na liście przy każdym rekordzie pokaż tytuł `LocalName` oraz: `Chronology`.
~~~

**B – obszar i numer AZP**

~~~text
Na liście przy każdym rekordzie pokaż tytuł `LocalName` oraz: `AzpArea`, `AzpNumber`.
~~~

**C – opis znalezisk**

~~~text
Na liście przy każdym rekordzie pokaż tytuł `LocalName` oraz: `Comment`.
~~~

### ⏸ Pauza – decyzja 3: jaki kafelek ze statystyką?

**A – liczba stanowisk w każdym obszarze AZP**

~~~text
Kafelek: liczba rekordów dla każdej wartości pola `AzpArea`, od największej (puste pomijamy). Zapytanie: `SELECT [AzpArea] AS Value, COUNT(*) AS Count FROM dbo.[ArchaeologicalSites] WHERE [AzpArea] IS NOT NULL GROUP BY [AzpArea] ORDER BY COUNT(*) DESC, [AzpArea]`
~~~

**B – 5 najczęstszych opisów epoki**

~~~text
Kafelek: 5 najczęstszych wartości pola `Chronology` z liczbą rekordów (puste pomijamy). Zapytanie: `SELECT TOP 5 [Chronology] AS Value, COUNT(*) AS Count FROM dbo.[ArchaeologicalSites] WHERE [Chronology] IS NOT NULL GROUP BY [Chronology] ORDER BY COUNT(*) DESC, [Chronology]`
~~~

**C – 5 najczęstszych nazw stanowisk**

~~~text
Kafelek: 5 najczęstszych wartości pola `LocalName` z liczbą rekordów (puste pomijamy). Zapytanie: `SELECT TOP 5 [LocalName] AS Value, COUNT(*) AS Count FROM dbo.[ArchaeologicalSites] WHERE [LocalName] IS NOT NULL GROUP BY [LocalName] ORDER BY COUNT(*) DESC, [LocalName]`
~~~

Wklejcie wybrane linie w prompcie kroku 3 zamiast `[DECYZJA 2]` i `[DECYZJA 3]`.

## Krok 3 – strona z wyszukiwarką i kafelkiem (HTMX)

~~~~text
CEL
Strona główna aplikacji ArcheoDąbrowa: wyszukiwarka działająca podczas pisania (HTMX 4), lista wyników i kafelek ze statystyką.

DECYZJA 2: [DECYZJA 2]
(Jeśli wyżej nadal jest napis w nawiasie kwadratowym, użyj: Na liście przy każdym rekordzie pokaż tytuł `LocalName` oraz: `Chronology`.)

DECYZJA 3: [DECYZJA 3]
(Jeśli wyżej nadal jest napis w nawiasie kwadratowym, użyj: Kafelek: liczba rekordów dla każdej wartości pola `AzpArea`, od największej (puste pomijamy). Zapytanie: `SELECT [AzpArea] AS Value, COUNT(*) AS Count FROM dbo.[ArchaeologicalSites] WHERE [AzpArea] IS NOT NULL GROUP BY [AzpArea] ORDER BY COUNT(*) DESC, [AzpArea]`)

ZASADY
- W `src/GzmApp/Pages/Index.cshtml`: `<input type="search" name="q" hx-get="?handler=Szukaj" hx-trigger="keyup changed delay:300ms" hx-target="#wyniki">` i `<div id="wyniki">` z listą na start (pusta fraza).
- HTMX 4 jest już w `_Layout.cshtml` (plik lokalny). Nie dodawaj CDN ani innych bibliotek.
- Metoda `IsHtmx()` sprawdza nagłówek `HX-Request`. W `OnGetSzukaj`: żądanie HTMX dostaje fragment `_Lista`, zwykłe wejście na ten sam adres – całą stronę z wynikami dla frazy `q`.
- Listę w `_Lista.cshtml` zmień według decyzji 2.
- Statystykę z decyzji 3 licz w SQL przez `SqlRunner` (nowa metoda w `RecordRepository`). Pokaż ją w `<div id="statystyka">` nad listą; liczby w formacie pl-PL (przecinek dziesiętny).
- W `src/GzmApp/appsettings.json` ustaw `"DataAttribution": "Źródło: Dąbrowa Górnicza, otwartedane.metropoliagzm.pl, CC BY 4.0"` (stopka już go pokazuje).
- Krótkie metody, bez zagnieżdżonych if-ów.
- Na koniec napisz w 3 punktach, co zrobiłeś.

Nie zmieniaj plików poza: `src/GzmApp/Pages/Index.cshtml`, `src/GzmApp/Pages/Index.cshtml.cs`, `src/GzmApp/Pages/Shared/_Lista.cshtml`, `src/GzmApp/Data/RecordRepository.cs`, `src/GzmApp/appsettings.json`. Nie dodawaj pakietów.
~~~~

### ⏸ Pauza – zobaczcie, co powstało

1. Otwórzcie `http://localhost:5080` i zacznijcie pisać w wyszukiwarce. Lista zmienia się bez przeładowania strony.
2. F12 → Network: przy pisaniu pojawiają się żądania `?handler=Szukaj` z nagłówkiem `HX-Request: true`. Tylko lista wraca z serwera, reszta strony zostaje.
3. Nad listą jest kafelek ze statystyką, a w stopce – źródło danych.
4. Adres `?handler=Szukaj&q=…` otwarty wprost w przeglądarce pokazuje teraz całą stronę – sam fragment dostaje tylko HTMX.

### ⏸ Pauza – decyzja 4: co pokazać w „Moich ulubionych”?

**A – lista tytułów**

~~~text
Na stronie „Moje ulubione” pokaż listę tytułów (`LocalName`) ulubionych rekordów.
~~~

**B – tytuły z datą dodania**

~~~text
Na stronie „Moje ulubione” pokaż tytuł (`LocalName`) i datę dodania (`CreatedAt`, w bazie UTC – pokaż czas polski jako `dd.MM.yyyy HH:mm`), od najnowszych.
~~~

**C – tytuły z przyciskiem „Usuń”**

~~~text
Na stronie „Moje ulubione” pokaż tytuł (`LocalName`) i przycisk „Usuń” (`hx-delete`), który usuwa wiersz z listy bez przeładowania strony.
~~~

Wklejcie wybraną linię w prompcie kroku 4 zamiast `[DECYZJA 4]`.

## Krok 4 – logowanie i ulubione

~~~~text
CEL
Zalogowany użytkownik oznacza rekordy gwiazdką ★ i widzi je na stronie „Moje ulubione”. Ulubione w ArcheoDąbrowa to stanowiska, które para chce odwiedzić.

DECYZJA 4: [DECYZJA 4]
(Jeśli wyżej nadal jest napis w nawiasie kwadratowym, użyj: Na stronie „Moje ulubione” pokaż listę tytułów (`LocalName`) ulubionych rekordów.)

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
- Przycisk ★ to widok częściowy `src/GzmApp/Pages/Shared/_Gwiazdka.cshtml` (model: `RecordId` i czy rekord jest w ulubionych): w ulubionych – „★” z `hx-delete="?handler=Ulubione&id=..."`, poza nimi – „☆” z `hx-post="?handler=Ulubione&id=..."`, oba z `hx-swap="outerHTML"`. Używa go `_Lista.cshtml` przy każdym rekordzie.
- Stan ★ `_Lista.cshtml` sprawdza sama: `@inject IFavoritesRepository` i ulubione zalogowanego użytkownika (niezalogowany: wszystkie „☆”). Nie przekazuj tego przez `ViewData` – `Partial(...)` w PageModel go nie kopiuje.
- Handlery `OnPostUlubione(int id)` i `OnDeleteUlubione(int id)` w `Index.cshtml.cs` zwracają `Partial("_Gwiazdka", …)` z nowym stanem. Niezalogowany: status 401 i fragment „Zaloguj się, aby dodać do ulubionych”.
- Token antiforgery wysyła już `<body hx-headers:inherited='{"RequestVerificationToken":"..."}'>` w `_Layout.cshtml`. Nie wyłączaj walidacji antiforgery.
- Strona `src/GzmApp/Pages/Ulubione.cshtml` z `[Authorize]` według decyzji 4. Link „Moje ulubione” w menu w `_Layout.cshtml`, widoczny zawsze (niezalogowanego strona przekieruje do logowania).
- Rejestracja repozytorium w `AddAppServices` w `src/GzmApp/AppServices.cs`. Krótkie metody, bez zagnieżdżonych if-ów.
- Na koniec napisz w 3 punktach, co zrobiłeś.

Nie zmieniaj plików poza: `db/900_favorites.sql`, `src/GzmApp/Data/FavoritesRepository.cs`, `src/GzmApp/Pages/Index.cshtml.cs`, `src/GzmApp/Pages/Shared/_Lista.cshtml`, `src/GzmApp/Pages/Shared/_Gwiazdka.cshtml`, `src/GzmApp/Pages/Ulubione.cshtml`, `src/GzmApp/Pages/Ulubione.cshtml.cs`, `src/GzmApp/Pages/Shared/_Layout.cshtml`, `src/GzmApp/AppServices.cs`. Nie dodawaj pakietów.
~~~~

### ⏸ Pauza – zobaczcie, co powstało

1. Kliknijcie ☆ bez logowania: pojawia się „Zaloguj się, aby dodać do ulubionych”.
2. Zarejestrujcie się (Register) i zalogujcie. Kliknijcie ☆ przy kilku rekordach – zmienią się w ★.
3. F12 → Network: żądanie POST `?handler=Ulubione` ma nagłówek `RequestVerificationToken` – to ochrona przed podszywaniem się pod formularz.
4. Otwórzcie „Moje ulubione”. W mssql zajrzyjcie do tabeli `Favorites` – tam są Wasze gwiazdki.

## Gotowe! Co dalej?

Macie całą aplikację: dane w bazie, wyszukiwarkę, stronę z HTMX, logowanie i ulubione. Jeśli zostało Wam trochę czasu, dodajcie coś od siebie – własnym promptem albo jednym z pomysłów:

1. Drugi kafelek ze statystyką – weźcie inny wariant z decyzji 3.
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
