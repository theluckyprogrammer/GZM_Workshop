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
| E1-01 | Po imporcie `COUNT(*)` = E1.rows | `ImportTests.Row_count_after_import_equals_E1` (15) | OK |
| E1-02 | Wartość pola kontrolnego w wierszu 7 (`SourceRow`) = E1.row7.value | `ImportTests.Check_field_in_row_7_equals_checkpoint` (15) | OK |
| E1-03 | Polskie znaki zachowane (rekord z ogonkami = surowy plik) | `ImportTests.Polish_letters_are_kept` (15; s08 bez polskich liter) | OK |
| E1-04 | Liczby z kropką lub przecinkiem dziesiętnym trafiają jako `decimal` z poprawną wartością | `ImportTests.Decimal_values_equal_the_file` (15, wszystkie wiersze) | OK |
| E1-05 | Daty sparsowane według formatu z `mapping.json` | `ImportTests.Dates_follow_the_format_from_mapping` (15, wszystkie wiersze) | OK |
| E1-06 | Puste pola → `NULL`, bez wyjątku | `ImportTests.Empty_or_unreadable_fields_are_NULL` (15) | OK |
| E1-07 | Idempotencja: drugi import daje tę samą liczbę wierszy | `ImportTests.Second_import_keeps_the_row_count` (15) | OK |
| E1-08 | Atomowość: import przerwany w połowie nie zostawia częściowych danych | `ImportTests.Import_interrupted_halfway_leaves_no_partial_data` (s06, trigger testowy) | OK |
| E2-01 | Filtr dla frazy z checkpointu = E2 (liczba i tytuły) | `SearchTests.Filter_for_the_checkpoint_phrase_gives_E2` (15) | OK |
| E2-02 | Wielkość liter nie ma znaczenia | `SearchTests.Letter_case_does_not_matter` (15) | OK |
| E2-03 | Fraza z ogonkami działa; zachowanie bez ogonków opisane w `findings.md` | `SearchTests.Phrase_with_Polish_letters_works_also_without_them` (15) | OK |
| E2-04 | Pusta fraza zwraca pierwsze 20 rekordów | `SearchTests.Empty_phrase_returns_the_first_20_records` (15) | OK |
| E2-05 | `' OR 1=1 --` daje 0 wyników, a log `SQL` ma parametr `@q`, nie sklejony tekst | `SearchTests.SQL_injection_gives_0_results_and_the_log_shows_parameter_q` (15) | OK |
| E2-06 | Fraza dłuższa niż 100 znaków → 400 | `SearchTests.Phrase_longer_than_100_characters_gives_400` (15) | OK |
| E3-01 | Żądanie z nagłówkiem `HX-Request` dostaje fragment bez `<html>` | `ViewTests.HTMX_request_gets_a_fragment_without_html` (15) | OK |
| E3-02 | Żądanie bez nagłówka dostaje pełną stronę | `ViewTests.Request_without_HTMX_header_gets_the_whole_page` (15) | OK |
| E3-03 | Statystyka w HTML = E3 | `ViewTests.Statistic_tile_equals_E3` (15) | OK |
| E3-04 | Strona wczytuje lokalny `/lib/htmx/4.0.0/htmx.min.js`, nie CDN | `ViewTests.Page_loads_the_local_htmx_4_file` (15) | OK |
| E3-05 | `<body>` ma `hx-headers:inherited` z tokenem | `ViewTests.Body_has_inherited_hx_headers_with_the_token` (15) | OK |
| E3-06 | Wyszukiwarka ma `hx-trigger` z `delay:300ms` | `ViewTests.Search_box_waits_300_ms` (15) | OK |
| E3-07 | Błąd 500 w żądaniu HTMX zwraca krótki fragment z komunikatem | `ViewTests.Server_error_in_HTMX_request_returns_a_short_fragment` (s02) | OK |
| E4-01 | Niezalogowany: `/Ulubione` przekierowuje do logowania, ★ daje 401 | `FavoritesTests.Anonymous_user_is_sent_to_login_and_gets_401_for_the_star` (15) | OK |
| E4-02 | Zalogowany bez tokenu antiforgery → 400 | `FavoritesTests.Signed_in_user_without_token_gets_400` (15) | OK |
| E4-03 | Z tokenem → 200 | `FavoritesTests.Signed_in_user_with_token_gets_200` (15) | OK |
| E4-04 | Dwukrotne dodanie daje 1 wiersz w `Favorites` | `FavoritesTests.Adding_twice_gives_one_row` (15) | OK |
| E4-05 | Dwukrotne usunięcie nie zwraca błędu | `FavoritesTests.Removing_twice_does_not_fail` (15) | OK |
| E4-06 | Użytkownik B nie widzi ulubionych użytkownika A | `FavoritesTests.User_B_does_not_see_favorites_of_user_A` (15) | OK |
| E4-07 | Tytuł ulubionego rekordu nr 7 = E4.title | `FavoritesTests.Favorite_record_7_has_the_title_E4` (15) | OK |
| UI-01 | Wpisanie frazy znak po znaku → po ~300 ms dokładnie 1 żądanie i zmiana `#wyniki` | `BrowserTests.Typing_sends_one_request_after_300_ms_and_changes_the_list` (s06, s04, s02) | OK |
| UI-02 | Brak błędów JS w konsoli przeglądarki | `BrowserTests.No_JavaScript_errors_in_the_console` (s06, s04, s02) | OK |
| UI-03 | Po zalogowaniu klik ★ → gwiazdka aktywna i rekord w bazie | `BrowserTests.Signed_in_user_clicks_the_star_and_the_record_is_saved` (s06, s04, s02) | OK |
| UI-04 | Wymuszony błąd 500 → komunikat w `#wyniki`, nie cała strona błędu | `BrowserTests.Server_error_shows_a_message_in_the_results_not_an_error_page` (s06, s04, s02) | OK |
| UI-05 | Strona działa offline: blokada żądań do CDN nic nie psuje | `BrowserTests.Page_works_with_requests_outside_the_app_blocked` (s06, s04, s02) | OK |
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
