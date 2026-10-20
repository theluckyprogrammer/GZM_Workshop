# Status

| NN | operacja | status | data | uwaga |
|---|---|---|---|---|
| 01 | repo startowe | OK | 2026-10-20 | Zweryfikowano na Linuksie z SQL Server 2022 (Docker, GZM_CONNSTR); LocalDB do sprawdzenia na Windows w kroku 12. |
| 02 | dane | OK | 2026-10-20 | 15/15 scenariuszy; 5 zbiorów głównych zastąpiono rezerwami (R2, R4, R5 oraz R6, R7 dodanymi przez prowadzącego), szczegóły w docs/ZAMIANY.md. |

## Gałęzie do utworzenia lub zmergowania ręcznie

Środowisko Claude Code w chmurze pozwala pushować tylko na gałąź sesji `claude/upbeat-volta-smzlon`.

- `main`: zmerguj (fast-forward) z `claude/upbeat-volta-smzlon` po commitach „01: repo startowe” i „02: dane (częściowo)” i „02: dane”.
