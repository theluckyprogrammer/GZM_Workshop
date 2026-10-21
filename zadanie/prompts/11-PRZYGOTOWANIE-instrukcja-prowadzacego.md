# 11 – Przygotowanie: instrukcja dla prowadzącego

Przeczytaj `zadanie/prompts/00-KONTEKST-wspolny.md` (sekcje: Projekt, Stos, Status, Gałęzie).

- **Wymaga:** 10 OK.
- **Sesja:** można kontynuować po 10.

## Zadanie
Napisz `docs/PROWADZACY.md` (zwięźle, do wydruku):

1. **Dzień przed — admin szkolny:**
   - .NET 10 SDK,
   - SqlLocalDB.msi,
   - VS Code (User Installer),
   - Git.
2. **Dzień przed — każde konto ucznia:**
   1. Rozszerzenia przez `code --install-extension …`.
   2. `git clone`.
   3. Wgranie `kilo.jsonc` i `.kilo/` PRZED pierwszym otwarciem folderu w VS Code, bo Kilo cache'uje konfigurację.
   4. `dotnet restore`.
   5. Pierwsze uruchomienie (utworzenie LocalDB).
   6. `tools/check.cmd`.
   7. Przydział scenariusza `sNN` do stanowiska.

   Na koniec checklista z polami do odhaczenia dla 15 stanowisk.
3. **Przebieg minuta po minucie:**

   | Minuty | Co się dzieje |
   |---|---|
   | 0–2 | otwarcie |
   | 2–8 | etap 1 |
   | 8–14 | etap 2 |
   | 14 | decyzja ratunkowa: pary bez etapu 2 → `ratunek.ps1 -Etap 2` |
   | 14–20 | etap 3 |
   | 20 | decyzja: pary bez etapu 3 → `ratunek.ps1 -Etap 3` |
   | 20–26 | etap 4 albo demonstracja prowadzącego na rzutniku |
   | 26–30 | zamknięcie |

   Wariant skrócony na wypadek opóźnień: etap 4 tylko jako demo.
4. **Tekst otwarcia** (do 60 sekund) i **tekst zamknięcia** (do 90 sekund) z pytaniem do sali: „Co zbudowaliście i co dodalibyście jako następne?”. Na otwarciu: uczniowie dostają gotowe prompty, w pauzach podejmują decyzje o swojej aplikacji – nic nie jest sprawdzianem.
5. **Tabela: problem → objaw → szybkie rozwiązanie.** Co najmniej:
   - port zajęty,
   - LocalDB nie startuje,
   - Kilo nie odpowiada lub limit kluczy,
   - AI dodało React lub EF Core,
   - zła liczba wierszy,
   - 400 przy ulubionych,
   - brak żądań w zakładce Network,
   - para skończyła wcześniej → „Co dalej?” na końcu `prompty.md` (m.in. SOL-6).
   - para nie wkleiła decyzji → Kilo użyje wariantu A, to nie błąd.

   Przy każdym scenariuszu korzystaj z `scenarios/sNN/prowadzacy.md` (wartości dla wariantów A i pułapki w danych).
6. **Klucze:**
   - Kilo: klucz per para, z limitem i wygaśnięciem 28.10.2026 23:59; wpisuje prowadzący.
   - SOL-6: `dotnet user-secrets set Sol6:ApiKey …`, wpisuje prowadzący.
   - Karty z kluczami zbierane po zajęciach, klucze rotowane po warsztacie.
7. **Atrybucja CC BY 4.0:** gdzie jest w aplikacji i co powiedzieć uczniom.

## Kryterium ukończenia
- Każde polecenie z instrukcji istnieje w repo albo jest standardowym poleceniem narzędzia.
- Nazwy plików i skryptów zgadzają się z repo. Sprawdź to skryptem (grep ścieżek z `PROWADZACY.md` i test istnienia).

## Na koniec
Commit `"11: instrukcja prowadzącego"` i wiersz w `docs/STATUS.md`.
