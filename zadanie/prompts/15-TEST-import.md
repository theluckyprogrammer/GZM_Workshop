# 15 – Testy: import (poziom 2, Spec E1-xx)

Przeczytaj `zadanie/prompts/00-KONTEKST-wspolny.md` (Zasady testów, Oszczędzanie tokenów, Gałęzie).

- **Wymaga:** 14 OK.
- **Sesja:** można kontynuować po 14.

## Zadanie
Gałąź: `ratunek/etap-1`. Testy z `[Trait("Poziom","2")]`, parametryzowane po wszystkich scenariuszach.

Po imporcie sprawdź:
1. `COUNT(*)` = E1.
2. Wartość w wierszu 7 (po `SourceRow`) = checkpoint.
3. Polskie znaki są zachowane: jeden rekord z ogonkami porównany z surowym plikiem.
4. Liczby z przecinkiem dziesiętnym trafiają do bazy jako `decimal` z poprawną wartością.
5. Daty są sparsowane według formatu z `mapping.json`.
6. Puste pola → `NULL`, a nie wyjątek.
7. Idempotencja: drugi import daje tę samą liczbę wierszy.
8. Atomowość: import przerwany w połowie (wyjątek wstrzyknięty przez test double) nie zostawia częściowych danych.

Uruchamiaj: `dotnet test --filter "Poziom=2"` z minimalnym loggerem. Przy błędzie pokaż tylko pierwszy błąd. Poprawki zmerguj w górę (etap-2, etap-3, etap-4).

## Kryterium ukończenia
- Wszystkie scenariusze są zielone.
- `docs/TEST-STATUS.md` zaktualizowany.

## Na koniec
Commit i wiersz w `docs/STATUS.md`: „15: testy importu”.
