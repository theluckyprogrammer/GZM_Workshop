# 12 – Przygotowanie: smoke test (bez pisania testów automatycznych)

Przeczytaj `zadanie/prompts/00-KONTEKST-wspolny.md` (sekcje: Oszczędzanie tokenów, Status, Gałęzie).

- **Wymaga:** 11 OK.
- **Sesja:** można kontynuować po 11.

## Zadanie
Szybka kontrola, czy pakiet w ogóle działa. Tylko scenariusz s02.

1. `main`: build + start. Strona startowa pokazuje s02 po ustawieniu `Scenario`.
2. `tools/ratunek.ps1 -Etap 1 -Scenariusz s02`: liczba wierszy = E1.
3. `-Etap 3`: wyszukiwarka zwraca fragment bez `<html>`, statystyka = E3.
4. `-Etap 4`: rejestracja, logowanie, „★” na rekordzie 7 → 1 wiersz w `Favorites`.
5. Powrót na `main`. Repo jest czyste (`git status`).

Nie pisz testów automatycznych. Pełne testy wykonują prompty 13–25.

## Kryterium ukończenia
Punkty 1–5 są OK. Każde odchylenie: poprawka minimalnym diffem albo wpis w `findings.md`.

## Na koniec
Commit (jeśli były poprawki) i wiersz w `docs/STATUS.md`: „12: smoke test”.
