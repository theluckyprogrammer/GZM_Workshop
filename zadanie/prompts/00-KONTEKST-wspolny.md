# 00 – Kontekst wspólny (nie uruchamiaj osobno)

Każdy prompt 01–25 zaczyna się od polecenia przeczytania tego pliku. Nie powtarzaj jego treści w odpowiedziach.

## Projekt
Pakiet warsztatowy „AI + otwarte dane GZM”:
- 30 minut, 15 par uczniów technikum informatycznego,
- uczniowie NIE umieją programować,
- Windows 10, konta bez uprawnień administratora,
- data warsztatu: 28.10.2026.

Format „Jeden przepis, różne składniki”: każda para ma inny zbiór CSV. Celem jest pokazać, jak postawić „całą” aplikację z AI – nie sprawdzamy wiedzy uczniów. Wszystkie pary przechodzą te same 4 etapy:
1. Dane → baza (import CSV przez Dapper),
2. Endpoint GET z filtrowaniem (zwraca fragment HTML),
3. Widok HTMX: lista + wyszukiwarka + jedna statystyka,
4. Ulubione (wymaga logowania).

Uczniowie dostają gotowe prompty (`scenarios/sNN/prompty.md`), po jednym na etap. Po każdym etapie jest pauza:
- **zobaczcie, co powstało** – dane w rozszerzeniu mssql, żądanie w zakładce Network, strona w przeglądarce (bez porównywania liczb),
- **decyzja** – wybór wariantu A/B/C (np. po jakim polu szukać, jaki kafelek statystyki); każdy wariant to gotowa linia, którą para wkleja w miejsce `[DECYZJA n]` w następnym prompcie. Bez wklejenia obowiązuje wariant A.

Na końcu jest lista „Co dalej?” z pomysłami do samodzielnej zabawy (m.in. opis rekordu przez SOL-6).

Role w parze zmieniają się co etap:
- **Pilot** wkleja prompt do Kilo Code i akceptuje zmiany.
- **Nawigator** prowadzi pauzy i decyzje, patrzy, czy wszystko działa.

Wartości kontrolne (`checkpoints.md`) i pułapki w danych (`prowadzacy.md`) są dla prowadzącego, gałęzi ratunkowych i testów – nie dla uczniów. Wartości dotyczą wariantów A.

Brudne dane (średnik, przecinek dziesiętny, kodowanie, puste pola, formaty dat) są prawdziwe, nie podkładamy sztucznych bugów. Prompty uczniów opisują je wprost, żeby aplikacja powstała bez szukania błędów.

Język:
- materiały dla uczniów i prowadzącego: po polsku,
- kod i identyfikatory: po angielsku.

## Stos (niezmienny)
- **.NET 10.** `global.json`: SDK 10.0.x, `rollForward: latestFeature`. Jeden projekt ASP.NET Core Razor Pages: `src/GzmApp`. Stały port `http://localhost:5080`.
- **Widok: HTMX 4.0.0** z pliku lokalnego `wwwroot/lib/htmx/4.0.0/htmx.min.js`.
  - Zero Node.js, zero bundlera, zero React/Vite.
  - Fragmenty HTML zwracają handlery Razor Pages (`Partial`).
  - NIE używaj Htmx.Net (wspiera tylko 1.x/2.x). Zamiast tego własna metoda `IsHtmx()`.
- **Różnice HTMX 4 względem 2.x:**
  - dziedziczenie atrybutów jest jawne (`:inherited`, także dla `hx-headers`),
  - odpowiedzi 4xx/5xx są podmieniane w DOM,
  - zdarzenia mają postać `htmx:phase:action`,
  - `htmx.config.logAll = true` zamiast `htmx.logAll()`,
  - domyślny timeout to 60 s,
  - selektory ze spacją w `hx-trigger from:` muszą być w apostrofach.
  - Nazwy nagłówków żądań sprawdź w https://four.htmx.org/docs/whats-new-in-htmx-4 (sekcja „Request headers”).
- **Baza: SQL Server LocalDB**, `(localdb)\MSSQLLocalDB`, baza `GzmWorkshop`.
  - Na systemie innym niż Windows: connection string ze zmiennej `GZM_CONNSTR`, kontener `mcr.microsoft.com/mssql/server:2022-latest` w edycji Developer. Odnotuj to w `findings.md`.
- **Dane domenowe WYŁĄCZNIE przez Dapper** + jawne skrypty SQL w `db/*.sql`.
  - EF Core jest dozwolony TYLKO w `src/GzmApp/Identity/` (`IdentityDbContext` dla tabel `AspNet*`).
- **Logowanie:** ASP.NET Core Identity (`AddDefaultIdentity` + magazyny EF Core, cookie).
  - Brak `MapIdentityApi`, brak Duende.
  - Antiforgery: `<body hx-headers:inherited='{"RequestVerificationToken":"..."}'>` oraz `app.UseAntiforgery()`.
- **CSV:** CsvHelper (MS-PL/Apache-2.0), `CultureInfo("pl-PL")`.
  - Kodowanie: wykrywanie BOM, ścisły UTF-8, fallback na Windows-1250 (`CodePagesEncodingProvider`).
  - Import idempotentny: tabela staging + `MERGE` po kluczu naturalnym lub `RowHash` (unikalny indeks), w transakcji.
  - Retry: 3 próby dla przejściowych `SqlException`.
  - Powyżej 5000 wierszy: `SqlBulkCopy` do tabeli staging.
