# {{app}} ({{code}}) – materiał dla prowadzącego

Nie rozdawaj uczniom. Uczniowie mają `karta.md`, `prompty.md` i specyfikację `spec/` (4 pliki, po jednym na krok).

Każde zdanie z sekcji „Reguły” w `spec/` to test w `tests/GzmApp.Specs` (generator `tools/Spectree.Generator`, kroki w `tests/GzmApp.Specs/Steps`). Testy liczą wartości oczekiwane z bazy i z `checkpoints.md`, więc działają dla każdego wariantu decyzji. Testy biorą scenariusz z `"Scenario"` w `src/GzmApp/appsettings.json` (po `ratunek.ps1` – z `appsettings.Development.json`); po zmianie trzeba przebudować projekt.

- **Pomysł:** {{idea}}
- **Dane:** „{{dataset}}”, plik `{{file}}`, tabela `{{table}}`.

## Co powinno wyjść przy wariantach A

Gałęzie `ratunek/etap-1..4` realizują warianty A wszystkich decyzji i dają dokładnie te wartości. Przy innych wariantach (B, C) liczby z kroków 2–4 będą inne – to w porządku.

- Krok 1: wierszy w tabeli {{cp:E1.rows}} (w pliku {{cp:E1.fileRows}}); wiersz 7, pole `{{checkField}}`: {{cp:E1.row7.value}}; rekordy z pustym albo nieczytelnym polem: {{cp:E1.weird}}.
- Krok 2: fraza {{cp:E2.phrase}} daje {{cp:E2.count}} wyników:
{{cpList:E2.result}}
- Krok 3: kafelek – {{statDescription}}
{{cpStat}}
- Krok 4: tytuł rekordu z wiersza 7 (np. do sprawdzenia „Moich ulubionych”): {{cp:E4.title}}.

Pełna lista wartości: `checkpoints.md` (generuje `tools/Expect`).

## Pułapki w danych

Co może pójść nie tak w tym zbiorze i jak to szybko zobaczyć.

### Krok 1 – import

{{traps:1}}

### Krok 2 – wyszukiwarka

{{traps:2}}

### Krok 3 – strona i statystyka

{{traps:3}}

### Krok 4 – ulubione

{{traps:4}}
