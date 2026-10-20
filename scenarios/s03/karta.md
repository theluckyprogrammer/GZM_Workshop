# RowerStat – scenariusz s03

- **Aplikacja:** RowerStat
- **Pomysł:** Ile rowerów miejskich jest dostępnych w każdej gminie GZM i gdzie jest ich za mało.
- **Ulubione:** gminy, które para obserwuje.
- **Dane:** „Dzienna dostępność rowerów w gminach”, plik `data/raw/s03_rowery-dostepnosc-gminy.csv`, w tabeli **40** (E1.rows) rekordów.
- **Atrybucja:** Źródło: Urząd Metropolitalny Górnośląsko-Zagłębiowskiej Metropolii, otwartedane.metropoliagzm.pl, CC BY 4.0. Strona zbioru: https://otwartedane.metropoliagzm.pl/dataset/4e4f0a8bdce9bb2857c6ef27eceb7a8308127b738dfd7c51decc88287d4b9084

## Jak pracujecie

1. Role zmieniają się co etap. **Pilot** wkleja prompt do Kilo Code i akceptuje zmiany. **Kontroler danych** patrzy w plik CSV i w tę kartę.
2. Prompty: `prompty/etap1.md` … `prompty/etap4.md`. Gdy wynik nie zgadza się z kartą: `prompty/detektyw.md`.
3. Po każdym etapie sprawdźcie sekcję GOTOWE GDY narzędziem i porównajcie z liczbą z karty.
4. Pułapki w Waszych danych: `pulapki.md`. Zadanie dodatkowe: `prompty/gwiazdka.md`.
