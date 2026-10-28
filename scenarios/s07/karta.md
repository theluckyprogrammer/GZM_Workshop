# SportGliwice – Wasz portal (scenariusz s07)

**Obiekty sportowe w Gliwicach: hale, boiska, baseny – gdzie są i kto nimi zarządza.** To portal, który zbudujecie w tym warsztacie – na prawdziwych otwartych danych Metropolii GZM.

- **Dane:** „Gliwice: obiekty sportowe” – pobierzecie je w kroku 0 prosto z portalu [otwartedane.metropoliagzm.pl](https://otwartedane.metropoliagzm.pl/dataset/f310c9b971378c90071de753fc47e60d0b7b7fa52a9a0408bc5937c9373f6bd3) (licencja CC BY 4.0, atrybucja: Źródło: Gliwice, otwartedane.metropoliagzm.pl, CC BY 4.0).
- **Co powstanie:** w 5 krokach – szkielet portalu z danymi → dane w bazie SQLite → wyszukiwarka → strona z listą i statystyką → analiza danych przez AI (a na koniec, dla chętnych, ulubione).
- **Kto pisze kod:** Kilo Code (AI) – wy nim sterujecie promptami z tej karty i decydujecie, jak portal ma działać.

## Jak pracujecie

1. **Zrób prompt:** wklej do Kilo Code cały blok tekstu z kroku 0 i zaakceptuj zmiany. Tak samo pracuj w kolejnych krokach.
2. **Zobacz, co powstało:** po każdym prompcie uruchom aplikację klawiszem F5 i sprawdź ją w przeglądarce. Potem przejdź do następnego kroku. W finale poświęć więcej czasu na pokaz gotowego portalu.

Jeśli potrzebujecie pomocy z uruchomieniem, zajrzyjcie do `docs/PORADNIK-UCZNIA.md`.

## Prompty – krok po kroku

W każdym kroku: **wklej prompt → zobacz efekt w aplikacji → przejdź dalej**. Aplikacja otwiera się pod adresem http://localhost:5080.

## Krok 0 – powstaje Wasz portal (szkielet i dane)

**Zrób prompt:** wklej poniższy blok do Kilo Code.

Ten krok tworzy aplikację i plik z danymi „Gliwice: obiekty sportowe” pobrany z portalu GZM.

~~~~text
CEL
Wasz portal SportGliwice zaczyna istnieć. Po tym prompcie na dysku jest aplikacja w src/GzmApp, projekt testów w tests/GzmApp.Specs i plik z danymi data/raw/s07_gliwice-obiekty-sportowe.csv. Scenariusz s07 jest już wpisany w konfigurację – pary nigdy nie wpisują go ręcznie.

WYKONAJ
1. Uruchom w terminalu (VS Code: Ctrl + `) polecenie:
   powershell -ExecutionPolicy Bypass -File tools/nowy-portal.ps1 -Scenariusz s07
   Skrypt tworzy też Wasz własny branch (s07-<losowy numer>, od main) i od razu na niego przechodzi, tworzy src/ i tests/, pobiera dane z portalu GZM i zakłada LOG-DECYZJI.md. Poczekaj na napis „GOTOWE”.
2. Zbuduj aplikację: dotnet build src/GzmApp
3. Sprawdź dane: dotnet test tests/GzmApp.Specs --filter Capability=s07-000-dane
4. Sprawdź całość: dotnet test tests/GzmApp.Specs
   Zielone mają być testy kroku 0 (dane); reguły kroków 1–4 są czerwone – tak ma być, to lista zadań pary.

SPECYFIKACJA
Ten krok nie buduje funkcji, ale ma swoją specyfikację: reguły pliku `scenarios/s07/spec/s07-000-dane.md` sprawdzają, że dane naprawdę są i są kompletne.

ZASADY
- Nie zmieniaj plików – wszystko tworzy skrypt. Gdy polecenie z punktu 1 się nie powiedzie, przeczytaj komunikat skryptu i powiedz parze, co zrobić dalej (np. brak internetu: poproś o plik z danych od prowadzącego i uruchom polecenie ponownie – skrypt pomija pobieranie, gdy plik już jest).
- Po każdym poleceniu krótko napisz, co z niego wynikło.
- Na koniec napisz w 3 punktach, co powstało.

Nie zmieniaj plików poza: niczym – w tym kroku wszystko tworzy skrypt. Nie dodawaj pakietów.
~~~~

### Zobacz, co powstało

Uruchomcie aplikację (F5). Kliknijcie „Rejestracja”, podajcie własny adres e-mail i hasło, a potem sprawdźcie, czy możecie się zalogować i wylogować.

## Krok 1 – dane z pliku do bazy

**Zrób prompt:** wklej poniższy blok do Kilo Code.

~~~~text
CEL
Wczytaj plik CSV „Gliwice: obiekty sportowe” do nowej tabeli `SportsFacilities` w bazie SQLite (plik `src/GzmApp/gzm.db`) przez Dapper. Import uruchamia się przy starcie aplikacji, gdy tabela jest pusta.

SPECYFIKACJA
Reguły tego kroku są w pliku `scenarios/s07/spec/s07-001-importuj-dane.md` (sekcja „Reguły”). Przeczytaj go najpierw. Każda reguła to jeden test w `tests/GzmApp.Specs`. Nie zmieniaj plików specyfikacji ani testów.

DANE
- Plik: `data/raw/s07_gliwice-obiekty-sportowe.csv` w katalogu głównym repozytorium. Ścieżka z aplikacji: `Path.Combine(environment.ContentRootPath, "..", "..", "data/raw/s07_gliwice-obiekty-sportowe.csv")` (`environment` to `IWebHostEnvironment`).
- Kodowanie: UTF-8. Separator: przecinek `,`. Nagłówek w pierwszej linii.
- Kolumny do importu (pozostałych kolumn pliku nie importuj):

| Kolumna w pliku | Pole w tabeli | Typ w SQLite | Kultura | Format | Może być puste |
|---|---|---|---|---|---|
| `OBIEKT` | `Facility` | `TEXT` | pl-PL | – | nie |
| `ADRES___ZARZĄDZAJĄCY` | `AddressManager` | `TEXT` | pl-PL | – | nie |
| `ObjectId` | `ObjectId` | `INTEGER` | pl-PL | – | nie |

- Klucz naturalny (unikalny): `ObjectId`. Wiersze zapisuj przez upsert: `INSERT … ON CONFLICT ([ObjectId]) DO UPDATE SET …` – powtórzony klucz nadpisuje wcześniejszy wiersz, bez wyjątku.
- Utwórz plik `db/s07_create.sql` z tym skryptem i uruchamiaj go przy starcie (w `RunAtStartupAsync`) przed importem. W działającej aplikacji czytaj go z `Path.Combine(AppContext.BaseDirectory, "db", "s07_create.sql")` – pliki z katalogu `db/` projektu kopiują się tam przy każdym budowaniu:

```sql
CREATE TABLE IF NOT EXISTS [SportsFacilities] (
    [RecordId] INTEGER PRIMARY KEY AUTOINCREMENT,
    [SourceRow] INTEGER NOT NULL,
    [Facility] TEXT NOT NULL,
    [AddressManager] TEXT NOT NULL,
    [ObjectId] INTEGER NOT NULL,
    CONSTRAINT [UQ_SportsFacilities_Key] UNIQUE ([ObjectId])
);
```

ZASADY
- Plik czytaj pakietem CsvHelper (jest już w projekcie) z `CultureInfo("pl-PL")`. Uwaga: ta kultura sama ustawia domyślny separator „;” – jeśli DANE mówią o innym, wskaż go jawnie w konfiguracji: `new CsvConfiguration(CultureInfo("pl-PL")) { Delimiter = "," }`. Nie dziel linii przez `Split`.
- Pola odczytuj po nazwie kolumny z pliku: najpierw `csv.Read()` i `csv.ReadHeader()` (w CsvHelper 33 nie ma `ReadHeaderAsync`), potem `while (csv.Read())` i `csv.GetField("Kolumna")`. Nie mapuj wiersza na klasę – nazwy w pliku różnią się od nazw pól w tabeli.
- Liczby i daty odczytuj z kulturą i formatem z tabeli kolumn. Do bazy zapisuj je jako tekst dokładnie w formacie z kolumny „Typ w SQLite” (kropka dziesiętna, kultura niezmienna): liczby z ułamkiem z pełną liczbą miejsc po kropce (np. `50.091530000`), daty `yyyy-MM-dd`, data z czasem `yyyy-MM-dd HH:mm:ss`, godzina `HH:mm`; pola tak/nie jako `1`/`0`, a liczby całkowite jako zwykłe liczby. Puste pole albo wartość, której nie da się odczytać, w kolumnie z „Może być puste: tak” zapisz jako NULL. W kolumnie z „nie” taki wiersz pomiń i zapisz jego numer w logu. Bez wyjątku.
- `SourceRow` = numer wiersza danych w pliku, od 1, bez nagłówka. Licz go sam (licznik zwiększany przy każdym wierszu danych, także pominiętym) – `csv.Parser.Row` liczy razem z nagłówkiem. Tekst przytnij (`Trim`).
- Bez `if` wewnątrz pętli: jeden wiersz odczytuje i sprawdza osobna metoda, która zwraca gotowy wiersz albo `null`; pętla tylko zbiera wyniki, a pominięte wiersze odfiltruj LINQ (`Where`).
- Zapytania wysyłaj przez istniejącą klasę `SqlRunner`, parametry przez Dappera.
- Po imporcie zapisz w logu liczbę wierszy w tabeli i czas importu. Gdy tabela nie jest pusta, zapisz w logu „Import pominięty – w tabeli jest N wierszy”.
- Klasa `CsvImporter` z interfejsem `ICsvImporter`. Rejestracja w metodzie `AddAppServices`, a skrypt tabeli i import w `RunAtStartupAsync` – obie w `src/GzmApp/AppServices.cs`. `Program.cs` już je wywołuje, nie zmieniaj go.
- Krótkie metody, bez zagnieżdżonych if-ów.
- Na koniec uruchom `dotnet test tests/GzmApp.Specs --filter Capability=s07-001-importuj-dane`. Jeśli test nie przechodzi, przeczytaj jego komunikat i popraw kod (nie test) – najwyżej raz. Napisz, ile testów przeszło.
- Na koniec napisz w 3 punktach, co zrobiłeś.

Nie zmieniaj plików poza: `db/s07_create.sql`, `src/GzmApp/Data/CsvImporter.cs`, `src/GzmApp/AppServices.cs`. Nie dodawaj pakietów.
~~~~

### Zobacz, co powstało

Uruchomcie aplikację (F5) i otwórzcie stronę główną. Dane są już w bazie; wyszukiwarka pojawi się w kolejnych krokach.

## Krok 2 – wyszukiwarka (endpoint)

**Zrób prompt:** wklej poniższy blok do Kilo Code.

~~~~text
CEL
Adres, który zwraca rekordy z tabeli `SportsFacilities` pasujące do wpisanej frazy. Odpowiedź to fragment HTML (sama lista), a nie cała strona.

SPECYFIKACJA
Reguły tego kroku są w pliku `scenarios/s07/spec/s07-002-szukaj-rekordow.md` (sekcja „Reguły”). Przeczytaj go najpierw. Każda reguła to jeden test w `tests/GzmApp.Specs`. Nie zmieniaj plików specyfikacji ani testów.

DANE
- Adres: `/?handler=Szukaj&q=<fraza>`.
- Pole, w którym szukasz, podaje reguła 1. „W dowolnym miejscu pola” to `LIKE '%' || @q || '%'`, a „zaczyna się od frazy” to `LIKE @q || '%'` (`||` skleja teksty w SQLite).
- Każdy wynik pokazuje „Nr `SourceRow`: `Facility`”.

ZASADY
- `IRecordRepository` i `RecordRepository` w `src/GzmApp/Data/RecordRepository.cs`, metoda `Search(string? q, int take)` (może być asynchroniczna, jak metody `SqlRunner`). Zapytanie przez `SqlRunner`, fraza jako parametr `@q` – bez sklejania tekstu SQL.
- `Search` zwraca cały wiersz: `RecordId`, `SourceRow` i wszystkie zaimportowane pola (`Facility`, `AddressManager`, `ObjectId`) – przydadzą się w krokach 3 i 4. Wiersz to klasa z właściwościami (`public int SourceRow { get; init; }`): Dapper dopasowuje je po nazwach kolumn. Pozycyjny `record` działa tylko wtedy, gdy kolejność kolumn w `SELECT` jest dokładnie taka jak kolejność jego parametrów.
- Porównanie bez rozróżniania wielkości liter i ogonków: funkcja `plain()` (rejestruje ją `SqlRunner` na każdym połączeniu): `WHERE plain(CAST([Pole] AS TEXT)) LIKE '%' || plain(@q) || '%'`.
- Wyniki według `SourceRow`, najwyżej 20 (`take` = 20), także dla pustej frazy. `Wyników: N` to liczba wszystkich pasujących rekordów (osobne `COUNT(*)`), lista pokazuje najwyżej 20.
- Fraza dłuższa niż 100 znaków: odpowiedź 400 z krótkim fragmentem `<p class="alert alert-warning">…</p>`.
- Handler `OnGetSzukaj(string? q)` w `src/GzmApp/Pages/Index.cshtml.cs` zwraca `Partial("_Lista", wyniki)`. Liczbę „Wyników: N” przekaż razem z listą jako jeden model (np. rekord `RecordList(int Count, …)`), bo `ViewData` nie przechodzi przez `Partial(...)`.
- W `IndexModel` zostaw właściwość `Scenario` i metodę `OnGet()` – używa ich obecny `Index.cshtml`, który zmienicie dopiero w kroku 3.
- Widok częściowy `src/GzmApp/Pages/Shared/_Lista.cshtml` (bez `@page` w pierwszej linii – to nie jest strona; klasy CSS z biblioteki Tabler, która jest już w `_Layout.cshtml`): `<p id="licznik" class="text-secondary px-3 pt-3 mb-2">Wyników: N</p>` i `<ul id="lista" class="list-group list-group-flush">` z jednym `<li class="list-group-item" data-record-id="@r.RecordId"><span class="badge bg-blue-lt me-2">Nr @r.SourceRow:</span> <span class="tytul fw-bold">…</span></li>` na rekord. Bez `<html>` i bez layoutu.
- Rejestracja repozytorium w `AddAppServices` w `src/GzmApp/AppServices.cs`. Krótkie metody, bez zagnieżdżonych if-ów.
- Na koniec uruchom `dotnet test tests/GzmApp.Specs --filter Capability=s07-002-szukaj-rekordow`. Jeśli test nie przechodzi, przeczytaj jego komunikat i popraw kod (nie test) – najwyżej raz. Napisz, ile testów przeszło.
- Na koniec napisz w 3 punktach, co zrobiłeś.

Nie zmieniaj plików poza: `src/GzmApp/Data/RecordRepository.cs`, `src/GzmApp/Pages/Index.cshtml.cs`, `src/GzmApp/Pages/Shared/_Lista.cshtml`, `src/GzmApp/AppServices.cs`. Nie dodawaj pakietów.
~~~~

### Zobacz, co powstało

Uruchomcie aplikację (F5). Otwórzcie w przeglądarce `http://localhost:5080/?handler=Szukaj&q=` z dopisanym słowem z danych i zobaczcie wyniki; pole wyszukiwania pojawi się w następnym kroku.

## Krok 3 – strona z wyszukiwarką i kafelkiem (HTMX)

**Zrób prompt:** wklej poniższy blok do Kilo Code.

~~~~text
CEL
Strona główna aplikacji SportGliwice: wyszukiwarka działająca podczas pisania (HTMX 4), lista wyników i kafelek ze statystyką.

SPECYFIKACJA
Reguły tego kroku są w pliku `scenarios/s07/spec/s07-003-pokaz-strone.md` (sekcja „Reguły”). Przeczytaj go najpierw. Każda reguła to jeden test w `tests/GzmApp.Specs`. Nie zmieniaj plików specyfikacji ani testów.

ZASADY
- `src/GzmApp/Pages/Index.cshtml` – układ z klasami Tablera (biblioteka CSS jest już w `_Layout.cshtml`), który dobrze wygląda też na telefonie:

```html
<div class="page-header mb-4">
  <h1 class="page-title">SportGliwice</h1>
  <div class="text-secondary">Obiekty sportowe w Gliwicach: hale, boiska, baseny – gdzie są i kto nimi zarządza.</div>
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
- Listę w `_Lista.cshtml` zmień według reguły 4: pola pokaż w tym samym `<li>`, za tytułem, w `<div class="text-secondary small">` jako „nazwa: wartość · nazwa: wartość” z polskimi nazwami pól: `Facility` – obiekt, `AddressManager` – adres i zarządca, `ObjectId` – nr. Liczby w formacie pl-PL (rok i numery/identyfikatory bez separatora tysięcy: „2012”, nie „2 012”), daty `yyyy-MM-dd` (gdy w danych jest też godzina: `yyyy-MM-dd HH:mm`), pola tak/nie jako „tak” albo „nie”.
- Statystykę z reguły 5 licz w SQL przez `SqlRunner` (nowa metoda w `RecordRepository`), zapytaniem z listy poniżej, z polem z reguły 5 w miejsce `[Pole]`:
  - suma: `SELECT SUM([Pole]) FROM [SportsFacilities]`
  - średnia: `SELECT ROUND(AVG(CAST([Pole] AS REAL)), 2) FROM [SportsFacilities]`
  - 5 najczęstszych wartości: `SELECT [Pole] AS Value, COUNT(*) AS Count FROM [SportsFacilities] WHERE [Pole] IS NOT NULL GROUP BY [Pole] ORDER BY COUNT(*) DESC, [Pole] LIMIT 5`
  - liczba rekordów dla każdej wartości: `SELECT [Pole] AS Value, COUNT(*) AS Count FROM [SportsFacilities] WHERE [Pole] IS NOT NULL GROUP BY [Pole] ORDER BY COUNT(*) DESC, [Pole]`
- Pokaż ją w karcie `<div id="statystyka">`: opis po polsku (np. „Średnia: obwód (cm)”) w `<div class="subheader">`, jedną liczbę w `<p data-stat-value class="h1 mb-0">`, a listę wartości jako `<ul class="list-unstyled mb-0">` z `<li data-stat class="d-flex justify-content-between">wartość: <strong>liczba</strong></li>`. Liczby w formacie pl-PL (przecinek dziesiętny).
- W `src/GzmApp/appsettings.json` ustaw `"DataAttribution": "Źródło: Gliwice, otwartedane.metropoliagzm.pl, CC BY 4.0"` (stopka już go pokazuje).
- Krótkie metody, bez zagnieżdżonych if-ów.
- Na koniec uruchom `dotnet test tests/GzmApp.Specs --filter Capability=s07-003-pokaz-strone`. Jeśli test nie przechodzi, przeczytaj jego komunikat i popraw kod (nie test) – najwyżej raz. Napisz, ile testów przeszło.
- Na koniec napisz w 3 punktach, co zrobiłeś.

Nie zmieniaj plików poza: `src/GzmApp/Pages/Index.cshtml`, `src/GzmApp/Pages/Index.cshtml.cs`, `src/GzmApp/Pages/Shared/_Lista.cshtml`, `src/GzmApp/Data/RecordRepository.cs`, `src/GzmApp/appsettings.json`. Nie dodawaj pakietów.
~~~~

### Zobacz, co powstało

Uruchomcie aplikację (F5). Wpiszcie frazę w wyszukiwarce, zobaczcie wyniki i kafelek ze statystyką.

## Krok 4 – finał: analiza AI i pokaz portalu

**Zrób prompt:** wklej poniższy blok do Kilo Code.

~~~~text
CEL
Przycisk „✨ Analizuj z AI”: rekordy z listy wyników idą do modelu sztucznej inteligencji GPT-6 Luna, a jego analiza pojawia się na stronie w karcie, która mówi wprost, że ten tekst napisała AI.

SPECYFIKACJA
Reguły tego kroku są w pliku `scenarios/s07/spec/s07-004-analiza-ai.md` (sekcja „Reguły”). Przeczytaj go najpierw. Każda reguła to jeden test w `tests/GzmApp.Specs`. Nie zmieniaj plików specyfikacji ani testów.

DANE
- Model AI (GPT-6 Luna) jest już podłączony w `Program.cs`: z DI bierzesz `IChatClient` (`Microsoft.Extensions.AI`) i `IOptions<Sol6Options>` (`GzmApp.Ai`, nazwa modelu w `Model`). Klucz jest w pliku `src/GzmApp/appsettings.Local.json` od prowadzącego. Nie twórz własnego klienta AI i nie wpisuj klucza w kod.
- Zadanie dla modelu to zdanie z reguły 3 specyfikacji, bez początku „Model dostaje zadanie: ”.
- Rekordy: te same co na liście dla frazy `q` – `Search(q, 20)` z `RecordRepository`. Do modelu wyślij pola: `Facility`, `AddressManager`, `ObjectId`. Jedna linia na rekord: `Nr <SourceRow>: Pole=wartość; Pole=wartość`. Wartości w tych samych formatach co na liście (liczby pl-PL, daty `yyyy-MM-dd`, tak/nie), puste pole jako „brak”. Jeśli `Search` nie zwraca któregoś pola, dopisz je w `RecordRepository.cs`.

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
- Na koniec uruchom `dotnet test tests/GzmApp.Specs --filter Capability=s07-004-analiza-ai`. Reguły 1–8 sprawdza udawany model (bez tokenów), a reguła 9 – jedno zapytanie do prawdziwego modelu. Jeśli test nie przechodzi, przeczytaj jego komunikat i popraw kod (nie test) – najwyżej raz. Napisz, ile testów przeszło.
- Na koniec napisz w 3 punktach, co zrobiłeś.

Nie zmieniaj plików poza: `src/GzmApp/Ai/AiAnalyzer.cs`, `src/GzmApp/Pages/Index.cshtml`, `src/GzmApp/Pages/Index.cshtml.cs`, `src/GzmApp/Pages/Shared/_Analiza.cshtml`, `src/GzmApp/Data/RecordRepository.cs`, `src/GzmApp/AppServices.cs`. Nie dodawaj pakietów.
~~~~

### Finał – pokażcie gotowy portal

Uruchomcie aplikację (F5). Wyszukajcie dwa różne hasła i zobaczcie, jak zmienia się lista; obejrzyjcie kafelek ze statystyką. Kliknijcie „✨ Analizuj z AI”, porównajcie odpowiedź z wynikami i otwórzcie „Co poszło do modelu?”. Na koniec pokażcie gotowy portal drugiej osobie: wyszukiwanie, statystykę i analizę AI.

## Dla chętnych – ulubione

Jeśli chcecie dodać ulubione, **zróbcie prompt:** wklejcie poniższy blok do Kilo Code.

~~~~text
CEL
Zalogowany użytkownik oznacza rekordy gwiazdką ★ i widzi je na stronie „Moje ulubione”. Ulubione w SportGliwice to obiekty, na których para trenuje.

PRZYGOTOWANIE
Przenieś `scenarios/s07/spec/dodatkowe/s07-005-zapisz-ulubione.md` do `scenarios/s07/spec/s07-005-zapisz-ulubione.md` i uruchom `dotnet build --no-incremental tests/GzmApp.Specs`, aby generator zauważył nową specyfikację. Nie zmieniaj treści reguł.

SPECYFIKACJA
Reguły tego kroku są w pliku `scenarios/s07/spec/s07-005-zapisz-ulubione.md` (sekcja „Reguły”). Przeczytaj go najpierw. Każda reguła to jeden test w `tests/GzmApp.Specs`. Nie zmieniaj plików specyfikacji ani testów.

DANE
- Utwórz plik `db/900_favorites.sql` z tym skryptem i uruchamiaj go przy starcie (w `RunAtStartupAsync` w `src/GzmApp/AppServices.cs`), czytając go z `Path.Combine(AppContext.BaseDirectory, "db", "900_favorites.sql")`:

```sql
CREATE TABLE IF NOT EXISTS [Favorites] (
    [UserId] TEXT NOT NULL,
    [Scenario] TEXT NOT NULL,
    [RecordId] INTEGER NOT NULL,
    [CreatedAt] TEXT NOT NULL DEFAULT (datetime('now')),
    CONSTRAINT [PK_Favorites] PRIMARY KEY (UserId, Scenario, RecordId)
);
```

- `RecordId` to kolumna `RecordId` z tabeli `SportsFacilities`. Tytuł rekordu: pole `Facility`.
- `Scenario` = `s07` (klucz `Scenario` w `appsettings.json`). `UserId` = identyfikator zalogowanego użytkownika.

ZASADY
- `IFavoritesRepository` i `FavoritesRepository` w `src/GzmApp/Data/FavoritesRepository.cs`: `Add`, `Remove`, `ListFor(userId)` przez `SqlRunner`. `Add` i `Remove` wykonane dwa razy dają ten sam stan (`INSERT OR IGNORE` w jednym zapytaniu). Każda publiczna metoda z `userId` zaczyna się od `ArgumentException.ThrowIfNullOrWhiteSpace(userId);`. Klasy pomocnicze (np. wiersz ulubionego: `RecordId`, tytuł, `CreatedAt`) trzymaj w tym samym pliku, jako klasy z właściwościami (Dapper dopasowuje po nazwach kolumn).
- `UserId` zalogowanego: `User.FindFirstValue(ClaimTypes.NameIdentifier)` w PageModel (`using System.Security.Claims;`), a w widoku `User.FindFirst(ClaimTypes.NameIdentifier)?.Value`.
- Przycisk ★ to widok częściowy `src/GzmApp/Pages/Shared/_Gwiazdka.cshtml` (model: `public record StarModel(int RecordId, bool IsFavorite);` w `FavoritesRepository.cs`): w ulubionych – „★” z `data-favorite="true"` i `hx-delete="?handler=Ulubione&id=..."`, poza nimi – „☆” z `data-favorite="false"` i `hx-post="?handler=Ulubione&id=..."`, oba z `hx-swap="outerHTML"`. Używa go `_Lista.cshtml` przy każdym rekordzie.
- Stan ★ `_Lista.cshtml` sprawdza sama: `@inject IFavoritesRepository` i ulubione zalogowanego użytkownika (niezalogowany: wszystkie „☆”). Nie przekazuj tego przez `ViewData` – `Partial(...)` w PageModel go nie kopiuje.
- Handlery `OnPostUlubione(int id)` i `OnDeleteUlubione(int id)` w `Index.cshtml.cs` zwracają `Partial("_Gwiazdka", …)` z nowym stanem. Niezalogowany: status 401 i fragment „Zaloguj się, aby dodać do ulubionych” – sprawdź `User.Identity?.IsAuthenticated` w handlerze i zwróć `new ContentResult { StatusCode = 401, ContentType = "text/html; charset=utf-8", Content = "<p class=\"alert alert-warning\">Zaloguj się, aby dodać do ulubionych</p>" }`. Nie dawaj `[Authorize]` na `IndexModel` ani na handlerach: wtedy niezalogowany dostaje przekierowanie 302 do logowania zamiast 401.
- Token antiforgery wysyła już `<body hx-headers:inherited='{"RequestVerificationToken":"..."}'>` w `_Layout.cshtml`. Nie wyłączaj walidacji antiforgery.
- Strona `src/GzmApp/Pages/Ulubione.cshtml` z `[Authorize]` według reguły 8 (strona „Moje ulubione”): `<ul id="ulubione">`, jeden `<li>` na rekord, tytuł w `<span class="tytul">`. Pokazuj tytuły ulubionych rekordów zgodnie z regułą 8.
- Link „Moje ulubione” w menu w `_Layout.cshtml`, widoczny zawsze (niezalogowanego strona przekieruje do logowania).
- Rejestracja repozytorium w `AddAppServices` w `src/GzmApp/AppServices.cs`. Krótkie metody, bez zagnieżdżonych if-ów.
- Na koniec uruchom `dotnet test tests/GzmApp.Specs --filter Capability=s07-005-zapisz-ulubione`. Jeśli test nie przechodzi, przeczytaj jego komunikat i popraw kod (nie test) – najwyżej raz. Napisz, ile testów przeszło.
- Na koniec napisz w 3 punktach, co zrobiłeś.

Nie zmieniaj plików poza: `scenarios/s07/spec/dodatkowe/s07-005-zapisz-ulubione.md` (przeniesienie), `scenarios/s07/spec/s07-005-zapisz-ulubione.md` (przeniesienie), `db/900_favorites.sql`, `src/GzmApp/Data/FavoritesRepository.cs`, `src/GzmApp/Pages/Index.cshtml.cs`, `src/GzmApp/Pages/Shared/_Lista.cshtml`, `src/GzmApp/Pages/Shared/_Gwiazdka.cshtml`, `src/GzmApp/Pages/Ulubione.cshtml`, `src/GzmApp/Pages/Ulubione.cshtml.cs`, `src/GzmApp/Pages/Shared/_Layout.cshtml`, `src/GzmApp/AppServices.cs`. Nie dodawaj pakietów.
~~~~

### Zobacz, co powstało

Uruchomcie aplikację (F5), zalogujcie się własnym adresem e-mail (jeśli nie macie konta, kliknijcie „Rejestracja”). Kliknijcie ☆ przy rekordzie, potem „Moje ulubione” i sprawdźcie, czy rekord tam jest.
