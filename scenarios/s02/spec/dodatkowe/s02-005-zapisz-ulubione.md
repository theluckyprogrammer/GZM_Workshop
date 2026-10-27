---
capability: s02-005-zapisz-ulubione
module: StacjaFinder
state: draft
---

# Dodatkowo – logowanie i ulubione

## Co się dzieje

Zalogowany użytkownik oznacza rekordy gwiazdką: ☆ zmienia się w ★. Ulubione w StacjaFinder to stacje Metroroweru, z których para korzysta najczęściej. Strona „Moje ulubione” pokazuje tylko rekordy zalogowanego użytkownika.

## Reguły

1. Strona „Moje ulubione” wymaga zalogowania.
2. Kliknięcie ☆ bez logowania pokazuje prośbę o zalogowanie.
3. Zalogowany użytkownik klika ☆, gwiazdka zmienia się w ★, a rekord trafia do ulubionych.
4. Dwa kliknięcia tej samej gwiazdki dają jeden wpis w ulubionych.
5. Usunięcie z ulubionych można powtórzyć bez błędu.
6. Użytkownik nie widzi ulubionych innych użytkowników.
7. Żądanie bez tokenu antiforgery zostaje odrzucone.
<!-- DECYZJA 5: zdanie domyślne. Wybrane zdanie (z propozycji Kilo) wklejcie w miejsce zdania po „8. ”. -->
8. Strona „Moje ulubione” pokazuje tytuły ulubionych rekordów.

## Czego się nauczyliśmy

<!-- Po kroku dopiszcie jedno zdanie: co Was zaskoczyło w danych albo w kodzie. -->
