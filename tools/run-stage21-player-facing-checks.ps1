[CmdletBinding()]
param(
    [switch]$SkipPlayerBuild,
    [switch]$SkipPlayerLog,
    [switch]$SkipCoreBuild,
    [switch]$SkipStage20Validation,
    [switch]$SkipStage21Validation
)

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

function Test-Stage21LogForRedErrors {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        throw "Expected log was not found: $Path"
    }

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

    $hit = Select-String -LiteralPath $Path -Pattern $patterns -CaseSensitive:$false | Select-Object -First 1
    if ($hit) {
        throw "Red-error signature found in $Path at line $($hit.LineNumber): $($hit.Line.Trim())"
    }

    Write-Host "Log inspection passed: $Path"
}

function Invoke-PlayerLaunchSmoke {
    param([string]$ExePath)

    if (-not (Test-Path -LiteralPath $ExePath)) {
        throw "Windows player EXE was not found: $ExePath"
    }

    Write-Host "Launching Windows player once: $ExePath"
    $process = Start-Process -FilePath $ExePath -WindowStyle Hidden -PassThru
    Start-Sleep -Seconds 12
    if (-not $process.HasExited) {
        Stop-Process -Id $process.Id -Force
        Start-Sleep -Seconds 2
    }

    Write-Host 'Windows player launch smoke completed.'
}

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$dotnet = Find-DotNet
$logRoot = Join-Path $repoRoot 'build\unity-logs'
$corePath = Join-Path $repoRoot 'src\Rts.Core'
$exePath = Join-Path $repoRoot 'build\windows-player-stage16\ProjectAegisRTS.exe'

New-Item -ItemType Directory -Force -Path $logRoot | Out-Null

Write-ValidationSection 'Stage 21 player-facing checks'
Write-Host 'Scope: core tests/build, medium recursion audit, direct Stage20 player-facing validation, Stage4/Stage5 hand-control validation, Stage21 Unity validation, optional Windows player build/log inspection, MVP visual QA checks, UnityEngine-free scan, and git diff whitespace check.'

if ($SkipCoreBuild) {
    Write-Host 'Skipping Rts.Core tests/build; caller already ran them.'
} else {
    Write-ValidationSection 'Rts.Core tests'
    Invoke-DotNetRunNoRestore -DotNetPath $dotnet -ProjectPath (Join-Path $repoRoot 'src\Rts.Core.Tests')
    if ($LASTEXITCODE -ne 0) {
        throw "Rts.Core.Tests failed with exit code $LASTEXITCODE."
    }

    Write-ValidationSection 'Build Rts.Core for Unity'
    & (Join-Path $repoRoot 'tools\build-rts-core-for-unity.ps1')
    if ($LASTEXITCODE -ne 0) {
        throw "build-rts-core-for-unity.ps1 failed with exit code $LASTEXITCODE."
    }
}

Write-ValidationSection 'Medium recursion audit'
& (Join-Path $repoRoot 'tools\audit-medium-validation-recursion.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "audit-medium-validation-recursion.ps1 failed with exit code $LASTEXITCODE."
}

if ($SkipStage20Validation) {
    Write-Host 'Skipping Stage 20 player-facing dependency; caller already ran it.'
} else {
    Write-ValidationSection 'Stage 20 player-facing dependency'
    & (Join-Path $repoRoot 'tools\run-stage20-player-facing-checks.ps1') -SkipPlayerBuild -SkipPlayerLog -SkipCoreBuild -SkipStage19_5Validation
    if ($LASTEXITCODE -ne 0) {
        throw "run-stage20-player-facing-checks.ps1 failed with exit code $LASTEXITCODE."
    }
}

Write-ValidationSection 'Stage 4 hand-control validation'
& (Join-Path $repoRoot 'tools\run-unity-stage4-validation.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-unity-stage4-validation.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 5 hand-control validation'
& (Join-Path $repoRoot 'tools\run-unity-stage5-validation.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-unity-stage5-validation.ps1 failed with exit code $LASTEXITCODE."
}

if ($SkipStage21Validation) {
    Write-Host 'Skipping Stage 21 Unity validation; caller already ran it.'
} else {
    Write-ValidationSection 'Stage 21 Unity validation'
    & (Join-Path $repoRoot 'tools\run-unity-stage21-validation.ps1') -SkipCoreBuild
    if ($LASTEXITCODE -ne 0) {
        throw "run-unity-stage21-validation.ps1 failed with exit code $LASTEXITCODE."
    }
}

Write-ValidationSection 'Stage 21 Unity log inspection'
$logsToInspect = @(
    (Join-Path $logRoot 'stage21-artist-import-scan.log'),
    (Join-Path $logRoot 'stage21-mvp-visual-qa.log'),
    (Join-Path $logRoot 'stage21-socket-pivot-scale.log'),
    (Join-Path $logRoot 'stage21-create.log'),
    (Join-Path $logRoot 'stage21-validate.log'),
    (Join-Path $logRoot 'stage21-playmode-smoke.log')
)
foreach ($log in $logsToInspect) {
    Test-Stage21LogForRedErrors -Path $log
}

if ($SkipPlayerBuild) {
    Write-Host 'Skipping Windows player build and launch.'
} else {
    Write-ValidationSection 'Windows player build'
    & (Join-Path $repoRoot 'tools\build-windows-player-stage16.ps1')
    if ($LASTEXITCODE -ne 0) {
        throw "build-windows-player-stage16.ps1 failed with exit code $LASTEXITCODE."
    }

    Write-ValidationSection 'Windows player launch smoke'
    Invoke-PlayerLaunchSmoke -ExePath $exePath
}

if ($SkipPlayerLog) {
    Write-Host 'Skipping Player.log inspection.'
} else {
    Write-ValidationSection 'Player.log inspection'
    & (Join-Path $repoRoot 'tools\inspect-latest-player-log.ps1') -CopyToDebugLogs
    if ($LASTEXITCODE -ne 0) {
        throw "inspect-latest-player-log.ps1 failed with exit code $LASTEXITCODE."
    }
}

Write-ValidationSection 'Rts.Core UnityEngine-free scan'
Test-RtsCoreUnityEngineFree -CorePath $corePath

Write-ValidationSection 'Normalize Unity-generated whitespace'
Repair-UnityGeneratedValidationWhitespace -RepoRoot $repoRoot

Write-ValidationSection 'Whitespace check'
Invoke-GitDiffCheck -RepoRoot $repoRoot

Write-Host 'Stage 21 player-facing checks passed.'
