# Warsztat „AI + otwarte dane GZM” – instrukcja prowadzącego

30 minut, 15 par, 28.10.2026. Każda para **losuje** swój scenariusz (wylosowany opis scenariusza `print/opis-scenariusza-sNN.html`), czyta `scenarios/sNN/karta.md` (o czym jest jej portal) i buduje go promptami z tej samej karty (sekcja „Prompty – krok po kroku”): krok 0 tworzy branch pary, szkielet aplikacji i pobiera dane z portalu GZM, potem 4 kroki – dane w bazie → wyszukiwarka → strona HTMX z kafelkiem → analiza AI (model GPT-6 Luna). Logowanie i ulubione są w „Co dalej?” dla szybkich par. Po każdym kroku jest pauza: patrzą, co powstało (poradnik `docs/PORADNIK-UCZNIA.md` prowadzi krok po kroku: F12, debugger, baza), a o kolejne decyzje **pytają Kilo** – AI proponuje 3 warianty na podstawie ich danych i zapisuje wybory w `LOG-DECYZJI.md`. Nikt nie jest sprawdzany.

Metoda pracy: **najpierw specyfikacja, potem testy, na końcu kod**. Każdy krok ma swój plik specyfikacji w `scenarios/sNN/spec/` (krok 0 też: `sNN-000-dane.md` sprawdza pobrany plik) z ponumerowanymi zdaniami „Reguły”. Każde zdanie to jeden test w `tests/GzmApp.Specs` (ikona kolby w VS Code; po kroku 0), a nazwa testu to to zdanie. W kolbie są tylko testy scenariusza pary (`szkielet/` jest ukryty w VS Code). Testy kroku 0 są zielone od początku; reguły kroków 1–4 są czerwone, dopóki para ich nie zbuduje. Decyzja to wybrane zdanie z propozycji Kilo wklejone do specyfikacji (zdanie domyślne jest już wpisane). Testy generuje `tools/Spectree.Generator`, a definicje kroków są w `szkielet/tests/GzmApp.Specs/Steps`. Jak to działa i jak zmienić zdanie: `docs/SPECTREE.md`.

Repozytorium startowe jest czyste: nie ma w nim `src/`, `tests/` ani danych – wszystko materializuje się w kroku 0 ze szablonu `szkielet/` (skrypt `tools/nowy-portal.ps1`). Po skopiowaniu skrypt usuwa `szkielet/` z branchu pary (na `main` zostaje; `-OdNowa` przywraca go z `main`), więc `tools/check.cmd` uruchamiaj na `main`, przed krokiem 0. Scenariusz pary wpisuje skrypt, uczniowie nigdy nie edytują `appsettings.json` w tym celu.

Materiały: opis scenariusza `print/opis-scenariusza-sNN.html` (druk, do losowania: co robi aplikacja i w jaką stronę można ją rozwijać; jest na nim też gotowe logowanie do portalu – `adm` / `TestPass123!`, konto tworzy się samo w kroku 0 – to samo jest w `karta.md`), ściąga `print/sciaga-prowadzacego.html` (tylko dla Ciebie), instrukcja stanowiska `print/instrukcja-stanowiska.html` (druk: rozszerzenia VS Code i klucz Kilo, ze zrzutami ekranu), przy każdym scenariuszu `scenarios/sNN/prowadzacy.md` (wartości dla zdań domyślnych i pułapki w danych).

## 1. Dzień przed – admin szkolny (uprawnienia administratora)

1. .NET 10 SDK: https://dotnet.microsoft.com/download/dotnet/10.0
2. VS Code – User Installer: https://code.visualstudio.com/Download (instaluje się na koncie ucznia, bez admina).
3. Git dla Windows: https://git-scm.com/download/win

Bazy danych nie instalujesz: aplikacja używa SQLite (plik `gzm.db`, baza wchodzi z pakietem NuGet).

## 2. Dzień przed – każde konto ucznia (bez uprawnień administratora)

