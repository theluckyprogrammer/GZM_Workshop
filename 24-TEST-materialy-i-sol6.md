# 24 – Testy: materiały do druku i zadanie * SOL-6 (poziom 9, Spec M-xx, AI-xx)

Przeczytaj `zadanie/prompts/00-KONTEKST-wspolny.md` (Stos – część AI, Zasady testów, Oszczędzanie tokenów).

- **Wymaga:** 23 OK.
- **Sesja:** można kontynuować po 23.

## Zadanie
1. **Materiały:**
   - ściąga prowadzącego ma wartości identyczne z `checkpoints.md` (test spójności dla 15 scenariuszy),
   - karty uczniów nie zawierają wartości oczekiwanych,
   - `print/*.html` nie ma pustych pól wartości ani placeholderów,
   - ścieżki i polecenia z `docs/PROWADZACY.md` istnieją w repo.
2. **Atrybucja:** stopka aplikacji ma tekst CC BY 4.0 dla każdego scenariusza (test HTTP na gałęzi `ratunek/etap-3`).
3. **SOL-6** (gałąź `ratunek/etap-4`):
   - test z fałszywym `IChatClient` → fragment HTML bez `<html>`, a do modelu trafiają tylko rekordy z bazy,
   - brak klucza → przyjazny komunikat, bez wyjątku,
   - klucza nie ma w logach (test przechwytuje log),
   - prawdziwe wywołanie tylko wtedy, gdy ustawiony jest sekret `Sol6:ApiKey`: JEDNO krótkie wywołanie dla s02. Bez sekretu oznacz jako pominięte.

## Kryterium ukończenia
- Wszystkie testy zielone.
- Prawdziwe wywołanie wykonane albo odnotowane jako pominięte.

## Na koniec
Commit i wiersz w `docs/STATUS.md`: „24: testy materiałów i SOL-6”.
