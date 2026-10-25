# PamięćZabrze – scenariusz s01

- **Aplikacja:** PamięćZabrze
- **Pomysł:** Wyszukiwarka tablic i miejsc pamięci w Zabrzu z informacją, w której dzielnicy jest ich najwięcej.
- **Analiza AI (krok 4):** Pogrupuj te miejsca pamięci według tego, kogo albo co upamiętniają, i zaproponuj spacer po 3 z nich.
- **Dane:** „Zabrze: miejsca pamięci”, plik `data/raw/s01_zabrze-miejsca-pamieci.csv`.
- **Atrybucja:** Źródło: Zabrze, otwartedane.metropoliagzm.pl, CC BY 4.0. Strona zbioru: https://otwartedane.metropoliagzm.pl/dataset/b78bdf8a194b381025a5c2f4a79b9daa01d335f107519145604fb35412178299

## Jak pracujecie

1. Budujecie całą aplikację w 4 krokach: dane w bazie → wyszukiwarka → strona → analiza AI.
2. Najpierw specyfikacja, potem testy, na końcu kod. Specyfikacja to `spec/` obok tej karty: każde zdanie w „Reguły” to jeden test (ikona kolby w VS Code). Krok jest gotowy, gdy jego testy są zielone.
3. Wszystkie prompty są w `prompty.md`. Idźcie od góry do dołu, zaczynając od kroku 0.
4. **Pilot** kopiuje prompt do Kilo Code i akceptuje zmiany. **Nawigator** czyta pauzy, prowadzi decyzje i patrzy, czy wszystko działa. Po każdym kroku zamieniacie się rolami.
5. W pauzach wybieracie, jak ma wyglądać Wasza aplikacja: wybrane zdanie wklejacie do specyfikacji. Nie ma złych odpowiedzi.
6. Skończyliście? Na końcu `prompty.md` są pomysły, co dodać samodzielnie.
