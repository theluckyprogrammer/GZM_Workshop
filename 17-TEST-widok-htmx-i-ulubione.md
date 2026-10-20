# 17 – Testy: widok HTMX i ulubione (poziom 4, Spec E3-xx, E4-xx)

Przeczytaj `zadanie/prompts/00-KONTEKST-wspolny.md` (Stos – część HTMX 4, Zasady testów, Oszczędzanie tokenów, Gałęzie).

- **Wymaga:** 16 OK.
- **Sesja:** można kontynuować po 16.

## Zadanie
Narzędzia: WebApplicationFactory + AngleSharp, `[Trait("Poziom","4")]`, wszystkie scenariusze.

**E3 (gałąź `ratunek/etap-3`):**
1. Żądanie z nagłówkiem HTMX (nazwa zgodna z dokumentacją HTMX 4) zwraca fragment bez `<html>`.
2. Żądanie bez tego nagłówka zwraca pełną stronę.
3. Statystyka w HTML = E3.
4. Strona wczytuje lokalny `/lib/htmx/4.0.0/htmx.min.js`, a nie CDN bez wersji.
5. `<body>` ma `hx-headers:inherited` z tokenem.
6. Wyszukiwarka ma `hx-trigger` z `delay:300ms`.
7. Błąd 500 w żądaniu HTMX zwraca krótki fragment z komunikatem.

**E4 (gałąź `ratunek/etap-4`):**
1. Niezalogowany użytkownik dostaje przekierowanie do logowania albo 401.
2. Zalogowany bez tokenu antiforgery → 400.
3. Z tokenem → 200.
4. Dwukrotne dodanie daje 1 wiersz w `Favorites`.
5. Dwukrotne usunięcie nie zwraca błędu.
6. Użytkownik B nie widzi ulubionych użytkownika A.
7. Tytuł ulubionego rekordu nr 7 = E4.

## Kryterium ukończenia
- Wszystkie scenariusze są zielone.
- `docs/TEST-STATUS.md` zaktualizowany.

## Na koniec
Commit i wiersz w `docs/STATUS.md`: „17: testy widoku i ulubionych”.
