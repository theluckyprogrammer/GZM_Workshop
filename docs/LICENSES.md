# Licencje

Dozwolone: MIT, Apache-2.0, MS-PL, BSD (w tym 0BSD).

## Pakiety NuGet (`src/GzmApp`)

| Pakiet | Wersja | Licencja |
|---|---|---|
| Dapper | 2.1.89 | Apache-2.0 |
| Microsoft.Data.SqlClient | 6.1.7 | MIT |
| Microsoft.EntityFrameworkCore.SqlServer | 10.0.13 | MIT |
| Microsoft.AspNetCore.Identity.EntityFrameworkCore | 10.0.13 | MIT |
| Microsoft.AspNetCore.Identity.UI | 10.0.13 | MIT |
| System.Security.Cryptography.Pkcs | 10.0.13 | MIT |
| CsvHelper | 33.1.0 | MS-PL OR Apache-2.0 |
| OpenAI | 2.14.0 | MIT |
| Microsoft.Extensions.AI.OpenAI | 10.11.0 | MIT |
| Azure.AI.OpenAI | 2.1.0 | MIT |

## Pakiety NuGet (`tools/Fetch`)

| Pakiet | Wersja | Licencja |
|---|---|---|
| CsvHelper | 33.1.0 | MS-PL OR Apache-2.0 |

## Pakiety NuGet (`tools/Spectree.Generator`)

Generator testów ze specyfikacji (analizator Roslyn, tylko przy kompilacji – nie trafia do aplikacji).

| Pakiet | Wersja | Licencja |
|---|---|---|
| Microsoft.CodeAnalysis.CSharp | 4.12.0 | MIT |

## Pakiety NuGet (`tests/GzmApp.Specs`)

Te same pakiety i wersje co `tests/GzmApp.Tests` (niżej): AngleSharp, Microsoft.AspNetCore.Mvc.Testing, Microsoft.NET.Test.Sdk, xunit, xunit.runner.visualstudio.

## Pakiety NuGet (`tools/Expect`)

| Pakiet | Wersja | Licencja |
|---|---|---|
| CsvHelper | 33.1.0 | MS-PL OR Apache-2.0 |

## `tools/Cards`

Bez pakietów NuGet (referencja do projektu `tools/Expect`).

## Pakiety NuGet (`tests/GzmApp.Tests`)

| Pakiet | Wersja | Licencja |
|---|---|---|
| Microsoft.NET.Test.Sdk | 18.11.0 | MIT |
| xunit | 2.9.3 | Apache-2.0 |
| xunit.runner.visualstudio | 4.0.1 | Apache-2.0 |
| Microsoft.AspNetCore.Mvc.Testing | 10.0.13 | MIT |
| AngleSharp | 1.8.5 | MIT |
| Microsoft.Playwright (tylko `ratunek/etap-4`) | 1.64.0 | MIT |

## Pakiety NuGet (`tools/CodeRules`)

| Pakiet | Wersja | Licencja |
|---|---|---|
| Microsoft.CodeAnalysis.CSharp | 5.9.0 | MIT |

## Pakiety przechodnie spoza listy dozwolonych licencji

| Pakiet | Wersja | Licencja | Skąd |
|---|---|---|---|
| Microsoft.Data.SqlClient.SNI.runtime | 6.0.3 | Microsoft Software License Terms (komponent redystrybuowalny) | zależność `Microsoft.Data.SqlClient` (natywny SNI na Windows) |
| Microsoft.Identity.Client.NativeInterop | 0.20.6 | Microsoft Software License Terms (komponent redystrybuowalny) | zależność `Microsoft.Data.SqlClient` (MSAL) |
| Microsoft.NETCore.Platforms | 1.1.0 | Microsoft .NET Library License (go.microsoft.com/fwlink/?LinkId=329770) | zależność `NETStandard.Library` w `tools/Spectree.Generator` (netstandard2.0) |

Dwa pierwsze przychodzą z `Microsoft.Data.SqlClient`, którego wymaga stos (Dapper + SQL Server); bez nich nie ma połączenia z bazą. Uzasadnienie w `findings.md` (krok 23).
`Microsoft.NETCore.Platforms` nie ma kodu (tylko graf identyfikatorów platform `runtime.json`). Wymaga go cel `netstandard2.0`, w którym musi być każdy generator Roslyn. Używany tylko przy kompilacji – nie trafia do aplikacji ani do testów. Pozostałe stare pakiety .NET generatora (`NETStandard.Library`, `System.Memory`, `System.Buffers`, …) mają licencję MIT podaną adresem `dotnet/corefx` albo `dotnet/standard`.

## Dane

Zbiory z otwartedane.metropoliagzm.pl: licencja i wymagana atrybucja każdego zbioru są w `data/metadata.json` (pole `license`).

## Biblioteki front-end (`wwwroot/lib`)

| Biblioteka | Wersja | Licencja |
|---|---|---|
| htmx | 4.0.0 | BSD-0-Clause |
| Bootstrap | v5.3.3 | MIT |
| jQuery | v3.7.1 | MIT |
| jquery-validation | z szablonu .NET 10 | MIT |
| jquery-validation-unobtrusive | z szablonu .NET 10 | MIT |
