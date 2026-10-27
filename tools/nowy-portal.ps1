<#
.SYNOPSIS
    Tworzy portal pary w kroku 0: szkielet aplikacji, dane z portalu GZM i log decyzji.

.DESCRIPTION
    1. Kopiuje szkielet/src -> src, szkielet/tests -> tests, szkielet/db -> db, szkielet/GzmWorkshop.slnx -> GzmWorkshop.slnx.
    2. Wpisuje "Scenario": "sNN" w src/GzmApp/appsettings.json (para nigdy nie robi tego ręcznie).
    3. Pobiera plik danych z otwartedane.metropoliagzm.pl do data/raw/ (gdy pliku brak) i sprawdza sumę SHA-256.
    4. Tworzy LOG-DECYZJI.md, jeśli go jeszcze nie ma (będzie go prowadzić Kilo).
    Uruchamia go Kilo Code w pierwszym prompcie (krok 0) albo para sama w terminalu VS Code.
    Nie wymaga uprawnień administratora. Jeśli portal już istnieje, skrypt odmawia –
    flaga -OdNowa kasuje src, tests, db i GzmWorkshop.slnx (LOG-DECYZJI.md zostaje).

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File tools\nowy-portal.ps1 -Scenariusz s05
#>
param(
    [Parameter(Mandatory = $true)][ValidatePattern('^s(0[1-9]|1[0-5])$')][string]$Scenariusz,
    [switch]$OdNowa
)

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
$repo = Split-Path -Parent $PSScriptRoot

function Write-Step([string]$Text) { Write-Host "-> $Text" }

function Read-Utf8([string]$Path) {
    return [IO.File]::ReadAllText($Path, [Text.Encoding]::UTF8)
}

function Write-Utf8([string]$Path, [string]$Text) {
    [IO.File]::WriteAllText($Path, $Text, (New-Object Text.UTF8Encoding($false)))
}

# Wpisuje "Scenario" w appsettings.json tekstowo, żeby nie przerabiać reszty pliku.
function Set-Scenario([string]$Path, [string]$Code) {
    $text = Read-Utf8 $Path
    if ($text -match '"Scenario"\s*:\s*"[^"]*"') {
        $text = $text -replace '"Scenario"\s*:\s*"[^"]*"', ('"Scenario": "' + $Code + '"')
    }
    else {
        $text = $text -replace '^\s*\{', ('{' + "`r`n" + '  "Scenario": "' + $Code + '",')
    }
    Write-Utf8 $Path $text
}

function Get-Dataset([string]$Code) {
    $zbiory = Join-Path $repo 'data/zbiory.json'
    $dane = (Read-Utf8 $zbiory | ConvertFrom-Json).datasets | Where-Object { $_.code -eq $Code }
    if (-not $dane) { throw "Nie znam scenariusza $Code – sprawdź data/zbiory.json." }
    return $dane
}

# Suma SHA-256 pliku; gdy się nie zgadza, druga próba po ujednoliceniu końców linii (LF).
function Test-FileHash([string]$Path, [string]$Expected) {
    $hash = (Get-FileHash -Path $Path -Algorithm SHA256).Hash
    if ($hash -eq $Expected) { return $true }
    $normalized = [IO.File]::ReadAllBytes($Path)
    $text = [Text.Encoding]::UTF8.GetString($normalized) -replace "`r`n", "`n"
    $bytes = [Text.Encoding]::UTF8.GetBytes($text)
    $stream = New-Object IO.MemoryStream(,$bytes)
    return (Get-FileHash -InputStream $stream -Algorithm SHA256).Hash -eq $Expected
}

