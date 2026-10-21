# Status

| NN | operacja | status | data | uwaga |
|---|---|---|---|---|
| 01 | repo startowe | OK | 2026-10-21 | Zweryfikowano na Linuksie z SQL Server 2022 (Docker, GZM_CONNSTR); LocalDB do sprawdzenia na Windows w kroku 12. |
| 02 | dane | OK | 2026-10-21 | 15/15 scenariuszy; zamiany jak w poprzednim przebiegu (R2, R4, R5, R6, R7, szczegóły w docs/ZAMIANY.md); pliki identyczne z pobraniem z 20.10. |
| 03 | mapowania i wartości kontrolne | OK | 2026-10-21 | 15 × mapping.json i checkpoints.md z tools/Expect (426 wartości, wszystkie w docs/expect-log.txt); ponowne uruchomienie daje identyczne pliki. |

## Gałęzie do utworzenia lub zmergowania ręcznie

Środowisko Claude Code w chmurze pushuje na gałąź sesji `claude/admiring-franklin-wpdxux`; `main` aktualizuje się przez PR.

- `main`: reset jest już w `main` (PR #6). Zmerguj `claude/admiring-franklin-wpdxux` (nowy PR) z commitami od „01: repo startowe”.
- `ratunek/etap-1` na GitHubie pochodzi sprzed resetu – usuń ją ręcznie; krok 05 utworzy ją od nowa.
