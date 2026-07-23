<#
.SYNOPSIS


.DESCRIPTION


.PARAMETER InstallDeps
   

.PARAMETER ServerOnly


.PARAMETER ClientOnly


.EXAMPLE

#>

[CmdletBinding()]
param(
    [switch]$InstallDeps,
    [switch]$ServerOnly,
    [switch]$ClientOnly
)

$ErrorActionPreference = 'Stop'

$root       = $PSScriptRoot
$serverPath = Join-Path $root 'ServerSide'
$clientPath = Join-Path $root 'ClientSide'
$envPath    = Join-Path $root '.env'

$defaultServerPort = 2567



function Get-ServerPort {
    if (-not (Test-Path $envPath)) {
        Write-Warning ".env nao encontrado em $envPath. A usar a porta $defaultServerPort."
        return $defaultServerPort
    }

    $match = Select-String -Path $envPath -Pattern '^\s*SERVER_PORT\s*=\s*["'']?(\d+)["'']?\s*$' |
             Select-Object -First 1

    if (-not $match) {
        Write-Warning "SERVER_PORT nao definido em $envPath. A usar a porta $defaultServerPort."
        return $defaultServerPort
    }

    return [int]$match.Matches[0].Groups[1].Value
}

$serverPort = Get-ServerPort

$serverUrl = "http://localhost:$serverPort"
$clientUrl = 'http://localhost:5173'



if (-not (Get-Command npm -ErrorAction SilentlyContinue)) {
    throw "npm nao foi encontrado no PATH. Instala o Node.js (>= 20.9.0) primeiro."
}

foreach ($p in @($serverPath, $clientPath)) {
    if (-not (Test-Path $p)) { throw "Pasta nao encontrada: $p" }
}



function Install-Deps {
    param([string]$Path, [string]$Name)

    $modules = Join-Path $Path 'node_modules'
    if ($InstallDeps -or -not (Test-Path $modules)) {
        Write-Host "[$Name] a instalar dependencias..." -ForegroundColor Yellow
        Push-Location $Path
        try {
            npm install
            if ($LASTEXITCODE -ne 0) { throw "npm install falhou em $Name." }
        }
        finally { Pop-Location }
    }
}



function Start-Part {
    param([string]$Path, [string]$Name, [string]$NpmScript, [string]$Url)

    Install-Deps -Path $Path -Name $Name

    Write-Host "[$Name] a arrancar em $Url" -ForegroundColor Green

    $command = "`$Host.UI.RawUI.WindowTitle = 'CountryFair - $Name'; " +
               "Set-Location '$Path'; " +
               "npm run $NpmScript"

    Start-Process -FilePath 'powershell.exe' `
                  -ArgumentList '-NoExit', '-NoProfile', '-Command', $command `
                  -WorkingDirectory $Path | Out-Null
}

$startServer = -not $ClientOnly
$startClient = -not $ServerOnly

if ($startServer) {
    Start-Part -Path $serverPath -Name 'Server' -NpmScript 'start' -Url $serverUrl
}

if ($startServer -and $startClient) {
    Start-Sleep -Seconds 3
}

if ($startClient) {
    Start-Part -Path $clientPath -Name 'Client' -NpmScript 'dev' -Url $clientUrl
}

Write-Host ''
Write-Host 'CountryFair Web App iniciada.' -ForegroundColor Cyan
if ($startServer) { Write-Host "  Servidor: $serverUrl" }
if ($startClient) { Write-Host "  Cliente:  $clientUrl" }
Write-Host 'Fecha as janelas abertas (ou Ctrl+C dentro delas) para parar.' -ForegroundColor DarkGray
