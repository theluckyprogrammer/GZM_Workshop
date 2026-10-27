---
capability: {{code}}-000-dane
module: {{app}}
state: draft
---

# Krok 0 – szkielet i dane

## Co się dzieje

Pierwszy prompt stawia fundament Waszego portalu: aplikację w `src/GzmApp` oraz plik „{{dataset}}” pobrany z portalu GZM do `{{file}}`. Ten plik to surowiec całego warsztatu – poniższe reguły sprawdzają, że naprawdę jest na miejscu i nic nie zgubiło się po drodze.

## Reguły

1. Plik z danymi `{{file}}` istnieje w repozytorium.
2. Plik z danymi ma pełną liczbę wierszy.
3. Nagłówek pliku ma wszystkie kolumny, które w kroku 1 zaimportujecie do bazy.

## Czego się nauczyliśmy

<!-- Po kroku dopiszcie jedno zdanie: co Was zaskoczyło w danych albo w kodzie. -->
