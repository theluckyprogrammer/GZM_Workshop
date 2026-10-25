# RowerStat – scenariusz s03

- **Aplikacja:** RowerStat
- **Pomysł:** Ile rowerów miejskich jest dostępnych w każdej gminie GZM i gdzie jest ich za mało.
- **Analiza AI (krok 4):** Wskaż dni i gminy, w których dostępnych rowerów było mniej niż wymaganych, i napisz, co z tego wynika.
- **Dane:** „Dzienna dostępność rowerów w gminach”, plik `data/raw/s03_rowery-dostepnosc-gminy.csv`.
- **Atrybucja:** Źródło: Urząd Metropolitalny Górnośląsko-Zagłębiowskiej Metropolii, otwartedane.metropoliagzm.pl, CC BY 4.0. Strona zbioru: https://otwartedane.metropoliagzm.pl/dataset/4e4f0a8bdce9bb2857c6ef27eceb7a8308127b738dfd7c51decc88287d4b9084

## Jak pracujecie

1. Budujecie całą aplikację w 4 krokach: dane w bazie → wyszukiwarka → strona → analiza AI.
2. Najpierw specyfikacja, potem testy, na końcu kod. Specyfikacja to `spec/` obok tej karty: każde zdanie w „Reguły” to jeden test (ikona kolby w VS Code). Krok jest gotowy, gdy jego testy są zielone.
3. Wszystkie prompty są w `prompty.md`. Idźcie od góry do dołu, zaczynając od kroku 0.
4. **Pilot** kopiuje prompt do Kilo Code i akceptuje zmiany. **Nawigator** czyta pauzy, prowadzi decyzje i patrzy, czy wszystko działa. Po każdym kroku zamieniacie się rolami.
5. W pauzach wybieracie, jak ma wyglądać Wasza aplikacja: wybrane zdanie wklejacie do specyfikacji. Nie ma złych odpowiedzi.
6. Skończyliście? Na końcu `prompty.md` są pomysły, co dodać samodzielnie.
