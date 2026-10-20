# 20 – Testy: przejście promptami ucznia, s06 DrzewoRekord (najtrudniejszy) (poziom 7, Spec P-xx)

Przeczytaj `zadanie/prompts/00-KONTEKST-wspolny.md` (Projekt, Stos, Oszczędzanie tokenów, Status).

- **Wymaga:** 19 OK.
- **Sesja:** NOWA. Kontekst ma być czysty, żeby nie podpowiadać sobie rozwiązania.

## Zadanie
Symulujesz parę uczniów z modelem w Kilo Code.

1. Utwórz git worktree `../walk-s06` z gałęzi `main`. Ustaw `Scenario` = `s06`.
2. Dla etapów 1–4 po kolei:
   1. Weź prompt kroku N z `scenarios/s06/prompty.md`. W miejsce `[DECYZJA n]` wklej linię wariantu A z pauzy (raz, w jednym kroku do wyboru, także wariant B – sprawdź, że też działa).
   2. Wykonaj ją DOSŁOWNIE, tak jak model w Kilo:
      - nie zaglądaj do gałęzi ratunkowych ani do rozwiązań,
      - nie używaj wiedzy spoza promptu, reguł z `.kilo/rules/` i plików repo `main`,
      - przestrzegaj uprawnień z `kilo.jsonc` (nie edytuj plików z listy deny).
   3. Wykonaj pauzę „zobaczcie, co powstało” z `prompty.md` (zapytanie SQL, odpowiedź HTTP) i porównaj wynik wariantu A z `scenarios/s06/checkpoints.md`.
   4. Zmierz czas.
   5. Jeśli wynik się nie zgadza, nie naprawiaj kodu ręcznie – przejdź do punktu 3.
3. Jeśli prompt jest niejednoznaczny lub prowadzi do złego wyniku:
   - popraw PROMPT w głównym repo, nie kod,
   - powtórz etap w świeżym worktree,
   - zapisz różnicę.
4. Opcjonalnie, gdy ustawione są `GLM_ENDPOINT` i `GLM_API_KEY`:
   - wyślij prompt etapu + wskazane pliki do `glm-5.3-flash` (API zgodne z OpenAI),
   - porównaj wynik z checkpointem,
   - typowe błędy modelu zapisz w `findings.md`, a ostrzeżenia dopisz do `.kilo/rules/stack.md`.
5. Usuń worktree.

## Kryterium ukończenia
- s06 przechodzi etapy 1–4 samymi promptami.
- Sekcja s06 w `docs/WALIDACJA.md`: czas każdego etapu, poprawki promptów, sprawdzone warianty decyzji.
- Jeśli czas etapu przekracza 6 min pracy modelu + 1,5 min checkpointu, w `findings.md` jest propozycja skrócenia.

## Na koniec
Commit (poprawki promptów) i wiersz w `docs/STATUS.md`: „20: walidacja s06”.
