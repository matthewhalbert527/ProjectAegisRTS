[CmdletBinding()]
param(
    [switch]$SkipCoreBuild,
    [switch]$SkipStage32_8Validation,
    [switch]$SkipPlayerBuild,
    [switch]$SkipPlayerLog
)

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$dotnet = Find-DotNet
$corePath = Join-Path $repoRoot 'src\Rts.Core'

Write-ValidationSection 'Stage 32.8 player-facing checks'
Write-Host 'Scope: optional core build, Stage32.8 terrain quality validation, optional Windows player build/log, UnityEngine-free scan, and whitespace.'

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

if ($SkipStage32_8Validation) {
    Write-Host 'Skipping Stage32.8 Unity validation; caller already ran it.'
} else {
    Write-ValidationSection 'Stage 32.8 Unity validation'
    & (Join-Path $repoRoot 'tools\run-unity-stage32-8-validation.ps1') -SkipCoreBuild
    if ($LASTEXITCODE -ne 0) {
        throw "run-unity-stage32-8-validation.ps1 failed with exit code $LASTEXITCODE."
    }
}

if ($SkipPlayerBuild) {
    Write-Host 'Skipping Windows player build.'
} else {
    Write-ValidationSection 'Windows player build'
    & (Join-Path $repoRoot 'tools\build-windows-player-stage16.ps1')
    if ($LASTEXITCODE -ne 0) {
        throw "build-windows-player-stage16.ps1 failed with exit code $LASTEXITCODE."
    }
}

if ($SkipPlayerLog) {
    Write-Host 'Skipping Player.log inspection.'
} else {
    Write-ValidationSection 'Player.log inspection'
    & (Join-Path $repoRoot 'tools\inspect-latest-player-log.ps1')
    if ($LASTEXITCODE -ne 0) {
        throw "inspect-latest-player-log.ps1 failed with exit code $LASTEXITCODE."
    }
}

Write-ValidationSection 'Rts.Core UnityEngine-free scan'
Test-RtsCoreUnityEngineFree -CorePath $corePath

Write-ValidationSection 'Whitespace check'
Invoke-GitDiffCheck -RepoRoot $repoRoot

Write-Host 'Stage 32.8 player-facing checks passed.'
