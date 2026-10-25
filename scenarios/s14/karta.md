# BiletBot – scenariusz s14

- **Aplikacja:** BiletBot
- **Pomysł:** Gdzie kupisz bilet ZTM: automaty biletowe na przystankach w Twojej gminie.
- **Ulubione:** automaty przy przystankach, z których korzysta para.
- **Dane:** „ZTM: automaty biletowe”, plik `data/raw/s14_ztm-automaty-biletowe.csv`.
- **Atrybucja:** Źródło: Zarząd Transportu Metropolitalnego, otwartedane.metropoliagzm.pl, Creative Commons Attribution. Strona zbioru: https://otwartedane.metropoliagzm.pl/dataset/lista-automatow-biletowych

## Jak pracujecie

1. Budujecie całą aplikację w 4 krokach: dane w bazie → wyszukiwarka → strona → logowanie i ulubione.
2. Najpierw specyfikacja, potem testy, na końcu kod. Specyfikacja to `spec/` obok tej karty: każde zdanie w „Reguły” to jeden test (ikona kolby w VS Code). Krok jest gotowy, gdy jego testy są zielone.
3. Wszystkie prompty są w `prompty.md`. Idźcie od góry do dołu, zaczynając od kroku 0.
4. **Pilot** kopiuje prompt do Kilo Code i akceptuje zmiany. **Nawigator** czyta pauzy, prowadzi decyzje i patrzy, czy wszystko działa. Po każdym kroku zamieniacie się rolami.
5. W pauzach wybieracie, jak ma wyglądać Wasza aplikacja: wybrane zdanie wklejacie do specyfikacji. Nie ma złych odpowiedzi.
6. Skończyliście? Na końcu `prompty.md` są pomysły, co dodać samodzielnie.