- **Podgląd SQL:** klasa `SqlRunner` (cienka warstwa nad Dapperem) loguje SQL i parametry przez `ILogger`, kategoria `"SQL"`.
- **AI (zadania *):** pakiety `OpenAI` + `Microsoft.Extensions.AI.OpenAI` (`IChatClient` przez `AsIChatClient()`).
  - Konfiguracja: `Sol6:Endpoint`, `Sol6:Model`, `Sol6:ApiKey`, `Sol6:Kind` (`"openai"` | `"azure"`).
  - Dla `"azure"`: `Azure.AI.OpenAI` (`AzureOpenAIClient` + `AzureKeyCredential`).
  - Klucz WYŁĄCZNIE z `dotnet user-secrets` albo zmiennej środowiskowej `SOL6__APIKEY`. Nigdy w repo.
- **Licencje:** tylko MIT, Apache, MS-PL, BSD. Każdy pakiet wpisz do `docs/LICENSES.md`.

## Zasady kodu (egzekwuj w każdym kroku)
1. Guard clauses na początku każdej publicznej metody (`ArgumentNullException.ThrowIfNull`, `ArgumentException.ThrowIfNullOrWhiteSpace`, własne `Ensure.*`).
2. Zero zagnieżdżonych if-ów i pętli. Używaj wczesnego `return`, LINQ i małych metod.
3. Metody ≤ 20 linii, klasy ≤ 150 linii, SOLID.
   - Interfejsy: `IRecordRepository`, `ICsvImporter`, `IFavoritesRepository`, `IAiSummarizer`.
4. Idempotencja i retry tam, gdzie jest I/O:
   - dwukrotny import daje te same liczby,
   - ulubione mają unikalny klucz `(UserId, RecordId)`.
5. Duplikacja jest OK, jeśli upraszcza czytanie kodu przez ucznia.

## Oszczędzanie tokenów (obowiązkowe)
- Nie wyświetlaj całych plików CSV, logów ani HTML.
  - CSV: nagłówek + najwyżej 3 wiersze.
  - Logi: podsumowanie i PIERWSZY błąd.
- `dotnet build` / `dotnet test` uruchamiaj z `-v minimal` / `--logger "console;verbosity=minimal"`. Testy filtruj (`--filter`).
- Nie czytaj plików niezwiązanych z bieżącym zadaniem.
- Poprawki rób minimalnym diffem, tylko w miejscu błędu.
- Raport końcowy promptu ma najwyżej 10 linii.

## Status i commity
- Przed startem przeczytaj `docs/STATUS.md`. Jeśli prompt wymagany w nagłówku nie ma statusu OK, zatrzymaj się i napisz, którego brakuje.
- Jeśli bieżący prompt ma już status OK, tylko sprawdź kryterium ukończenia i zakończ.
- Na koniec:
  - commit `"NN: <operacja>"`,
  - wiersz w `docs/STATUS.md` w formacie: `| NN | operacja | OK/BŁĄD | data | uwaga w 1 zdaniu |`.
- Obserwacje, ryzyka i sugestie (nie-błędy) zapisuj do `findings.md`.

## Gałęzie
- `main` to szkielet i logowanie. To punkt startu uczniów.
- `ratunek/etap-1`…`ratunek/etap-4` są kumulatywne: każda zawiera poprzednią plus kolejny etap. Realizują warianty A wszystkich decyzji z `prompty.md`.
- Silnik jest generyczny i sterowany plikiem `scenarios/sNN/mapping.json`. Scenariusz wybiera klucz `"Scenario"` w `appsettings.Development.json`.
- Poprawka we wspólnym kodzie:
  1. commit na najniższej gałęzi, której dotyczy,
  2. merge w górę: etap-1 → etap-2 → etap-3 → etap-4.

## Gdy URL danych nie działa
1. Nie zgaduj URL-a.
2. Ponów `package_show` (3 próby).
3. Jeśli nadal błąd, użyj kolejnego zbioru rezerwowego (lista w prompcie 02) i zapisz powód w `docs/ZAMIANY.md`.
4. Dla zamienionego zbioru wygeneruj komplet z promptów 03 i 04.

## Zasady testów (prompty 13–25)
- Kolejność: spec → test → poprawka.
- Każdy test ma identyfikator wymagania (`[Trait("Spec","E1-03")]`) i poziom (`[Trait("Poziom","2")]`).
- Mapowanie prowadź w `docs/SPEC-TESTY.md`: ID → opis → test(y) → status.
- Testuj tylko przepływy, happy path i prawdopodobne edge case'y. Nie pisz testów dla samej liczby testów.
- Kod testów spełnia te same zasady kodu co produkcja.
- Biblioteki: xUnit, `Microsoft.AspNetCore.Mvc.Testing`, AngleSharp, `Microsoft.Playwright` (tylko prompt 18), `Microsoft.CodeAnalysis.CSharp` (tylko prompt 23).
- Jeden parametryzowany zestaw dla wszystkich scenariuszy: `TheoryData` czytane ze `scenarios/*/mapping.json` i `checkpoints.md`. Nie pisz kodu per scenariusz.
- Baza testowa:
  - connection string ze zmiennej `GZM_TEST_CONNSTR`,
  - domyślnie LocalDB z bazą `GzmTest_{scenariusz}`, którą tworzy i usuwa fixture.
- Testy danego poziomu dodawaj na gałęzi, na której ten poziom istnieje, i mergeuj w górę.
- Test nie może odwoływać się do typów z późniejszych etapów. Endpointy testuj przez HTTP (WebApplicationFactory).
