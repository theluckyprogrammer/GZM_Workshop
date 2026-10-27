# Warsztat „AI + otwarte dane GZM”

W tym repozytorium zbudujecie (para: **Pilot + Nawigator**) własny portal na prawdziwych
otwartych danych Metropolii GZM. Kod pisze Kilo Code (AI) – Wy nim sterujecie.

## Jak zacząć (na warsztacie)

1. **Losujecie scenariusz** – karta od prowadzącego mówi, który to scenariusz, np. `s05`.
2. Otwieracie folder `scenarios/s05/` i czytacie **`karta.md`** – dowiadujecie się, o czym jest Wasz portal.
3. Otwieracie **`prompty.md`** i zaczynacie od **kroku 0**: pierwszy prompt tworzy szkielet aplikacji
   i pobiera dane z portalu GZM. Potem budujecie portal krok po kroku: dane w bazie → wyszukiwarka →
   strona → analiza AI.
4. W pauzach pytacie Kilo o propozycje decyzji – każda decyzja trafia do `LOG-DECYZJI.md`.

Repozytorium na starcie jest czyste: `src/` i `tests/` powstają dopiero w kroku 0 (ze `szkielet/`),
a dane pobiera się prosto z otwartedane.metropoliagzm.pl.

## Gdzie co jest

| Miejsce | Co tam jest |
|---|---|
| `scenarios/sNN/` | materiały scenariusza: `karta.md`, `prompty.md`, `spec/` (specyfikacja kroków), `prowadzacy.md` (tylko dla prowadzącego). |
| `docs/PORADNIK-UCZNIA.md` | poradnik pary: uruchamianie (F5 / `dotnet run`), testy (ikona kolby / `dotnet test`), F12 w Edge, debugger VS Code, baza w mssql. |
| `szkielet/` | szablon aplikacji i testów – krokiem 0 skrypt robi z niego Wasz portal. |
| `data/` | tu wyląduje pobrany plik CSV (`data/raw/`). |
| `LOG-DECYZJI.md` | log decyzji pary – prowadzi go Kilo (powstaje w kroku 0). |
| `tools/` | narzędzia warsztatu (m.in. `nowy-portal.ps1` z kroku 0) – nie zmieniacie. |

Licencja danych: CC BY 4.0, źródło: [Portal Otwartych Danych GZM](https://otwartedane.metropoliagzm.pl) –
każdy portal pokazuje atrybucję w stopce.
