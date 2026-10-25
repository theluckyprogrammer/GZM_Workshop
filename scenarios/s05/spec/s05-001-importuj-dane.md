---
capability: s05-001-importuj-dane
module: ZgubaDąbrowa
state: draft
---

# Krok 1 – dane z pliku do bazy

## Co się dzieje

Przy starcie aplikacja sprawdza tabelę `DabrowaLostItems`. Gdy tabela jest pusta, aplikacja wczytuje do niej plik „Dąbrowa Górnicza: rzeczy znalezione” (`data/raw/s05_dabrowa-rzeczy-znalezione.csv`). Każdy wiersz pliku to jeden rekord, a jego numer w pliku to `SourceRow`.

## Reguły

<!-- Każde zdanie to jeden test (ikona kolby → GzmApp.Specs). Numerów nie zmieniamy. -->
1. Po starcie aplikacji tabela `DabrowaLostItems` ma tyle wierszy, ile poprawnych rekordów jest w pliku.
2. Wiersz 7 ma w polu `FoundDate` tę samą wartość co w pliku.
3. Polskie litery z pliku są w bazie bez zmian.
4. Liczby i daty z pliku są w bazie z tą samą wartością.
5. Puste albo nieczytelne pola są w bazie jako NULL.
6. Kolejne uruchomienie aplikacji nie zmienia liczby wierszy.

## Czego się nauczyliśmy

<!-- Po kroku dopiszcie jedno zdanie: co Was zaskoczyło w danych albo w kodzie. -->
