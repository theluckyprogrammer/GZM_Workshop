# 02 – Przygotowanie: pobranie danych i metadane

Przeczytaj `zadanie/prompts/00-KONTEKST-wspolny.md` (sekcje: Stos, Zasady kodu, Oszczędzanie tokenów, Status, Gdy URL danych nie działa).

- **Wymaga:** 01 OK.
- **Sesja:** można kontynuować po 01.

## Zadanie
1. Napisz narzędzie `tools/Fetch` (aplikacja konsolowa .NET 10). Dla każdego ID ma:
   1. wywołać `https://otwartedane.metropoliagzm.pl/api/3/action/package_show?id=<ID>`,
   2. wybrać zasób w formacie CSV,
   3. wziąć `resources[].url`.
      - NIE składaj URL-a ręcznie: wewnętrzny UUID w adresie pliku różni się od ID zbioru.
   4. pobrać plik bajt w bajt do `data/raw/sNN_<slug>.csv`.
2. Zbiory główne:
   - s01 `a53f258b2c21f503a1bea267cc912bb033db0d0db90ee1483f66323771634127` (Metrorower: popularne połączenia)
   - s02 `b161e77ded1548e6044a0f52796df030fe96755c3e1470e49d8524084682d464` (Metrorower: stacje aktualne)
   - s03 `4e4f0a8bdce9bb2857c6ef27eceb7a8308127b738dfd7c51decc88287d4b9084` (dzienna dostępność rowerów w gminach)
   - s04 `dea28113f214eb0fb3fc9fcc9ec149f6cd5f61b7382e422645bd2fb52a60c67b` (Bytom: rzeczy znalezione)
   - s05 `565dc7a79264b260f100cd7e07e40a06e5bd073b5d0721e8ea6619e721178b8c` (Dąbrowa Górnicza: rzeczy znalezione)
   - s06 `498f49f43a2b0c7b39c26ee824eae5bcdb95c1001277727318ffe297ba02f7d1` (Dąbrowa Górnicza: pomniki przyrody)
   - s07 `e0f4337467ef2f60fc31f553213fe5283db5070d2b896df5c999bc988d12f2fb` (Siewierz: wydarzenia)
   - s08 `ae2be0e5ed4ec1863d7cb72e2397cd98805667e1acc6aaf975ae411d3056b9f7` (CK Victoria: wydarzenia)
   - s09 `09524009d1dd5bc2f80c409d8849c8edc3222c8deda8188963cab53456eb4180` (Przystanek Otwartej Kultury: wydarzenia)
   - s10 `05bc52c0eed46e917739a13ee88cc94f45a672c3ffaa9ef5bc88f4ad6bdb660b` (Gliwice: boiska i stadiony)
   - s11 `1def0df4cbc60ff042636848df79479a71a3b87247138be2615f72499323ac64` (Katowice: miejsca pamięci narodowej)
   - s12 `77714e3bb8cad342bc54cfe6d77c478c4d2141b97aefc1cb6df6cc0719ce52a3` (Bytom: zabytki)
   - s13 `1ea36ecce32f5766c78654f9cd1358a75830a7799d8a002fc251bec6b7359f47` (SCK Siemianowice: zajęcia sekcyjne)
   - s14 `lista-automatow-biletowych` (ZTM: automaty biletowe; URL pliku zawiera datę, zapisz ją w metadanych)
   - s15 `ed990d78435a34a373b870f349aad5b56987d961840a5ecf8062bdb1bd7fcc53` (Gliwice: kina i teatry)
3. Zbiory rezerwowe, w tej kolejności użycia:
   - R1 `6d8bcaf6e7076ea75fa5006178bb6f9044ef90573bb0ba0cfe87269c6bee5023` (Gliwice: muzea)
   - R2 `b78bdf8a194b381025a5c2f4a79b9daa01d335f107519145604fb35412178299` (Zabrze: miejsca pamięci)
   - R3 `73ad234d3f98dc1c34a87474bb54446510ac7c104fbb9e6e02e6a63fe3b1f945` (Gliwice: biblioteki)
   - R4 `f310c9b971378c90071de753fc47e60d0b7b7fa52a9a0408bc5937c9373f6bd3` (Gliwice: obiekty sportowe)
   - R5 `27d74bc7362cbcb3aeaf9b41b3635cc331c227cf122431be9a6aab11e29ac975` (Metrorower: rezerwacje)
4. Plik `data/metadata.json`. Dla każdego zbioru zapisz:
   - źródło: id, tytuł, wydawca, URL strony, URL pliku,
   - licencja i wymagana atrybucja („Źródło: <wydawca>, otwartedane.metropoliagzm.pl, CC BY 4.0”),
   - plik: data pobrania, SHA-256, rozmiar,
   - format: wykryte kodowanie (BOM / UTF-8 / CP1250), separator, nagłówki dokładnie jak w pliku,
   - zawartość: liczba wierszy danych, wykryty typ każdej kolumny (liczba z przecinkiem? data, a jeśli tak, to w jakim formacie?), liczba pustych pól w każdej kolumnie.
5. Reguły odrzucenia. Odrzucony zbiór zastąp kolejnym rezerwowym, a powód zapisz w `docs/ZAMIANY.md`:
   - błąd URL lub HTTP po 3 próbach,
   - brak zasobu CSV,
   - mniej niż 30 albo więcej niż 20 000 wierszy (przy 5000–20 000: przycięta kopia robocza w `data/work/`, oryginał zostaje),
   - dane osobowe: PESEL `\b\d{11}\b`, numery telefonów komórkowych, e-maile osób, wzorzec „Imię Nazwisko” w polach opisowych. Trafienie zgłoś w `findings.md` do przeglądu ręcznego,
   - brak pola tekstowego do filtrowania albo brak pola liczbowego, datowego lub kategorii do statystyki.

## Kryterium ukończenia
- W `data/raw` jest 15 zbiorów głównych + rezerwowe.
- `data/metadata.json` jest kompletne (każde pole wypełnione).
- `docs/ZAMIANY.md` opisuje każdą zamianę albo stwierdza, że zamian nie było.

## Na koniec
Commit `"02: dane"` i wiersz w `docs/STATUS.md`. W raporcie wypisz tylko tabelę: scenariusz | wiersze | kodowanie | separator | uwaga.
