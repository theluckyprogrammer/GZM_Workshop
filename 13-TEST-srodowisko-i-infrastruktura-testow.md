# 13 – Testy: środowisko i infrastruktura testów (poziom 0)

Przeczytaj `zadanie/prompts/00-KONTEKST-wspolny.md` (całość, zwłaszcza Zasady testów).

- **Wymaga:** 12 OK.
- **Sesja:** nowa.

## Zadanie
1. **Środowisko:**
   - uruchom `tools/check.ps1` (na systemie innym niż Windows: tylko analiza składni + wpis w `findings.md`),
   - `dotnet build` całego rozwiązania bez ostrzeżeń.
2. **Projekt testów `tests/GzmApp.Tests`** (xUnit) na `main`, potem merge w górę do wszystkich gałęzi ratunkowych. Zawiera:
   - `DbFixture`: baza `GzmTest_{scenariusz}` z `GZM_TEST_CONNSTR` albo LocalDB; tworzy ją i usuwa,
   - `AppFactory`: WebApplicationFactory, ustawia `Scenario` i connection string,
   - `ScenarioData`: `TheoryData` ze `scenarios/*/mapping.json` + parser `checkpoints.md`,
   - helpery: logowanie użytkownika testowego, pobranie tokenu antiforgery ze strony, parsowanie HTML przez AngleSharp.
3. **Pliki dokumentacji testów:**
   - `docs/SPEC-TESTY.md`: tabela wymagań D, E1, E2, E3, E4, UI, R, P, Q, M, AI. Same ID i opisy, bez testów;
   - `docs/TEST-STATUS.md`: tabela poziom × scenariusz.
4. Jeden test dymny: `ScenarioData` znajduje 15 scenariuszy, a każdy ma niepuste wartości E1–E4.

## Kryterium ukończenia
- Build jest zielony.
- Test dymny jest zielony na wszystkich gałęziach.
- `check.ps1` daje OK albo każdy BŁĄD jest uzasadniony w `findings.md`.

## Na koniec
Commit i wiersz w `docs/STATUS.md`: „13: infrastruktura testów”.
