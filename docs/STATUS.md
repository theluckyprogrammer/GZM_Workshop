# Status

| NN | operacja | status | data | uwaga |
|---|---|---|---|---|
| 01 | repo startowe | OK | 2026-10-21 | Zweryfikowano na Linuksie z SQL Server 2022 (Docker, GZM_CONNSTR); LocalDB do sprawdzenia na Windows w kroku 12. |

## Gałęzie do utworzenia lub zmergowania ręcznie

Środowisko Claude Code w chmurze pozwala pushować tylko na gałąź sesji `claude/admiring-franklin-wpdxux`.

- `main`: nadal zawiera projekt sprzed resetu (do commitu 28fca45). Ustaw `main` na stan gałęzi `claude/admiring-franklin-wpdxux` po commicie „01: repo startowe” (np. PR z tej gałęzi albo reset `main` na ten commit).
- `ratunek/etap-1`: pochodzi sprzed resetu; zostanie utworzona od nowa w kroku 05.
