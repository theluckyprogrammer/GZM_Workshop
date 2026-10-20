# RezerwacjaRadar – scenariusz s08

- **Aplikacja:** RezerwacjaRadar
- **Pomysł:** Ostatnie rezerwacje Metroroweru: które kończą się wypożyczeniem, a które wygasają albo są anulowane.
- **Ulubione:** stacje, których rezerwacje para obserwuje.
- **Dane:** „Metrorower: rezerwacje”, plik `data/raw/s08_metrorower-rezerwacje.csv`, w tabeli **98** (E1.rows) rekordów.
- **Atrybucja:** Źródło: Urząd Metropolitalny Górnośląsko-Zagłębiowskiej Metropolii, otwartedane.metropoliagzm.pl, CC BY 4.0. Strona zbioru: https://otwartedane.metropoliagzm.pl/dataset/27d74bc7362cbcb3aeaf9b41b3635cc331c227cf122431be9a6aab11e29ac975

## Jak pracujecie

1. Role zmieniają się co etap. **Pilot** wkleja prompt do Kilo Code i akceptuje zmiany. **Kontroler danych** patrzy w plik CSV i w tę kartę.
2. Prompty: `prompty/etap1.md` … `prompty/etap4.md`. Gdy wynik nie zgadza się z kartą: `prompty/detektyw.md`.
3. Po każdym etapie sprawdźcie sekcję GOTOWE GDY narzędziem i porównajcie z liczbą z karty.
4. Pułapki w Waszych danych: `pulapki.md`. Zadanie dodatkowe: `prompty/gwiazdka.md`.
