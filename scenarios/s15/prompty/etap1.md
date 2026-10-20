# Etap 1 – dane → baza (KlubKatowice, s15)

> Pilot: skopiuj do Kilo Code wszystko od „CEL” do końca. Kontroler danych: przygotuj narzędzia z sekcji GOTOWE GDY.

## CEL
Wczytaj plik CSV „Katowice: uczniowskie kluby sportowe” do tabeli `SchoolSportsClubs` w SQL Server przez Dapper. Import uruchamia się przy starcie aplikacji, gdy tabela jest pusta, i nie dubluje danych przy kolejnym starcie.

## DANE
- Plik: `data/raw/s15_katowice-uczniowskie-kluby-sportowe.csv` w katalogu głównym repozytorium. Ścieżka z aplikacji: `Path.Combine(builder.Environment.ContentRootPath, "..", "..", "data/raw/s15_katowice-uczniowskie-kluby-sportowe.csv")`.
- Kodowanie: UTF-8. Separator: przecinek `,`. Nagłówek w pierwszej linii.
- Kolumny do importu (nazwy w pliku dokładnie tak, jak niżej):

| Kolumna w pliku | Pole w tabeli | Typ SQL | Kultura | Format | Może być puste |
|---|---|---|---|---|---|
| `id` | `Id` | `nvarchar(10)` | pl-PL | – | nie |
| `name` | `Name` | `nvarchar(150)` | pl-PL | – | nie |
| `registrationDate` | `RegisteredOn` | `date` | pl-PL | milisekundy od 1970-01-01 UTC | tak |
| `residenceAddressstreetName` | `Street` | `nvarchar(40)` | pl-PL | – | nie |
| `status` | `Status` | `nvarchar(20)` | pl-PL | – | nie |

- Kolumny w pliku, których NIE importujemy: `area`, `comments`, `dateOfStatutesEnactment`, `expungeDate`, `purpose`, `registrationNumber`, `residenceAddressadditionalInformation`, `residenceAddressbuildingNumber`, `residenceAddresscity`, `residenceAddressflatNumber`, `residenceAddresspostalCode`, `residenceAddressstreetPrefix`, `residenceName`, `statutes`, `statutesChangeDate`, `type`.
- Klucz naturalny (unikalny): `Id`.
- Tabela – zapisz ten skrypt w `db/s15_create.sql` i uruchom go przy starcie przed importem:

```sql
IF OBJECT_ID(N'dbo.SchoolSportsClubs') IS NULL
CREATE TABLE dbo.[SchoolSportsClubs] (
    [RecordId] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_SchoolSportsClubs] PRIMARY KEY,
    [SourceRow] int NOT NULL,
    [Id] nvarchar(10) NOT NULL,
    [Name] nvarchar(150) NOT NULL,
    [RegisteredOn] date NULL,
    [Street] nvarchar(40) NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    CONSTRAINT [UQ_SchoolSportsClubs_Key] UNIQUE ([Id])
);
```

## ZASADY
- W `src/GzmApp/appsettings.json` ustaw `"Scenario": "s15"`.
- Plik czytaj pakietem CsvHelper (jest już w projekcie): `CultureInfo("pl-PL")`, separator jak wyżej. Nie dziel linii przez `Split`.
- Liczby i daty odczytuj z kulturą i formatem z tabeli kolumn.
- Każde zapytanie wysyłaj przez istniejącą klasę `SqlRunner` (loguje SQL), parametry przez Dappera – bez sklejania tekstu SQL.
- `SourceRow` = numer wiersza danych w pliku, od 1, bez nagłówka.
- Tekst przytnij (`Trim`). Puste pole albo wartość, której nie da się odczytać → `NULL`, bez wyjątku. Rekord z co najmniej jednym takim polem to jeden „dziwny” rekord.
- Po imporcie zapisz w logu: liczbę wierszy w tabeli, liczbę „dziwnych” rekordów i czas importu.
- Klasa `CsvImporter` z interfejsem `ICsvImporter`, rejestracja w `Program.cs`. Metody do 20 linii, bez zagnieżdżonych if-ów.

## GOTOWE GDY
1. Rozszerzenie mssql: `SELECT COUNT(*) FROM SchoolSportsClubs` zwraca **94** (E1.rows).
2. Breakpoint w pętli importu, Watch na wierszu 7: kolumna `registrationDate` ma tekst **995846400000** (E1.row7.raw).
3. Rozszerzenie mssql: `SELECT RegisteredOn FROM SchoolSportsClubs WHERE SourceRow = 7` zwraca **2001-07-23** (E1.row7.value).
4. Log importu w terminalu: „dziwnych” rekordów jest **2** (E1.weird).
5. Po ponownym uruchomieniu aplikacji `COUNT(*)` nadal zwraca **94** (E1.rows).

Nie zmieniaj plików poza: `db/s15_create.sql`, `src/GzmApp/Data/CsvImporter.cs`, `src/GzmApp/Program.cs`, `src/GzmApp/appsettings.json`. Nie dodawaj pakietów.
