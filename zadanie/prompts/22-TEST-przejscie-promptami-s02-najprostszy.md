# 22 – Testy: przejście promptami ucznia, s02 StacjaFinder (najprostszy) (poziom 7, Spec P-xx)

Przeczytaj `zadanie/prompts/00-KONTEKST-wspolny.md` (Projekt, Stos, Oszczędzanie tokenów, Status).

- **Wymaga:** 21 OK.
- **Sesja:** NOWA.

## Zadanie
1. Wykonaj DOKŁADNIE procedurę z `zadanie/prompts/20-TEST-przejscie-promptami-s06-najtrudniejszy.md`, zastępując `s06` przez `s02` (worktree `../walk-s02`).
2. Po zakończeniu przejrzyj poprawki promptów z kroków 20–22.
   - Jeśli poprawka jest wspólna dla wszystkich scenariuszy (np. dopisek o `hx-headers:inherited`), wprowadź ją w szablonie `tools/Cards/templates/prompty.md` (albo w `tools/Cards/scenarios.json`) i wygeneruj materiały ponownie: `dotnet run --project tools/Cards`. NIE uruchamiaj dla pozostałych scenariuszy pełnego przejścia.
   - Każdą zmianę zapisz w `docs/WALIDACJA.md`.

## Kryterium ukończenia
- s02 przechodzi etapy 1–4.
- Wspólne poprawki naniesione na wszystkie 15 scenariuszy.
- Karty do druku wygenerowane ponownie, jeśli zmieniły się wartości lub treści (skrypt z promptu 10).

## Na koniec
Commit i wiersz w `docs/STATUS.md`: „22: walidacja s02 + propagacja poprawek”.
