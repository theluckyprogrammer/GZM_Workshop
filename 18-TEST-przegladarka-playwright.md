# 18 – Testy: przeglądarka, Playwright (poziom 5, Spec UI-xx)

Przeczytaj `zadanie/prompts/00-KONTEKST-wspolny.md` (Zasady testów, Oszczędzanie tokenów).

- **Wymaga:** 17 OK.
- **Sesja:** nowa.

## Zadanie
Gałąź: `ratunek/etap-4`. Tylko scenariusze s06, s04, s02. `Microsoft.Playwright` (Apache-2.0), Chromium headless, `[Trait("Poziom","5")]`.

1. Wpisanie frazy znak po znaku → po ~300 ms dokładnie 1 żądanie (debounce) i zmiana zawartości `#wyniki`.
2. W konsoli przeglądarki nie ma błędów JS.
3. Po zalogowaniu klik „★” → gwiazdka aktywna i rekord w bazie.
4. Wymuszony błąd 500 → komunikat w `#wyniki`, a nie cała strona błędu.
5. Strona działa offline: blokada żądań do CDN nic nie psuje.

Jeśli Playwright nie może pobrać przeglądarek, zapisz to w `findings.md`, oznacz poziom jako pominięty i zakończ.

## Kryterium ukończenia
- 3 scenariusze są zielone albo poziom jest pominięty z uzasadnieniem.
- `docs/TEST-STATUS.md` zaktualizowany.

## Na koniec
Commit i wiersz w `docs/STATUS.md`: „18: testy przeglądarkowe”.
