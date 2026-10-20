# Zadanie * – opis rekordu przez SOL-6 (ArcheoDąbrowa, s09)

> Dla par, które skończyły etap 4. Pilot: skopiuj do Kilo Code wszystko od „CEL” do końca.

## CEL
Przycisk „✨ Opisz” przy rekordzie na liście. Po kliknięciu pod rekordem pojawia się opis od modelu SOL-6. Zadanie dla modelu: Opisz w jednym zdaniu to stanowisko i jego chronologię. Korzystaj tylko z rekordu.

## DANE
- Rekord z tabeli `ArchaeologicalSites` wybrany po `RecordId`. Do modelu wyślij pola: `AzpArea`, `SiteNumberInCity`, `AzpNumber`, `Chronology`, `Comment`, `Id`, `LocalName`.
- Konfiguracja w `src/GzmApp/appsettings.json`: `Sol6:Endpoint`, `Sol6:Model`, `Sol6:Kind` (`openai` albo `azure`). Wartości poda prowadzący.
- Klucz `Sol6:ApiKey` tylko w `dotnet user-secrets` albo w zmiennej środowiskowej `SOL6__APIKEY`. Nigdy w pliku w repozytorium.

## ZASADY
- `IAiSummarizer` i `AiSummarizer` w `src/GzmApp/Ai/AiSummarizer.cs` na `IChatClient`: pakiety `OpenAI` i `Microsoft.Extensions.AI.OpenAI` (`AsIChatClient()`), dla `azure`: `AzureOpenAIClient` z `AzureKeyCredential`. Pakiety są już w projekcie.
- Jedno wywołanie modelu na jedno kliknięcie. Prompt systemowy: „Odpowiadaj tylko na podstawie przekazanych rekordów, nie wymyślaj faktów.”
- Handler `OnGetPodsumuj(int id)` w `Index.cshtml.cs` zwraca fragment HTML `<p>` z opisem.
- Przycisk w `_Lista.cshtml`: `hx-get="?handler=Podsumuj&id=@r.RecordId" hx-target="#opis-@r.RecordId"` i pusty `<p id="opis-@r.RecordId">`.
- Brak klucza: fragment „Brak klucza SOL-6 – poproś prowadzącego”, bez wyjątku. Błąd modelu: krótki komunikat we fragmencie.
- Rejestracja w `Program.cs`. Metody do 20 linii, bez zagnieżdżonych if-ów.

## GOTOWE GDY
1. Kliknij „✨ Opisz” przy rekordzie z wiersza 7 (**Osada; Osada** (E4.title)): opis pojawia się bez przeładowania strony.
2. Zakładka Network: `?handler=Podsumuj` ma status 200, odpowiedź to `<p>` bez `<html>`.
3. Opis nie zawiera faktów spoza rekordu. Porównaj z rozszerzeniem mssql: `SELECT * FROM ArchaeologicalSites WHERE SourceRow = 7`.
4. Bez klucza (`dotnet user-secrets remove "Sol6:ApiKey"`) pojawia się „Brak klucza SOL-6 – poproś prowadzącego”, a aplikacja działa dalej.

Nie zmieniaj plików poza: `src/GzmApp/Ai/AiSummarizer.cs`, `src/GzmApp/Pages/Index.cshtml.cs`, `src/GzmApp/Pages/Shared/_Lista.cshtml`, `src/GzmApp/Program.cs`, `src/GzmApp/appsettings.json`. Nie dodawaj pakietów.
