# Log decyzji pary

Tego pliku nie wypełniacie ręcznie – prowadzi go Kilo Code. Po każdej decyzji
(z propozycji Kilo w pauzach „decyzja 1…5") Kilo dopisuje tu jeden wiersz.
Dzięki temu na końcu warsztatu widać, jak powstał Wasz portal.

| Krok | Pytanie | Wasza decyzja | Dlaczego |
|---|---|---|---|
| 2 | Po czym użytkownik będzie szukał? | Wyszukiwarka szuka frazy w dowolnym miejscu pola `Commune` (kolumna w pliku: `Gmina`). | Para nie miała preferencji – wariant domyślny: szukanie po gminie najlepiej pasuje do portalu (użytkownik zna nazwę gminy). |
| 3 | Co pokazać na liście? | Przy każdym rekordzie lista pokazuje tytuł `Commune` i pola: `AvailableBikes`, `RequiredBikes`. | Wariant domyślny: od razu widać, ile rowerów jest dostępnych i ile ich brakuje. |
| 3 | Jaki kafelek ze statystyką? | Kafelek ze statystyką pokazuje sumę pola `AvailableBikes`. | Wariant domyślny: jedna liczba pokazuje skalę całego systemu rowerowego w GZM. |
| 4 | Co ma przeanalizować AI? | Model dostaje zadanie: Wskaż dni i gminy, w których dostępnych rowerów było mniej niż wymaganych, i napisz, co z tego wynika. | Wariant domyślny: wprost odpowiada na główne pytanie portalu – „gdzie jest ich za mało”. |