# Etap 1 – dane → baza (RezerwacjaRadar, s08)

> Pilot: skopiuj do Kilo Code wszystko od „CEL” do końca. Kontroler danych: przygotuj narzędzia z sekcji GOTOWE GDY.

## CEL
Wczytaj plik CSV „Metrorower: rezerwacje” do tabeli `BikeReservations` w SQL Server przez Dapper. Import uruchamia się przy starcie aplikacji, gdy tabela jest pusta, i nie dubluje danych przy kolejnym starcie.

## DANE
- Plik: `data/raw/s08_metrorower-rezerwacje.csv` w katalogu głównym repozytorium. Ścieżka z aplikacji: `Path.Combine(builder.Environment.ContentRootPath, "..", "..", "data/raw/s08_metrorower-rezerwacje.csv")`.
- Kodowanie: UTF-8. Separator: przecinek `,`. Nagłówek w pierwszej linii.
- Kolumny do importu (nazwy w pliku dokładnie tak, jak niżej):

| Kolumna w pliku | Pole w tabeli | Typ SQL | Kultura | Format | Może być puste |
|---|---|---|---|---|---|
| `Data` | `ReservedAt` | `datetime2(0)` | invariant | `yyyy-MM-dd'T'HH:mm:ss'Z'` | nie |
| `Wypożyczono` | `Rented` | `bit` | pl-PL | „true” = 1, „false” = 0 | nie |
| `Ilość wypożyczonych rowerów` | `RentedBikes` | `int` | pl-PL | – | nie |
| `Ilość zarezerwowanych rowerów` | `ReservedBikes` | `int` | pl-PL | – | nie |
| `Numer stacji rezerwacji` | `StationNumber` | `int` | pl-PL | – | tak |
| `Nazwa stacji rezerwacji` | `StationName` | `nvarchar(30)` | pl-PL | – | nie |
| `Kanał` | `Channel` | `nvarchar(30)` | pl-PL | – | nie |
| `Status` | `Status` | `nvarchar(20)` | pl-PL | – | nie |
| `Czas wygaśnięcia` | `ExpiresAt` | `datetime2(0)` | invariant | `yyyy-MM-dd'T'HH:mm:ss'Z'` | nie |

- Kolumny w pliku, których NIE importujemy: brak.
- Klucz: `RowHash` = SHA-256 z wartości wszystkich importowanych pól (po `Trim`) połączonych znakiem `\u001F`, zapisany jako `binary(32)`.
- W pliku są identyczne wiersze (drugie z par: 63, 74). Wstaw tylko pierwszy z nich – pomiń wiersz, którego `RowHash` już jest w tabeli.
- Tabela – zapisz ten skrypt w `db/s08_create.sql` i uruchom go przy starcie przed importem:

```sql
IF OBJECT_ID(N'dbo.BikeReservations') IS NULL
CREATE TABLE dbo.[BikeReservations] (
    [RecordId] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_BikeReservations] PRIMARY KEY,
    [SourceRow] int NOT NULL,
    [ReservedAt] datetime2(0) NOT NULL,
    [Rented] bit NOT NULL,
    [RentedBikes] int NOT NULL,
    [ReservedBikes] int NOT NULL,
    [StationNumber] int NULL,
    [StationName] nvarchar(30) NOT NULL,
    [Channel] nvarchar(30) NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [ExpiresAt] datetime2(0) NOT NULL,
    [RowHash] binary(32) NOT NULL,
    CONSTRAINT [UQ_BikeReservations_Key] UNIQUE ([RowHash])
);
```

## ZASADY
- W `src/GzmApp/appsettings.json` ustaw `"Scenario": "s08"`.
- Plik czytaj pakietem CsvHelper (jest już w projekcie): `CultureInfo("pl-PL")`, separator jak wyżej. Nie dziel linii przez `Split`.
- Liczby i daty odczytuj z kulturą i formatem z tabeli kolumn.
- Każde zapytanie wysyłaj przez istniejącą klasę `SqlRunner` (loguje SQL), parametry przez Dappera – bez sklejania tekstu SQL.
- `SourceRow` = numer wiersza danych w pliku, od 1, bez nagłówka.
- Tekst przytnij (`Trim`). Puste pole albo wartość, której nie da się odczytać → `NULL`, bez wyjątku. Rekord z co najmniej jednym takim polem to jeden „dziwny” rekord.
- Po imporcie zapisz w logu: liczbę wierszy w tabeli, liczbę „dziwnych” rekordów i czas importu.
- Klasa `CsvImporter` z interfejsem `ICsvImporter`, rejestracja w `Program.cs`. Metody do 20 linii, bez zagnieżdżonych if-ów.

## GOTOWE GDY
1. Rozszerzenie mssql: `SELECT COUNT(*) FROM BikeReservations` zwraca **98** (E1.rows).
2. Breakpoint w pętli importu, Watch na wierszu 7: kolumna `Data` ma tekst **2026-10-06T14:55:37Z** (E1.row7.raw).
3. Rozszerzenie mssql: `SELECT ReservedAt FROM BikeReservations WHERE SourceRow = 7` zwraca **2026-10-06 14:55:37** (E1.row7.value).
4. Log importu w terminalu: „dziwnych” rekordów jest **12** (E1.weird).
5. Po ponownym uruchomieniu aplikacji `COUNT(*)` nadal zwraca **98** (E1.rows).

Nie zmieniaj plików poza: `db/s08_create.sql`, `src/GzmApp/Data/CsvImporter.cs`, `src/GzmApp/Program.cs`, `src/GzmApp/appsettings.json`. Nie dodawaj pakietów.
