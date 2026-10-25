---
capability: s14-002-szukaj-rekordow
module: BiletBot
state: draft
---

# Krok 2 – wyszukiwarka

## Co się dzieje

Użytkownik wpisuje frazę. Adres `/?handler=Szukaj&q=<fraza>` odsyła fragment HTML: licznik wyników i listę pasujących rekordów z tabeli `TicketMachines`. Każdy wynik to numer wiersza i tytuł `StopName`.

## Reguły

<!-- DECYZJA 1: wariant A. Inny wariant z prompty.md wklejcie w miejsce zdania po „1. ”. -->
1. Wyszukiwarka szuka frazy w dowolnym miejscu pola `StopName` (kolumna w pliku: `Nazwa przystanku`).
2. Wielkość liter i polskie znaki we frazie nie mają znaczenia.
3. Licznik pokazuje liczbę wszystkich trafień, a lista najwyżej 20 rekordów.
4. Pusta fraza pokazuje pierwsze 20 rekordów według numeru wiersza.
5. Fraza `' OR 1=1 --` nie znajduje żadnego rekordu, bo trafia do zapytania jako parametr.
6. Fraza dłuższa niż 100 znaków dostaje krótki komunikat ze statusem 400.

## Czego się nauczyliśmy

<!-- Po kroku dopiszcie jedno zdanie: co Was zaskoczyło w danych albo w kodzie. -->
