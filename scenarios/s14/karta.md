# BiletBot – scenariusz s14

- **Aplikacja:** BiletBot
- **Pomysł:** Gdzie kupisz bilet ZTM: automaty biletowe na przystankach w Twojej gminie.
- **Ulubione:** automaty przy przystankach, z których korzysta para.
- **Dane:** „ZTM: automaty biletowe”, plik `data/raw/s14_ztm-automaty-biletowe.csv`, w tabeli **266** (E1.rows) rekordów.
- **Atrybucja:** Źródło: Zarząd Transportu Metropolitalnego, otwartedane.metropoliagzm.pl, Creative Commons Attribution. Strona zbioru: https://otwartedane.metropoliagzm.pl/dataset/lista-automatow-biletowych

## Jak pracujecie

1. Role zmieniają się co etap. **Pilot** wkleja prompt do Kilo Code i akceptuje zmiany. **Kontroler danych** patrzy w plik CSV i w tę kartę.
2. Prompty: `prompty/etap1.md` … `prompty/etap4.md`. Gdy wynik nie zgadza się z kartą: `prompty/detektyw.md`.
3. Po każdym etapie sprawdźcie sekcję GOTOWE GDY narzędziem i porównajcie z liczbą z karty.
4. Pułapki w Waszych danych: `pulapki.md`. Zadanie dodatkowe: `prompty/gwiazdka.md`.
