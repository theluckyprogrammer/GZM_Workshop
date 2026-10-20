# StacjaFinder – scenariusz s02

- **Aplikacja:** StacjaFinder
- **Pomysł:** Znajdź stację Metroroweru w swojej gminie i sprawdź, ile ma stojaków.
- **Ulubione:** stacje Metroroweru, z których para korzysta najczęściej.
- **Dane:** „Metrorower: stacje aktualne”, plik `data/raw/s02_metrorower-stacje.csv`, w tabeli **945** (E1.rows) rekordów.
- **Atrybucja:** Źródło: Urząd Metropolitalny Górnośląsko-Zagłębiowskiej Metropolii, otwartedane.metropoliagzm.pl, CC BY 4.0. Strona zbioru: https://otwartedane.metropoliagzm.pl/dataset/b161e77ded1548e6044a0f52796df030fe96755c3e1470e49d8524084682d464

## Jak pracujecie

1. Role zmieniają się co etap. **Pilot** wkleja prompt do Kilo Code i akceptuje zmiany. **Kontroler danych** patrzy w plik CSV i w tę kartę.
2. Prompty: `prompty/etap1.md` … `prompty/etap4.md`. Gdy wynik nie zgadza się z kartą: `prompty/detektyw.md`.
3. Po każdym etapie sprawdźcie sekcję GOTOWE GDY narzędziem i porównajcie z liczbą z karty.
4. Pułapki w Waszych danych: `pulapki.md`. Zadanie dodatkowe: `prompty/gwiazdka.md`.
