<#
.SYNOPSIS
    Sprawdza, czy komputer jest gotowy na warsztat „AI + otwarte dane GZM”.

.DESCRIPTION
    Dla każdego punktu wypisuje OK / BŁĄD (albo POMINIĘTO) i podpowiedź naprawy.
    Działa w Windows PowerShell 5.1 i pwsh 7, bez uprawnień administratora.
    Kod wyjścia: 0 – wszystko OK, 1 – co najmniej jeden BŁĄD.
    Klucze (Kilo, model AI) nigdy nie są wypisywane na ekran ani do logu.

.PARAMETER KiloEndpoint
    Adres HTTPS usługi Kilo do sprawdzenia połączenia (domyślnie zmienna KILO_ENDPOINT albo https://api.kilo.ai).

.EXAMPLE
    tools\check.cmd
#>
param(
    [string]$KiloEndpoint = $(if ($env:KILO_ENDPOINT) { $env:KILO_ENDPOINT } else { 'https://api.kilo.ai' })
)

$ErrorActionPreference = 'Continue'
$repo = Split-Path -Parent $PSScriptRoot
$script:failures = 0
$onWindows = [System.Environment]::OSVersion.Platform -eq [System.PlatformID]::Win32NT

function Write-Result([string]$Name, [string]$State, [string]$Hint) {
    $color = @{ 'OK' = 'Green'; 'BŁĄD' = 'Red'; 'POMINIĘTO' = 'Yellow' }[$State]
    Write-Host ("[{0,-9}] {1}" -f $State, $Name) -ForegroundColor $color
    if ($State -ne 'OK' -and $Hint) { Write-Host "            -> $Hint" }
    if ($State -eq 'BŁĄD') { $script:failures++ }
}

function Test-Step([string]$Name, [string]$Hint, [scriptblock]$Check) {
    try {
        $result = & $Check
        if ($result -is [string] -and $result -eq 'skip') { Write-Result $Name 'POMINIĘTO' $Hint; return }
        if ($result) { Write-Result $Name 'OK' $null } else { Write-Result $Name 'BŁĄD' $Hint }
    }
    catch {
        Write-Result $Name 'BŁĄD' "$Hint ($($_.Exception.GetType().Name))"
    }
}

function Test-Command([string]$Name) {
    return $null -ne (Get-Command $Name -ErrorAction SilentlyContinue)
}

function Invoke-SqlSelect1([string]$ConnectionString) {
    if (Test-Command 'sqlcmd') {
        $output = sqlcmd -S '(localdb)\MSSQLLocalDB' -E -h -1 -Q 'SET NOCOUNT ON; SELECT 1' 2>&1
        return ($LASTEXITCODE -eq 0) -and ("$output".Trim() -eq '1')
    }
    Add-Type -AssemblyName System.Data
    $connection = New-Object System.Data.SqlClient.SqlConnection $ConnectionString
    try {
        $connection.Open()
        $command = $connection.CreateCommand()
        $command.CommandText = 'SELECT 1'
        return ($command.ExecuteScalar() -eq 1)
    }
    finally {
        $connection.Dispose()
    }
}

function Get-HtmxExpectedHash {
    $line = Get-Content -Encoding UTF8 (Join-Path $repo 'docs/HTMX.md') | Where-Object { $_ -match 'SHA-256: `([0-9a-f]{64})`' } | Select-Object -First 1
    if ($line -match '([0-9a-f]{64})') { return $Matches[1] }
    return ''
}

function Get-FileSha256([string]$Path) {
    $sha = [System.Security.Cryptography.SHA256]::Create()
    $stream = [System.IO.File]::OpenRead($Path)
    try { return (-join ($sha.ComputeHash($stream) | ForEach-Object { $_.ToString('x2') })) }
    finally { $stream.Dispose(); $sha.Dispose() }
}

# Sol6 settings as the app reads them: Model from appsettings.json, Endpoint and ApiKey from appsettings.Local.json
# (copied by the teacher, ignored by git), environment variables SOL6__* win. The key is used only in the request header.
function Get-Sol6Settings {
    $sol6 = [pscustomobject]@{ Endpoint = ''; Model = ''; ApiKey = '' }
    foreach ($name in 'appsettings.json', 'appsettings.Local.json') {
        $file = Join-Path $repo "src/GzmApp/$name"
        if (-not (Test-Path $file)) { continue }
        $section = (Get-Content -Raw -Encoding UTF8 $file | ConvertFrom-Json).Sol6
        foreach ($key in 'Endpoint', 'Model', 'ApiKey') { if ($section -and $section.$key) { $sol6.$key = $section.$key } }
    }
    foreach ($key in 'Endpoint', 'Model', 'ApiKey') {
        $value = [Environment]::GetEnvironmentVariable("SOL6__$($key.ToUpperInvariant())")
        if ($value) { $sol6.$key = $value }
    }
    return $sol6
}

# Azure AI Foundry, OpenAI-compatible endpoint .../openai/v1/: POST chat/completions with the model name.
function Invoke-Sol6Ping($Sol6) {
    if (-not $Sol6.Endpoint -or -not $Sol6.Model) { return $false }
    $uri = "$($Sol6.Endpoint.TrimEnd('/'))/chat/completions"
    $headers = @{ 'Authorization' = "Bearer $($Sol6.ApiKey)" }
    $json = @{ model = $Sol6.Model; messages = @(@{ role = 'user'; content = 'Odpowiedz jednym słowem: OK' }) } | ConvertTo-Json -Depth 5
    $response = Invoke-WebRequest -Uri $uri -Method Post -Headers $headers -Body ([Text.Encoding]::UTF8.GetBytes($json)) -ContentType 'application/json' -UseBasicParsing -TimeoutSec 30
    return $response.StatusCode -eq 200
}

Write-Host "Sprawdzam komputer do warsztatu (repo: $repo)" -ForegroundColor Cyan
Push-Location $repo
try {
    Test-Step '.NET 10 SDK (dotnet --list-sdks)' 'Zainstaluj .NET 10 SDK (admin szkolny) i otwórz terminal ponownie.' {
        (Test-Command 'dotnet') -and ((dotnet --list-sdks) -match '^10\.0\.').Count -gt 0
    }

    Test-Step 'SQL Server LocalDB i połączenie (SELECT 1)' 'Zainstaluj SqlLocalDB.msi; potem: sqllocaldb create MSSQLLocalDB -s' {
        if (-not $onWindows) { return 'skip' }
        $info = sqllocaldb info MSSQLLocalDB 2>&1
        if ($LASTEXITCODE -ne 0) { return $false }
        Invoke-SqlSelect1 'Server=(localdb)\MSSQLLocalDB;Database=master;Trusted_Connection=True;TrustServerCertificate=True'
    }

    Test-Step 'Rozszerzenia VS Code (C#, C# Dev Kit, mssql, Kilo Code)' 'code --install-extension <id> dla brakujących: ms-dotnettools.csharp, ms-dotnettools.csdevkit, ms-mssql.mssql, kilocode.kilo-code' {
        if (-not (Test-Command 'code')) { return $false }
        $installed = code --list-extensions 2>$null | ForEach-Object { $_.ToLowerInvariant() }
        $required = 'ms-dotnettools.csharp', 'ms-dotnettools.csdevkit', 'ms-mssql.mssql', 'kilocode.kilo-code'
        ($required | Where-Object { $installed -notcontains $_ }).Count -eq 0
    }

    Test-Step 'Port 5080 wolny' 'Zamknij inną uruchomioną aplikację (Shift+F5 w VS Code) albo uruchom ponownie komputer.' {
        $listener = New-Object System.Net.Sockets.TcpListener ([System.Net.IPAddress]::Loopback), 5080
        try { $listener.Start(); $true } catch { $false } finally { $listener.Stop() }
    }

    Test-Step 'Git i gałęzie ratunek/etap-1..4' 'Zainstaluj Git; w repo: git fetch origin "ratunek/*:ratunek/*"' {
        if (-not (Test-Command 'git')) { return $false }
        $branches = git branch -a --list '*ratunek/etap-*' 2>$null | ForEach-Object { ($_ -replace '^[\*\s]+', '') -replace '^remotes/origin/', '' } | Sort-Object -Unique
        (1..4 | Where-Object { $branches -notcontains "ratunek/etap-$_" }).Count -eq 0
    }

    Test-Step 'HTMX 4.0.0 (plik i SHA-256 z docs/HTMX.md)' 'Przywróć plik: git checkout -- src/GzmApp/wwwroot/lib/htmx/4.0.0/htmx.min.js' {
        $file = Join-Path $repo 'src/GzmApp/wwwroot/lib/htmx/4.0.0/htmx.min.js'
        (Test-Path $file) -and ((Get-FileSha256 $file) -eq (Get-HtmxExpectedHash))
    }

    Test-Step 'dotnet build (pakiety z cache NuGet)' 'Uruchom raz z internetem: dotnet restore src/GzmApp; potem build działa offline.' {
        dotnet build (Join-Path $repo 'src/GzmApp') -v q -nologo 2>&1 | Out-Null
        $LASTEXITCODE -eq 0
    }

    # Every sentence of the specification has a step; no database needed. Proves that Test Explorer will show the tests.
    Test-Step 'Testy specyfikacji (tests/GzmApp.Specs: build + test kontrolny)' 'Uruchom raz z internetem: dotnet restore tests/GzmApp.Specs; potem build i testy działają offline.' {
        dotnet test (Join-Path $repo 'tests/GzmApp.Specs') --filter 'Capability=kontrola' -v q -nologo 2>&1 | Out-Null
        $LASTEXITCODE -eq 0
    }

    Test-Step "Połączenie HTTPS z Kilo ($KiloEndpoint)" 'Sprawdź sieć szkolną / proxy; adres usługi ustaw w KILO_ENDPOINT.' {
        try { $response = Invoke-WebRequest -Uri $KiloEndpoint -Method Head -UseBasicParsing -TimeoutSec 15; $response.StatusCode -lt 500 }
        catch { $null -ne $_.Exception.Response -and [int]$_.Exception.Response.StatusCode -lt 500 }
    }

    $sol6 = Get-Sol6Settings
    Test-Step "Model AI ($($sol6.Model)): jedno krótkie wywołanie – krok 4 warsztatu" 'Brak klucza albo adresu: skopiuj plik src/GzmApp/appsettings.Local.json od prowadzącego (Sol6:Endpoint i Sol6:ApiKey).' {
        if (-not $sol6.ApiKey) { return $false }
        Invoke-Sol6Ping $sol6
    }
    Remove-Variable sol6 -ErrorAction SilentlyContinue
}
finally {
    Pop-Location
}

Write-Host ''
if ($script:failures -eq 0) {
    Write-Host 'Wszystko OK – komputer jest gotowy.' -ForegroundColor Green
    exit 0
}
Write-Host "Błędów: $($script:failures). Popraw je według podpowiedzi i uruchom sprawdzenie ponownie." -ForegroundColor Red
exit 1