Punkt 1 (rozszerzenia) i klucz Kilo (rozdział 6) krok po kroku, ze zrzutami ekranu: `print/instrukcja-stanowiska.html` – pojedyncza strona do wydruku, do położenia przy stanowiskach albo do przekazania osobie, która je przygotowuje.

1. Rozszerzenia VS Code:
   ```
   code --install-extension ms-dotnettools.csharp
   code --install-extension ms-dotnettools.csdevkit
   code --install-extension qwtel.sqlite-viewer
   code --install-extension kilocode.kilo-code
   ```
2. Klon repozytorium:
   ```
   git clone https://github.com/theluckyprogrammer/GZM_Workshop.git
   cd GZM_Workshop
   ```
3. `kilo.jsonc`, `.kilo/` i `.kilocode/` są w repo – sklonuj PRZED pierwszym otwarciem folderu w VS Code, bo Kilo zapamiętuje konfigurację przy pierwszym otwarciu.
4. Pakiety NuGet do cache (z internetem): `dotnet restore szkielet/src/GzmApp` i `dotnet restore szkielet/tests/GzmApp.Specs`, potem `dotnet build szkielet/src/GzmApp` i `dotnet build szkielet/tests/GzmApp.Specs` (w `szkielet/` nie ma testów – testy scenariusza powstają w kroku 0).
5. Klucz do modelu AI: skopiuj plik `appsettings.Local.json` (z pendrive'a prowadzącego, nie z repozytorium) do `szkielet\src\GzmApp\` – krok 0 przeniesie go do aplikacji. Plik jest w `.gitignore` – `git status` go nie pokazuje i nie trafi do commita.
6. Sprawdzenie komputera: `tools\check.cmd` (dwuklik). Wszystkie punkty OK – także „Model AI (gpt-6-luna): jedno krótkie wywołanie” (krok 4 bez niego nie zadziała).
7. Kartki do losowania: wydrukuj 15 opisów scenariuszy `print/opis-scenariusza-sNN.html` (każdy: co robi aplikacja i w jaką stronę można ją rozwijać) i potasuj – każda para losuje jeden na początku warsztatu. Do każdego stanowiska dołóż wydruk `print/instrukcja-stanowiska.html`.
8. (Opcjonalnie, gdy sieć szkolna bywa słaba) Zapas danych: pobierz 15 plików z `data/zbiory.json` (pole `fileUrl`) na pendrive. Gdy pobieranie w kroku 0 zawiedzie, połóż plik w `data/raw/` i poproś parę o ponowne uruchomienie skryptu – pominie pobieranie.

### Checklista stanowisk

| Stan. | Scen. | SDK | Baza SQLite | Rozszerzenia | Klon | restore + build szkieletu | check.cmd OK | Klucz Kilo | Plik klucza AI (szkielet/src) | Karta do losowania |
|---|---|---|---|---|---|---|---|---|---|---|
| 1 | los. | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 2 | los. | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 3 | los. | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 4 | los. | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 5 | los. | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 6 | los. | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 7 | los. | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 8 | los. | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 9 | los. | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 10 | los. | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 11 | los. | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 12 | los. | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 13 | los. | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 14 | los. | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 15 | los. | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |

## 3. Przebieg minuta po minucie

| Minuty | Co się dzieje |
|---|---|
| 0–2 | Otwarcie (tekst niżej). **Losowanie kart** – każda para bierze jedną, znajduje swój folder `scenarios/sNN/` i czyta `karta.md`. |
| 2–6 | Krok 0 – pierwszy prompt: skrypt tworzy szkielet aplikacji i pobiera dane z portalu GZM; Kilo buduje i uruchamia testy. Pauza: para sprawdza plik danych, F5, testy (zielone: kroku 0; czerwone: kroki 1–4 – lista zadań), bazę w SQLite Viewer. |
| 6–11 | Krok 1 – dane z pliku do bazy. Pauza: tabela w rozszerzeniu SQLite Viewer, zielone testy kroku 1, pierwsza pułapka w debuggerze; decyzja 1 (pytają Kilo o propozycje). |
| 11–16 | Krok 2 – wyszukiwarka (endpoint). Pauza: F12 → Network/Elements, debugger na `OnGetSzukaj`; decyzje 2 i 3 (Kilo proponuje). |
| 16–21 | Krok 3 – strona z wyszukiwarką i kafelkiem (HTMX). Pauza: żądania przy pisaniu, tryb telefonu; decyzja 4 (co ma przeanalizować AI – Kilo proponuje na podstawie danych i wcześniejszych decyzji). |
| 21–26 | Krok 4 – analiza AI: przycisk „✨ Analizuj z AI”, pauza: karta z napisem „wygenerowała sztuczna inteligencja”, „Co poszło do modelu?”, log `AI (gpt-6-luna) … tokeny` w terminalu. Albo demonstracja prowadzącego na rzutniku. |
| 26–30 | Zamknięcie (tekst niżej). Szybkie pary: „Co dalej?” w `karta.md`. |

**Wariant skrócony** (opóźnienie ponad 4 minuty): krok 4 tylko jako demo na rzutniku – przygotuj sobie wcześniej na stanowisku demonstracyjnym własny portal (przejdź prompty dowolnego scenariusza) albo pokaż portal pary, która skończyła.

**Para, która nie nadąża:** nie ma gałęzi ratunkowych – pomóż bezpośrednio (najczęstsze przyczyny: port zajęty, klucz Kilo – tabela niżej), a w ostateczności para kontynuuje od kroku, na którym jest, i ogląda dalsze kroki na rzutniku. Każdy krok jest samodzielny: nawet aplikacja po kroku 2 działa i pokazuje listę wyników.

## 4. Teksty

**Otwarcie (do 60 s):** „Dzisiaj w 30 minut postawicie całą aplikację internetową na prawdziwych otwartych danych z Metropolii GZM – od pliku CSV pobranego prosto z portalu, przez bazę danych, po stronę z wyszukiwarką i analizę danych przez sztuczną inteligencję. Kod napisze AI – Kilo Code. Wy jesteście szefami projektu i pracujecie jak zawodowcy: najpierw specyfikacja – zdania, co aplikacja ma robić – potem testy, które to sprawdzają, na końcu kod. Każde zdanie specyfikacji to test: na początku te od funkcji są czerwone, a Waszym zadaniem jest zamienić je w zielone. W pauzach pytacie Kilo, jak ma wyglądać Wasza aplikacja – po czym szukać, co pokazać, jaką statystykę, co ma przeanalizować AI – i wybieracie z jego propozycji. Każda decyzja ląduje w Waszym logu decyzji. Nie ma złych decyzji i nikt Was nie sprawdza. Pilot wkleja prompt i akceptuje zmiany, Nawigator prowadzi pauzy. Po każdym kroku zamieniacie się. Każda para losuje inny zbiór danych – na koniec zobaczymy 15 różnych aplikacji.”

**Zamknięcie (do 90 s):** „Zbudowaliście aplikację, która ma wszystkie warstwy: dane z portalu GZM w bazie SQL, zapytanie z parametrem, fragment HTML wysyłany przez serwer, stronę, która odświeża się bez przeładowania, i analizę Waszych danych przez model AI. AI napisało kod i analizę, ale to Wy zdecydowaliście, co ta aplikacja robi – Wasz log decyzji to pokazuje – i Wy sprawdzacie, czy AI się nie myli. Dane pochodzą z portalu otwartedane.metropoliagzm.pl na licencji CC BY 4.0 – dlatego w stopce jest źródło. Pytanie do sali: co zbudowaliście i co dodalibyście jako następne? Repozytorium zostaje – na końcu `karta.md` są pomysły, co dodać samodzielnie.”

## 5. Problem → objaw → szybkie rozwiązanie

| Problem | Objaw | Szybkie rozwiązanie |
|---|---|---|
| Skrypt kroku 0 nie może pobrać danych | Komunikat o pobieraniu / przekroczonym czasie | Sieć szkolna: połóż plik z pendrive'a w `data/raw/` (nazwa z `data/zbiory.json`) i uruchom skrypt ponownie – pominie pobieranie. |
| Skrypt kroku 0 odmawia | „Portal już istnieje (src/GzmApp)” | Portal już stoi – to normalne po pierwszym uruchomieniu. Od zera: `tools\nowy-portal.ps1 -Scenariusz sNN -OdNowa` (kasuje dotychczasowy kod pary). |
| Port zajęty | „address already in use”, przeglądarka pokazuje starą wersję | Shift+F5 w VS Code; w terminalu zamknij `dotnet`; `tools\check.cmd` pokaże, czy port 5080 jest wolny. |
| Baza uszkodzona albo zablokowana | Błąd bazy przy starcie aplikacji | Zamknij drugą kopię aplikacji (Shift+F5). W ostateczności skasuj `src/GzmApp/gzm.db` – przy następnym starcie aplikacja utworzy plik i zaimportuje dane od nowa. |
| Kilo nie odpowiada / limit klucza | Brak odpowiedzi, komunikat o limicie | Sprawdź klucz pary w ustawieniach Kilo; podmień na zapasowy klucz z puli prowadzącego. |
| AI dodało React, npm albo EF Core | Nowe pliki `package.json`, `DbContext` w danych | Odrzuć zmiany w Kilo (Reject) albo `git checkout -- .` i wklej prompt ponownie. Reguły w `.kilo/rules/stack.md` tego zabraniają. |
| Kilo proponuje decyzję poza wzorcami | Zdanie nie pasuje do wzorców z pauzy | Powiedz parze: „poproś Kilo, żeby użyło tylko wzorców z bloku” – wzorce są w prompcie pauzy i każdy ma swój test. |
| Zła liczba wierszy | W SQLite Viewer inna liczba niż w `scenarios/sNN/prowadzacy.md` | Zajrzyj do „Pułapki w danych” w `prowadzacy.md` (kodowanie, separator, nagłówek). |
| „Brak klucza do modelu AI” w karcie | Karta pokazuje „Brak klucza do modelu AI – poproś prowadzącego.” | Brak `appsettings.Local.json` w aplikacji – skopiuj plik z pendrive'a do `src\GzmApp\` i uruchom aplikację ponownie (F5). Reguła 9 kroku 4 (prawdziwy model) jest wtedy czerwona; reguły 1–8 przechodzą i bez niego (udawany model). |
| „Model AI nie odpowiedział” | Karta z ostrzeżeniem po ok. 45 s | Sieć szkolna albo limit wdrożenia – kliknij jeszcze raz; terminal (kategoria `AI`) pokazuje typ błędu. W ostateczności demo na rzutniku. |
| Odpowiedź AI z dziwnymi znaczkami | `**`, `#` albo znaczniki HTML w karcie | Model czasem pisze Markdownem – to tylko wygląd. Znaczniki HTML pokazane jako tekst to dobrze (reguła 7: bez `Html.Raw`). |
| 400 przy ulubionych (Co dalej) | ★ nie działa, Network: status 400 | Brak tokenu antiforgery – sprawdź, czy `<body hx-headers:inherited=...>` w `_Layout.cshtml` nie zostało zmienione; nie wyłączaj walidacji. |
| Brak żądań w zakładce Network | Lista się nie zmienia przy pisaniu | Odśwież z otwartym F12; sprawdź `hx-get`, `hx-trigger` i `hx-target` w `Index.cshtml` oraz czy strona ładuje `/lib/htmx/4.0.0/htmx.min.js`. |
| Para nie wkleiła decyzji | W specyfikacji jest zdanie domyślne | Nic nie rób: zdanie domyślne jest wpisane w `scenarios/sNN/spec/` i ma swoje testy. |
| Nie widać testów | Testing (kolba) pusty | Testy pojawiają się po kroku 0 (projekt `tests/GzmApp.Specs`). Potem: Ctrl+Shift+P → „.NET: Build” i ⟳ w panelu Testing. |
| Czerwony test | ✗ przy zdaniu „Reguła N: …” | Kliknij test: komunikat po polsku mówi, czego brakuje (np. „Tabela … jeszcze nie istnieje – tworzy ją krok 1”). Wklej komunikat do Kilo: „Ten test nie przechodzi: …”. Testów kolejnych kroków nie naprawiamy wcześniej. |
| Kilo zmienia test albo specyfikację | Kilo chce edytować `tests/` albo `scenarios/` | Zablokowane w `kilo.jsonc`. Powiedz parze: test mówi prawdę, poprawiamy kod. |
| Para skończyła wcześniej | – | „Co dalej?” na końcu `karta.md` (drugi kafelek – inne propozycje decyzji 3, drugie zadanie dla AI, sortowanie, „Pokaż więcej”, logowanie i ulubione – specyfikacja w `spec/dodatkowe/`). |

## 6. Klucze

- **Kilo:** klucz na parę, z limitem i wygaśnięciem 28.10.2026 23:59. Wpisuje prowadzący w ustawieniach rozszerzenia Kilo Code (jak to kliknąć: `print/instrukcja-stanowiska.html`) – uczniowie go nie widzą i nie przepisują.
- **Model AI (Azure AI Foundry, `gpt-6-luna`; w kodzie i konfiguracji nazywa się `Sol6`):** plik `appsettings.Local.json` z `Sol6:Endpoint` (`https://<zasób>.services.ai.azure.com/openai/v1/`) i `Sol6:ApiKey`. Nazwa modelu jest w `appsettings.json` (`Sol6:Model`). Przed warsztatem plik leży w `szkielet\src\GzmApp\` – krok 0 kopiuje go do aplikacji. Zamiast pliku działają też zmienne `SOL6__ENDPOINT` i `SOL6__APIKEY`. Bez klucza karta AI pokazuje „Brak klucza do modelu AI – poproś prowadzącego.”, a reguła 9 kroku 4 (prawdziwy model) jest czerwona. Zamiana na `gpt-6.1-sol`: `Sol6:Model` w `appsettings.json` – ok. 3 razy wolniejszy (6–10 s zamiast 2–3 s), bogatsze odpowiedzi.
- Jedno kliknięcie „✨ Analizuj z AI” to ok. 2 000 tokenów wejścia i 300 wyjścia (5–6 s). 15 par × kilka kliknięć to kilkaset tysięcy tokenów – ustaw limit na wdrożeniu.
- Karty z kluczami zbierasz po zajęciach. Po warsztacie wszystkie klucze rotujesz (Kilo i model AI) – klucz do modelu AI leży jawnie na 15 komputerach uczniów. Klucze nigdy nie trafiają do repozytorium.

## 7. Atrybucja CC BY 4.0

- Każdy zbiór pochodzi z otwartedane.metropoliagzm.pl; licencja i wymagana atrybucja są w `data/zbiory.json` (z URL-em pobierania i sumą SHA-256), `data/metadata.json` i w `scenarios/sNN/mapping.json` (`attribution`).
- W aplikacji atrybucja jest w stopce każdej strony: para wpisuje ją w kroku 3 (`"DataAttribution"` w `src/GzmApp/appsettings.json`); gałęzie ratunkowe biorą ją z `mapping.json`.
- Co powiedzieć: „Te dane są otwarte, ale nie anonimowe – autor zbioru chce, żeby było widać, skąd pochodzą. Licencja CC BY 4.0 pozwala z nich korzystać, także w aplikacjach, pod warunkiem podania źródła. Dlatego w stopce jest «Źródło: …».”
