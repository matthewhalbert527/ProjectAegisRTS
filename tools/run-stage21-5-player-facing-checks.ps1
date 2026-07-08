[CmdletBinding()]
param(
    [switch]$SkipPlayerBuild,
    [switch]$SkipPlayerLog,
    [switch]$SkipCoreBuild,
    [switch]$SkipStage21Validation,
    [switch]$SkipStage21_5Validation
)

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

function Invoke-PlayerLaunchSmoke {
    param(
        [string]$ExePath,
        [string[]]$Arguments = @(),
        [string]$Label = 'Windows player'
    )

    if (-not (Test-Path -LiteralPath $ExePath)) {
        throw "Windows player EXE was not found: $ExePath"
    }

    Write-Host "Launching $Label once: $ExePath $($Arguments -join ' ')"
    if ($Arguments.Count -gt 0) {
        $process = Start-Process -FilePath $ExePath -ArgumentList $Arguments -WindowStyle Hidden -PassThru
    } else {
        $process = Start-Process -FilePath $ExePath -WindowStyle Hidden -PassThru
    }
    Start-Sleep -Seconds 12
    if (-not $process.HasExited) {
        Stop-Process -Id $process.Id -Force
        Start-Sleep -Seconds 2
    }

    Write-Host "$Label launch smoke completed."
}

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$dotnet = Find-DotNet
$corePath = Join-Path $repoRoot 'src\Rts.Core'
$exePath = Join-Path $repoRoot 'build\windows-player-stage16\ProjectAegisRTS.exe'

Write-ValidationSection 'Stage 21.5 player-facing checks'
Write-Host 'Scope: core tests/build, medium recursion audit, direct Stage21 player-facing validation, Stage4/Stage5 hand-control validation, Stage21.5 Unity validation, optional Windows player build/launch/log inspection, display startup logging, UnityEngine-free scan, and git diff whitespace check.'

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

if ($SkipStage21Validation) {
    Write-Host 'Skipping Stage 21 player-facing dependency; caller already ran it.'
} else {
    Write-ValidationSection 'Direct Stage 21 player-facing validation'
    & (Join-Path $repoRoot 'tools\run-stage21-player-facing-checks.ps1') -SkipPlayerBuild -SkipPlayerLog -SkipCoreBuild -SkipStage20Validation
    if ($LASTEXITCODE -ne 0) {
        throw "run-stage21-player-facing-checks.ps1 failed with exit code $LASTEXITCODE."
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

if ($SkipStage21_5Validation) {
    Write-Host 'Skipping Stage 21.5 Unity validation; caller already ran it.'
} else {
    Write-ValidationSection 'Stage 21.5 Unity validation'
    & (Join-Path $repoRoot 'tools\run-unity-stage21-5-validation.ps1') -SkipCoreBuild
    if ($LASTEXITCODE -ne 0) {
        throw "run-unity-stage21-5-validation.ps1 failed with exit code $LASTEXITCODE."
    }
}

if ($SkipPlayerBuild) {
    Write-Host 'Skipping Windows player build and launch.'
} else {
    Write-ValidationSection 'Windows player build'
    & (Join-Path $repoRoot 'tools\build-windows-player-stage16.ps1')
    if ($LASTEXITCODE -ne 0) {
        throw "build-windows-player-stage16.ps1 failed with exit code $LASTEXITCODE."
    }

    Write-ValidationSection 'Normal Windows player launch smoke'
    Invoke-PlayerLaunchSmoke -ExePath $exePath -Label 'normal Windows player'

    Write-ValidationSection '1920x1080 windowed launch smoke'
    & (Join-Path $repoRoot 'tools\run-player-windowed-1080p.ps1') -SmokeSeconds 12 -Hidden
    if ($LASTEXITCODE -ne 0) {
        throw "run-player-windowed-1080p.ps1 failed with exit code $LASTEXITCODE."
    }
}

if ($SkipPlayerLog) {
    Write-Host 'Skipping Player.log inspection.'
} else {
    Write-ValidationSection 'Player.log inspection'
    if ($SkipPlayerBuild) {
        & (Join-Path $repoRoot 'tools\inspect-latest-player-log.ps1') -CopyToDebugLogs
    } else {
        & (Join-Path $repoRoot 'tools\inspect-latest-player-log.ps1') -CopyToDebugLogs -RequireDisplayStartup
    }
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

Write-Host 'Stage 21.5 player-facing checks passed.'
