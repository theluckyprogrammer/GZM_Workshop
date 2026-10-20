# Etap 4 – ulubione (SekcjaSCK, s13)

> Pilot: skopiuj do Kilo Code wszystko od „CEL” do końca. Kontroler danych: przygotuj narzędzia z sekcji GOTOWE GDY.

## CEL
Zalogowany użytkownik oznacza rekordy gwiazdką ★ i widzi je w zakładce „Moje ulubione”. Ulubione w SekcjaSCK to zajęcia, na które para chce się zapisać.

## DANE
- Tabela – zapisz ten skrypt w `db/900_favorites.sql` i uruchom go przy starcie:

```sql
IF OBJECT_ID(N'dbo.Favorites') IS NULL
CREATE TABLE dbo.Favorites (
    UserId nvarchar(450) NOT NULL,
    Scenario nvarchar(10) NOT NULL,
    RecordId int NOT NULL,
    CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_Favorites_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Favorites PRIMARY KEY (UserId, Scenario, RecordId)
);
```

- `RecordId` to kolumna `RecordId` z tabeli `CultureClasses`. Tytuł rekordu na liście ulubionych: pole `Section`.
- `Scenario` = `s13` (klucz `Scenario` w `appsettings.json`). `UserId` = identyfikator zalogowanego użytkownika.

## ZASADY
- `IFavoritesRepository` i `FavoritesRepository` w `src/GzmApp/Data/FavoritesRepository.cs`: `Add`, `Remove`, `ListFor(userId)` przez `SqlRunner`. `Add` i `Remove` wykonane dwa razy dają ten sam stan (jedno zapytanie z `IF NOT EXISTS` albo `MERGE`).
- W `_Lista.cshtml` przy każdym rekordzie przycisk ★: `hx-post="?handler=Ulubione&id=@r.RecordId"` (dodaj) albo `hx-delete="?handler=Ulubione&id=@r.RecordId"` (usuń), `hx-swap="outerHTML"`.
- Handlery `OnPostUlubione(int id)` i `OnDeleteUlubione(int id)` w `Index.cshtml.cs` zwracają nowy przycisk. Niezalogowany: status 401 i fragment „Zaloguj się, aby dodać do ulubionych”.
- Token antiforgery wysyła już `<body hx-headers:inherited='{"RequestVerificationToken":"..."}'>` w `_Layout.cshtml`. Nie wyłączaj walidacji antiforgery.
- Strona `src/GzmApp/Pages/Ulubione.cshtml` z `[Authorize]`: lista tytułów ulubionych. Link „Moje ulubione” w menu w `_Layout.cshtml`.
- Rejestracja repozytorium w `Program.cs`. Metody do 20 linii, bez zagnieżdżonych if-ów.

## GOTOWE GDY
1. Zaloguj się i wyszukaj rekord z wiersza 7: **Pracownia Plastyczna „Okienko”** (E4.title) (na liście „Nr 7”). Kliknij ★.
2. Zakładka Network: żądanie POST `?handler=Ulubione` ma nagłówek `RequestVerificationToken` i status 200.
3. „Moje ulubione” pokazuje **Pracownia Plastyczna „Okienko”** (E4.title).
4. Rozszerzenie mssql: `SELECT COUNT(*) FROM Favorites WHERE Scenario = 's13'` zwraca 1. Otwórzcie stronę w drugiej karcie i kliknijcie ★ przy tym samym rekordzie: nadal 1.
5. Po wylogowaniu adres `/Ulubione` przekierowuje do logowania, a ★ daje status 401 (Network).

Nie zmieniaj plików poza: `db/900_favorites.sql`, `src/GzmApp/Data/FavoritesRepository.cs`, `src/GzmApp/Pages/Index.cshtml.cs`, `src/GzmApp/Pages/Shared/_Lista.cshtml`, `src/GzmApp/Pages/Ulubione.cshtml`, `src/GzmApp/Pages/Ulubione.cshtml.cs`, `src/GzmApp/Pages/Shared/_Layout.cshtml`, `src/GzmApp/Program.cs`. Nie dodawaj pakietów.
