---
capability: s12-003-pokaz-strone
module: ZabytkiBytom
state: draft
---

# Krok 3 – strona z wyszukiwarką i kafelkiem

## Co się dzieje

Strona główna ZabytkiBytom pokazuje kafelek ze statystyką, pole wyszukiwania i listę wyników. Lista zmienia się podczas pisania, bez przeładowania strony (HTMX 4). W stopce jest źródło danych.

## Reguły

1. Wyszukiwarka na stronie głównej wysyła frazę 300 ms po ostatnim wpisanym znaku.
2. Wyszukiwarka podmienia tylko listę wyników, a nie całą stronę.
3. Adres wyszukiwarki otwarty wprost w przeglądarce pokazuje całą stronę z wynikami.
<!-- DECYZJA 2: wariant A. Inny wariant z prompty.md wklejcie w miejsce zdania po „4. ”. -->
4. Przy każdym rekordzie lista pokazuje tytuł `Name` i pola: `Street`, `BuildingNumber`.
<!-- DECYZJA 3: wariant A. Inny wariant z prompty.md wklejcie w miejsce zdania po „5. ”. -->
5. Kafelek ze statystyką pokazuje 5 najczęstszych wartości pola `Style` z liczbą rekordów.
6. Stopka pokazuje źródło danych z licencją CC BY 4.0.

## Czego się nauczyliśmy

<!-- Po kroku dopiszcie jedno zdanie: co Was zaskoczyło w danych albo w kodzie. -->
