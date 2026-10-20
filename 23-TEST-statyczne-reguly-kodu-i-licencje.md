# 23 – Testy: statyczne – reguły kodu, Kilo, sekrety, licencje (poziom 8, Spec Q-xx)

Przeczytaj `zadanie/prompts/00-KONTEKST-wspolny.md` (Zasady kodu, Zasady testów, Oszczędzanie tokenów).

- **Wymaga:** 22 OK.
- **Sesja:** nowa.

## Zadanie
Gałąź: `ratunek/etap-4` (zawiera cały kod) + `main`.

1. **Kilo:**
   - `kilo.jsonc` to poprawny JSONC,
   - zawiera deny dla: `*.csproj`, `launch.json`, `tasks.json`, `launchSettings.json`, `Program.cs`, `Identity/**`, `global.json`,
   - zawiera `bash "*": "deny"`.
2. **Narzędzie `tools/CodeRules`** (`Microsoft.CodeAnalysis.CSharp`, MIT) zgłasza:
   - zagnieżdżone if-y i pętle,
   - metody dłuższe niż 20 linii, klasy dłuższe niż 150 linii,
   - publiczne metody bez guard clause dla parametrów referencyjnych.

   Uruchom je na `src/` i `tools/`. Naruszenia popraw albo uzasadnij w `findings.md`. Kod z Identity UI generowany szablonem jest wyłączony z kontroli.
3. **Sekrety:** `git grep` wzorców kluczy (`sk-`, `api_key`, `ApiKey":"`, długie ciągi base64) we wszystkich gałęziach i w historii (`git log -p`). Trafienie = BŁĄD krytyczny.
4. **Licencje:** `dotnet list package --include-transitive` porównane z `docs/LICENSES.md`. Każdy pakiet ma licencję MIT, Apache, MS-PL albo BSD.

## Kryterium ukończenia
- 0 naruszeń albo każde uzasadnione w `findings.md`.
- 0 sekretów.

## Na koniec
Commit i wiersz w `docs/STATUS.md`: „23: testy statyczne”.
