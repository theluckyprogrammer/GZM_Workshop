---
capability: s13-004-analiza-ai
module: SekcjaSCK
state: draft
---

# Krok 4 – analiza AI

## Co się dzieje

Przycisk „✨ Analizuj z AI” wysyła rekordy z listy wyników do modelu sztucznej inteligencji GPT-6 Luna. Model analizuje dane SekcjaSCK, a jego odpowiedź pojawia się na stronie w osobnej karcie. Karta mówi wprost, że ten tekst napisała AI, i pokazuje, co poszło do modelu.

## Reguły

1. Przycisk „✨ Analizuj z AI” na stronie głównej wysyła frazę z wyszukiwarki do analizy.
2. Do modelu trafiają rekordy z listy wyników dla wpisanej frazy, każdy z numerem `SourceRow`.
<!-- DECYZJA 4: zdanie domyślne. Wybrane zdanie (z propozycji Kilo) wklejcie w miejsce zdania po „3. ”. -->
3. Model dostaje zadanie: Dopasuj zajęcia do trzech osób: 7-latka, 12-latka i 16-latka. Podaj dzień i godzinę.
4. Odpowiedź modelu pojawia się w karcie `#analiza` bez przeładowania strony.
5. Karta mówi, że tekst wygenerowała sztuczna inteligencja, i podaje nazwę modelu.
6. Jedno kliknięcie to jedno zapytanie do modelu.
7. Odpowiedź modelu jest pokazana jako tekst, a nie jako kod HTML.
8. Gdy model nie odpowiada, karta pokazuje krótki komunikat zamiast strony błędu.
9. Prawdziwy model AI odpowiada na analizę danych z Waszej bazy.

## Czego się nauczyliśmy

<!-- Po kroku dopiszcie jedno zdanie: co Was zaskoczyło w odpowiedzi AI albo w danych. -->
