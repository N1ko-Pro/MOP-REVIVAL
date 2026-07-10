<#
.SYNOPSIS
    Сборка и (по желанию) установка мода MOP - REVIVAL прямо в папку Mods игры.

.DESCRIPTION
    Скрипт собирает MOPR.csproj через .NET SDK (MSBuild). Референс-сборки .NET 3.5
    подтягиваются из NuGet, поэтому Visual Studio не нужна — достаточно установленного
    dotnet SDK.

    Итоговый файл мода: MOPR.dll (имя сборки в проекте — MOPR; отображаемое имя в игре —
    «MOP - REVIVAL»). При установке (deploy) DLL кладётся в <Mods>, а BouncyCastle.Crypto.dll —
    в <Mods>\References (нужен моду для HTTPS/TLS 1.2 при синхронизации правил с сервером).

.PARAMETER Mode
    Режим работы:
      build         — только Release-сборка (значение по умолчанию)
      debug         — только Debug-сборка
      deploy        — Release-сборка, затем установка MOPR.dll в папку Mods игры
      deploy-debug  — Debug-сборка, затем установка MOPR.dll (+ .pdb) в папку Mods игры

.PARAMETER ManagedDir
    Папка Managed игры (где лежат референс-DLL: Assembly-CSharp, MSCLoader и т.д.).
    Меняйте, если игра установлена в другом месте.

.PARAMETER ModsDir
    Папка Mods игры. Если не указана — вычисляется автоматически из ManagedDir.

.EXAMPLE
    .\Scripts\build.ps1                  # Release-сборка
    .\Scripts\build.ps1 -Mode debug      # Debug-сборка
    .\Scripts\build.ps1 -Mode deploy     # собрать Release и установить в игру
    .\Scripts\build.ps1 -Mode deploy-debug
    .\Scripts\build.ps1 -Mode deploy -ManagedDir 'E:\Games\MSC\mysummercar_Data\Managed'
#>
[CmdletBinding()]
param(
    [ValidateSet('build', 'debug', 'deploy', 'deploy-debug')]
    [string]$Mode = 'build',

    # Путь к папке Managed игры по умолчанию (переопределяется параметром при необходимости).
    [string]$ManagedDir = 'D:\SteamLibrary\steamapps\common\My Summer Car\mysummercar_Data\Managed',

    # Пустая строка => вычислить папку Mods из ManagedDir.
    [string]$ModsDir = ''
)

# Любая ошибка останавливает скрипт; вывод dotnet — на английском (стабильные сообщения).
$ErrorActionPreference = 'Stop'
$env:DOTNET_CLI_UI_LANGUAGE = 'en'

# Небольшой помощник для единообразного цветного вывода этапов.
function Write-Step([string]$Message, [string]$Color = 'Cyan') {
    Write-Host "[MOP-REVIVAL] $Message" -ForegroundColor $Color
}

# --- Пути проекта (вычисляются относительно этого скрипта) --------------------------------
# Скрипт лежит в <КореньРепо>\Scripts, проект — в <КореньРепо>\MOPR\MOPR.csproj,
# а папки Images/Libs/Server лежат в самом <КореньРепо>.
$ScriptDir  = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot   = Split-Path -Parent $ScriptDir
$ProjectDir = Join-Path $RepoRoot 'MOPR'
$Project    = Join-Path $ProjectDir 'MOPR.csproj'

if (-not (Test-Path $Project)) {
    Write-Step "Project file not found: $Project" 'Red'
    exit 1
}

# --- Режим -> конфигурация сборки + флаг установки ----------------------------------------
switch ($Mode) {
    'build'        { $Configuration = 'Release'; $Deploy = $false }
    'debug'        { $Configuration = 'Debug';   $Deploy = $false }
    'deploy'       { $Configuration = 'Release'; $Deploy = $true  }
    'deploy-debug' { $Configuration = 'Debug';   $Deploy = $true  }
}

Write-Step "Mode: $Mode  |  Configuration: $Configuration"

# --- Сборка -------------------------------------------------------------------------------
Write-Step 'Building...'
dotnet build $Project -c $Configuration -nologo -p:ManagedDir=$ManagedDir
if ($LASTEXITCODE -ne 0) {
    Write-Step 'Build FAILED.' 'Red'
    exit $LASTEXITCODE
}

# Проверяем, что итоговая DLL действительно появилась (MOPR\bin\<Configuration>\MOPR.dll).
$OutputDll = Join-Path $ProjectDir "bin\$Configuration\MOPR.dll"
if (-not (Test-Path $OutputDll)) {
    Write-Step "Build reported success but output is missing: $OutputDll" 'Red'
    exit 1
}

$SizeKb = [math]::Round((Get-Item $OutputDll).Length / 1KB, 1)
Write-Step "Build OK -> $OutputDll ($SizeKb KB)" 'Green'

# --- Установка в игру (только для режимов deploy / deploy-debug) ---------------------------
if ($Deploy) {

    # Если папку Mods не задали явно — выводим её из ManagedDir.
    # Managed -> ...\mysummercar_Data\Managed, папка игры на два уровня выше, Mods рядом с игрой.
    if ([string]::IsNullOrEmpty($ModsDir)) {
        $GameDir = Split-Path -Parent (Split-Path -Parent $ManagedDir)
        $ModsDir = Join-Path $GameDir 'Mods'
    }

    if (-not (Test-Path $ModsDir)) {
        Write-Step "Mods folder not found: $ModsDir" 'Red'
        Write-Step 'Pass -ModsDir to specify it manually.' 'Yellow'
        exit 1
    }

    # 1) Сам мод -> <Mods>\MOPR.dll
    Copy-Item $OutputDll -Destination $ModsDir -Force
    Write-Step "Deployed MOPR.dll -> $ModsDir" 'Green'

    # 2) BouncyCastle.Crypto.dll -> <Mods>\References
    #    Даёт моду управляемый TLS 1.2 для связи с сервером правил (старый Mono игры этого не умеет).
    #    В проекте он подключён с Private=false, поэтому рядом с MOPR.dll не копируется — кладём вручную.
    $BouncySrc = Join-Path $RepoRoot 'Libs\BouncyCastle.Crypto.dll'
    if (Test-Path $BouncySrc) {
        $RefDst = Join-Path $ModsDir 'References'
        if (-not (Test-Path $RefDst)) {
            New-Item -ItemType Directory -Path $RefDst | Out-Null
        }
        Copy-Item $BouncySrc -Destination $RefDst -Force
        Write-Step "Deployed BouncyCastle.Crypto.dll -> $RefDst" 'Green'
    }
    else {
        Write-Step "BouncyCastle not found at $BouncySrc (rule syncing may not work)." 'Yellow'
    }

    # 3) Для Debug дополнительно копируем .pdb (символы для отладки/читаемых стектрейсов).
    if ($Configuration -eq 'Debug') {
        $Pdb = [System.IO.Path]::ChangeExtension($OutputDll, '.pdb')
        if (Test-Path $Pdb) {
            Copy-Item $Pdb -Destination $ModsDir -Force
            Write-Step "Deployed MOPR.pdb -> $ModsDir" 'Green'
        }
    }

    Write-Step 'Close the game before deploying if the DLL is locked.' 'DarkGray'
}

Write-Step 'Done.' 'Green'
