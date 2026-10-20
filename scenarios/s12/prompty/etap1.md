# Etap 1 – dane → baza (ZabytkiBytom, s12)

> Pilot: skopiuj do Kilo Code wszystko od „CEL” do końca. Kontroler danych: przygotuj narzędzia z sekcji GOTOWE GDY.

## CEL
Wczytaj plik CSV „Bytom: zabytki” do tabeli `HistoricBuildings` w SQL Server przez Dapper. Import uruchamia się przy starcie aplikacji, gdy tabela jest pusta, i nie dubluje danych przy kolejnym starcie.

## DANE
- Plik: `data/raw/s12_bytom-zabytki.csv` w katalogu głównym repozytorium. Ścieżka z aplikacji: `Path.Combine(builder.Environment.ContentRootPath, "..", "..", "data/raw/s12_bytom-zabytki.csv")`.
- Kodowanie: UTF-8. Separator: przecinek `,`. Nagłówek w pierwszej linii.
- Kolumny do importu (nazwy w pliku dokładnie tak, jak niżej):

| Kolumna w pliku | Pole w tabeli | Typ SQL | Kultura | Format | Może być puste |
|---|---|---|---|---|---|
| `addressbuildingNumber` | `BuildingNumber` | `nvarchar(10)` | pl-PL | – | tak |
| `addressstreetName` | `Street` | `nvarchar(100)` | pl-PL | – | tak |
| `id` | `Id` | `int` | pl-PL | – | nie |
| `name` | `Name` | `nvarchar(1000)` | pl-PL | – | nie |
| `style` | `Style` | `nvarchar(80)` | pl-PL | – | tak |

- Kolumny w pliku, których NIE importujemy: `additionalInfo`, `addressadditionalInformation`, `addresscity`, `addressflatNumber`, `addresspostalCode`, `addressstreetPrefix`, `area`, `author`, `comment`, `condition`, `coordinates`, `currentFunction`, `expungeDate`, `expungeDecisionNumber`, `historicSiteId`, `material`, `originalFunction`, `originatedPeriod`, `originatedYear`, `owner`, `protectionForm`, `registrationDate`, `registryNumber`, `type`, `unescoId`.
- Klucz naturalny (unikalny): `Id`.
- Tabela – zapisz ten skrypt w `db/s12_create.sql` i uruchom go przy starcie przed importem:

```sql
IF OBJECT_ID(N'dbo.HistoricBuildings') IS NULL
CREATE TABLE dbo.[HistoricBuildings] (
    [RecordId] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_HistoricBuildings] PRIMARY KEY,
    [SourceRow] int NOT NULL,
    [BuildingNumber] nvarchar(10) NULL,
    [Street] nvarchar(100) NULL,
    [Id] int NOT NULL,
    [Name] nvarchar(1000) NOT NULL,
    [Style] nvarchar(80) NULL,
    CONSTRAINT [UQ_HistoricBuildings_Key] UNIQUE ([Id])
);
```

## ZASADY
- W `src/GzmApp/appsettings.json` ustaw `"Scenario": "s12"`.
- Plik czytaj pakietem CsvHelper (jest już w projekcie): `CultureInfo("pl-PL")`, separator jak wyżej. Nie dziel linii przez `Split`.
- Liczby i daty odczytuj z kulturą i formatem z tabeli kolumn.
- Każde zapytanie wysyłaj przez istniejącą klasę `SqlRunner` (loguje SQL), parametry przez Dappera – bez sklejania tekstu SQL.
- `SourceRow` = numer wiersza danych w pliku, od 1, bez nagłówka.
- Tekst przytnij (`Trim`). Puste pole albo wartość, której nie da się odczytać → `NULL`, bez wyjątku. Rekord z co najmniej jednym takim polem to jeden „dziwny” rekord.
- Po imporcie zapisz w logu: liczbę wierszy w tabeli, liczbę „dziwnych” rekordów i czas importu.
- Klasa `CsvImporter` z interfejsem `ICsvImporter`, rejestracja w `Program.cs`. Metody do 20 linii, bez zagnieżdżonych if-ów.

## GOTOWE GDY
1. Rozszerzenie mssql: `SELECT COUNT(*) FROM HistoricBuildings` zwraca **2313** (E1.rows).
2. Breakpoint w pętli importu, Watch na wierszu 7: kolumna `style` ma tekst **Modernizm** (E1.row7.raw).
3. Rozszerzenie mssql: `SELECT Style FROM HistoricBuildings WHERE SourceRow = 7` zwraca **Modernizm** (E1.row7.value).
4. Log importu w terminalu: „dziwnych” rekordów jest **176** (E1.weird).
5. Po ponownym uruchomieniu aplikacji `COUNT(*)` nadal zwraca **2313** (E1.rows).

Nie zmieniaj plików poza: `db/s12_create.sql`, `src/GzmApp/Data/CsvImporter.cs`, `src/GzmApp/Program.cs`, `src/GzmApp/appsettings.json`. Nie dodawaj pakietów.
