# 07 – Przygotowanie: gałąź ratunek/etap-4 (ulubione + zadanie *) i skrypt ratunek.ps1

Przeczytaj `zadanie/prompts/00-KONTEKST-wspolny.md` (sekcje: Stos, Zasady kodu, Oszczędzanie tokenów, Status, Gałęzie).

- **Wymaga:** 06 OK.
- **Sesja:** można kontynuować po 06.

## Zadanie
1. **`ratunek/etap-4`** (utwórz z `ratunek/etap-3`):
   - Tabela `Favorites(UserId, Scenario, RecordId)` z unikalnym indeksem (`db/900_favorites.sql`).
   - `IFavoritesRepository`:
     - `Add` jest idempotentne (`MERGE` albo `IF NOT EXISTS` w jednym zapytaniu),
     - `Remove` jest idempotentne,
     - `ListFor(userId)`.
   - Przycisk „★” przy rekordzie: `hx-post` / `hx-delete`.
     - Wymaga `[Authorize]` i walidacji antiforgery.
     - Token idzie z `<body hx-headers:inherited=...>`.
   - Zakładka „Moje ulubione” dla zalogowanego użytkownika (wariant A decyzji 4: lista tytułów).
   - Nazwy jak w kroku 4 `prompty.md`: `OnPostUlubione` / `OnDeleteUlubione`, strona `Ulubione`, niezalogowany przy ★ dostaje 401 z fragmentem.
2. **SOL-6 na tej samej gałęzi** (w materiałach uczniów to pomysł z „Co dalej?”, tu – działający przykład do pokazania):
   - `IAiSummarizer` z implementacją na `IChatClient`; konfiguracja `Sol6:*` opisana w kontekście.
   - Handler `OnGetPodsumuj` zwraca fragment HTML.
   - Brak klucza → fragment „Brak klucza SOL-6 – poproś prowadzącego”, bez wyjątku.
   - Prompt systemowy: „Odpowiadaj tylko na podstawie przekazanych rekordów, nie wymyślaj faktów.”
3. **`tools/ratunek.ps1 -Etap N -Scenariusz sNN`**:
   1. `git stash` (z komunikatem zawierającym datę),
   2. `git switch ratunek/etap-N`,
   3. ustawienie `Scenario` w `appsettings.Development.json`,
   4. import,
   5. wypisanie na ekranie, co się stało i jak wrócić (`git stash list`).
   - Bez wymagania uprawnień administratora.

## Kryterium ukończenia
- Niezalogowany użytkownik dostaje przekierowanie do logowania.
- Żądanie bez tokenu antiforgery → 400. Z tokenem → 200.
- Podwójne dodanie daje 1 wiersz.
- `ratunek.ps1` przełącza między etapami 1–4 dla s02 bez błędów.
- Wszystko sprawdzone skryptem z żądaniami HTTP.

## Na koniec
Commit na `ratunek/etap-4` i wiersz w `docs/STATUS.md` (w `main`). `tools/ratunek.ps1` musi być na wszystkich gałęziach: dodaj go do `main` i zmerguj w górę.
