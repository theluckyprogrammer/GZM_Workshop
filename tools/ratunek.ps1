<#
.SYNOPSIS
    Przełącza stanowisko na gotowy etap warsztatu (gałąź ratunek/etap-N) i importuje dane scenariusza.

.DESCRIPTION
    1. Odkłada zmiany pary do schowka (git stash, także nowe pliki) z datą w opisie.
    2. Przełącza na gałąź ratunek/etap-N.
    3. Ustawia "Scenario" w src/GzmApp/appsettings.Development.json (na gałęzi ratunek/* ta zmiana nie trafia do schowka).
    4. Uruchamia import danych (dotnet run -- import).
    5. Wypisuje, co się stało i jak odzyskać zmiany pary.
    Nie wymaga uprawnień administratora.

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File tools\ratunek.ps1 -Etap 3 -Scenariusz s06
#>
param(
    [Parameter(Mandatory = $true)][ValidateRange(1, 4)][int]$Etap,
    [Parameter(Mandatory = $true)][ValidatePattern('^s(0[1-9]|1[0-5])$')][string]$Scenariusz
)

$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot
$branch = "ratunek/etap-$Etap"
$settings = Join-Path $repo 'src/GzmApp/appsettings.Development.json'

function Invoke-Step([string]$Description, [scriptblock]$Command) {
    Write-Host "-> $Description"
    & $Command
    if ($LASTEXITCODE -ne 0) { throw "Nie udało się: $Description (kod $LASTEXITCODE)." }
}

function Set-Scenario([string]$Path, [string]$Code) {
    $json = Get-Content -Raw -Encoding UTF8 $Path | ConvertFrom-Json
    if ($null -eq $json.PSObject.Properties['Scenario']) {
        $json | Add-Member -NotePropertyName 'Scenario' -NotePropertyValue $Code
    }
    $json.Scenario = $Code
    [IO.File]::WriteAllText($Path, ($json | ConvertTo-Json -Depth 10), (New-Object Text.UTF8Encoding($false)))
}

Push-Location $repo
try {
    # On a rescue branch the Scenario line was written by this script, not by the pair: drop it before the stash.
    if ((git branch --show-current) -like 'ratunek/*') {
        git checkout -- $settings
    }
    $stamp = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
    $before = (git stash list | Measure-Object).Count
    Invoke-Step "Odkładam Wasze zmiany do schowka (git stash)" { git stash push --include-untracked -m "ratunek $stamp przed $branch ($Scenariusz)" }
    $saved = (git stash list | Measure-Object).Count -gt $before

    Invoke-Step "Przełączam na gałąź $branch" { git switch $branch }

    Write-Host "-> Ustawiam scenariusz $Scenariusz w appsettings.Development.json"
    Set-Scenario $settings $Scenariusz

    Invoke-Step "Importuję dane scenariusza $Scenariusz" { dotnet run --project src/GzmApp -- import }

    Write-Host ''
    Write-Host "GOTOWE: stanowisko jest na etapie $Etap ($branch), scenariusz $Scenariusz, dane w bazie." -ForegroundColor Green
    Write-Host 'Uruchom aplikację klawiszem F5 w VS Code.'
    if ($saved) {
        Write-Host "Wasze wcześniejsze zmiany są w schowku: 'ratunek $stamp ...'."
        Write-Host 'Podgląd schowka:   git stash list'
        Write-Host 'Powrót do zmian:   git switch <Wasza gałąź>, potem git stash apply stash@{N} (N z listy, opis zaczyna się od "ratunek")'
    }
    else {
        Write-Host 'Nie było zmian do odłożenia (schowek bez zmian).'
    }
}
finally {
    Pop-Location
}
