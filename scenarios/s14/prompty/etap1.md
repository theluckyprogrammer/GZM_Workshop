# Etap 1 – dane → baza (BiletBot, s14)

> Pilot: skopiuj do Kilo Code wszystko od „CEL” do końca. Kontroler danych: przygotuj narzędzia z sekcji GOTOWE GDY.

## CEL
Wczytaj plik CSV „ZTM: automaty biletowe” do tabeli `TicketMachines` w SQL Server przez Dapper. Import uruchamia się przy starcie aplikacji, gdy tabela jest pusta, i nie dubluje danych przy kolejnym starcie.

## DANE
- Plik: `data/raw/s14_ztm-automaty-biletowe.csv` w katalogu głównym repozytorium. Ścieżka z aplikacji: `Path.Combine(builder.Environment.ContentRootPath, "..", "..", "data/raw/s14_ztm-automaty-biletowe.csv")`.
- Kodowanie: Windows-1250. Separator: średnik `;`. Nagłówek dopiero w linii 5: linie 1–4 to tytuł, pomiń je.
- Kolumny do importu (nazwy w pliku dokładnie tak, jak niżej):

| Kolumna w pliku | Pole w tabeli | Typ SQL | Kultura | Format | Może być puste |
|---|---|---|---|---|---|
| `L.p.` | `No` | `int` | pl-PL | – | nie |
| `Gmina` | `Commune` | `nvarchar(40)` | pl-PL | – | nie |
| `Kod automatu` | `MachineCode` | `nvarchar(20)` | pl-PL | – | nie |
| `NR` | `StopNumber` | `int` | pl-PL | – | tak |
| `Nazwa przystanku` | `StopName` | `nvarchar(80)` | pl-PL | – | nie |
| `Opis lokalizacji` | `LocationDescription` | `nvarchar(150)` | pl-PL | – | tak |
| `Nr stanowiska` | `Platform` | `nvarchar(10)` | pl-PL | – | tak |
| `Współrzędne geograficzne` | `Coordinates` | `nvarchar(60)` | pl-PL | – | tak |

- Kolumny w pliku, których NIE importujemy: (kolumna bez nazwy).
- Klucz naturalny (unikalny): `MachineCode`.
- Pomiń wiersze, w których kolumna `L.p.` jest pusta: to legenda i suma pod tabelą (wiersze 267, 268, 269, 270, 271, 272).
- Tabela – zapisz ten skrypt w `db/s14_create.sql` i uruchom go przy starcie przed importem:

```sql
IF OBJECT_ID(N'dbo.TicketMachines') IS NULL
CREATE TABLE dbo.[TicketMachines] (
    [RecordId] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_TicketMachines] PRIMARY KEY,
    [SourceRow] int NOT NULL,
    [No] int NOT NULL,
    [Commune] nvarchar(40) NOT NULL,
    [MachineCode] nvarchar(20) NOT NULL,
    [StopNumber] int NULL,
    [StopName] nvarchar(80) NOT NULL,
    [LocationDescription] nvarchar(150) NULL,
    [Platform] nvarchar(10) NULL,
    [Coordinates] nvarchar(60) NULL,
    CONSTRAINT [UQ_TicketMachines_Key] UNIQUE ([MachineCode])
);
```

## ZASADY
- W `src/GzmApp/appsettings.json` ustaw `"Scenario": "s14"`.
- Plik czytaj pakietem CsvHelper (jest już w projekcie): `CultureInfo("pl-PL")`, separator jak wyżej. Nie dziel linii przez `Split`.
- Liczby i daty odczytuj z kulturą i formatem z tabeli kolumn.
- Każde zapytanie wysyłaj przez istniejącą klasę `SqlRunner` (loguje SQL), parametry przez Dappera – bez sklejania tekstu SQL.
- `SourceRow` = numer wiersza danych w pliku, od 1, bez nagłówka.
- Tekst przytnij (`Trim`). Puste pole albo wartość, której nie da się odczytać → `NULL`, bez wyjątku. Rekord z co najmniej jednym takim polem to jeden „dziwny” rekord.
- Po imporcie zapisz w logu: liczbę wierszy w tabeli, liczbę „dziwnych” rekordów i czas importu.
- Klasa `CsvImporter` z interfejsem `ICsvImporter`, rejestracja w `Program.cs`. Metody do 20 linii, bez zagnieżdżonych if-ów.

## GOTOWE GDY
1. Rozszerzenie mssql: `SELECT COUNT(*) FROM TicketMachines` zwraca **266** (E1.rows).
2. Breakpoint w pętli importu, Watch na wierszu 7: kolumna `Współrzędne geograficzne` ma tekst **50.39760177303035 18.877324014902115** (E1.row7.raw).
3. Rozszerzenie mssql: `SELECT Coordinates FROM TicketMachines WHERE SourceRow = 7` zwraca **50.39760177303035 18.877324014902115** (E1.row7.value).
4. Log importu w terminalu: „dziwnych” rekordów jest **25** (E1.weird).
5. Po ponownym uruchomieniu aplikacji `COUNT(*)` nadal zwraca **266** (E1.rows).

Nie zmieniaj plików poza: `db/s14_create.sql`, `src/GzmApp/Data/CsvImporter.cs`, `src/GzmApp/Program.cs`, `src/GzmApp/appsettings.json`. Nie dodawaj pakietów.
