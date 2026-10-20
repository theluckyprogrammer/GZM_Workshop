# Detektyw – szablon naprawy błędu ({{app}}, {{code}})

Gdy wynik nie zgadza się z kartą, nie piszcie „nie działa”. Najpierw zbierzcie dowód narzędziem, potem wklejcie prompt do Kilo Code.

## Szablon
Zamieńcie teksty w «…» na to, co widzicie w narzędziach:

> Oczekiwałem «wartość z karty» (z karty). Debugger pokazuje «wartość» w zmiennej «nazwa zmiennej» w linii «numer linii». Network pokazuje status «kod statusu». Nie zmieniaj niczego poza metodą «nazwa metody». Najpierw wyjaśnij przyczynę w 2 zdaniach, potem popraw.

Jeśli etap nie ma żądania w Network (import), napiszcie „Network: nie dotyczy”.

## Gotowe przykłady dla {{app}}
Liczby z debuggera w przykładach są wymyślone – wpiszcie swoje.

1. Etap 1: Oczekiwałem {{cp:E1.rows}} wierszy w tabeli `{{table}}` (z karty). Debugger pokazuje 0 w zmiennej `records` w linii 25. Network: nie dotyczy. Nie zmieniaj niczego poza metodą `ImportAsync`. Najpierw wyjaśnij przyczynę w 2 zdaniach, potem popraw.
2. Etap 1: Oczekiwałem wartości {{cp:E1.row7.value}} w polu `{{checkField}}` dla `SourceRow = 7` (z karty). Debugger pokazuje `null` w zmiennej `value` w linii 40. Network: nie dotyczy. Nie zmieniaj niczego poza metodą, która odczytuje kolumnę `{{checkCsv}}`. Najpierw wyjaśnij przyczynę w 2 zdaniach, potem popraw.
3. Etap 2: Oczekiwałem {{cp:E2.count}} wyników dla frazy {{cp:E2.phrase}} (z karty). Debugger pokazuje 0 w zmiennej `results` w linii 18. Network pokazuje status 200. Nie zmieniaj niczego poza metodą `Search`. Najpierw wyjaśnij przyczynę w 2 zdaniach, potem popraw.
4. Etap 4: Oczekiwałem dodania {{cp:E4.title}} do ulubionych (z karty). Debugger nie zatrzymuje się w metodzie `OnPostUlubione`. Network pokazuje status 400. Nie zmieniaj niczego poza metodą `OnPostUlubione` i przyciskiem ★ w `_Lista.cshtml`. Najpierw wyjaśnij przyczynę w 2 zdaniach, potem popraw.

Nie zmieniaj plików poza: plikiem z metodą wskazaną w prompcie. Nie dodawaj pakietów.
