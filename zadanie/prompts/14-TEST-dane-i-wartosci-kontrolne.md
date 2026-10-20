# 14 – Testy: dane i wartości kontrolne (poziom 1, Spec D-xx)

Przeczytaj `zadanie/prompts/00-KONTEKST-wspolny.md` (Zasady testów, Oszczędzanie tokenów).

- **Wymaga:** 13 OK.
- **Sesja:** można kontynuować po 13.

## Zadanie
Gałąź: `main`.

1. Dla każdego scenariusza i każdej rezerwy sprawdź zgodność:
   - SHA-256 i liczba wierszy pliku = `data/metadata.json`,
   - nagłówki w pliku = nagłówki w metadanych = kolumny w `mapping.json`,
   - licencja i atrybucja są niepuste.
2. **Niezależne przeliczenie wartości kontrolnych.**
   - Napisz drugi, minimalny parser, którym jest INNY kod niż `tools/Expect`. Bez CsvHelpera, np. własny split z obsługą cudzysłowów.
   - Porównaj jego wyniki z `checkpoints.md`: E1 (liczba wierszy, wiersz 7, „dziwne” rekordy), E2, E3, E4.
   - Rozbieżność → ustal, który parser się myli, popraw go i wygeneruj `checkpoints.md` ponownie przez `tools/Expect`.
3. Kontrola danych osobowych regexami z promptu 02 na wszystkich plikach. Trafienia → `findings.md` + decyzja: zostaje albo zamiana na rezerwę zgodnie z `docs/ZAMIANY.md`.

## Kryterium ukończenia
- 0 rozbieżności albo każda wyjaśniona i poprawiona.
- `docs/SPEC-TESTY.md` i `docs/TEST-STATUS.md` zaktualizowane.

## Na koniec
Commit i wiersz w `docs/STATUS.md`: „14: testy danych”.
