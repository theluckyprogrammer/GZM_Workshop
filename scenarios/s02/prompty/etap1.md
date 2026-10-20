# Etap 1 – dane → baza (StacjaFinder, s02)

> Pilot: skopiuj do Kilo Code wszystko od „CEL” do końca. Kontroler danych: przygotuj narzędzia z sekcji GOTOWE GDY.

## CEL
Wczytaj plik CSV „Metrorower: stacje aktualne” do tabeli `BikeStations` w SQL Server przez Dapper. Import uruchamia się przy starcie aplikacji, gdy tabela jest pusta, i nie dubluje danych przy kolejnym starcie.

## DANE
- Plik: `data/raw/s02_metrorower-stacje.csv` w katalogu głównym repozytorium. Ścieżka z aplikacji: `Path.Combine(builder.Environment.ContentRootPath, "..", "..", "data/raw/s02_metrorower-stacje.csv")`.
- Kodowanie: UTF-8. Separator: przecinek `,`. Nagłówek w pierwszej linii.
- Kolumny do importu (nazwy w pliku dokładnie tak, jak niżej):

| Kolumna w pliku | Pole w tabeli | Typ SQL | Kultura | Format | Może być puste |
|---|---|---|---|---|---|
| `Nazwa` | `Name` | `nvarchar(20)` | pl-PL | – | nie |
| `Numer` | `Number` | `int` | pl-PL | – | nie |
| `Obszar` | `Area` | `nvarchar(50)` | pl-PL | – | nie |
| `# bazowa` | `BaseBikes` | `int` | pl-PL | – | nie |
| `# stojaków` | `Racks` | `int` | pl-PL | – | nie |
| `Dostępna` | `Available` | `bit` | pl-PL | „Tak” = 1, „Nie” = 0 | nie |
| `Lat` | `Lat` | `decimal(12,9)` | invariant | – | nie |
| `Lng` | `Lng` | `decimal(12,9)` | invariant | – | nie |

- Kolumny w pliku, których NIE importujemy: `Typ`.
- Klucz naturalny (unikalny): `Number`.
- Tabela – zapisz ten skrypt w `db/s02_create.sql` i uruchom go przy starcie przed importem:

```sql
IF OBJECT_ID(N'dbo.BikeStations') IS NULL
CREATE TABLE dbo.[BikeStations] (
    [RecordId] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_BikeStations] PRIMARY KEY,
    [SourceRow] int NOT NULL,
    [Name] nvarchar(20) NOT NULL,
    [Number] int NOT NULL,
    [Area] nvarchar(50) NOT NULL,
    [BaseBikes] int NOT NULL,
    [Racks] int NOT NULL,
    [Available] bit NOT NULL,
    [Lat] decimal(12,9) NOT NULL,
    [Lng] decimal(12,9) NOT NULL,
    CONSTRAINT [UQ_BikeStations_Key] UNIQUE ([Number])
);
```

## ZASADY
- W `src/GzmApp/appsettings.json` ustaw `"Scenario": "s02"`.
- Plik czytaj pakietem CsvHelper (jest już w projekcie): `CultureInfo("pl-PL")`, separator jak wyżej. Nie dziel linii przez `Split`.
- Liczby i daty odczytuj z kulturą i formatem z tabeli kolumn.
- Każde zapytanie wysyłaj przez istniejącą klasę `SqlRunner` (loguje SQL), parametry przez Dappera – bez sklejania tekstu SQL.
- `SourceRow` = numer wiersza danych w pliku, od 1, bez nagłówka.
- Tekst przytnij (`Trim`). Puste pole albo wartość, której nie da się odczytać → `NULL`, bez wyjątku. Rekord z co najmniej jednym takim polem to jeden „dziwny” rekord.
- Po imporcie zapisz w logu: liczbę wierszy w tabeli, liczbę „dziwnych” rekordów i czas importu.
- Klasa `CsvImporter` z interfejsem `ICsvImporter`, rejestracja w `Program.cs`. Metody do 20 linii, bez zagnieżdżonych if-ów.

## GOTOWE GDY
1. Rozszerzenie mssql: `SELECT COUNT(*) FROM BikeStations` zwraca **945** (E1.rows).
2. Breakpoint w pętli importu, Watch na wierszu 7: kolumna `Lat` ma tekst **50.275952** (E1.row7.raw).
3. Rozszerzenie mssql: `SELECT Lat FROM BikeStations WHERE SourceRow = 7` zwraca **50.275952000** (E1.row7.value).
4. Log importu w terminalu: „dziwnych” rekordów jest **0** (E1.weird).
5. Po ponownym uruchomieniu aplikacji `COUNT(*)` nadal zwraca **945** (E1.rows).

Nie zmieniaj plików poza: `db/s02_create.sql`, `src/GzmApp/Data/CsvImporter.cs`, `src/GzmApp/Program.cs`, `src/GzmApp/appsettings.json`. Nie dodawaj pakietów.
