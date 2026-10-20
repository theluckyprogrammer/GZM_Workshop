# Findings

- [01] Linux (Claude Code w chmurze): brak LocalDB. Użyto SQL Server 2022 Developer w Dockerze (`mcr.microsoft.com/mssql/server:2022-latest`) przez zmienną `GZM_CONNSTR`; ścieżka LocalDB z `appsettings.json` nie była uruchomiona.
- [01] W repo był już `.gitignore` (szablon .NET z pierwszego commita). Nie nadpisano go, dopisano brakujące wpisy: `*.user`, `secrets.json`, `data/work/`.
- [01] `Microsoft.Data.SqlClient` przypięty do 6.1.7 (wersja, której używa EF Core 10.0.13). Wersja 7.x działałaby z niezgodną zależnością EF.
- [01] SqlClient 6.1.7 ciągnie `System.Security.Cryptography.Pkcs` 9.0.11 z podatnościami (NU1902/NU1903). Dodano bezpośrednią referencję 10.0.13, żeby build był bez ostrzeżeń.
- [01] Ryzyko: `EnsureCreated` tworzy tabele `AspNet*` tylko w pustej bazie. Jeśli ktoś ręcznie uruchomi skrypty domenowe przed pierwszym startem aplikacji, logowanie nie dostanie tabel. Kolejność w `DatabaseBootstrapper`: baza → Identity → (później) skrypty domenowe.
- [01] `MapStaticAssets` dodaje odcisk do nazwy pliku (`htmx.min.<hash>.js`). W zakładce Network uczniowie zobaczą tę nazwę, a nie `htmx.min.js`.
- [01] Domyślna polityka haseł Identity (wielka litera, cyfra, znak specjalny, min. 6 znaków) może spowolnić rejestrację w 30-minutowym warsztacie. Do decyzji prowadzącego; nie zmieniano.
