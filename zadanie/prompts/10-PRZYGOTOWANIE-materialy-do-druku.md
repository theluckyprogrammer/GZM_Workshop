# 10 – Przygotowanie: materiały do druku dla uczniów

Przeczytaj `zadanie/prompts/00-KONTEKST-wspolny.md` (sekcje: Projekt, Oszczędzanie tokenów, Status).

- **Wymaga:** 04 OK (wartości) i 09 OK.
- **Sesja:** nowa.

## Zadanie
Dla każdego scenariusza wygeneruj skryptem `print/karta-pary-sNN.html`. Jeden szablon, dane z `karta.md` i `checkpoints.md`, nie pisz kart ręcznie. Format: A4, maksymalnie 2 strony, czarno-białe, `@media print`.

Zawartość karty:
1. Nazwa aplikacji, pomysł, zbiór danych i atrybucja.
2. Role z rubryką do wpisania imion:
   - **Pilot:** pisze prompt w Kilo, akceptuje zmiany,
   - **Kontroler danych:** sprawdza CSV, wypełnia przewidywania.

   Tabela zmiany ról: etap 1–4.
3. Klawisze:
   - F5 start, Shift+F5 stop,
   - F9 breakpoint, F10 krok, F11 wejście do metody,
   - F12 DevTools, Ctrl+Shift+P paleta poleceń.

   Do tego 3 kroki: jak dodać Watch i breakpoint warunkowy.
4. Dla każdego etapu checkpoint w formie tabeli: co sprawdzamy | narzędzie | **przewidujemy:** ____ | **zmierzyliśmy:** ____ | ✓.
   - Wartości oczekiwane NIE są na karcie ucznia. Są w ściądze prowadzącego `print/sciaga-prowadzacego.html`: jedna tabela ze wszystkimi scenariuszami.
5. „Czerwone flagi AI”, czyli kiedy nie ufać wygenerowanemu kodowi:
   - React lub npm,
   - EF Core w danych,
   - `htmx.logAll()`,
   - unpkg bez wersji,
   - `float` dla pieniędzy lub pomiarów,
   - `DateTime.Parse` bez kultury.
6. Ramka „Utknęliście?”: zawołajcie prowadzącego, który uruchomi `tools/ratunek.ps1`.

## Kryterium ukończenia
- 15 kart + ściąga prowadzącego.
- Ściąga ma wszystkie wartości zgodne z `checkpoints.md` (porównanie skryptem).
- Karty renderują się bez błędów (otwórz w przeglądarce headless albo sprawdź parserem HTML).

## Na koniec
Commit `"10: materiały do druku"` i wiersz w `docs/STATUS.md`.
