# ZgubaBytom – scenariusz s04

- **Aplikacja:** ZgubaBytom
- **Pomysł:** Sprawdź, czy ktoś znalazł Twoją zgubę w Bytomiu i czy sprawa jest jeszcze otwarta.
- **Ulubione:** rzeczy podobne do zgubionej, które para chce śledzić.
- **Dane:** „Bytom: rzeczy znalezione”, plik `data/raw/s04_bytom-rzeczy-znalezione.csv`, w tabeli **451** (E1.rows) rekordów.
- **Atrybucja:** Źródło: Bytom, otwartedane.metropoliagzm.pl, CC BY 4.0. Strona zbioru: https://otwartedane.metropoliagzm.pl/dataset/dea28113f214eb0fb3fc9fcc9ec149f6cd5f61b7382e422645bd2fb52a60c67b

## Jak pracujecie

1. Role zmieniają się co etap. **Pilot** wkleja prompt do Kilo Code i akceptuje zmiany. **Kontroler danych** patrzy w plik CSV i w tę kartę.
2. Prompty: `prompty/etap1.md` … `prompty/etap4.md`. Gdy wynik nie zgadza się z kartą: `prompty/detektyw.md`.
3. Po każdym etapie sprawdźcie sekcję GOTOWE GDY narzędziem i porównajcie z liczbą z karty.
4. Pułapki w Waszych danych: `pulapki.md`. Zadanie dodatkowe: `prompty/gwiazdka.md`.
