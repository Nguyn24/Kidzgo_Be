param(
    [string]$AppPath = "C:\apps\kidzgo-api",
    [string]$Pattern = "",
    [int]$Tail = 80,
    [int]$IntervalSeconds = 2,
    [ValidateSet("Error", "All", "Nssm", "NssmError", "DualError")]
    [string]$Mode = "Error",
    [string]$NssmStdoutPath = "C:\logs\backend.log",
    [string]$NssmStderrPath = "C:\logs\backend-error.log",
    [switch]$ErrorsOnly,
    [switch]$NoClear
)

$ErrorActionPreference = "Stop"

function Get-LatestLogFile {
    param(
        [string]$LogsPath,
        [string[]]$SearchPatterns,
        [switch]$ExcludeErrorLogs
    )

    if (-not (Test-Path -LiteralPath $LogsPath)) {
        return $null
    }

    foreach ($searchPattern in $SearchPatterns) {
        $files = Get-ChildItem -Path $LogsPath -Filter $searchPattern -File

        if ($ExcludeErrorLogs) {
            $files = $files | Where-Object { $_.Name -notmatch "errors?" }
        }

        $latest = $files |
            Sort-Object LastWriteTime -Descending |
            Select-Object -First 1

        if ($null -ne $latest) {
            return $latest
        }
    }

    return $null
}

function Resolve-Target {
    param(
        [string]$SelectedMode,
        [string]$SelectedPattern,
        [string]$ResolvedLogsPath
    )

    switch ($SelectedMode) {
        "Nssm" {
            return [pscustomobject]@{
                Type = "File"
                File = Get-Item -LiteralPath $NssmStdoutPath -ErrorAction SilentlyContinue
                Description = $NssmStdoutPath
            }
        }
        "NssmError" {
            return [pscustomobject]@{
                Type = "File"
                File = Get-Item -LiteralPath $NssmStderrPath -ErrorAction SilentlyContinue
                Description = $NssmStderrPath
            }
        }
        "All" {
            $patterns = if ([string]::IsNullOrWhiteSpace($SelectedPattern)) {
                @("kidzgo-prod-*.log", "kidzgo-*.log", "*.log")
            }
            else {
                @($SelectedPattern)
            }

            return [pscustomobject]@{
                Type = "Latest"
                File = Get-LatestLogFile -LogsPath $ResolvedLogsPath -SearchPatterns $patterns -ExcludeErrorLogs
                Description = "$ResolvedLogsPath ($($patterns -join ', '))"
            }
        }
        default {
            $patterns = if ([string]::IsNullOrWhiteSpace($SelectedPattern)) {
                @("kidzgo-prod-errors-*.log", "kidzgo-errors-*.log", "*errors*.log")
            }
            else {
                @($SelectedPattern)
            }

            return [pscustomobject]@{
                Type = "Latest"
                File = Get-LatestLogFile -LogsPath $ResolvedLogsPath -SearchPatterns $patterns
                Description = "$ResolvedLogsPath ($($patterns -join ', '))"
            }
        }
    }
}

function Show-LogSection {
    param(
        [string]$Title,
        [object]$LogFile,
        [int]$TailCount,
        [string]$FilterPattern,
        [switch]$FilterErrorsOnly
    )

    Write-Host ("=== {0} ===" -f $Title) -ForegroundColor Cyan

    if ($null -eq $LogFile) {
        Write-Host "No log file found." -ForegroundColor DarkYellow
        Write-Host ""
        return
    }

    $content = Get-Content -Path $LogFile.FullName -Tail $TailCount -ErrorAction SilentlyContinue

    if ($FilterErrorsOnly) {
        $content = $content | Select-String -Pattern $FilterPattern | ForEach-Object { $_.Line }
    }

    Write-Host ("File: {0}" -f $LogFile.FullName) -ForegroundColor Green
    Write-Host ("LastWriteTime: {0}" -f $LogFile.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss")) -ForegroundColor Green
    Write-Host ""

    if ($content -and $content.Count -gt 0) {
        $content
    }
    else {
        if ($FilterErrorsOnly) {
            Write-Host "No matching error lines found in the latest tail." -ForegroundColor DarkYellow
        }
        else {
            Write-Host "Log file is empty or unreadable." -ForegroundColor DarkYellow
        }
    }

    Write-Host ""
}

$logsPath = Join-Path $AppPath "logs"
$errorPattern = "ERR|Error|Exception|Unhandled|Fatal"

Write-Host "Mode: $Mode" -ForegroundColor Cyan
Write-Host "App logs path: $logsPath" -ForegroundColor Cyan
Write-Host "Pattern: $(if ([string]::IsNullOrWhiteSpace($Pattern)) { '<auto>' } else { $Pattern })" -ForegroundColor Cyan
Write-Host "NSSM stdout: $NssmStdoutPath" -ForegroundColor Cyan
Write-Host "NSSM stderr: $NssmStderrPath" -ForegroundColor Cyan
Write-Host "ErrorsOnly: $($ErrorsOnly.IsPresent)" -ForegroundColor Cyan
Write-Host "Press Ctrl+C to stop." -ForegroundColor Yellow

while ($true) {
    try {
        if ($Mode -eq "DualError") {
            $appErrorTarget = Resolve-Target -SelectedMode "Error" -SelectedPattern $Pattern -ResolvedLogsPath $logsPath
            $nssmErrorTarget = Resolve-Target -SelectedMode "NssmError" -SelectedPattern $Pattern -ResolvedLogsPath $logsPath

            if (-not $NoClear) {
                Clear-Host
            }

            Write-Host ("[{0}] Dual error watcher" -f (Get-Date -Format "yyyy-MM-dd HH:mm:ss")) -ForegroundColor Green
            Write-Host ""

            Show-LogSection -Title "Serilog Error Log" -LogFile $appErrorTarget.File -TailCount $Tail -FilterPattern $errorPattern -FilterErrorsOnly:$ErrorsOnly
            Show-LogSection -Title "NSSM STDERR" -LogFile $nssmErrorTarget.File -TailCount $Tail -FilterPattern $errorPattern -FilterErrorsOnly:$ErrorsOnly

            Start-Sleep -Seconds $IntervalSeconds
            continue
        }

        $target = Resolve-Target -SelectedMode $Mode -SelectedPattern $Pattern -ResolvedLogsPath $logsPath
        $logFile = $target.File

        if ($null -eq $logFile) {
            if (-not $NoClear) {
                Clear-Host
            }

            Write-Host "No log file found yet for $($target.Description)" -ForegroundColor Yellow
            Write-Host "Waiting..." -ForegroundColor Yellow
            Start-Sleep -Seconds $IntervalSeconds
            continue
        }

        if (-not $NoClear) {
            Clear-Host
        }

        Write-Host ("[{0}] Single log watcher" -f (Get-Date -Format "yyyy-MM-dd HH:mm:ss")) -ForegroundColor Green
        Write-Host ""
        Show-LogSection -Title $Mode -LogFile $logFile -TailCount $Tail -FilterPattern $errorPattern -FilterErrorsOnly:$ErrorsOnly
    }
    catch {
        Write-Host ("Watcher error: {0}" -f $_.Exception.Message) -ForegroundColor Red
    }

    Start-Sleep -Seconds $IntervalSeconds
}
