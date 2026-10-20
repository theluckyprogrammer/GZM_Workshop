# Etap 1 – dane → baza (RowerStat, s03)

> Pilot: skopiuj do Kilo Code wszystko od „CEL” do końca. Kontroler danych: przygotuj narzędzia z sekcji GOTOWE GDY.

## CEL
Wczytaj plik CSV „Dzienna dostępność rowerów w gminach” do tabeli `BikeAvailability` w SQL Server przez Dapper. Import uruchamia się przy starcie aplikacji, gdy tabela jest pusta, i nie dubluje danych przy kolejnym starcie.

## DANE
- Plik: `data/raw/s03_rowery-dostepnosc-gminy.csv` w katalogu głównym repozytorium. Ścieżka z aplikacji: `Path.Combine(builder.Environment.ContentRootPath, "..", "..", "data/raw/s03_rowery-dostepnosc-gminy.csv")`.
- Kodowanie: UTF-8. Separator: przecinek `,`. Nagłówek w pierwszej linii.
- Kolumny do importu (nazwy w pliku dokładnie tak, jak niżej):

| Kolumna w pliku | Pole w tabeli | Typ SQL | Kultura | Format | Może być puste |
|---|---|---|---|---|---|
| `Dzień` | `Day` | `date` | pl-PL | `yyyy-MM-dd` | nie |
| `Gmina` | `Commune` | `nvarchar(40)` | pl-PL | – | nie |
| `Liczba dostępnych rowerów` | `AvailableBikes` | `int` | pl-PL | – | nie |
| `Wymagana liczba dostępnych rowerów` | `RequiredBikes` | `int` | pl-PL | – | nie |
| `Przekroczono` | `Exceeded` | `bit` | pl-PL | „Tak” = 1, „Nie” = 0 | nie |

- Kolumny w pliku, których NIE importujemy: brak.
- Klucz naturalny (unikalny): `Day` + `Commune`.
- Tabela – zapisz ten skrypt w `db/s03_create.sql` i uruchom go przy starcie przed importem:

```sql
IF OBJECT_ID(N'dbo.BikeAvailability') IS NULL
CREATE TABLE dbo.[BikeAvailability] (
    [RecordId] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_BikeAvailability] PRIMARY KEY,
    [SourceRow] int NOT NULL,
    [Day] date NOT NULL,
    [Commune] nvarchar(40) NOT NULL,
    [AvailableBikes] int NOT NULL,
    [RequiredBikes] int NOT NULL,
    [Exceeded] bit NOT NULL,
    CONSTRAINT [UQ_BikeAvailability_Key] UNIQUE ([Day], [Commune])
);
```

## ZASADY
- W `src/GzmApp/appsettings.json` ustaw `"Scenario": "s03"`.
- Plik czytaj pakietem CsvHelper (jest już w projekcie): `CultureInfo("pl-PL")`, separator jak wyżej. Nie dziel linii przez `Split`.
- Liczby i daty odczytuj z kulturą i formatem z tabeli kolumn.
- Każde zapytanie wysyłaj przez istniejącą klasę `SqlRunner` (loguje SQL), parametry przez Dappera – bez sklejania tekstu SQL.
- `SourceRow` = numer wiersza danych w pliku, od 1, bez nagłówka.
- Tekst przytnij (`Trim`). Puste pole albo wartość, której nie da się odczytać → `NULL`, bez wyjątku. Rekord z co najmniej jednym takim polem to jeden „dziwny” rekord.
- Po imporcie zapisz w logu: liczbę wierszy w tabeli, liczbę „dziwnych” rekordów i czas importu.
- Klasa `CsvImporter` z interfejsem `ICsvImporter`, rejestracja w `Program.cs`. Metody do 20 linii, bez zagnieżdżonych if-ów.

## GOTOWE GDY
1. Rozszerzenie mssql: `SELECT COUNT(*) FROM BikeAvailability` zwraca **40** (E1.rows).
2. Breakpoint w pętli importu, Watch na wierszu 7: kolumna `Liczba dostępnych rowerów` ma tekst **400** (E1.row7.raw).
3. Rozszerzenie mssql: `SELECT AvailableBikes FROM BikeAvailability WHERE SourceRow = 7` zwraca **400** (E1.row7.value).
4. Log importu w terminalu: „dziwnych” rekordów jest **0** (E1.weird).
5. Po ponownym uruchomieniu aplikacji `COUNT(*)` nadal zwraca **40** (E1.rows).

Nie zmieniaj plików poza: `db/s03_create.sql`, `src/GzmApp/Data/CsvImporter.cs`, `src/GzmApp/Program.cs`, `src/GzmApp/appsettings.json`. Nie dodawaj pakietów.
