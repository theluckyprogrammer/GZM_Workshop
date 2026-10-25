# HTMX 4 – różnice względem 2.x (używamy 4.0.0)

Źródło: https://four.htmx.org/docs/whats-new-in-htmx-4

1. **Dziedziczenie atrybutów jest jawne.** Atrybut na rodzicu działa na dzieci tylko z `:inherited`.
   - Źle: `<div hx-target="#wyniki"><input hx-get="?handler=Szukaj"></div>`
   - Dobrze: `<input hx-get="?handler=Szukaj" hx-target="#wyniki">` albo `<div hx-target:inherited="#wyniki">…</div>`
2. **Token antiforgery jest już na `<body>`** (`hx-headers:inherited='{"RequestVerificationToken":"…"}'` w `_Layout.cshtml`).
   - Źle: drugi `hx-headers` na przycisku albo wyłączenie walidacji (`[IgnoreAntiforgeryToken]`).
   - Dobrze: zwykły `hx-post` / `hx-delete` – token dojdzie sam.
3. **Odpowiedzi 4xx i 5xx są podmieniane w stronie** (nie podmienia się tylko 204 i 304).
   - Źle: zwrócić całą stronę błędu – wyląduje w środku listy.
   - Dobrze: krótki fragment z komunikatem i właściwym statusem, np. `<p class="alert alert-warning">Zaloguj się…</p>` ze statusem 401.
4. **Zdarzenia mają postać `htmx:faza:akcja`.**
   - Źle: `htmx:beforeRequest`, `htmx:afterSwap`, `htmx:responseError`
   - Dobrze: `htmx:before:request`, `htmx:after:swap`, `htmx:response:error`
5. **Logowanie:** `htmx.logAll()` nie istnieje.
   - Dobrze: `htmx.config.logAll = true;`
6. **Domyślny timeout to 60 s** (w 2.x nie było limitu).
7. **Selektory ze spacją lub przecinkiem w `hx-trigger ... from:` muszą być w apostrofach.**
   - Źle: `hx-trigger="keyup from:(closest form)"`
   - Dobrze: `hx-trigger="keyup from:'closest form'"`
8. **Nagłówki żądań:** `HX-Request` (zawsze `true`), `HX-Request-Type` (`full` / `partial`), `HX-Source` (zamiast `HX-Trigger`, format `tag#id`), `HX-Target`, `HX-Current-URL`. Rozpoznawaj żądanie HTMX po `HX-Request: true`.
9. **Usunięte:** `hx-vars` (zamiast tego `hx-vals` z `js:`), `hx-params`, `hx-ext`, `hx-disinherit`, `hx-inherit`, modyfikator `queue` w `hx-trigger` (zamiast tego `hx-sync`).
10. **Wyszukiwarka na żywo:** `hx-trigger="keyup changed delay:300ms"`.
11. **`hx-disable` zmienił znaczenie.** W 4.x `hx-disable="this"` blokuje przycisk na czas żądania (dawne `hx-disabled-elt`). Wyłączenie HTMX dla fragmentu strony to teraz `hx-ignore`.
   - Źle: usuwać `hx-disable="this"` z przycisku „Analizuj”, bo „wyłącza HTMX”.
   - Dobrze: zostawić – chroni przed podwójnym kliknięciem i drugim zapytaniem do AI.
