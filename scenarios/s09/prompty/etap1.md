# Etap 1 – dane → baza (ArcheoDąbrowa, s09)

> Pilot: skopiuj do Kilo Code wszystko od „CEL” do końca. Kontroler danych: przygotuj narzędzia z sekcji GOTOWE GDY.

## CEL
Wczytaj plik CSV „Dąbrowa Górnicza: zabytki archeologiczne” do tabeli `ArchaeologicalSites` w SQL Server przez Dapper. Import uruchamia się przy starcie aplikacji, gdy tabela jest pusta, i nie dubluje danych przy kolejnym starcie.

## DANE
- Plik: `data/raw/s09_dabrowa-zabytki-archeologiczne.csv` w katalogu głównym repozytorium. Ścieżka z aplikacji: `Path.Combine(builder.Environment.ContentRootPath, "..", "..", "data/raw/s09_dabrowa-zabytki-archeologiczne.csv")`.
- Kodowanie: UTF-8. Separator: przecinek `,`. Nagłówek w pierwszej linii.
- Kolumny do importu (nazwy w pliku dokładnie tak, jak niżej):

| Kolumna w pliku | Pole w tabeli | Typ SQL | Kultura | Format | Może być puste |
|---|---|---|---|---|---|
| `archaeologicalSiteNumberInArea` | `AzpArea` | `nvarchar(10)` | pl-PL | – | nie |
| `archaeologicalSiteNumberInCity` | `SiteNumberInCity` | `int` | pl-PL | – | nie |
| `azpnumber` | `AzpNumber` | `int` | pl-PL | – | nie |
| `chronology` | `Chronology` | `nvarchar(200)` | pl-PL | – | tak |
| `comment` | `Comment` | `nvarchar(400)` | pl-PL | – | tak |
| `id` | `Id` | `int` | pl-PL | – | nie |
| `localName` | `LocalName` | `nvarchar(200)` | pl-PL | – | nie |

- Kolumny w pliku, których NIE importujemy: `azpsiteNumber`, `city`, `commune`, `county`, `expungeDate`, `expungeDecisionNumber`, `originalFunction`, `protectionForm`, `registrationDate`, `registryNumber`, `type`.
- Klucz naturalny (unikalny): `Id`.
- Tabela – zapisz ten skrypt w `db/s09_create.sql` i uruchom go przy starcie przed importem:

```sql
IF OBJECT_ID(N'dbo.ArchaeologicalSites') IS NULL
CREATE TABLE dbo.[ArchaeologicalSites] (
    [RecordId] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_ArchaeologicalSites] PRIMARY KEY,
    [SourceRow] int NOT NULL,
    [AzpArea] nvarchar(10) NOT NULL,
    [SiteNumberInCity] int NOT NULL,
    [AzpNumber] int NOT NULL,
    [Chronology] nvarchar(200) NULL,
    [Comment] nvarchar(400) NULL,
    [Id] int NOT NULL,
    [LocalName] nvarchar(200) NOT NULL,
    CONSTRAINT [UQ_ArchaeologicalSites_Key] UNIQUE ([Id])
);
```

## ZASADY
- W `src/GzmApp/appsettings.json` ustaw `"Scenario": "s09"`.
- Plik czytaj pakietem CsvHelper (jest już w projekcie): `CultureInfo("pl-PL")`, separator jak wyżej. Nie dziel linii przez `Split`.
- Liczby i daty odczytuj z kulturą i formatem z tabeli kolumn.
- Każde zapytanie wysyłaj przez istniejącą klasę `SqlRunner` (loguje SQL), parametry przez Dappera – bez sklejania tekstu SQL.
- `SourceRow` = numer wiersza danych w pliku, od 1, bez nagłówka.
- Tekst przytnij (`Trim`). Puste pole albo wartość, której nie da się odczytać → `NULL`, bez wyjątku. Rekord z co najmniej jednym takim polem to jeden „dziwny” rekord.
- Po imporcie zapisz w logu: liczbę wierszy w tabeli, liczbę „dziwnych” rekordów i czas importu.
- Klasa `CsvImporter` z interfejsem `ICsvImporter`, rejestracja w `Program.cs`. Metody do 20 linii, bez zagnieżdżonych if-ów.

## GOTOWE GDY
1. Rozszerzenie mssql: `SELECT COUNT(*) FROM ArchaeologicalSites` zwraca **135** (E1.rows).
2. Breakpoint w pętli importu, Watch na wierszu 7: kolumna `chronology` ma tekst **Wczesne średniowiecze - XI-XII/XIIIw.; Okres nowożytny - XVII-XIXw.** (E1.row7.raw).
3. Rozszerzenie mssql: `SELECT Chronology FROM ArchaeologicalSites WHERE SourceRow = 7` zwraca **Wczesne średniowiecze - XI-XII/XIIIw.; Okres nowożytny - XVII-XIXw.** (E1.row7.value).
4. Log importu w terminalu: „dziwnych” rekordów jest **8** (E1.weird).
5. Po ponownym uruchomieniu aplikacji `COUNT(*)` nadal zwraca **135** (E1.rows).

Nie zmieniaj plików poza: `db/s09_create.sql`, `src/GzmApp/Data/CsvImporter.cs`, `src/GzmApp/Program.cs`, `src/GzmApp/appsettings.json`. Nie dodawaj pakietów.
