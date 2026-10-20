# 06 – Przygotowanie: gałęzie ratunek/etap-2 i ratunek/etap-3 (endpoint + widok HTMX)

Przeczytaj `zadanie/prompts/00-KONTEKST-wspolny.md` (sekcje: Stos, Zasady kodu, Oszczędzanie tokenów, Status, Gałęzie).

- **Wymaga:** 05 OK.
- **Sesja:** można kontynuować po 05.

## Zadanie
1. **`ratunek/etap-2`** (utwórz z `ratunek/etap-1`):
   - `IRecordRepository.Search(string? q, int take)`:
     - parametr `@q`, `LIKE` z collation nieczułą na wielkość liter i ogonki,
     - guard: `q` dłuższe niż 100 znaków → `ArgumentException`, co daje odpowiedź 400.
   - Handler `OnGetSzukaj(string? q)` zwraca `Partial("_Lista")`.
   - Pusta fraza zwraca pierwsze 20 rekordów.
2. **`ratunek/etap-3`** (utwórz z `ratunek/etap-2`):
   - Strona z:
     - polem wyszukiwania: `hx-get="?handler=Szukaj" hx-trigger="keyup changed delay:300ms" hx-target="#wyniki"`,
     - kontenerem `#wyniki`,
     - kafelkiem statystyki zgodnym z definicją z `mapping.json`.
   - Metoda `IsHtmx()`: żądanie HTMX dostaje fragment, zwykłe żądanie dostaje pełną stronę.
   - Błąd serwera w żądaniu HTMX zwraca krótki fragment z komunikatem, a nie stronę błędu (HTMX 4 podmienia 4xx/5xx).
   - Stopka z atrybucją z `mapping.json`.

## Kryterium ukończenia
- Dla każdego scenariusza: wynik filtra = E2, statystyka = E3 z `checkpoints.md`. Sprawdź żądaniami HTTP w skrypcie.
- Odpowiedź z nagłówkiem HTMX nie zawiera `<html>`.
- Fraza `' OR 1=1 --` zwraca 0 wyników.

## Na koniec
Commity na obu gałęziach i wiersz w `docs/STATUS.md` (przenieś go do `main`).
