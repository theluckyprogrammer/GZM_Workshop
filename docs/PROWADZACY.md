# Warsztat „AI + otwarte dane GZM” – instrukcja prowadzącego

30 minut, 15 par, 28.10.2026. Pary dostają gotowe prompty (`scenarios/sNN/prompty.md`) i budują całą aplikację w 4 krokach: dane w bazie → wyszukiwarka → strona HTMX z kafelkiem → logowanie i ulubione. Po każdym kroku jest pauza: patrzą, co powstało, i wybierają wariant dalszej budowy (A/B/C). Nikt nie jest sprawdzany.

Materiały: karta pary `print/karta-pary-sNN.html` (druk), ściąga `print/sciaga-prowadzacego.html` (tylko dla Ciebie), przy każdym scenariuszu `scenarios/sNN/prowadzacy.md` (wartości dla wariantów A i pułapki w danych).

## 1. Dzień przed – admin szkolny (uprawnienia administratora)

1. .NET 10 SDK: https://dotnet.microsoft.com/download/dotnet/10.0
2. SQL Server Express LocalDB: instalator `SqlLocalDB.msi` (z instalatora SQL Server Express, opcja „LocalDB”).
3. VS Code – User Installer: https://code.visualstudio.com/Download (instaluje się na koncie ucznia, bez admina).
4. Git dla Windows: https://git-scm.com/download/win

## 2. Dzień przed – każde konto ucznia (bez uprawnień administratora)

1. Rozszerzenia VS Code:
   ```
   code --install-extension ms-dotnettools.csharp
   code --install-extension ms-dotnettools.csdevkit
   code --install-extension ms-mssql.mssql
   code --install-extension kilocode.kilo-code
   ```
2. Klon repozytorium (z gałęziami ratunkowymi):
   ```
   git clone https://github.com/theluckyprogrammer/GZM_Workshop.git
   cd GZM_Workshop
   ```
3. `kilo.jsonc`, `.kilo/` i `.kilocode/` są w repo – sklonuj PRZED pierwszym otwarciem folderu w VS Code, bo Kilo zapamiętuje konfigurację przy pierwszym otwarciu.
4. Pakiety NuGet do cache (z internetem): `dotnet restore src/GzmApp`, potem `dotnet build src/GzmApp`.
5. Pierwsze uruchomienie (tworzy bazę `GzmWorkshop` w LocalDB i tabele logowania): `dotnet run --project src/GzmApp`, zamknij Ctrl+C. Jeśli LocalDB nie wstaje: `sqllocaldb create MSSQLLocalDB -s`.
6. Sprawdzenie komputera: `tools\check.cmd` (dwuklik). Wszystkie punkty OK; SOL-6 może być POMINIĘTO.
7. Przydział scenariusza: wydrukowana karta `print/karta-pary-sNN.html` leży przy stanowisku. Para sama wpisze swój scenariusz w kroku 1 (prompt ustawia `"Scenario"` w `src/GzmApp/appsettings.json`).

### Checklista stanowisk

| Stan. | Scen. | SDK | LocalDB | Rozszerzenia | Klon | restore + build | 1. uruchomienie | check.cmd OK | Klucz Kilo | Karta |
|---|---|---|---|---|---|---|---|---|---|---|
| 1 | s01 | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 2 | s02 | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 3 | s03 | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 4 | s04 | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 5 | s05 | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 6 | s06 | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 7 | s07 | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 8 | s08 | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 9 | s09 | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 10 | s10 | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 11 | s11 | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 12 | s12 | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 13 | s13 | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 14 | s14 | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 15 | s15 | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |

## 3. Przebieg minuta po minucie

| Minuty | Co się dzieje |
|---|---|
| 0–2 | Otwarcie (tekst niżej). Pary wpisują imiona na karcie i otwierają `scenarios/sNN/prompty.md`. |
| 2–8 | Krok 1 – dane z pliku do bazy. Pauza: tabela w rozszerzeniu mssql; decyzja 1 (po czym szukać). |
| 8–14 | Krok 2 – wyszukiwarka (endpoint). Pauza: fragment HTML w zakładce Network; decyzje 2 i 3. |
| 14 | Decyzja ratunkowa: pary bez kroku 2 → `tools\ratunek.ps1 -Etap 2 -Scenariusz sNN`. |
| 14–20 | Krok 3 – strona z wyszukiwarką i kafelkiem (HTMX). Pauza: żądania przy pisaniu; decyzja 4. |
| 20 | Decyzja: pary bez kroku 3 → `tools\ratunek.ps1 -Etap 3 -Scenariusz sNN`. |
| 20–26 | Krok 4 – logowanie i ulubione albo demonstracja prowadzącego na rzutniku. |
| 26–30 | Zamknięcie (tekst niżej). |

**Wariant skrócony** (opóźnienie ponad 4 minuty): krok 4 tylko jako demo na rzutniku – stanowisko prowadzącego po `tools\ratunek.ps1 -Etap 4 -Scenariusz s02`.

`ratunek.ps1` ustawia gotowy etap z wariantami A. Zmiany pary idą do schowka (`git stash list`), a para kontynuuje od następnego kroku w `prompty.md`. Uruchamiaj w terminalu VS Code: `powershell -ExecutionPolicy Bypass -File tools\ratunek.ps1 -Etap 3 -Scenariusz s06`.

## 4. Teksty

