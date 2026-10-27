# Specyfikacja i testy w ćwiczeniach (Spectree)

Uczniowie pracują według zasady „najpierw specyfikacja, potem testy, na końcu kod”. Ten plik opisuje, jak to działa i co zrobić przy zmianie materiałów.

## Jak to działa

- Każdy scenariusz ma 5 plików specyfikacji w `scenarios/sNN/spec/`, po jednym na krok warsztatu: `sNN-000-dane` (krok 0: pobrany plik danych), `-001-importuj-dane`, `-002-szukaj-rekordow`, `-003-pokaz-strone`, `-004-analiza-ai`. Krok dodatkowy z „Co dalej?” (`-005-zapisz-ulubione`) leży w `spec/dodatkowe/` – nie ma testów, dopóki para nie przeniesie pliku do `spec/`. Generuje je `tools/Cards` z szablonów `tools/Cards/templates/spec/*.md`. Nie edytuj ich ręcznie.
- Plik ma frontmatter z `capability: <nazwa>` i sekcję `## Reguły` z ponumerowanymi zdaniami. Komentarze `<!-- … -->` w tej sekcji generator pomija.
- `tools/Spectree.Generator` (generator Roslyn) robi z każdego pliku klasę testów xUnit, a z każdej reguły test `[Fact(DisplayName = "Reguła N: <zdanie>")]` z cechami `Capability` i `Rule`. Plik bez `capability:`, bez reguł albo z powtórzonym numerem to błąd kompilacji SPEC0001–SPEC0003.
- `tools/Spectree` wiąże zdanie z metodą `[Step("…")]`. `{nazwa}` w zdaniu kroku to parametr tekstowy, np. `` [Step("Wiersz 7 ma w polu `{pole}` tę samą wartość co w pliku.")] ``. Zdanie bez kroku to czerwony test z komunikatem „Brak definicji kroku…”.
- Testy biorą scenariusz z `"Scenario"` w `src/GzmApp/appsettings.json` – zapisuje go tam wyłącznie skrypt `tools/nowy-portal.ps1` w kroku 0 (równolegle działa `appsettings.Development.json`, gdyby kiedyś był potrzebny). Ręcznie scenariusza się nie wpisuje; po zmianie trzeba przebudować projekt. Przed krokiem 0 projektu testów w ogóle nie ma – powstaje ze `szkielet/` (buduje się też wprost z `szkielet/tests`, wtedy bez scenariusza i bez żadnych testów). `szkielet/` jest ukryty w VS Code (`.vscode/settings.json`), więc w kolbie (Testing) widać tylko testy scenariusza pary: `GzmApp.Specs.Spec`. To jedyne testy w repo: projekt testów samych warsztatów (`tests/GzmApp.Tests`) został usunięty.
- Kroki są w `szkielet/tests/GzmApp.Specs/Steps` (po kroku 0: `tests/GzmApp.Specs/Steps`). Sprawdzają aplikację z zewnątrz (HTTP + SQL na bazie `GzmTest_sNN`, kasowanej na starcie i na końcu przebiegu), a wartości oczekiwane liczą z bazy i z `checkpoints.md`. Dzięki temu działają dla każdego wyboru pary. Komunikaty są po polsku, bo czytają je uczniowie i Kilo.
- Kroki kroku 0 (`DataFileSteps`) sprawdzają sam plik CSV (istnienie, liczba wierszy z `E1.fileRows`, nagłówki) – bez aplikacji i bazy: to pierwsze zielone testy pary.
- Krok 4 (analiza AI) nie woła prawdziwego modelu: `SpecApp` podmienia `IChatClient` na `FakeModel` (znana odpowiedź, licznik wywołań, zapis wysłanego tekstu). Wyjątek: reguła 9 („Prawdziwy model AI odpowiada…”) – `FakeModel.UseRealModel` przepuszcza jedno zapytanie do prawdziwego modelu (`Sol6:Model`, domyślnie `gpt-6-luna`; klucz z `src/GzmApp/appsettings.Local.json`). Bez klucza ta reguła jest czerwona z komunikatem „poproś prowadzącego”.
- Decyzje 1–5 nie mają wydrukowanych wariantów: w pauzie para wkleja Kilo prośbę o propozycje, a Kilo proponuje zdania według **wzorców** osadzonych w `karta.md` (w blokach „Zaproponuj nam…”), podstawiając pola z danych i patrząc na `LOG-DECYZJI.md`. Wybrane przez parę zdanie para wkleja do specyfikacji.
- Warsztatu samego w sobie nie testujemy: nie ma testu, który sprawdza, że każde zdanie i każdy wzorzec decyzji ma krok. Zdanie bez kroku widać dopiero w portalu pary jako czerwony test – po zmianie zdań lub wzorców sprawdź to ręcznie (krok 0 na próbnym branchu i `dotnet test tests/GzmApp.Specs`).

## Zmiana zdania albo wzorca decyzji

1. Zdanie reguły zmieniasz w `tools/Cards/templates/spec/*.md`, a zdanie domyślne decyzji w `tools/Cards/scenarios.json` (`search[0]`, `listFields[0]`, `stats[0]`, `aiTask` – pilnuje tego `Decisions.cs`).
2. Wzorce zdań decyzji (to, co Kilo dostaje do proponowania) zmieniasz w `tools/Cards/templates/karta.md` w blokach pauz „decyzja N”. Każdy wzorzec musi pasować do jakiegoś `[Step]`.
3. `dotnet run --project tools/Cards`, potem `dotnet run --project tools/Cards -- check`.
4. Nowe zdanie potrzebuje kroku w `szkielet/tests/GzmApp.Specs/Steps`.
5. Jeśli krok czyta nowy znacznik HTML, dopisz go do promptu w `tools/Cards/templates/karta.md` – inaczej kod ucznia może działać, a test i tak będzie czerwony.
6. Sprawdź: przed krokiem 0 (czyste repozytorium) testy kroku są czerwone z sensownym komunikatem, a po kroku 0 i zbudowaniu funkcji – zielone. Test, który przechodzi bez zbudowanej funkcji, niczego nie sprawdza.

## Uruchamianie

- Uczniowie: ikona kolby w VS Code → `GzmApp.Specs` (testy pojawiają się po kroku 0). Kilo może uruchomić `dotnet test tests/GzmApp.Specs --filter Capability=<capability>`, ale nie może zmieniać `tests/`, `scenarios/` ani `tools/` (`kilo.jsonc`).
- Poza Windows: baza testowa z `GZM_TEST_CONNSTR`.
- Gdy Razor nagle zgłasza RZ1021 w niezmienionych `.cshtml`, to zawieszony serwer kompilacji: `dotnet build-server shutdown`, potem build.
