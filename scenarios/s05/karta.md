# ZgubaDąbrowa – Wasz portal (scenariusz s05)

**Rzeczy znalezione w Dąbrowie Górniczej: co znaleziono i do kiedy można to odebrać.** To portal, który zbudujecie w tym warsztacie – na prawdziwych otwartych danych Metropolii GZM.

- **Dane:** „Dąbrowa Górnicza: rzeczy znalezione” – pobierzecie je w kroku 0 prosto z portalu [otwartedane.metropoliagzm.pl](https://otwartedane.metropoliagzm.pl/dataset/565dc7a79264b260f100cd7e07e40a06e5bd073b5d0721e8ea6619e721178b8c) (licencja CC BY 4.0, atrybucja: Źródło: Dąbrowa Górnicza, otwartedane.metropoliagzm.pl, CC BY 4.0).
- **Co powstanie:** w 5 krokach – szkielet portalu z danymi → dane w bazie SQL → wyszukiwarka → strona z listą i statystyką → analiza danych przez AI (a na koniec, dla chętnych, logowanie i ulubione).
- **Kto pisze kod:** Kilo Code (AI) – wy nim sterujecie promptami z `prompty.md` i decydujecie, jak portal ma działać.

## Jak pracujecie

1. **Pilot** kopiuje prompt do Kilo Code i akceptuje zmiany. **Nawigator** czyta pauzy, prowadzi decyzje i patrzy, czy wszystko działa. Po każdym kroku zamieniacie się rolami.
2. Zaczynacie od **kroku 0** w `prompty.md`: pierwszy prompt tworzy szkielet aplikacji i pobiera dane. Potem budujecie funkcję za funkcją – w każdym kroku nowa część portalu.
3. Pracujecie jak zawodowcy: najpierw **specyfikacja** (`spec/` obok tej karty – każde zdanie w „Reguły” to jeden test), potem **testy**, na końcu **kod**. Krok jest gotowy, gdy jego testy są zielone. Ikona kolby w VS Code pokazuje je na żywo.
4. W pauzach „decyzja” **pytacie Kilo o propozycje** – AI zna Wasze dane i proponuje warianty dopasowane do Waszego portalu, a Wy wybieracie. Każdą decyzję Kilo zapisuje w `LOG-DECYZJI.md` – powstaje zapis, jak powstawał Wasz portal.
5. Jak coś uruchomić, gdzie zajrzeć w przeglądarce, terminalu, debuggerze i bazie – pokazuje krok po kroku `docs/PORADNIK-UCZNIA.md`. W każdej pauzie jest wskazane, który akapit otworzyć.
6. Skończyliście? Na końcu `prompty.md` są pomysły, co dodać samodzielnie.
