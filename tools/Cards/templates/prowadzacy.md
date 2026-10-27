# {{app}} ({{code}}) – materiał dla prowadzącego

Nie rozdawaj uczniom. Uczniowie mają `karta.md` (opis portalu, zasady i prompty) i specyfikację `spec/` (5 plików, po jednym na krok, oraz `spec/dodatkowe/` z ulubionymi do „Co dalej?”).

Para **losuje** scenariusz (wylosowany wydruk `print/opis-scenariusza-{{code}}.html`), czyta `scenarios/{{code}}/karta.md` i zaczyna od kroku 0 w promptych z tej samej karty. Pierwszy prompt uruchamia `tools/nowy-portal.ps1`: tworzy oddzielny branch pary ({{code}}-<losowy numer>, od main) i przechodzi na niego, kopiuje szkielet (`szkielet/` → `src`, `tests`, `db`), wpisuje `"Scenario": "{{code}}"` w `src/GzmApp/appsettings.json` (pary NIE wpisują scenariusza ręcznie), pobiera plik danych z portalu GZM i zakłada `LOG-DECYZJI.md`. Potem Kilo buduje i uruchamia testy.

Decyzje 1–5 nie mają wydrukowanych wariantów: para wkleja Kilo prośbę o propozycje, Kilo proponuje 3 warianty według wzorców zdań (z `karta.md`), bierze pod uwagę dane i `LOG-DECYZJI.md`, pyta parę i czeka. Para wkleja wybrane zdanie do specyfikacji (poza tym plikiem `scenarios/` są dla Kilo zablokowane). Bez decyzji działają zdania domyślne.

- **Pomysł:** {{idea}}
- **Dane:** „{{dataset}}”, plik `{{file}}`, tabela `{{table}}`. Gdy pobieranie w kroku 0 się nie powiedzie (sieć), połóż plik z pendrive'a w `data/raw/` i poproś parę o ponowne uruchomienie skryptu – pominie pobieranie.

## Co powinno wyjść przy zdaniach domyślnych

Zdania domyślne (wpisane w specyfikacji) dają dokładnie te wartości. Przy innych decyzjach par liczby z kroków 2–4 będą inne – to w porządku.

- Krok 0: skrypt pisze „GOTOWE”, testy `{{code}}-000-dane` zielone (plik, liczba wierszy {{cp:E1.fileRows}}, nagłówki).
- Krok 1: wierszy w tabeli {{cp:E1.rows}}; wiersz 7, pole `{{checkField}}`: {{cp:E1.row7.value}}; rekordy z pustym albo nieczytelnym polem: {{cp:E1.weird}}.
- Krok 2: fraza {{cp:E2.phrase}} daje {{cp:E2.count}} wyników:
{{cpList:E2.result}}
- Krok 3: kafelek – {{statDescription}}
{{cpStat}}
- Krok 4: karta „✨ Analizuj z AI” z odpowiedzią modelu AI GPT-6 Luna (treść za każdym razem trochę inna – nie ma wartości kontrolnej). Zadanie zależy od decyzji 4 pary (pytają Kilo o propozycje). Bez pliku `src/GzmApp/appsettings.Local.json` karta pokazuje „Brak klucza do modelu AI – poproś prowadzącego.”.
- Dodatkowo (ulubione): tytuł rekordu z wiersza 7 (np. do sprawdzenia „Moich ulubionych”): {{cp:E4.title}}.

Pełna lista wartości: `checkpoints.md` (generuje `tools/Expect`).

## Pułapki w danych

Co może pójść nie tak w tym zbiorze i jak to szybko zobaczyć.

### Krok 1 – import

{{traps:1}}

### Krok 2 – wyszukiwarka

{{traps:2}}

### Krok 3 – strona i statystyka

{{traps:3}}

### Krok 4 – analiza AI

{{traps:4}}

### Dodatkowo – ulubione

{{traps:5}}
