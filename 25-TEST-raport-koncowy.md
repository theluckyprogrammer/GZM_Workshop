# 25 – Testy: raport końcowy

Przeczytaj `zadanie/prompts/00-KONTEKST-wspolny.md` (Oszczędzanie tokenów, Status).

- **Wymaga:** 24 OK.
- **Sesja:** można kontynuować po 24.

## Zadanie
1. Uruchom cały zestaw testów raz na `ratunek/etap-4`, bez Playwright i bez prawdziwego SOL-6. Logger minimalny.
2. Napisz `docs/RAPORT-TESTOW.md` (najwyżej 40 linii):
   - tabela: poziom × status (z `docs/TEST-STATUS.md`),
   - lista poprawek: plik + 1 zdanie,
   - lista zmian w promptach uczniów,
   - top 5 pozycji z `findings.md`,
   - czasy etapów z `docs/WALIDACJA.md` i ocena, czy plan 6 min/etap jest realny,
   - czynności do wykonania ręcznie przed 28.10.2026, co najmniej:
     - `check.cmd` na fizycznym komputerze szkolnym,
     - test reguł Kilo z `docs/KILO-TEST.md`,
     - test kluczy przy 15 równoczesnych sesjach,
     - wydruk kart.
3. Sprawdź, że `main` jest czysty i gotowy do sklonowania przez uczniów:
   - brak plików testowych baz,
   - brak sekretów,
   - `git status` czysty.

## Kryterium ukończenia
- `docs/RAPORT-TESTOW.md` istnieje.
- Pełny zestaw testów zielony albo każdy czerwony test opisany w raporcie.

## Na koniec
Commit i wiersz w `docs/STATUS.md`: „25: raport końcowy”. W odpowiedzi wypisz treść sekcji „czynności ręczne” z raportu.