**Otwarcie (do 60 s):** „Dzisiaj w 30 minut postawicie całą aplikację internetową na prawdziwych otwartych danych z Metropolii GZM – od pliku CSV, przez bazę danych, po stronę z wyszukiwarką i logowaniem. Kod napisze AI – Kilo Code. Wy jesteście szefami projektu: wklejacie gotowe prompty, a w pauzach decydujecie, jak ma wyglądać Wasza aplikacja – po czym szukać, co pokazać, jaką statystykę. Nie ma złych decyzji i nikt Was nie sprawdza. Pilot wkleja prompt i akceptuje zmiany, Nawigator prowadzi pauzy. Po każdym kroku zamieniacie się. Każda para ma inny zbiór danych – na koniec zobaczymy 15 różnych aplikacji.”

**Zamknięcie (do 90 s):** „Zbudowaliście aplikację, która ma wszystkie warstwy: dane z pliku w bazie SQL, zapytanie z parametrem, fragment HTML wysyłany przez serwer, stronę, która odświeża się bez przeładowania, logowanie i ulubione zapisane dla każdego użytkownika. AI napisało kod, ale to Wy zdecydowaliście, co ta aplikacja robi. Dane pochodzą z portalu otwartedane.metropoliagzm.pl na licencji CC BY 4.0 – dlatego w stopce jest źródło. Pytanie do sali: co zbudowaliście i co dodalibyście jako następne? Repozytorium zostaje – w `prompty.md` na końcu są pomysły, co dodać samodzielnie.”

## 5. Problem → objaw → szybkie rozwiązanie

| Problem | Objaw | Szybkie rozwiązanie |
|---|---|---|
| Port zajęty | „address already in use”, przeglądarka pokazuje starą wersję | Shift+F5 w VS Code; w terminalu zamknij `dotnet`; `tools\check.cmd` pokaże, czy port 5080 jest wolny. |
| LocalDB nie startuje | Błąd połączenia przy starcie aplikacji | `sqllocaldb start MSSQLLocalDB`; jeśli nie istnieje: `sqllocaldb create MSSQLLocalDB -s`. |
| Kilo nie odpowiada / limit klucza | Brak odpowiedzi, komunikat o limicie | Sprawdź klucz pary w ustawieniach Kilo; zapasowy klucz z puli prowadzącego; w ostateczności `ratunek.ps1` do kolejnego etapu. |
| AI dodało React, npm albo EF Core | Nowe pliki `package.json`, `DbContext` w danych | Odrzuć zmiany w Kilo (Reject) albo `git checkout -- .` i wklej prompt ponownie. Reguły w `.kilo/rules/stack.md` tego zabraniają. |
| Zła liczba wierszy | W mssql inna liczba niż w `scenarios/sNN/prowadzacy.md` | Zajrzyj do „Pułapki w danych” w `prowadzacy.md` (kodowanie, separator, nagłówek); jeśli brak czasu – `ratunek.ps1 -Etap 1`. |
| 400 przy ulubionych | ★ nie działa, Network: status 400 | Brak tokenu antiforgery – sprawdź, czy `<body hx-headers:inherited=...>` w `_Layout.cshtml` nie zostało zmienione; nie wyłączaj walidacji. |
| Brak żądań w zakładce Network | Lista się nie zmienia przy pisaniu | Odśwież z otwartym F12; sprawdź `hx-get`, `hx-trigger` i `hx-target` w `Index.cshtml` oraz czy strona ładuje `/lib/htmx/4.0.0/htmx.min.js`. |
| Para nie wkleiła decyzji | W prompcie zostało `[DECYZJA n]` | Nic nie rób: Kilo użyje wariantu A (jest wpisany w prompcie). |
| Para skończyła wcześniej | – | „Co dalej?” na końcu `prompty.md` (drugi kafelek, sortowanie, „Pokaż więcej”, opis rekordu przez SOL-6). |

## 6. Klucze

- **Kilo:** klucz na parę, z limitem i wygaśnięciem 28.10.2026 23:59. Wpisuje prowadzący w ustawieniach rozszerzenia Kilo Code – uczniowie go nie widzą i nie przepisują.
- **SOL-6:** wpisuje prowadzący na stanowisku: `dotnet user-secrets set "Sol6:ApiKey" "<klucz>" --project src/GzmApp`. Adres i model: sekcja `Sol6` w `src/GzmApp/appsettings.json` (`Endpoint`, `Model`, `Kind`: `openai` albo `azure`). Bez klucza aplikacja pokazuje „Brak klucza SOL-6 – poproś prowadzącego”.
- Karty z kluczami zbierasz po zajęciach. Po warsztacie wszystkie klucze rotujesz (Kilo i SOL-6). Klucze nigdy nie trafiają do repozytorium.

## 7. Atrybucja CC BY 4.0

- Każdy zbiór pochodzi z otwartedane.metropoliagzm.pl; licencja i wymagana atrybucja są w `data/metadata.json` i w `scenarios/sNN/mapping.json` (`attribution`).
- W aplikacji atrybucja jest w stopce każdej strony: para wpisuje ją w kroku 3 (`"DataAttribution"` w `src/GzmApp/appsettings.json`); gałęzie ratunkowe biorą ją z `mapping.json`.
- Co powiedzieć: „Te dane są otwarte, ale nie anonimowe – autor zbioru chce, żeby było widać, skąd pochodzą. Licencja CC BY 4.0 pozwala z nich korzystać, także w aplikacjach, pod warunkiem podania źródła. Dlatego w stopce jest «Źródło: …».”
