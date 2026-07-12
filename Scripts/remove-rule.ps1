<#
.SYNOPSIS
    Удаление правил (.mopconfig) из базы MOP - REVIVAL напрямую на GitHub.

.DESCRIPTION
    Работает с ЖИВЫМ состоянием репозитория на GitHub (ветка origin/main), а не с
    локальной папкой Server\rules — поэтому список и удаление всегда актуальны даже
    после мерджа PR, когда локальный проект ещё не подтянут.

    Как это устроено:
      * Список правил берётся из origin/main (после git fetch) командой git ls-tree.
      * Удаление выполняется во ВРЕМЕННОМ worktree на базе origin/main: файлы удаляются,
        создаётся коммит и пушится в main. Ваша рабочая папка при этом не затрагивается,
        а git использует уже настроенную авторизацию (токен не нужен).
      * После пуша Vercel автоматически пересоберёт сайт и manifest.json.

    Требуется только установленный git и права на пуш в репозиторий (как обычно).

.PARAMETER Name
    Идентификатор(ы) правила — имя файла с расширением .mopconfig или без него.
    Можно передать несколько через запятую или пробел.

.PARAMETER List
    Показать все правила на GitHub и выйти (ничего не удаляя).

.PARAMETER Branch
    Ветка, с которой работаем. По умолчанию main.

.PARAMETER Force
    Не спрашивать подтверждение перед удалением.

.PARAMETER DryRun
    Только показать, что будет удалено, без фактического удаления.

.EXAMPLE
    .\Scripts\remove-rule.ps1 -List
    .\Scripts\remove-rule.ps1 -Name MOPR_PipelineTest
    .\Scripts\remove-rule.ps1 -Name FURY,GAZ24 -Force
    .\Scripts\remove-rule.ps1 -Name OldRule -DryRun
#>
[CmdletBinding()]
param(
    [Parameter(Position = 0, ValueFromRemainingArguments = $true)]
    [string[]]$Name,

    [switch]$List,
    [string]$Branch = 'main',
    [switch]$Force,
    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'
$env:GIT_TERMINAL_PROMPT = '0'

function Write-Step([string]$Message, [string]$Color = 'Cyan') {
    Write-Host "[MOP-REVIVAL] $Message" -ForegroundColor $Color
}

# git пишет прогресс/подсказки в stderr — под 'Stop' PowerShell принял бы это за
# ошибку. Обёртка выполняет git с ErrorActionPreference=Continue и возвращает вывод;
# код возврата остаётся в $global:GitExit.
function Invoke-Git {
    param([Parameter(ValueFromRemainingArguments = $true)][string[]]$GitArgs)
    $prev = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    try {
        $output = & git @GitArgs 2>&1
        $global:GitExit = $LASTEXITCODE
        return $output
    }
    finally {
        $ErrorActionPreference = $prev
    }
}

# --- Пути и проверки ---------------------------------------------------------
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot  = Split-Path -Parent $ScriptDir
$RemoteRef = "origin/$Branch"
$RulesPath = 'Server/rules'

if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
    Write-Step 'git not found in PATH. Install Git and try again.' 'Red'
    exit 1
}

