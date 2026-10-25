# Specyfikacja i testy w ćwiczeniach (Spectree)

Uczniowie pracują według zasady „najpierw specyfikacja, potem testy, na końcu kod”. Ten plik opisuje, jak to działa i co zrobić przy zmianie materiałów.

## Jak to działa

- Każdy scenariusz ma 4 pliki specyfikacji w `scenarios/sNN/spec/`, po jednym na krok warsztatu: `sNN-001-importuj-dane`, `-002-szukaj-rekordow`, `-003-pokaz-strone`, `-004-analiza-ai`. Krok dodatkowy z „Co dalej?” (`-005-zapisz-ulubione`) leży w `spec/dodatkowe/` – nie ma testów, dopóki para nie przeniesie pliku do `spec/`. Generuje je `tools/Cards` z szablonów `tools/Cards/templates/spec/*.md`. Nie edytuj ich ręcznie.
- Plik ma frontmatter z `capability: <nazwa>` i sekcję `## Reguły` z ponumerowanymi zdaniami. Komentarze `<!-- … -->` w tej sekcji generator pomija.
- `tools/Spectree.Generator` (generator Roslyn) robi z każdego pliku klasę testów xUnit, a z każdej reguły test `[Fact(DisplayName = "Reguła N: <zdanie>")]` z cechami `Capability` i `Rule`. Plik bez `capability:`, bez reguł albo z powtórzonym numerem to błąd kompilacji SPEC0001–SPEC0003.
- `tools/Spectree` wiąże zdanie z metodą `[Step("…")]`. `{nazwa}` w zdaniu kroku to parametr tekstowy, np. `` [Step("Wiersz 7 ma w polu `{pole}` tę samą wartość co w pliku.")] ``. Zdanie bez kroku to czerwony test z komunikatem „Brak definicji kroku…”.
- `tests/GzmApp.Specs` bierze specyfikację scenariusza z `"Scenario"` w `src/GzmApp/appsettings.Development.json` (tam zapisuje go `ratunek.ps1`), a gdy go tam nie ma – z `appsettings.json`. Po zmianie scenariusza trzeba przebudować projekt.
- Kroki są w `tests/GzmApp.Specs/Steps`. Sprawdzają aplikację z zewnątrz (HTTP + SQL na bazie `GzmTest_sNN`, kasowanej na starcie i na końcu przebiegu), a wartości oczekiwane liczą z bazy i z `checkpoints.md`. Dzięki temu działają dla każdego wariantu decyzji. Komunikaty są po polsku, bo czytają je uczniowie i Kilo.
- Krok 4 (analiza AI) nie woła prawdziwego modelu: `SpecApp` podmienia `IChatClient` na `FakeModel` (znana odpowiedź, licznik wywołań, zapis wysłanego tekstu). Wyjątek: reguła 9 („Prawdziwy model AI odpowiada…”) – `FakeModel.UseRealModel` przepuszcza jedno zapytanie do prawdziwego modelu (`Sol6:Model`, domyślnie `gpt-6-luna`; klucz z `src/GzmApp/appsettings.Local.json`). Bez klucza ta reguła jest czerwona z komunikatem „poproś prowadzącego”.
- Test kontrolny `SpecCoverageTests` (bez bazy) pilnuje, że każde zdanie 15 scenariuszy i każdy wariant decyzji z `prompty.md` ma dokładnie jeden krok. Uruchamia go też `tools/check.ps1`.

## Zmiana zdania albo wariantu decyzji

1. Zdanie reguły zmieniasz w `tools/Cards/templates/spec/*.md`, a warianty decyzji w `tools/Cards/Decisions.cs`. Numerów reguł nie zmieniasz, nowa reguła dostaje kolejny numer.
2. `dotnet run --project tools/Cards`, potem `dotnet run --project tools/Cards -- check`.
3. Nowe zdanie potrzebuje kroku w `tests/GzmApp.Specs/Steps`. Test kontrolny pokaże zdania bez kroku.
4. Jeśli krok czyta nowy znacznik HTML, dopisz go do promptu w `tools/Cards/templates/prompty.md` – inaczej kod ucznia może działać, a test i tak będzie czerwony.
5. Sprawdź: na `main` testy kroku są czerwone z sensownym komunikatem, a na `ratunek/etap-4` zielone. Test, który przechodzi na pustym `main`, niczego nie sprawdza.

## Uruchamianie

- Uczniowie: ikona kolby w VS Code → `GzmApp.Specs`. Kilo może uruchomić `dotnet test tests/GzmApp.Specs --filter Capability=<capability>`, ale nie może zmieniać `tests/`, `scenarios/` ani `tools/` (`kilo.jsonc`).
- Poza Windows: baza testowa z `GZM_TEST_CONNSTR`.
- Gdy Razor nagle zgłasza RZ1021 w niezmienionych `.cshtml`, to zawieszony serwer kompilacji: `dotnet build-server shutdown`, potem build.
