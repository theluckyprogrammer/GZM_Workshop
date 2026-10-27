# Instrukcja dla Claude Code: pakiet warsztatowy „AI + otwarte dane GZM”

Ten folder (`zadanie/`) zawiera zadanie do wykonania w tym repozytorium. Projekt warsztatu (kod, dane, scenariusze, materiały) budujesz w **katalogu głównym repo**, obok `zadanie/`. Folderu `zadanie/` nie modyfikuj. Wyjątki: poprawki promptów uczniów robisz w szablonach `tools/Cards/templates/` (i generujesz `scenarios/`), nie tutaj; 20.10.2026 prowadzący zmienił podejście warsztatu (gotowe prompty z pauzami zamiast sprawdzania wiedzy) i na jego polecenie dostosowano prompty 00, 04–08, 10, 11, 20, 22 i 24.

## Specyfikacja w ćwiczeniach (od 25.10.2026)
Uczniowie pracują według zasady „najpierw specyfikacja, potem testy, na końcu kod”: `scenarios/sNN/spec/` i testy `tests/GzmApp.Specs`. Praca nad tym repo tej metody nie wymaga. Przy zmianie materiałów uczniów pamiętaj tylko, że każde zdanie reguły (także wariant decyzji) musi mieć definicję kroku (zdanie bez kroku daje czerwony test w portalu pary). Jak to działa i jak zmienić zdanie: `docs/SPECTREE.md`.

## Jak pracować
1. Prompty leżą w `zadanie/prompts/` i są ponumerowane w kolejności wykonania (01–25).
2. Plik `zadanie/prompts/00-KONTEKST-wspolny.md` to wspólny kontekst: stos, zasady kodu, zasady testów i oszczędzania tokenów. Każdy prompt wskazuje, które sekcje przeczytać. Nie uruchamiaj go jako osobnego kroku.
3. Gdy użytkownik napisze **„dalej”**, „następny krok” albo „kontynuuj”:
   1. przeczytaj `docs/STATUS.md` (jeśli nie istnieje, następnym krokiem jest 01),
   2. wybierz pierwszy prompt bez statusu OK,
   3. napisz w jednym zdaniu, który prompt wykonujesz, i wykonaj go.
4. Gdy użytkownik wskaże numer (np. „wykonaj 07”), wykonaj ten prompt, o ile spełnione są jego wymagania z nagłówka.
5. Wykonuj JEDEN prompt na polecenie. Po zakończeniu:
   - podaj raport (najwyżej 10 linii),
   - podaj numer następnego promptu,
   - jeśli nagłówek następnego promptu mówi „Sesja: nowa”, napisz, że należy zrobić `/clear` przed kolejnym poleceniem.

## Git i GitHub
- Gałąź `main` to punkt startu uczniów. Gałęzie `ratunek/etap-1..4` tworzysz zgodnie z promptami.
- Po każdym prompcie wypchnij zmienione gałęzie: `git push origin <gałąź>` (także `ratunek/*`).
- Jeśli środowisko pozwala pushować tylko na gałąź sesji:
  1. wykonaj pracę i wypchnij to, co możliwe,
  2. w `docs/STATUS.md` i w raporcie wypisz, które gałęzie trzeba utworzyć lub zmergować ręcznie i z jakich commitów.
  - Nie obchodź ograniczeń.
- Nigdy nie commituj sekretów: kluczy Kilo, SOL-6, GLM, connection stringów z hasłem.

## Środowisko
Prompty zakładają Windows z SQL Server LocalDB. Jeśli działasz na Linuksie (np. Claude Code w chmurze):
1. Użyj `GZM_CONNSTR` / `GZM_TEST_CONNSTR`, jeśli są ustawione.
2. Jeśli nie są: spróbuj uruchomić SQL Server 2022 (edycja Developer) w Dockerze albo zainstalować go lokalnie.
3. Jeśli żadna baza nie jest dostępna:
   - wykonaj części niewymagające bazy (dane, mapowania, `tools/Expect`, karty, prompty, reguły Kilo, materiały do druku),
   - kroki bazodanowe oznacz w `docs/STATUS.md` jako `DO WYKONANIA NA WINDOWS` z listą poleceń do uruchomienia,
   - nie oznaczaj ich jako OK.
4. Skrypty `.ps1` sprawdzaj przez `pwsh`, jeśli jest dostępny. Pełny test `tools/check.ps1` zostaw na komputer szkolny.

## Priorytety, gdy brakuje czasu lub tokenów
Minimum przed warsztatem (28.10.2026) to prompty 01–12. Testy 13–25 są osobnym etapem.
