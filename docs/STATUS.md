# Status

| NN | operacja | status | data | uwaga |
|---|---|---|---|---|
| 01 | repo startowe | OK | 2026-10-20 | Zweryfikowano na Linuksie z SQL Server 2022 (Docker, GZM_CONNSTR); LocalDB do sprawdzenia na Windows w kroku 12. |
| 02 | dane | OK | 2026-10-20 | 15/15 scenariuszy; 5 zbiorów głównych zastąpiono rezerwami (R2, R4, R5 oraz R6, R7 dodanymi przez prowadzącego), szczegóły w docs/ZAMIANY.md. |
| 03 | mapowania i wartości kontrolne | OK | 2026-10-20 | 15 × mapping.json i checkpoints.md z tools/Expect (426 wartości, wszystkie w docs/expect-log.txt); ponowne uruchomienie daje identyczne pliki. |
| 04 | karty i prompty uczniów | OK | 2026-10-20 | 15 × 8 plików generuje tools/Cards z szablonów i checkpoints.md, `tools/Cards -- check` bez błędów; do GzmApp dodano CsvHelper i pakiety AI. |
| 05 | ratunek etap-1: generyczny import | OK | 2026-10-20 | 15/15 scenariuszy: `COUNT(*)` = E1.rows i liczba „dziwnych” = E1.weird, drugi import bez zmian (SQL Server 2022 w Dockerze, pętla `Scenario=sNN dotnet run -- import`). |

## Gałęzie do utworzenia lub zmergowania ręcznie

Środowisko Claude Code w chmurze pozwala pushować tylko na gałąź sesji.

- `main`: prompty 01–03 są już w `main` (PR #3). Zmerguj `claude/admiring-franklin-wpdxux` po commicie „04: karty i prompty uczniów” (nowy PR).
