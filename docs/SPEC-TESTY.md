# Wymagania i testy

Każdy test ma `[Trait("Spec","ID")]` i `[Trait("Poziom","N")]`. Uruchamianie: `dotnet test tests/GzmApp.Tests --filter "Poziom=2"` (albo `Spec=E1-01`).
Wartości oczekiwane dotyczą wariantów A (`checkpoints.md`). Status: `OK`, `BŁĄD`, `—` (jeszcze bez testu), `ręcznie` (poza testami automatycznymi).

| ID | Opis | Test(y) | Status |
|---|---|---|---|
| INF-01 | AppFactory uruchamia aplikację na własnej bazie testowej `GzmTest_{scenariusz}` i usuwa ją po testach | `InfrastructureTests.AppFactory_starts_the_app_on_its_own_test_database` | OK |
| D-00 | `ScenarioData` znajduje 15 scenariuszy, każdy ma niepuste wartości E1–E4 | `SmokeTests.ScenarioData_finds_15_scenarios_with_values_E1_to_E4` | OK |
| D-01 | Plik w `data/raw` ma SHA-256 i liczbę wierszy z `data/metadata.json` | `DataTests.File_has_SHA256_and_row_count_from_metadata` (15) | OK |
| D-02 | Nagłówki w pliku = nagłówki w metadanych = kolumny w `mapping.json` | `DataTests.Headers_in_file_metadata_and_mapping_are_the_same` (15) | OK |
| D-03 | Licencja i atrybucja każdego zbioru są niepuste | `DataTests.License_and_attribution_are_filled_in` (15) | OK |
| D-04 | Niezależny parser (inny kod niż `tools/Expect`) daje te same E1–E4 co `checkpoints.md` | `DataTests.Second_parser_gives_the_values_of_checkpoints` (15) | OK |
| D-05 | Kontrola danych osobowych (PESEL, telefony, e-maile, „Imię Nazwisko”) – trafienia opisane w `findings.md` | `DataTests.No_PESEL_or_email_and_phone_numbers_only_where_reviewed` | OK |
| E1-01 | Po imporcie `COUNT(*)` = E1.rows | — | — |
| E1-02 | Wartość pola kontrolnego w wierszu 7 (`SourceRow`) = E1.row7.value | — | — |
| E1-03 | Polskie znaki zachowane (rekord z ogonkami = surowy plik) | — | — |
| E1-04 | Liczby z kropką lub przecinkiem dziesiętnym trafiają jako `decimal` z poprawną wartością | — | — |
| E1-05 | Daty sparsowane według formatu z `mapping.json` | — | — |
| E1-06 | Puste pola → `NULL`, bez wyjątku | — | — |
| E1-07 | Idempotencja: drugi import daje tę samą liczbę wierszy | — | — |
| E1-08 | Atomowość: import przerwany w połowie nie zostawia częściowych danych | — | — |
| E2-01 | Filtr dla frazy z checkpointu = E2 (liczba i tytuły) | — | — |
| E2-02 | Wielkość liter nie ma znaczenia | — | — |
| E2-03 | Fraza z ogonkami działa; zachowanie bez ogonków opisane w `findings.md` | — | — |
| E2-04 | Pusta fraza zwraca pierwsze 20 rekordów | — | — |
| E2-05 | `' OR 1=1 --` daje 0 wyników, a log `SQL` ma parametr `@q`, nie sklejony tekst | — | — |
| E2-06 | Fraza dłuższa niż 100 znaków → 400 | — | — |
| E3-01 | Żądanie z nagłówkiem `HX-Request` dostaje fragment bez `<html>` | — | — |
| E3-02 | Żądanie bez nagłówka dostaje pełną stronę | — | — |
| E3-03 | Statystyka w HTML = E3 | — | — |
| E3-04 | Strona wczytuje lokalny `/lib/htmx/4.0.0/htmx.min.js`, nie CDN | — | — |
| E3-05 | `<body>` ma `hx-headers:inherited` z tokenem | — | — |
| E3-06 | Wyszukiwarka ma `hx-trigger` z `delay:300ms` | — | — |
| E3-07 | Błąd 500 w żądaniu HTMX zwraca krótki fragment z komunikatem | — | — |
| E4-01 | Niezalogowany: `/Ulubione` przekierowuje do logowania, ★ daje 401 | — | — |
| E4-02 | Zalogowany bez tokenu antiforgery → 400 | — | — |
| E4-03 | Z tokenem → 200 | — | — |
| E4-04 | Dwukrotne dodanie daje 1 wiersz w `Favorites` | — | — |
| E4-05 | Dwukrotne usunięcie nie zwraca błędu | — | — |
| E4-06 | Użytkownik B nie widzi ulubionych użytkownika A | — | — |
| E4-07 | Tytuł ulubionego rekordu nr 7 = E4.title | — | — |
| UI-01 | Wpisanie frazy znak po znaku → po ~300 ms dokładnie 1 żądanie i zmiana `#wyniki` | — | — |
| UI-02 | Brak błędów JS w konsoli przeglądarki | — | — |
| UI-03 | Po zalogowaniu klik ★ → gwiazdka aktywna i rekord w bazie | — | — |
| UI-04 | Wymuszony błąd 500 → komunikat w `#wyniki`, nie cała strona błędu | — | — |
| UI-05 | Strona działa offline: blokada żądań do CDN nic nie psuje | — | — |
| R-01 | Każda gałąź `ratunek/etap-N` buduje się i przechodzi testy poziomów ≤ N+1 | — | — |
| R-02 | `ratunek.ps1` odkłada niezacommitowane zmiany do schowka i da się je odzyskać | — | — |
| R-03 | `ratunek.ps1 -Etap 3 -Scenariusz s06` ustawia scenariusz i importuje dane; powtórka jest bezpieczna | — | — |
| R-04 | Gałęzie są spójne: wyższa zawiera wszystkie commity niższej | — | — |
| P-01 | s06 przechodzi kroki 1–4 samymi promptami z `prompty.md` (warianty A, jeden wariant B) | — | — |
| P-02 | s04 przechodzi kroki 1–4 samymi promptami | — | — |
| P-03 | s02 przechodzi kroki 1–4 samymi promptami | — | — |
| P-04 | Wspólne poprawki promptów naniesione na 15 scenariuszy (szablony `tools/Cards`) | — | — |
| Q-01 | `kilo.jsonc` to poprawny JSONC z blokadami plików i `bash "*": "deny"` | — | — |
| Q-02 | Kod w `src/` i `tools/`: bez zagnieżdżonych if-ów i pętli, metody ≤ 20 linii, klasy ≤ 150, guard clauses | — | — |
| Q-03 | Brak sekretów we wszystkich gałęziach i w historii | — | — |
| Q-04 | Każdy pakiet ma licencję MIT, Apache, MS-PL albo BSD i jest w `docs/LICENSES.md` | — | — |
| M-01 | Ściąga prowadzącego = `checkpoints.md` dla 15 scenariuszy | — | — |
| M-02 | Karty uczniów i `prompty.md` nie zawierają wartości oczekiwanych; decyzje mają ≥ 2 warianty, wariant A = `mapping.json` | — | — |
| M-03 | `print/*.html` bez pustych pól i placeholderów | — | — |
| M-04 | Ścieżki i polecenia z `docs/PROWADZACY.md` istnieją w repo | — | — |
| M-05 | Stopka aplikacji ma atrybucję CC BY 4.0 w każdym scenariuszu | — | — |
| AI-01 | Fałszywy `IChatClient` → fragment HTML bez `<html>`, do modelu trafiają tylko pola rekordu z bazy | — | — |
| AI-02 | Brak klucza → „Brak klucza SOL-6 – poproś prowadzącego”, bez wyjątku | — | — |
| AI-03 | Klucza nie ma w logach | — | — |
| AI-04 | Prawdziwe wywołanie SOL-6 (tylko z sekretem `Sol6:ApiKey`) | — | — |
