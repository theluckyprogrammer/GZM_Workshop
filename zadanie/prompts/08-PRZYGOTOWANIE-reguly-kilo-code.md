# 08 – Przygotowanie: reguły i uprawnienia Kilo Code

Przeczytaj `zadanie/prompts/00-KONTEKST-wspolny.md` (sekcje: Stos, Zasady kodu, Status, Gałęzie).

- **Wymaga:** 07 OK.
- **Sesja:** nowa.

## Zadanie
Pracuj na `main`, potem zmerguj w górę do wszystkich `ratunek/etap-N`.

1. **`kilo.jsonc`** w katalogu głównym repo:
   - `"instructions": [".kilo/rules/*.md"]`.
   - Uprawnienia `edit`: allow, z regułami deny dla:
     - `**/*.csproj`, `**/launch.json`, `**/tasks.json`, `**/launchSettings.json`, `**/Program.cs`,
     - `src/GzmApp/Identity/**`, `global.json`, `kilo.jsonc`, `.kilo/**`, `data/**`, `scenarios/**`.
   - `bash`: `{"*":"deny","dotnet build*":"allow"}`.
   - `webfetch`: deny.
   - Przed zapisem sprawdź aktualny schemat konfiguracji w dokumentacji Kilo (https://kilo.ai/docs) i dopasuj składnię.
2. **`.kilo/rules/stack.md`:**
   - HTMX 4.0.0 (nie 2.x); zakaz React, Vite, npm, fetch+JSON.
   - Dapper + SQL; zakaz EF Core poza Identity.
   - Zakaz zmian logowania, pakietów NuGet, portów, `launch.json`.
   - Zawsze guard clauses, zero zagnieżdżeń, krótkie metody.
   - Nie zmieniaj plików spoza listy z promptu.
   - Odpowiadaj po polsku, prostym językiem.
3. **`.kilo/rules/htmx4.md`:** ściąga różnic HTMX 2 → 4 (z kontekstu wspólnego) z przykładami „źle / dobrze”.
4. **`.kilo/agents/detektyw.md`:** agent oparty na trybie debug.
   - Może edytować tylko metodę wskazaną w prompcie.
   - Przed poprawką musi wyjaśnić przyczynę.
5. Kopie reguł dla zgodności wstecznej: `.kilocode/rules/` oraz `AGENTS.md`.
6. **`docs/KILO-TEST.md`:** instrukcja ręcznego testu na komputerze z Kilo. Prośby „dodaj paczkę X”, „zmień port na 6000” i „zmień logowanie na JWT” mają zostać zablokowane.

## Kryterium ukończenia
- `kilo.jsonc` to poprawny JSONC (sprawdź parserem).
- Wszystkie gałęzie mają identyczne pliki reguł (`git diff main ratunek/etap-4 -- kilo.jsonc .kilo` jest pusty).

## Na koniec
Commit `"08: reguły Kilo"` i wiersz w `docs/STATUS.md`.
