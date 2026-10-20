# Etap 1 – dane → baza (SekcjaSCK, s13)

> Pilot: skopiuj do Kilo Code wszystko od „CEL” do końca. Kontroler danych: przygotuj narzędzia z sekcji GOTOWE GDY.

## CEL
Wczytaj plik CSV „SCK Siemianowice: zajęcia sekcyjne” do tabeli `CultureClasses` w SQL Server przez Dapper. Import uruchamia się przy starcie aplikacji, gdy tabela jest pusta, i nie dubluje danych przy kolejnym starcie.

## DANE
- Plik: `data/raw/s13_sck-zajecia-sekcyjne.csv` w katalogu głównym repozytorium. Ścieżka z aplikacji: `Path.Combine(builder.Environment.ContentRootPath, "..", "..", "data/raw/s13_sck-zajecia-sekcyjne.csv")`.
- Kodowanie: UTF-8. Separator: przecinek `,`. Nagłówek w pierwszej linii.
- Kolumny do importu (nazwy w pliku dokładnie tak, jak niżej):

| Kolumna w pliku | Pole w tabeli | Typ SQL | Kultura | Format | Może być puste |
|---|---|---|---|---|---|
| `classType` | `ClassType` | `nvarchar(40)` | pl-PL | – | nie |
| `day` | `Day` | `nvarchar(40)` | pl-PL | – | tak |
| `id` | `Id` | `int` | pl-PL | – | nie |
| `minimalAge` | `MinAge` | `decimal(4,1)` | invariant | – | tak |
| `room` | `Room` | `nvarchar(60)` | pl-PL | – | tak |
| `section` | `Section` | `nvarchar(80)` | pl-PL | – | nie |
| `startHour` | `StartHour` | `time(0)` | pl-PL | `HH:mm` albo `HH.mm` | tak |
| `unit` | `Unit` | `nvarchar(30)` | pl-PL | – | nie |

- Kolumny w pliku, których NIE importujemy: `additionalInfo`, `additionalInfoAboutParticipants`, `endHour`, `maximalAge`.
- Klucz naturalny (unikalny): `Id`.
- Tabela – zapisz ten skrypt w `db/s13_create.sql` i uruchom go przy starcie przed importem:

```sql
IF OBJECT_ID(N'dbo.CultureClasses') IS NULL
CREATE TABLE dbo.[CultureClasses] (
    [RecordId] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_CultureClasses] PRIMARY KEY,
    [SourceRow] int NOT NULL,
    [ClassType] nvarchar(40) NOT NULL,
    [Day] nvarchar(40) NULL,
    [Id] int NOT NULL,
    [MinAge] decimal(4,1) NULL,
    [Room] nvarchar(60) NULL,
    [Section] nvarchar(80) NOT NULL,
    [StartHour] time(0) NULL,
    [Unit] nvarchar(30) NOT NULL,
    CONSTRAINT [UQ_CultureClasses_Key] UNIQUE ([Id])
);
```

## ZASADY
- W `src/GzmApp/appsettings.json` ustaw `"Scenario": "s13"`.
- Plik czytaj pakietem CsvHelper (jest już w projekcie): `CultureInfo("pl-PL")`, separator jak wyżej. Nie dziel linii przez `Split`.
- Liczby i daty odczytuj z kulturą i formatem z tabeli kolumn.
- Każde zapytanie wysyłaj przez istniejącą klasę `SqlRunner` (loguje SQL), parametry przez Dappera – bez sklejania tekstu SQL.
- `SourceRow` = numer wiersza danych w pliku, od 1, bez nagłówka.
- Tekst przytnij (`Trim`). Puste pole albo wartość, której nie da się odczytać → `NULL`, bez wyjątku. Rekord z co najmniej jednym takim polem to jeden „dziwny” rekord.
- Po imporcie zapisz w logu: liczbę wierszy w tabeli, liczbę „dziwnych” rekordów i czas importu.
- Klasa `CsvImporter` z interfejsem `ICsvImporter`, rejestracja w `Program.cs`. Metody do 20 linii, bez zagnieżdżonych if-ów.

## GOTOWE GDY
1. Rozszerzenie mssql: `SELECT COUNT(*) FROM CultureClasses` zwraca **38** (E1.rows).
2. Breakpoint w pętli importu, Watch na wierszu 7: kolumna `minimalAge` ma tekst **5** (E1.row7.raw).
3. Rozszerzenie mssql: `SELECT MinAge FROM CultureClasses WHERE SourceRow = 7` zwraca **5.0** (E1.row7.value).
4. Log importu w terminalu: „dziwnych” rekordów jest **23** (E1.weird).
5. Po ponownym uruchomieniu aplikacji `COUNT(*)` nadal zwraca **38** (E1.rows).

Nie zmieniaj plików poza: `db/s13_create.sql`, `src/GzmApp/Data/CsvImporter.cs`, `src/GzmApp/Program.cs`, `src/GzmApp/appsettings.json`. Nie dodawaj pakietów.
