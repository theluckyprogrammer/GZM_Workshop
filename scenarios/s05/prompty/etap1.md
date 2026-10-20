# Etap 1 – dane → baza (ZgubaDąbrowa, s05)

> Pilot: skopiuj do Kilo Code wszystko od „CEL” do końca. Kontroler danych: przygotuj narzędzia z sekcji GOTOWE GDY.

## CEL
Wczytaj plik CSV „Dąbrowa Górnicza: rzeczy znalezione” do tabeli `DabrowaLostItems` w SQL Server przez Dapper. Import uruchamia się przy starcie aplikacji, gdy tabela jest pusta, i nie dubluje danych przy kolejnym starcie.

## DANE
- Plik: `data/raw/s05_dabrowa-rzeczy-znalezione.csv` w katalogu głównym repozytorium. Ścieżka z aplikacji: `Path.Combine(builder.Environment.ContentRootPath, "..", "..", "data/raw/s05_dabrowa-rzeczy-znalezione.csv")`.
- Kodowanie: UTF-8. Separator: przecinek `,`. Nagłówek w pierwszej linii.
- Kolumny do importu (nazwy w pliku dokładnie tak, jak niżej):

| Kolumna w pliku | Pole w tabeli | Typ SQL | Kultura | Format | Może być puste |
|---|---|---|---|---|---|
| `collectionDate` | `CollectionDate` | `date` | pl-PL | milisekundy od 1970-01-01 UTC | nie |
| `foundDate` | `FoundDate` | `date` | pl-PL | milisekundy od 1970-01-01 UTC | nie |
| `id` | `Id` | `nvarchar(30)` | pl-PL | – | nie |
| `itemName` | `ItemName` | `nvarchar(60)` | pl-PL | – | nie |

- Kolumny w pliku, których NIE importujemy: `comments`, `demand`, `documentTransferDate`, `entryDate`, `giveOutDate`, `issueNumber`, `letterNumber`, `placing`, `status`, `storageLocation`, `year`.
- Klucz naturalny (unikalny): `Id`.
- Tabela – zapisz ten skrypt w `db/s05_create.sql` i uruchom go przy starcie przed importem:

```sql
IF OBJECT_ID(N'dbo.DabrowaLostItems') IS NULL
CREATE TABLE dbo.[DabrowaLostItems] (
    [RecordId] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_DabrowaLostItems] PRIMARY KEY,
    [SourceRow] int NOT NULL,
    [CollectionDate] date NOT NULL,
    [FoundDate] date NOT NULL,
    [Id] nvarchar(30) NOT NULL,
    [ItemName] nvarchar(60) NOT NULL,
    CONSTRAINT [UQ_DabrowaLostItems_Key] UNIQUE ([Id])
);
```

## ZASADY
- W `src/GzmApp/appsettings.json` ustaw `"Scenario": "s05"`.
- Plik czytaj pakietem CsvHelper (jest już w projekcie): `CultureInfo("pl-PL")`, separator jak wyżej. Nie dziel linii przez `Split`.
- Liczby i daty odczytuj z kulturą i formatem z tabeli kolumn.
- Każde zapytanie wysyłaj przez istniejącą klasę `SqlRunner` (loguje SQL), parametry przez Dappera – bez sklejania tekstu SQL.
- `SourceRow` = numer wiersza danych w pliku, od 1, bez nagłówka.
- Tekst przytnij (`Trim`). Puste pole albo wartość, której nie da się odczytać → `NULL`, bez wyjątku. Rekord z co najmniej jednym takim polem to jeden „dziwny” rekord.
- Po imporcie zapisz w logu: liczbę wierszy w tabeli, liczbę „dziwnych” rekordów i czas importu.
- Klasa `CsvImporter` z interfejsem `ICsvImporter`, rejestracja w `Program.cs`. Metody do 20 linii, bez zagnieżdżonych if-ów.

## GOTOWE GDY
1. Rozszerzenie mssql: `SELECT COUNT(*) FROM DabrowaLostItems` zwraca **64** (E1.rows).
2. Breakpoint w pętli importu, Watch na wierszu 7: kolumna `foundDate` ma tekst **1732147200000** (E1.row7.raw).
3. Rozszerzenie mssql: `SELECT FoundDate FROM DabrowaLostItems WHERE SourceRow = 7` zwraca **2024-11-21** (E1.row7.value).
4. Log importu w terminalu: „dziwnych” rekordów jest **0** (E1.weird).
5. Po ponownym uruchomieniu aplikacji `COUNT(*)` nadal zwraca **64** (E1.rows).

Nie zmieniaj plików poza: `db/s05_create.sql`, `src/GzmApp/Data/CsvImporter.cs`, `src/GzmApp/Program.cs`, `src/GzmApp/appsettings.json`. Nie dodawaj pakietów.
