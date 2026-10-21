# Walidacja promptów uczniów (kroki 20–22, poziom 7)

Trzy scenariusze przeszły kroki 1–4 samymi promptami z `scenarios/sNN/prompty.md`.

Jak przebiegała walidacja:
- Każdy przebieg robiła osobna sesja agenta, która symulowała parę z modelem w Kilo Code.
- Agent pracował we własnym worktree ze stanem `main`, bez dostępu do gałęzi ratunkowych i historii.
- Edytował tylko pliki z listy „Nie zmieniaj plików poza: …” i respektował listę deny z `kilo.jsonc`.
- Baza: SQL Server 2022 w Dockerze, osobna baza dla każdego przebiegu (`GzmWalk_sNN`).
- Aplikacja działała na porcie 5081–5083 zamiast 5080, żeby przebiegi szły równolegle.

Pomiar czasu:
- Czas modelu = pisanie kodu + udany `dotnet build`.
- Czas pauzy = uruchomienie aplikacji, żądania HTTP (zamiast przeglądarki) i zapytania SQL (zamiast rozszerzenia mssql).
- Model w symulacji był szybszy niż `glm-5.3-flash` w Kilo. Czasy pokazują tylko, że prompty nie każą robić niczego zbędnego, a nie czas, który zajmie to parze.

Opcjonalnego wywołania GLM (punkt 4 promptu 20) nie wykonano, bo w sesji nie było ustawionych `GLM_ENDPOINT` ani `GLM_API_KEY`.

Limit z promptu 20 to 6 min pracy modelu i 1,5 min checkpointu na krok. Żaden krok go nie przekroczył: najdłużej trwał krok 4 w s04, 64 s modelu i 50 s pauzy.

## s06 DrzewoRekord (najtrudniejszy)

Decyzje: 1–3 = A, **4 = B** (tytuł i data dodania).

| Krok | Model (s) | Pauza (s) | Wynik wobec `checkpoints.md` |
|---|---|---|---|
| 1 – import | 29 | 14 | OK: 31 wierszy, wiersz 7 `Circumference` = 290, puste pola zgodne z E1.weird |
| 2 – endpoint | 20 | 12 | OK: „wyniosły” → 3 wyniki (Nr 5, 14, 28); „WYNIOSLY” też 3; pusta fraza → 20; 101 znaków → 400 |
| 3 – widok HTMX | 34 | 9 | OK: kafelek 311,40 cm (E3 = 311.40), fragment HTMX bez `<html>`, atrybucja w stopce |
| 4 – ulubione (B) | 74 | 28 | OK: ★ na Nr 7, „Moje ulubione” = „Buk pospolity (Fagus sylvatica) – dodano: …”; bez tokenu 400, niezalogowany 401, `/Ulubione` → logowanie |

Wariant B decyzji 4 działa.

Poprawki promptów z tego przebiegu:
- przycisk ★ jako osobny widok częściowy `_Gwiazdka`,
- stan ★ w `_Lista`,
- zachowanie zwykłego żądania `?handler=Szukaj`,
- format pl-PL kafelka,
- strefa czasowa daty w 4B,
- ścieżka skryptów SQL,
- reguła NULL dla kolumn wymaganych,
- MERGE po kluczu.

Szczegóły są w tabeli zmian niżej.

## s04 ZgubaBytom (typowy)

Decyzje: 1 = A, **2 = B** (rok `Year` na liście), 3–4 = A.

| Krok | Model (s) | Pauza (s) | Wynik wobec `checkpoints.md` |
|---|---|---|---|
| 1 – import | 43 | 19 | OK: 451 wierszy, wiersz 7 `EntryDate` = 2012-11-14, 7 pustych `EntryDate` = E1.weird |
| 2 – endpoint | 23 | 11 | OK: „wózek” → 5 wyników; „WOZEK” też 5; pusta fraza → 20; 101 znaków → 400 |
| 3 – widok HTMX | 26 | 11 | OK: FINISHED 402 / UNFINISHED 49 = E3, fragment bez `<html>`, atrybucja, rok na liście (2B) |
| 4 – ulubione | 60 + 4 | 50 | OK po poprawce: ★ na Nr 7 → „Umowa” = E4.title; 400 / 401 / przekierowanie jak wyżej |

Wariant B decyzji 2 działa.

W pierwszej wersji kroku 4 był błąd. W odpowiedzi HTMX z wyszukiwarki wszystkie ★ wracały do stanu „niedodane”. Przyczyna: model przekazał stan ★ przez `ViewData`, a `PageModel.Partial()` zaczyna z pustym `ViewData`. Prompt nie mówił, skąd `_Lista` ma wziąć ten stan.

Kod poprawiono w dozwolonych plikach, a prompt zmieniono (zmiana 7 w tabeli niżej).

## s02 StacjaFinder (najprostszy)

Decyzje: 1–2 = A, **3 = B** (TOP 5 gmin), 4 = A.

