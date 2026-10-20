# Status

| NN | operacja | status | data | uwaga |
|---|---|---|---|---|
| 01 | repo startowe | OK | 2026-10-20 | Zweryfikowano na Linuksie z SQL Server 2022 (Docker, GZM_CONNSTR); LocalDB do sprawdzenia na Windows w kroku 12. |
| 02 | dane | BŁĄD | 2026-10-20 | 13/15 scenariuszy gotowe; s09 i s15 odrzucone (4 wiersze), a rezerwy R1–R5 wyczerpane – czeka na decyzję o dodatkowych rezerwach. |

## Gałęzie do utworzenia lub zmergowania ręcznie

Środowisko Claude Code w chmurze pozwala pushować tylko na gałąź sesji `claude/upbeat-volta-smzlon`.

- `main`: zmerguj (fast-forward) z `claude/upbeat-volta-smzlon` po commitach „01: repo startowe” i „02: dane (częściowo)”.
