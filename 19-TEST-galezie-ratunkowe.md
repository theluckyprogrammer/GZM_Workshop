# 19 – Testy: gałęzie ratunkowe (poziom 6, Spec R-xx)

Przeczytaj `zadanie/prompts/00-KONTEKST-wspolny.md` (Zasady testów, Oszczędzanie tokenów, Gałęzie).

- **Wymaga:** 18 OK (albo 18 pominięty z uzasadnieniem).
- **Sesja:** można kontynuować po 18.

## Zadanie
1. Dla każdej gałęzi `ratunek/etap-1..4` wykonaj jeden przebieg (razem 4, nie 60):
   - checkout,
   - build,
   - `dotnet test --filter "Poziom<=N+1"`. Jeśli filtr tego nie obsługuje, wypisz poziomy jawnie.

   Każdy przebieg obejmuje wszystkie scenariusze.
2. `tools/ratunek.ps1`:
   - na kopii repo z niezacommitowaną zmianą ucznia: zmiana trafia do stasha i jest odzyskiwalna,
   - przełączenie `-Etap 3 -Scenariusz s06` ustawia scenariusz i importuje dane,
   - powtórne uruchomienie z tymi samymi parametrami jest bezpieczne (idempotentne).
3. Gałęzie są spójne: każda wyższa zawiera wszystkie commity niższej (`git merge-base --is-ancestor`).

## Kryterium ukończenia
- 4 gałęzie są zielone.
- Brakujące poprawki zmergowane w górę, opisane w `docs/TEST-STATUS.md`.

## Na koniec
Commit i wiersz w `docs/STATUS.md`: „19: testy gałęzi ratunkowych”.
