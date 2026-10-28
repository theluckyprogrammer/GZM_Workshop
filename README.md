# Warsztat „AI + otwarte dane GZM”

W tym repozytorium zbudujecie (para: **Pilot + Nawigator**) własny portal na prawdziwych
otwartych danych Metropolii GZM. Kod pisze Kilo Code (AI) – Wy nim sterujecie.

## Jak zacząć (na warsztacie)

1. **Losujecie scenariusz** – karta od prowadzącego mówi, który to scenariusz, np. `s05`.
2. Otwieracie folder `scenarios/s05/` i czytacie **`karta.md`** – dowiadujecie się, o czym jest Wasz portal (są tam też prompty krok po kroku).
3. Zaczynacie od **kroku 0** w `karta.md`: pierwszy prompt tworzy szkielet aplikacji
   i pobiera dane z portalu GZM. Potem budujecie portal krok po kroku: dane w bazie → wyszukiwarka →
   strona → analiza AI.
4. W każdym kroku: wklejacie prompt, uruchamiacie aplikację (F5), oglądacie efekt i przechodzicie dalej. Na koniec pokazujecie gotowy portal z wyszukiwarką, statystyką i analizą AI. Do logowania rejestrujecie własny adres e-mail i hasło.

Repozytorium na starcie jest czyste: `src/` i `tests/` powstają dopiero w kroku 0 (ze `szkielet/`),
a dane pobiera się prosto z otwartedane.metropoliagzm.pl.

## Gdzie co jest

| Miejsce | Co tam jest |
|---|---|
| `scenarios/sNN/` | materiały scenariusza: `karta.md` (opis portalu, zasady pracy i prompty), `spec/` (specyfikacja kroków), `checkpoints.md` i `mapping.json` (dane dla testów). |
| `docs/PORADNIK-UCZNIA.md` | poradnik pary: uruchamianie (F5 / `dotnet run`), testy (ikona kolby / `dotnet test`), F12 w Edge, debugger VS Code, baza w SQLite Viewer. |
| `szkielet/` | szablon aplikacji i testów – krokiem 0 skrypt robi z niego Wasz portal. |
| `data/` | tu wyląduje pobrany plik CSV (`data/raw/`). |
| `LOG-DECYZJI.md` | log decyzji pary – prowadzi go Kilo (powstaje w kroku 0). |
| `tools/` | narzędzia warsztatu (m.in. `nowy-portal.ps1` z kroku 0) – nie zmieniacie. |

Materiały prowadzącego (wydruki, notatki, narzędzia do generowania kart i danych) są na gałęzi `organizacyjne`.

Licencja danych: CC BY 4.0, źródło: [Portal Otwartych Danych GZM](https://otwartedane.metropoliagzm.pl) –
każdy portal pokazuje atrybucję w stopce.