Push-Location $RepoRoot
try {
    # --- Синхронизируем состояние ветки с GitHub -----------------------------
    Write-Step "Fetching latest state from GitHub ($RemoteRef)..."
    Invoke-Git fetch origin $Branch --quiet | Out-Null
    if ($global:GitExit -ne 0) {
        Write-Step 'git fetch failed. Check your connection / remote.' 'Red'
        exit 1
    }

    function Get-RemoteRules {
        $lines = Invoke-Git ls-tree -r --name-only $RemoteRef -- $RulesPath
        if ($global:GitExit -ne 0) { return @() }
        $lines |
            Where-Object { $_ -match '\.mopconfig$' } |
            ForEach-Object { [System.IO.Path]::GetFileNameWithoutExtension($_.Trim()) } |
            Sort-Object
    }

    $remote = @(Get-RemoteRules)

    # --- Режим списка --------------------------------------------------------
    if ($List) {
        Write-Step "Rules on GitHub ($Branch): $($remote.Count)" 'Green'
        $remote | ForEach-Object { "  - $_" }
        exit 0
    }

    if (-not $Name -or $Name.Count -eq 0) {
        Write-Step 'No rule name given. Use -List to see rules, or pass -Name <Id>.' 'Yellow'
        exit 1
    }

    # --- Сопоставляем цели с живым списком GitHub ----------------------------
    # Разбиваем на случай, когда имена переданы через запятую одной строкой
    # (так бывает при запуске через powershell -File "...").
    $wanted = @()
    foreach ($n in $Name) { $wanted += ($n -split '[,\s]+') }

    $targets = @()
    $missing = @()
    foreach ($n in $wanted) {
        $base = ($n -replace '\.mopconfig$', '').Trim()
        if ([string]::IsNullOrWhiteSpace($base)) { continue }
        if ($remote -contains $base) { $targets += $base }
        else { $missing += $base }
    }

    foreach ($m in $missing) {
        Write-Step "Rule not found on GitHub (skipped): $m" 'Yellow'
    }

    if ($targets.Count -eq 0) {
        Write-Step 'Nothing to delete.' 'Red'
        exit 1
    }

    Write-Step "Rule(s) to delete from GitHub: $($targets.Count)" 'Yellow'
    $targets | ForEach-Object { "  - $_" }

    # --- Сухой прогон --------------------------------------------------------
    if ($DryRun) {
        Write-Step 'Dry run - nothing was deleted.' 'DarkGray'
        exit 0
    }

    # --- Подтверждение -------------------------------------------------------
    if (-not $Force) {
        $answer = Read-Host "Delete these rule(s) from GitHub ($Branch)? [y/N]"
        if ($answer -notmatch '^(y|yes)$') {
            Write-Step 'Cancelled.' 'DarkGray'
            exit 0
        }
    }

    # --- Удаление во временном worktree на базе origin/main -------------------
    # Так мы не трогаем вашу рабочую папку и коммитим ровно поверх свежего main.
    $work = Join-Path ([System.IO.Path]::GetTempPath()) ("mopr-rules-" + [guid]::NewGuid().ToString('N'))

    Write-Step 'Preparing a clean checkout of the remote branch...'
    Invoke-Git worktree add --detach --quiet $work $RemoteRef | Out-Null
    if ($global:GitExit -ne 0 -or -not (Test-Path $work)) {
        Write-Step 'Failed to create a temporary worktree.' 'Red'
        exit 1
    }

    try {
        Push-Location $work
        try {
            foreach ($base in $targets) {
                Invoke-Git rm --quiet -- ("$RulesPath/$base.mopconfig") | Out-Null
                if ($global:GitExit -ne 0) {
                    Write-Step "Failed to stage deletion of $base." 'Red'
                    exit 1
                }
                Write-Step "Removed $base.mopconfig" 'Green'
            }

            $msg = 'Remove rule(s): ' + ($targets -join ', ')
            Invoke-Git commit -m $msg --quiet | Out-Null
            if ($global:GitExit -ne 0) {
                Write-Step 'Nothing to commit (already removed?).' 'Yellow'
                exit 0
            }

            Write-Step 'Pushing to GitHub...'
            Invoke-Git push origin "HEAD:$Branch" | Out-Null
            if ($global:GitExit -ne 0) {
                Write-Step 'Push rejected (remote moved?). Re-run the script.' 'Red'
                exit 1
            }
            Write-Step 'Pushed. Vercel will redeploy and drop the rule(s) from the live manifest.' 'Green'
        }
        finally {
            Pop-Location
        }
    }
    finally {
        # Всегда убираем временный worktree.
        Invoke-Git worktree remove --force $work | Out-Null
        Invoke-Git worktree prune | Out-Null
    }

    Write-Step 'Tip: run "git pull" to sync your local copy with the change.' 'DarkGray'
    Write-Step 'Done.' 'Green'
}
finally {
    Pop-Location
}
