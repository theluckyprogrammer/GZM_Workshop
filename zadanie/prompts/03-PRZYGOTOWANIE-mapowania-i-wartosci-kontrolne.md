# 03 – Przygotowanie: mapowania scenariuszy i wartości kontrolne

Przeczytaj `zadanie/prompts/00-KONTEKST-wspolny.md` (sekcje: Projekt, Zasady kodu, Oszczędzanie tokenów, Status).

- **Wymaga:** 02 OK.
- **Sesja:** nowa.

## Zadanie
1. Dla każdego scenariusza `s01`–`s15` (po ewentualnych zamianach z `docs/ZAMIANY.md`) utwórz `scenarios/sNN/mapping.json`. Źródłem jest `data/metadata.json` i nagłówek pliku (najwyżej 3 wiersze podglądu). Plik zawiera:
   - nazwę tabeli SQL (angielską, np. `BikeStations`),
   - mapowanie kolumny CSV → pole: nazwa, typ SQL, kultura, format daty, czy nullable,
   - klucz naturalny albo informację „RowHash”,
   - pole filtra (tekst),
   - definicję statystyki: `top5` | `sum` | `avg` | `count-by` + pole,
   - pole wyświetlane jako tytuł rekordu (do ulubionych),
   - atrybucję.
2. Napisz narzędzie `tools/Expect` (konsola .NET 10). Dla każdego scenariusza czyta `data/raw` (albo `data/work`, jeśli istnieje) według `mapping.json` i zapisuje `scenarios/sNN/checkpoints.md` z wartościami:
   - **E1:** liczba wierszy; wartość pola X w wierszu 7 (numeracja od 1, bez nagłówka); liczba „dziwnych” rekordów (puste, nieparsowalne) z podziałem na kolumny;
   - **E2:** wynik filtra dla przykładowej frazy (fraza wybrana tak, żeby dawała 2–10 wyników; jedna fraza z polskim znakiem, jeśli to możliwe);
   - **E3:** wynik statystyki;
   - **E4:** tytuł rekordu nr 7 (ten rekord para oznacza jako ulubiony).
3. Wartości mają pochodzić WYŁĄCZNIE z uruchomienia `tools/Expect`, nie z głowy. Zapisz log uruchomienia w `docs/expect-log.txt`.

## Kryterium ukończenia
- 15 plików `mapping.json` i 15 plików `checkpoints.md`.
- Każda wartość ma odpowiednik w `docs/expect-log.txt`.
- Ponowne uruchomienie `tools/Expect` daje identyczne pliki (`git diff` jest pusty).

## Na koniec
Commit `"03: mapowania i wartości kontrolne"` i wiersz w `docs/STATUS.md`.
