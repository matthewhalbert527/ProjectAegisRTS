[CmdletBinding()]
param(
    [datetime]$FreshAfter,
    [switch]$CopyToDebugLogs
)

$ErrorActionPreference = 'Stop'

$patterns = @(
    'error CS',
    'NullReferenceException',
    'MissingReferenceException',
    'MissingComponentException',
    'MissingMethodException',
    'TypeLoadException',
    'FileNotFoundException',
    'ArgumentException',
    'InvalidOperationException',
    'Scripts have compiler errors',
    'Script Compilation Error'
)

function Find-LatestPlayerLog {
    $root = Join-Path $env:USERPROFILE 'AppData\LocalLow'
    if (-not (Test-Path -LiteralPath $root)) {
        return $null
    }

    $preferred = @(
        (Join-Path $root 'DefaultCompany\ProjectAegisRTS\Player.log'),
        (Join-Path $root 'DefaultCompany\unity\Player.log'),
        (Join-Path $root 'ProjectAegisRTS\ProjectAegisRTS\Player.log'),
        (Join-Path $root 'ProjectAegisRTS\Project Aegis RTS\Player.log')
    )

    $logs = @()
    foreach ($candidate in $preferred) {
        if (Test-Path -LiteralPath $candidate) {
            $logs += Get-Item -LiteralPath $candidate
        }
    }

    $logs += Get-ChildItem -LiteralPath $root -Recurse -Filter 'Player.log' -ErrorAction SilentlyContinue |
        Where-Object { $_.FullName -match 'ProjectAegis|Project Aegis|DefaultCompany|unity' }

    if ($logs.Count -eq 0) {
        return $null
    }

    return $logs | Sort-Object LastWriteTime -Descending | Select-Object -First 1
}

$latest = Find-LatestPlayerLog
if (-not $latest) {
    Write-Warning 'No relevant Player.log was found under AppData\LocalLow. Launch the Windows player once, then rerun this script.'
    $global:LASTEXITCODE = 0
    return
}

Write-Host "Latest Player.log: $($latest.FullName)"
Write-Host "Last write: $($latest.LastWriteTime)"
Write-Host "Length: $($latest.Length)"

if ($PSBoundParameters.ContainsKey('FreshAfter') -and $latest.LastWriteTime -lt $FreshAfter) {
    Write-Warning "Newest Player.log is older than requested freshness time $FreshAfter. Launch the rebuilt EXE once to create a fresh log."
}

$hit = Select-String -LiteralPath $latest.FullName -Pattern $patterns -CaseSensitive:$false | Select-Object -First 1
if ($hit) {
    Write-Error "Red-error signature found in Player.log at line $($hit.LineNumber): $($hit.Line.Trim())"
    exit 1
}

if ($CopyToDebugLogs) {
    $repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
    $debugLogRoot = Join-Path $repoRoot 'debug-logs'
    New-Item -ItemType Directory -Force -Path $debugLogRoot | Out-Null
    Copy-Item -LiteralPath $latest.FullName -Destination (Join-Path $debugLogRoot 'latest-player.log') -Force
    Write-Host "Copied latest Player.log to $(Join-Path $debugLogRoot 'latest-player.log')"
}

Write-Host 'Player.log inspection passed; no red-error signatures found.'
$global:LASTEXITCODE = 0
