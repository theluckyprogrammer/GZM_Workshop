# {{app}} ({{code}}) – materiał dla prowadzącego

Nie rozdawaj uczniom. Uczniowie mają `karta.md` i `prompty.md`.

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
