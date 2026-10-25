# RezerwacjaRadar – scenariusz s08

- **Aplikacja:** RezerwacjaRadar
- **Pomysł:** Ostatnie rezerwacje Metroroweru: które kończą się wypożyczeniem, a które wygasają albo są anulowane.
- **Analiza AI (krok 4):** Policz, ile rezerwacji zakończyło się wypożyczeniem, a ile wygasło albo zostało anulowanych, i wskaż stację z największą liczbą rezerwacji.
- **Dane:** „Metrorower: rezerwacje”, plik `data/raw/s08_metrorower-rezerwacje.csv`.
- **Atrybucja:** Źródło: Urząd Metropolitalny Górnośląsko-Zagłębiowskiej Metropolii, otwartedane.metropoliagzm.pl, CC BY 4.0. Strona zbioru: https://otwartedane.metropoliagzm.pl/dataset/27d74bc7362cbcb3aeaf9b41b3635cc331c227cf122431be9a6aab11e29ac975

## Jak pracujecie

1. Budujecie całą aplikację w 4 krokach: dane w bazie → wyszukiwarka → strona → analiza AI.
2. Najpierw specyfikacja, potem testy, na końcu kod. Specyfikacja to `spec/` obok tej karty: każde zdanie w „Reguły” to jeden test (ikona kolby w VS Code). Krok jest gotowy, gdy jego testy są zielone.
3. Wszystkie prompty są w `prompty.md`. Idźcie od góry do dołu, zaczynając od kroku 0.
4. **Pilot** kopiuje prompt do Kilo Code i akceptuje zmiany. **Nawigator** czyta pauzy, prowadzi decyzje i patrzy, czy wszystko działa. Po każdym kroku zamieniacie się rolami.
5. W pauzach wybieracie, jak ma wyglądać Wasza aplikacja: wybrane zdanie wklejacie do specyfikacji. Nie ma złych odpowiedzi.
6. Skończyliście? Na końcu `prompty.md` są pomysły, co dodać samodzielnie.
