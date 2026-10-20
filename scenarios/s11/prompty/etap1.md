# Etap 1 – dane → baza (PamięćKatowic, s11)

> Pilot: skopiuj do Kilo Code wszystko od „CEL” do końca. Kontroler danych: przygotuj narzędzia z sekcji GOTOWE GDY.

## CEL
Wczytaj plik CSV „Katowice: miejsca pamięci narodowej” do tabeli `KatowiceMemorials` w SQL Server przez Dapper. Import uruchamia się przy starcie aplikacji, gdy tabela jest pusta, i nie dubluje danych przy kolejnym starcie.

## DANE
- Plik: `data/raw/s11_katowice-miejsca-pamieci.csv` w katalogu głównym repozytorium. Ścieżka z aplikacji: `Path.Combine(builder.Environment.ContentRootPath, "..", "..", "data/raw/s11_katowice-miejsca-pamieci.csv")`.
- Kodowanie: UTF-8. Separator: przecinek `,`. Nagłówek w pierwszej linii.
- Kolumny do importu (nazwy w pliku dokładnie tak, jak niżej):

| Kolumna w pliku | Pole w tabeli | Typ SQL | Kultura | Format | Może być puste |
|---|---|---|---|---|---|
| `addressstreetName` | `Street` | `nvarchar(80)` | pl-PL | – | tak |
| `description` | `Description` | `nvarchar(600)` | pl-PL | – | nie |
| `recordNumber` | `RecordNumber` | `int` | pl-PL | – | nie |
| `typeOfObject` | `ObjectType` | `nvarchar(30)` | pl-PL | – | nie |

- Kolumny w pliku, których NIE importujemy: `addressadditionalInformation`, `addressbuildingNumber`, `addresscity`, `addressflatNumber`, `addresspostalCode`, `addressstreetPrefix`, `author`, `cemeteryRow`, `cemeterySide`, `cemeterySpot`, `comments`, `creationDate`, `custodian`, `founder`, `graveNumber`, `inscription`, `locationName`, `materialType`, `size`, `status`.
- Klucz naturalny (unikalny): `RecordNumber`.
- Tabela – zapisz ten skrypt w `db/s11_create.sql` i uruchom go przy starcie przed importem:

```sql
IF OBJECT_ID(N'dbo.KatowiceMemorials') IS NULL
CREATE TABLE dbo.[KatowiceMemorials] (
    [RecordId] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_KatowiceMemorials] PRIMARY KEY,
    [SourceRow] int NOT NULL,
    [Street] nvarchar(80) NULL,
    [Description] nvarchar(600) NOT NULL,
    [RecordNumber] int NOT NULL,
    [ObjectType] nvarchar(30) NOT NULL,
    CONSTRAINT [UQ_KatowiceMemorials_Key] UNIQUE ([RecordNumber])
);
```

## ZASADY
- W `src/GzmApp/appsettings.json` ustaw `"Scenario": "s11"`.
- Plik czytaj pakietem CsvHelper (jest już w projekcie): `CultureInfo("pl-PL")`, separator jak wyżej. Nie dziel linii przez `Split`.
- Liczby i daty odczytuj z kulturą i formatem z tabeli kolumn.
- Każde zapytanie wysyłaj przez istniejącą klasę `SqlRunner` (loguje SQL), parametry przez Dappera – bez sklejania tekstu SQL.
- `SourceRow` = numer wiersza danych w pliku, od 1, bez nagłówka.
- Tekst przytnij (`Trim`). Puste pole albo wartość, której nie da się odczytać → `NULL`, bez wyjątku. Rekord z co najmniej jednym takim polem to jeden „dziwny” rekord.
- Po imporcie zapisz w logu: liczbę wierszy w tabeli, liczbę „dziwnych” rekordów i czas importu.
- Klasa `CsvImporter` z interfejsem `ICsvImporter`, rejestracja w `Program.cs`. Metody do 20 linii, bez zagnieżdżonych if-ów.

## GOTOWE GDY
1. Rozszerzenie mssql: `SELECT COUNT(*) FROM KatowiceMemorials` zwraca **391** (E1.rows).
2. Breakpoint w pętli importu, Watch na wierszu 7: kolumna `addressstreetName` ma tekst **pl. Bolesława Chrobrego** (E1.row7.raw).
3. Rozszerzenie mssql: `SELECT Street FROM KatowiceMemorials WHERE SourceRow = 7` zwraca **pl. Bolesława Chrobrego** (E1.row7.value).
4. Log importu w terminalu: „dziwnych” rekordów jest **11** (E1.weird).
5. Po ponownym uruchomieniu aplikacji `COUNT(*)` nadal zwraca **391** (E1.rows).

Nie zmieniaj plików poza: `db/s11_create.sql`, `src/GzmApp/Data/CsvImporter.cs`, `src/GzmApp/Program.cs`, `src/GzmApp/appsettings.json`. Nie dodawaj pakietów.
