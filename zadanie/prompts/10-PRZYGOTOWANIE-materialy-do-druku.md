# 10 – Przygotowanie: materiały do druku dla uczniów

Przeczytaj `zadanie/prompts/00-KONTEKST-wspolny.md` (sekcje: Projekt, Oszczędzanie tokenów, Status).

- **Wymaga:** 04 OK (wartości) i 09 OK.
- **Sesja:** nowa.

## Zadanie
Dla każdego scenariusza wygeneruj skryptem `print/karta-pary-sNN.html`. Jeden szablon, dane z `karta.md` i `prompty.md`, nie pisz kart ręcznie. Format: A4, maksymalnie 2 strony, czarno-białe, `@media print`. Prompty uczniowie kopiują z `prompty.md` w VS Code; karta jest mapą drogi, nie kopią promptów.

Zawartość karty:
1. Nazwa aplikacji, pomysł, zbiór danych i atrybucja.
2. Role z rubryką do wpisania imion:
   - **Pilot:** wkleja prompt do Kilo, akceptuje zmiany,
   - **Nawigator:** prowadzi pauzy i decyzje, patrzy, czy wszystko działa.

   Tabela zmiany ról: krok 1–4.
3. Klawisze: F5 start, Shift+F5 stop, F12 DevTools (zakładka Network), Ctrl+Shift+P paleta poleceń.
4. Droga przez 4 kroki: dla każdego kroku nazwa, „zobaczcie, co powstało” (z `prompty.md`, 1 linia na punkt) i decyzje z wariantami A/B/C jako pola do zaznaczenia (☐ A ☐ B ☐ C z krótkim opisem wariantu). Bez wartości oczekiwanych.
5. „Czerwone flagi AI”, czyli kiedy nie ufać wygenerowanemu kodowi:
   - React lub npm,
   - EF Core w danych,
   - `htmx.logAll()`,
   - unpkg bez wersji,
   - `float` dla pieniędzy lub pomiarów,
   - `DateTime.Parse` bez kultury.
6. Ramka „Utknęliście?”: zawołajcie prowadzącego, który uruchomi `tools/ratunek.ps1`.
7. „Co dalej?”: lista pomysłów z końca `prompty.md`.

Ściąga prowadzącego `print/sciaga-prowadzacego.html`: jedna tabela ze wszystkimi scenariuszami – wartości dla wariantów A z `prowadzacy.md` (E1 liczba wierszy, E2 fraza i liczba wyników, E3, E4) i najważniejsza pułapka każdego zbioru.

## Kryterium ukończenia
- 15 kart + ściąga prowadzącego.
- Ściąga ma wszystkie wartości zgodne z `checkpoints.md` (porównanie skryptem).
- Karty uczniów nie zawierają wartości z `checkpoints.md` (sprawdzenie skryptem).
- Karty renderują się bez błędów (otwórz w przeglądarce headless albo sprawdź parserem HTML).

## Na koniec
Commit `"10: materiały do druku"` i wiersz w `docs/STATUS.md`.
