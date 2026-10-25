---
capability: s07-004-zapisz-ulubione
module: SportGliwice
state: draft
---

# Krok 4 – logowanie i ulubione

## Co się dzieje

Zalogowany użytkownik oznacza rekordy gwiazdką: ☆ zmienia się w ★. Ulubione w SportGliwice to obiekty, na których para trenuje. Strona „Moje ulubione” pokazuje tylko rekordy zalogowanego użytkownika.

## Reguły

1. Strona „Moje ulubione” wymaga zalogowania.
2. Kliknięcie ☆ bez logowania pokazuje prośbę o zalogowanie.
3. Zalogowany użytkownik klika ☆, gwiazdka zmienia się w ★, a rekord trafia do ulubionych.
4. Dwa kliknięcia tej samej gwiazdki dają jeden wpis w ulubionych.
5. Usunięcie z ulubionych można powtórzyć bez błędu.
6. Użytkownik nie widzi ulubionych innych użytkowników.
7. Żądanie bez tokenu antiforgery zostaje odrzucone.
<!-- DECYZJA 4: wariant A. Inny wariant z prompty.md wklejcie w miejsce zdania po „8. ”. -->
8. Strona „Moje ulubione” pokazuje tytuły ulubionych rekordów.

## Czego się nauczyliśmy

<!-- Po kroku dopiszcie jedno zdanie: co Was zaskoczyło w danych albo w kodzie. -->