| Krok | Model (s) | Pauza (s) | Wynik wobec `checkpoints.md` |
|---|---|---|---|
| 1 – import | 58 | 17 | OK: 945 wierszy, wiersz 7 `Lat` = 50.275952000 |
| 2 – endpoint | 31 | 11 | OK: „bieruń” → 6 wyników (27025, 27231, 27254, 27259, 27544, 27909); „BIERUN” też 6 |
| 3 – widok HTMX (B) | 25 | 23 | OK: Katowice 155, Gliwice 91, Sosnowiec 86, Zabrze 80, Bytom 66 = zapytanie SQL wariantu B; fragment bez `<html>`, atrybucja |
| 4 – ulubione | 70 | 59 | checkpoint OK (27007 = E4.title), ale ten sam błąd stanu ★ w HTMX co w s04 |
| 4 – po poprawce promptu | 15 | 12 | OK: lista HTMX zalogowanego pokazuje ★ jako dodaną; POST/DELETE 2× → ten sam stan |

Wariant B decyzji 3 działa.

## Zmiany w promptach (szablon `tools/Cards/templates/prompty.md`, wszystkie 15 scenariuszy)

Wszystkie poprawki są wspólne dla scenariuszy, więc naniesiono je w szablonie (oraz w `Decisions.cs` i `Variables.cs`) i wygenerowano materiały ponownie: `prompty.md`, `prowadzacy.md`, `print/`. `Cards check` nie zgłosił błędów. Pozostałych scenariuszy nie przechodzono w całości, zgodnie z promptem 22.

| # | Krok | Problem (scenariusze) | Zmiana |
|---|---|---|---|
| 1 | 1, 4 | Nie wiadomo, skąd czytać `db/*.sql` (s06, s04, s02) | `Path.Combine(AppContext.BaseDirectory, "db", "…")`, jak w `DatabaseBootstrapper` |
| 2 | 1 | „Puste = NULL” kłóci się z kolumnami NOT NULL (s06, s04, s02) | NULL tylko dla „Może być puste: tak”; przy „nie” wiersz pominięty, numer w logu |
| 3 | 1 | Brak zasady dla powtórzonego klucza (s06, s04) | `MERGE` po kluczu naturalnym: powtórzony klucz nadpisuje wiersz (`Variables.cs`) |
| 4 | 1 | Przy drugim starcie log milczy, a pauza obiecuje wpis (s02) | Log „Import pominięty – w tabeli jest N wierszy” |
| 5 | 2 | Nieokreślone `take` i „Wyników: N” (s04, s02) | `take` = 20 zawsze; `Wyników: N` = wszystkie pasujące (`COUNT(*)`); 400 z krótkim fragmentem `alert` |
| 6 | 2 | Encje `&#x144;` w F12 → Response mylą (s06, s04, s02) | Uwaga w pauzie 2: to ten sam znak zapisany kodem HTML |
| 7 | 3 | Niejasne, co dostaje zwykłe wejście na `?handler=Szukaj` (s06, s04) | `IsHtmx()` w `OnGetSzukaj`: zwykłe wejście → cała strona z wynikami dla `q`; uwaga w pauzie 3 |
| 8 | 3 | Separator dziesiętny kafelka nieokreślony (s06) | Liczby w formacie pl-PL (przecinek) |
| 9 | 4 | Brak pliku na przycisk ★, znacznik powielany w C# (s06, s04, s02) | Nowy widok częściowy `Pages/Shared/_Gwiazdka.cshtml` na liście plików; handlery zwracają `Partial("_Gwiazdka", …)` |
| 10 | 4 | Stan ★ przez `ViewData` ginie w `Partial()` (błąd w s04, s02) | `_Lista` sama: `@inject IFavoritesRepository`; zakaz `ViewData` z wyjaśnieniem |
| 11 | 4 | Widoczność linku „Moje ulubione” (s06, s04) | Widoczny zawsze; niezalogowanego strona przekieruje do logowania |
| 12 | 4 (4B) | Strefa czasowa i format `CreatedAt` (s06) | „w bazie UTC – pokaż czas polski jako `dd.MM.yyyy HH:mm`” (`Decisions.cs`) |
| 13 | 4 | Oba stany przycisku miały ten sam znak (s06) | „☆” = dodaj (`hx-post`), „★” = usuń (`hx-delete`) |

Czego nie zmieniono:
- Razor koduje polskie litery jako encje. Naprawa (`WebEncoderOptions` z `UnicodeRanges.All`) wymaga zmiany `Program.cs`, a ten plik jest na liście deny promptów. Zostaje decyzja prowadzącego: zmiana w gałęzi startowej albo tylko uwaga w pauzie 2, którą już dodano.
- Kafelek s04 pokazuje surowe kody `FINISHED` / `UNFINISHED`. To poprawne dane; tłumaczenie na „zakończone” / „otwarte” to pomysł do „Co dalej?”.
- Znaki `%` i `_` we frazie działają jak wzorzec `LIKE`. Nie przeszkadza to w warsztacie, a zmiana skomplikowałaby prompt.

Żaden krok w żadnym przebiegu nie wymagał zmiany pliku spoza listy promptu ani z listy deny `kilo.jsonc`.