function Save-Dataset($Dataset) {
    $target = Join-Path $repo $Dataset.file
    $directory = Split-Path -Parent $target
    if (-not (Test-Path $directory)) { New-Item -ItemType Directory -Path $directory | Out-Null }

    if ((Test-Path $target) -and (Test-FileHash $target $Dataset.sha256)) {
        Write-Host ("   Plik {0} już jest na miejscu – pobieranie pominięte." -f $Dataset.file)
        return
    }
    if (Test-Path $target) {
        Write-Host ("   Plik {0} jest, ale ma inną treść – pobieram świeżą wersję z portalu." -f $Dataset.file)
    }
    else {
        Write-Host ("   Pobieram {0} z otwartedane.metropoliagzm.pl…" -f $Dataset.file)
    }

    $temp = Join-Path $directory ($Dataset.code + '.partial')
    try {
        Invoke-WebRequest -Uri $Dataset.fileUrl -OutFile $temp -UseBasicParsing `
            -UserAgent 'Mozilla/5.0 (Windows NT 10.0; Win64; x64)' -TimeoutSec 120
        if (-not (Test-FileHash $temp $Dataset.sha256)) {
            throw "Pobrany plik ma inną sumę SHA-256, niż zapisano w data/zbiory.json. Odśwież dane: python tools/zbiory-gen.py"
        }
        Move-Item -Path $temp -Destination $target -Force
    }
    finally {
        if (Test-Path $temp) { Remove-Item $temp -Force }
    }
}

function New-DecisionLog {
    $log = Join-Path $repo 'LOG-DECYZJI.md'
    if (Test-Path $log) { return }
    $header = @'
# Log decyzji pary

Tego pliku nie wypełniacie ręcznie – prowadzi go Kilo Code. Po każdej decyzji
(z propozycji Kilo w pauzach „decyzja 1…5") Kilo dopisuje tu jeden wiersz.
Dzięki temu na końcu warsztatu widać, jak powstał Wasz portal.

| Krok | Pytanie | Wasza decyzja | Dlaczego |
|---|---|---|---|
'@
    Write-Utf8 $log $header
    Write-Host '   Tworzę LOG-DECYZJI.md (tu Kilo zapisze Wasze decyzje).'
}

Push-Location $repo
try {
    $szkielet = Join-Path $repo 'szkielet'
    $istnieje = Test-Path (Join-Path $repo 'src/GzmApp')
    if ($istnieje -and -not $OdNowa) {
        throw "Portal już istnieje (src/GzmApp). Jeśli chcecie zacząć od zera, uruchom: tools\nowy-portal.ps1 -Scenariusz $Scenariusz -OdNowa (skasuje dotychczasowy kod pary)."
    }
    if ($istnieje -and $OdNowa) {
        Write-Step 'Kasuję dotychczasowy portal (-OdNowa): src, tests, db, GzmWorkshop.slnx'
        'src', 'tests', 'db', 'GzmWorkshop.slnx' | ForEach-Object {
            $path = Join-Path $repo $_
            if (Test-Path $path) { Remove-Item -Recurse -Force $path }
        }
    }

    $dataset = Get-Dataset $Scenariusz

    Write-Step "Kopiuję szkielet aplikacji (szkielet/ -> src, tests, db) – scenariusz $Scenariusz"
    Copy-Item -Path (Join-Path $szkielet 'src') -Destination (Join-Path $repo 'src') -Recurse
    Copy-Item -Path (Join-Path $szkielet 'tests') -Destination (Join-Path $repo 'tests') -Recurse
    Copy-Item -Path (Join-Path $szkielet 'db') -Destination (Join-Path $repo 'db') -Recurse
    Copy-Item -Path (Join-Path $szkielet 'GzmWorkshop.slnx') -Destination (Join-Path $repo 'GzmWorkshop.slnx')

    Write-Step 'Ustawiam scenariusz w src/GzmApp/appsettings.json'
    Set-Scenario (Join-Path $repo 'src/GzmApp/appsettings.json') $Scenariusz

    Write-Step ("Dane: {0} - {1}" -f $dataset.dataset, $dataset.file)
    Save-Dataset $dataset

    New-DecisionLog

    $size = [math]::Round((Get-Item (Join-Path $repo $dataset.file)).Length / 1KB, 1)
    Write-Host ''
    Write-Host ("GOTOWE: portal {0} stoi. Scenariusz: {1}." -f $Scenariusz, $Scenariusz) -ForegroundColor Green
    Write-Host ("   Dane: {0} ({1} KB, suma SHA-256 zgodna). Zbiór: {2}" -f $dataset.file, $size, $dataset.sourceUrl)
    Write-Host '   Co dalej (pauza w prompty.md po pierwszym prompcie):'
    Write-Host '   1. F5 w VS Code – aplikacja startuje pod http://localhost:5080'
    Write-Host '   2. Testy: ikona kolby (Testing) w VS Code albo dotnet test tests/GzmApp.Specs'
    Write-Host ("   3. Testy danych kroku 0: dotnet test tests/GzmApp.Specs --filter Capability={0}-000-dane" -f $Scenariusz)
}
finally {
    Pop-Location
}
