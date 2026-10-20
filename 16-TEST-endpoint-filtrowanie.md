# 16 – Testy: endpoint z filtrowaniem (poziom 3, Spec E2-xx)

Przeczytaj `zadanie/prompts/00-KONTEKST-wspolny.md` (Zasady testów, Oszczędzanie tokenów, Gałęzie).

- **Wymaga:** 15 OK.
- **Sesja:** nowa (zalecane).

## Zadanie
Gałąź: `ratunek/etap-2`. WebApplicationFactory, `[Trait("Poziom","3")]`, wszystkie scenariusze.

1. Filtr dla frazy z checkpointu zwraca wynik zgodny z E2.
2. Wielkość liter nie ma znaczenia.
3. Fraza z ogonkami działa. Jeśli zbiór ma taką frazę, sprawdź też wersję bez ogonków i opisz zachowanie w `findings.md`.
4. Pusta fraza zwraca pierwsze 20 rekordów.
5. Fraza `' OR 1=1 --` zwraca 0 wyników, a w logu kategorii `"SQL"` jest parametr `@q`, a nie sklejony tekst.
6. Fraza dłuższa niż 100 znaków → 400.

Poprawki zmerguj w górę.

## Kryterium ukończenia
- Wszystkie scenariusze są zielone.
- `docs/TEST-STATUS.md` zaktualizowany.

## Na koniec
Commit i wiersz w `docs/STATUS.md`: „16: testy endpointu”.
