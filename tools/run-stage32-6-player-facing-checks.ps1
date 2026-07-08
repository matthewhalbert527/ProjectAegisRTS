[CmdletBinding()]
param(
    [switch]$SkipCoreBuild,
    [switch]$SkipStage32_6Validation,
    [switch]$SkipPlayerBuild,
    [switch]$SkipPlayerLog
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
    $process = Start-Process -FilePath $ExePath -ArgumentList $Arguments -WindowStyle Hidden -PassThru
    Start-Sleep -Seconds 45
    if (-not $process.HasExited) {
        Stop-Process -Id $process.Id -Force
        Start-Sleep -Seconds 3
    }

    Write-Host "$Label launch smoke completed."
}

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$dotnet = Find-DotNet
$corePath = Join-Path $repoRoot 'src\Rts.Core'
$exePath = Join-Path $repoRoot 'build\windows-player-stage16\ProjectAegisRTS.exe'

Write-ValidationSection 'Stage 32.6 player-facing checks'
Write-Host 'Scope: core tests/build, Stage32.6 terrain validation, optional Windows player build/launch/log, UnityEngine-free scan, and whitespace.'

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

if ($SkipStage32_6Validation) {
    Write-Host 'Skipping Stage32.6 Unity validation; caller already ran it.'
} else {
    Write-ValidationSection 'Stage 32.6 Unity validation'
    & (Join-Path $repoRoot 'tools\run-unity-stage32-6-validation.ps1') -SkipCoreBuild
    if ($LASTEXITCODE -ne 0) {
        throw "run-unity-stage32-6-validation.ps1 failed with exit code $LASTEXITCODE."
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

    Write-ValidationSection 'Windows player launch smoke at 1600x900'
    Invoke-PlayerLaunchSmoke -ExePath $exePath -Arguments @('-screen-width', '1600', '-screen-height', '900', '-screen-fullscreen', '0') -Label 'Stage 32.6 Windows player'
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

Write-Host 'Stage 32.6 player-facing checks passed.'
