# Etap 1 – dane → baza ({{app}}, {{code}})

> Pilot: skopiuj do Kilo Code wszystko od „CEL” do końca. Kontroler danych: przygotuj narzędzia z sekcji GOTOWE GDY.

## CEL
Wczytaj plik CSV „{{dataset}}” do tabeli `{{table}}` w SQL Server przez Dapper. Import uruchamia się przy starcie aplikacji, gdy tabela jest pusta, i nie dubluje danych przy kolejnym starcie.

## DANE
- Plik: `{{file}}` w katalogu głównym repozytorium. Ścieżka z aplikacji: `Path.Combine(builder.Environment.ContentRootPath, "..", "..", "{{file}}")`.
- Kodowanie: {{encoding}}. Separator: {{separatorText}}. {{headerText}}
- Kolumny do importu (nazwy w pliku dokładnie tak, jak niżej):

{{columnsTable}}

- Kolumny w pliku, których NIE importujemy: {{ignoredColumns}}.
{{dataNotes}}
- Tabela – zapisz ten skrypt w `db/{{code}}_create.sql` i uruchom go przy starcie przed importem:

```sql
{{createTable}}
```

## ZASADY
- W `src/GzmApp/appsettings.json` ustaw `"Scenario": "{{code}}"`.
- Plik czytaj pakietem CsvHelper (jest już w projekcie): `CultureInfo("pl-PL")`, separator jak wyżej. Nie dziel linii przez `Split`.
- Liczby i daty odczytuj z kulturą i formatem z tabeli kolumn.
- Każde zapytanie wysyłaj przez istniejącą klasę `SqlRunner` (loguje SQL), parametry przez Dappera – bez sklejania tekstu SQL.
- `SourceRow` = numer wiersza danych w pliku, od 1, bez nagłówka.
- Tekst przytnij (`Trim`). Puste pole albo wartość, której nie da się odczytać → `NULL`, bez wyjątku. Rekord z co najmniej jednym takim polem to jeden „dziwny” rekord.
- Po imporcie zapisz w logu: liczbę wierszy w tabeli, liczbę „dziwnych” rekordów i czas importu.
- Klasa `CsvImporter` z interfejsem `ICsvImporter`, rejestracja w `Program.cs`. Metody do 20 linii, bez zagnieżdżonych if-ów.

## GOTOWE GDY
1. Rozszerzenie mssql: `SELECT COUNT(*) FROM {{table}}` zwraca {{cp:E1.rows}}.
2. Breakpoint w pętli importu, Watch na wierszu 7: kolumna `{{checkCsv}}` ma tekst {{cp:E1.row7.raw}}.
3. Rozszerzenie mssql: `SELECT {{checkField}} FROM {{table}} WHERE SourceRow = 7` zwraca {{cp:E1.row7.value}}.
4. Log importu w terminalu: „dziwnych” rekordów jest {{cp:E1.weird}}.
5. Po ponownym uruchomieniu aplikacji `COUNT(*)` nadal zwraca {{cp:E1.rows}}.

Nie zmieniaj plików poza: `db/{{code}}_create.sql`, `src/GzmApp/Data/CsvImporter.cs`, `src/GzmApp/Program.cs`, `src/GzmApp/appsettings.json`. Nie dodawaj pakietów.
