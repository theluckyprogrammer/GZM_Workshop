# Etap 1 – dane → baza (PamięćZabrze, s01)

> Pilot: skopiuj do Kilo Code wszystko od „CEL” do końca. Kontroler danych: przygotuj narzędzia z sekcji GOTOWE GDY.

## CEL
Wczytaj plik CSV „Zabrze: miejsca pamięci” do tabeli `ZabrzeMemorials` w SQL Server przez Dapper. Import uruchamia się przy starcie aplikacji, gdy tabela jest pusta, i nie dubluje danych przy kolejnym starcie.

## DANE
- Plik: `data/raw/s01_zabrze-miejsca-pamieci.csv` w katalogu głównym repozytorium. Ścieżka z aplikacji: `Path.Combine(builder.Environment.ContentRootPath, "..", "..", "data/raw/s01_zabrze-miejsca-pamieci.csv")`.
- Kodowanie: UTF-8. Separator: przecinek `,`. Nagłówek w pierwszej linii.
- Kolumny do importu (nazwy w pliku dokładnie tak, jak niżej):

| Kolumna w pliku | Pole w tabeli | Typ SQL | Kultura | Format | Może być puste |
|---|---|---|---|---|---|
| `addresscity` | `City` | `nvarchar(40)` | pl-PL | – | tak |
| `addressstreetName` | `Street` | `nvarchar(60)` | pl-PL | – | tak |
| `description` | `Description` | `nvarchar(800)` | pl-PL | – | nie |
| `inscription` | `Inscription` | `nvarchar(1500)` | pl-PL | – | tak |
| `locationName` | `LocationName` | `nvarchar(100)` | pl-PL | – | tak |
| `recordNumber` | `RecordNumber` | `nvarchar(20)` | pl-PL | – | nie |

- Kolumny w pliku, których NIE importujemy: `addressadditionalInformation`, `addressbuildingNumber`, `addressflatNumber`, `addresspostalCode`, `addressstreetPrefix`, `author`, `cemeteryRow`, `cemeterySide`, `cemeterySpot`, `comments`, `creationDate`, `custodian`, `founder`, `graveNumber`, `materialType`, `size`, `status`, `typeOfObject`.
- Klucz naturalny (unikalny): `RecordNumber`.
- Tabela – zapisz ten skrypt w `db/s01_create.sql` i uruchom go przy starcie przed importem:

```sql
IF OBJECT_ID(N'dbo.ZabrzeMemorials') IS NULL
CREATE TABLE dbo.[ZabrzeMemorials] (
    [RecordId] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_ZabrzeMemorials] PRIMARY KEY,
    [SourceRow] int NOT NULL,
    [City] nvarchar(40) NULL,
    [Street] nvarchar(60) NULL,
    [Description] nvarchar(800) NOT NULL,
    [Inscription] nvarchar(1500) NULL,
    [LocationName] nvarchar(100) NULL,
    [RecordNumber] nvarchar(20) NOT NULL,
    CONSTRAINT [UQ_ZabrzeMemorials_Key] UNIQUE ([RecordNumber])
);
```

## ZASADY
- W `src/GzmApp/appsettings.json` ustaw `"Scenario": "s01"`.
- Plik czytaj pakietem CsvHelper (jest już w projekcie): `CultureInfo("pl-PL")`, separator jak wyżej. Nie dziel linii przez `Split`.
- Liczby i daty odczytuj z kulturą i formatem z tabeli kolumn.
- Każde zapytanie wysyłaj przez istniejącą klasę `SqlRunner` (loguje SQL), parametry przez Dappera – bez sklejania tekstu SQL.
- `SourceRow` = numer wiersza danych w pliku, od 1, bez nagłówka.
- Tekst przytnij (`Trim`). Puste pole albo wartość, której nie da się odczytać → `NULL`, bez wyjątku. Rekord z co najmniej jednym takim polem to jeden „dziwny” rekord.
- Po imporcie zapisz w logu: liczbę wierszy w tabeli, liczbę „dziwnych” rekordów i czas importu.
- Klasa `CsvImporter` z interfejsem `ICsvImporter`, rejestracja w `Program.cs`. Metody do 20 linii, bez zagnieżdżonych if-ów.

## GOTOWE GDY
1. Rozszerzenie mssql: `SELECT COUNT(*) FROM ZabrzeMemorials` zwraca **58** (E1.rows).
2. Breakpoint w pętli importu, Watch na wierszu 7: kolumna `addresscity` ma tekst **Zabrze** (E1.row7.raw).
3. Rozszerzenie mssql: `SELECT City FROM ZabrzeMemorials WHERE SourceRow = 7` zwraca **Zabrze** (E1.row7.value).
4. Log importu w terminalu: „dziwnych” rekordów jest **20** (E1.weird).
5. Po ponownym uruchomieniu aplikacji `COUNT(*)` nadal zwraca **58** (E1.rows).

Nie zmieniaj plików poza: `db/s01_create.sql`, `src/GzmApp/Data/CsvImporter.cs`, `src/GzmApp/Program.cs`, `src/GzmApp/appsettings.json`. Nie dodawaj pakietów.
