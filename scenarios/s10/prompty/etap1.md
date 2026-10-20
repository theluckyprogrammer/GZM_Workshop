# Etap 1 – dane → baza (BoiskoMap, s10)

> Pilot: skopiuj do Kilo Code wszystko od „CEL” do końca. Kontroler danych: przygotuj narzędzia z sekcji GOTOWE GDY.

## CEL
Wczytaj plik CSV „Gliwice: boiska i stadiony” do tabeli `SportsFields` w SQL Server przez Dapper. Import uruchamia się przy starcie aplikacji, gdy tabela jest pusta, i nie dubluje danych przy kolejnym starcie.

## DANE
- Plik: `data/raw/s10_gliwice-boiska-stadiony.csv` w katalogu głównym repozytorium. Ścieżka z aplikacji: `Path.Combine(builder.Environment.ContentRootPath, "..", "..", "data/raw/s10_gliwice-boiska-stadiony.csv")`.
- Kodowanie: UTF-8. Separator: przecinek `,`. Nagłówek w pierwszej linii.
- Kolumny do importu (nazwy w pliku dokładnie tak, jak niżej):

| Kolumna w pliku | Pole w tabeli | Typ SQL | Kultura | Format | Może być puste |
|---|---|---|---|---|---|
| `FID` | `Fid` | `int` | pl-PL | – | nie |
| `RODZAJ` | `Kind` | `nvarchar(80)` | pl-PL | – | nie |
| `NAZWA` | `Name` | `nvarchar(80)` | pl-PL | – | tak |
| `ADRES` | `Address` | `nvarchar(50)` | pl-PL | – | nie |
| `x` | `X` | `decimal(20,10)` | invariant | – | nie |
| `y` | `Y` | `decimal(20,10)` | invariant | – | nie |

- Kolumny w pliku, których NIE importujemy: `FID_1`, `uwagi`, `TYP`.
- Klucz naturalny (unikalny): `Fid`.
- Tabela – zapisz ten skrypt w `db/s10_create.sql` i uruchom go przy starcie przed importem:

```sql
IF OBJECT_ID(N'dbo.SportsFields') IS NULL
CREATE TABLE dbo.[SportsFields] (
    [RecordId] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_SportsFields] PRIMARY KEY,
    [SourceRow] int NOT NULL,
    [Fid] int NOT NULL,
    [Kind] nvarchar(80) NOT NULL,
    [Name] nvarchar(80) NULL,
    [Address] nvarchar(50) NOT NULL,
    [X] decimal(20,10) NOT NULL,
    [Y] decimal(20,10) NOT NULL,
    CONSTRAINT [UQ_SportsFields_Key] UNIQUE ([Fid])
);
```

## ZASADY
- W `src/GzmApp/appsettings.json` ustaw `"Scenario": "s10"`.
- Plik czytaj pakietem CsvHelper (jest już w projekcie): `CultureInfo("pl-PL")`, separator jak wyżej. Nie dziel linii przez `Split`.
- Liczby i daty odczytuj z kulturą i formatem z tabeli kolumn.
- Każde zapytanie wysyłaj przez istniejącą klasę `SqlRunner` (loguje SQL), parametry przez Dappera – bez sklejania tekstu SQL.
- `SourceRow` = numer wiersza danych w pliku, od 1, bez nagłówka.
- Tekst przytnij (`Trim`). Puste pole albo wartość, której nie da się odczytać → `NULL`, bez wyjątku. Rekord z co najmniej jednym takim polem to jeden „dziwny” rekord.
- Po imporcie zapisz w logu: liczbę wierszy w tabeli, liczbę „dziwnych” rekordów i czas importu.
- Klasa `CsvImporter` z interfejsem `ICsvImporter`, rejestracja w `Program.cs`. Metody do 20 linii, bez zagnieżdżonych if-ów.

## GOTOWE GDY
1. Rozszerzenie mssql: `SELECT COUNT(*) FROM SportsFields` zwraca **121** (E1.rows).
2. Breakpoint w pętli importu, Watch na wierszu 7: kolumna `x` ma tekst **2077273.6463570932** (E1.row7.raw).
3. Rozszerzenie mssql: `SELECT X FROM SportsFields WHERE SourceRow = 7` zwraca **2077273.6463570932** (E1.row7.value).
4. Log importu w terminalu: „dziwnych” rekordów jest **2** (E1.weird).
5. Po ponownym uruchomieniu aplikacji `COUNT(*)` nadal zwraca **121** (E1.rows).

Nie zmieniaj plików poza: `db/s10_create.sql`, `src/GzmApp/Data/CsvImporter.cs`, `src/GzmApp/Program.cs`, `src/GzmApp/appsettings.json`. Nie dodawaj pakietów.
