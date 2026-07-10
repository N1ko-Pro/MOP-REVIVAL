<#
.SYNOPSIS
    Удаление правил (.mopconfig) из базы MOP - REVIVAL.

.DESCRIPTION
    Удаляет один или несколько файлов правил из Server\rules, перегенерирует
    manifest.json локально и (по флагу -Push) коммитит и пушит изменения — тогда
    Vercel автоматически пересоберёт сайт и уберёт правила из живого манифеста.

    Скрипт лежит в <КореньРепо>\Scripts, правила — в <КореньРепо>\Server\rules.

.PARAMETER Name
    Идентификатор(ы) правила — имя файла с расширением .mopconfig или без него.
    Можно передать несколько через запятую или пробел.

.PARAMETER List
    Показать все имеющиеся правила и выйти (ничего не удаляя).

.PARAMETER Push
    После удаления сделать git commit + push в origin/main (запускает авто-деплой Vercel).

.PARAMETER Force
    Не спрашивать подтверждение перед удалением.

.PARAMETER NoManifest
    Не перегенерировать manifest.json локально после удаления.

.PARAMETER DryRun
    Только показать, что будет удалено, без фактического удаления.

.EXAMPLE
    .\Scripts\remove-rule.ps1 -List
    .\Scripts\remove-rule.ps1 -Name MOPR_PipelineTest
    .\Scripts\remove-rule.ps1 -Name FURY,GAZ24 -Push -Force
    .\Scripts\remove-rule.ps1 -Name OldRule -DryRun
#>
[CmdletBinding()]
param(
    [Parameter(Position = 0, ValueFromRemainingArguments = $true)]
    [string[]]$Name,

    [switch]$List,
    [switch]$Push,
    [switch]$Force,
    [switch]$NoManifest,
    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'
$env:DOTNET_CLI_UI_LANGUAGE = 'en'

function Write-Step([string]$Message, [string]$Color = 'Cyan') {
    Write-Host "[MOP-REVIVAL] $Message" -ForegroundColor $Color
}

# --- Пути (вычисляются относительно этого скрипта) ---------------------------
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot  = Split-Path -Parent $ScriptDir
$ServerDir = Join-Path $RepoRoot 'Server'
$RulesDir  = Join-Path $ServerDir 'rules'

if (-not (Test-Path $RulesDir)) {
    Write-Step "Rules folder not found: $RulesDir" 'Red'
    exit 1
}

function Get-Rules {
    Get-ChildItem -Path $RulesDir -Filter '*.mopconfig' -File | Sort-Object Name
}

# --- Режим списка ------------------------------------------------------------
if ($List) {
    $rules = Get-Rules
    Write-Step "Rules in database: $($rules.Count)" 'Green'
    $rules | ForEach-Object { "  - " + $_.BaseName }
    exit 0
}

if (-not $Name -or $Name.Count -eq 0) {
    Write-Step 'No rule name given. Use -List to see rules, or pass -Name <Id>.' 'Yellow'
    exit 1
}

# --- Находим файлы к удалению ------------------------------------------------
$targets = @()
$missing = @()
foreach ($n in $Name) {
    $base = ($n -replace '\.mopconfig$', '').Trim()
    if ([string]::IsNullOrWhiteSpace($base)) { continue }
    $path = Join-Path $RulesDir ($base + '.mopconfig')
    if (Test-Path $path) { $targets += (Get-Item $path) }
    else { $missing += $base }
}

foreach ($m in $missing) {
    Write-Step "Rule not found (skipped): $m" 'Yellow'
}

if ($targets.Count -eq 0) {
    Write-Step 'Nothing to delete.' 'Red'
    exit 1
}

Write-Step "Rule file(s) to delete: $($targets.Count)" 'Yellow'
$targets | ForEach-Object { "  - " + $_.BaseName }

# --- Сухой прогон ------------------------------------------------------------
if ($DryRun) {
    Write-Step 'Dry run - nothing was deleted.' 'DarkGray'
    exit 0
}

# --- Подтверждение -----------------------------------------------------------
if (-not $Force) {
    $answer = Read-Host 'Delete these rule(s)? [y/N]'
    if ($answer -notmatch '^(y|yes)$') {
        Write-Step 'Cancelled.' 'DarkGray'
        exit 0
    }
}

# --- Удаление ----------------------------------------------------------------
$deleted = @()
foreach ($t in $targets) {
    Remove-Item -LiteralPath $t.FullName -Force
    Write-Step "Deleted $($t.Name)" 'Green'
    $deleted += $t.BaseName
}

# --- Перегенерация манифеста -------------------------------------------------
if (-not $NoManifest) {
    if (Test-Path (Join-Path $ServerDir 'package.json')) {
        Write-Step 'Regenerating manifest...'
        Push-Location $ServerDir
        # Внешние процессы (npm) могут писать в stderr — под 'Stop' это рвёт скрипт.
        $prevEap = $ErrorActionPreference
        $ErrorActionPreference = 'Continue'
        try {
            npm run manifest
            if ($LASTEXITCODE -ne 0) { Write-Step 'Manifest regeneration failed.' 'Red' }
        }
        finally {
            $ErrorActionPreference = $prevEap
            Pop-Location
        }
    }
    else {
        Write-Step 'Server/package.json not found - skipping manifest regeneration.' 'Yellow'
    }
}

# --- Git commit + push -------------------------------------------------------
if ($Push) {
    Write-Step 'Committing and pushing...'
    Push-Location $RepoRoot
    # git пишет прогресс в stderr — под 'Stop' PowerShell принял бы это за ошибку.
    $prevEap = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    try {
        foreach ($b in $deleted) {
            git add -A -- ("Server/rules/" + $b + '.mopconfig') 2>&1 | Out-Null
        }
        $msg = 'Remove rule(s): ' + ($deleted -join ', ')
        git commit -m $msg 2>&1 | Out-Null
        if ($LASTEXITCODE -ne 0) {
            Write-Step 'Nothing to commit.' 'Yellow'
        }
        else {
            git push origin main 2>&1 | Out-Null
            if ($LASTEXITCODE -ne 0) {
                Write-Step 'Push failed - commit is local. Push manually.' 'Red'
                exit 1
            }
            Write-Step 'Pushed. Vercel will redeploy automatically.' 'Green'
        }
    }
    finally {
        $ErrorActionPreference = $prevEap
        Pop-Location
    }
}
else {
    Write-Step 'Done (local only). Re-run with -Push to deploy the removal.' 'DarkGray'
}

Write-Step 'Done.' 'Green'
