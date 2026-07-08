[CmdletBinding()]
param(
    [switch]$SkipCoreBuild,
    [switch]$SkipPlayerBuild,
    [switch]$SkipPlayerLog,
    [switch]$SkipStage30PlayerFacing,
    [switch]$SkipStage31HandoffValidation
)

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$dotnet = Find-DotNet
$corePath = Join-Path $repoRoot 'src\Rts.Core'

Write-ValidationSection 'Stage 31 player-facing checks'
Write-Host 'Scope: core tests/build, recursion audits, Stage30 player-facing preservation, Stage31 handoff validation, optional Windows player build/log inspection, UnityEngine-free scan, and git diff whitespace check.'

if (-not $SkipCoreBuild) {
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
} else {
    Write-Host 'Skipping Rts.Core tests/build; caller already ran them.'
}

Write-ValidationSection 'Medium recursion audit'
& (Join-Path $repoRoot 'tools\audit-medium-validation-recursion.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "audit-medium-validation-recursion.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Full recursion audit'
& (Join-Path $repoRoot 'tools\audit-full-validation-recursion.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "audit-full-validation-recursion.ps1 failed with exit code $LASTEXITCODE."
}

if (-not $SkipStage30PlayerFacing) {
    Write-ValidationSection 'Direct Stage 30 player-facing validation'
    $stage30Args = @('-SkipCoreBuild')
    if ($SkipPlayerBuild) {
        $stage30Args += '-SkipPlayerBuild'
    }
    if ($SkipPlayerLog) {
        $stage30Args += '-SkipPlayerLog'
    }
    & (Join-Path $repoRoot 'tools\run-stage30-player-facing-checks.ps1') @stage30Args
    if ($LASTEXITCODE -ne 0) {
        throw "run-stage30-player-facing-checks.ps1 failed with exit code $LASTEXITCODE."
    }
} else {
    Write-Host 'Skipping Stage 30 player-facing validation.'
}

if (-not $SkipStage31HandoffValidation) {
    Write-ValidationSection 'Stage 31 handoff validation'
    & (Join-Path $repoRoot 'tools\run-stage31-handoff-validation.ps1') -RequireScreenshots
    if ($LASTEXITCODE -ne 0) {
        throw "run-stage31-handoff-validation.ps1 failed with exit code $LASTEXITCODE."
    }
}

if (-not $SkipPlayerBuild) {
    Write-ValidationSection 'Windows player build'
    & (Join-Path $repoRoot 'tools\build-windows-player-stage16.ps1')
    if ($LASTEXITCODE -ne 0) {
        throw "build-windows-player-stage16.ps1 failed with exit code $LASTEXITCODE."
    }
} else {
    Write-Host 'Skipping Windows player build and launch.'
}

if (-not $SkipPlayerLog) {
    Write-ValidationSection 'Player.log inspection'
    & (Join-Path $repoRoot 'tools\inspect-latest-player-log.ps1') -CopyToDebugLogs -RequireDisplayStartup
    if ($LASTEXITCODE -ne 0) {
        throw "inspect-latest-player-log.ps1 failed with exit code $LASTEXITCODE."
    }
} else {
    Write-Host 'Skipping Player.log inspection.'
}

Write-ValidationSection 'Rts.Core UnityEngine-free scan'
Test-RtsCoreUnityEngineFree -CorePath $corePath

Write-ValidationSection 'Normalize Unity-generated whitespace'
Repair-UnityGeneratedValidationWhitespace -RepoRoot $repoRoot

Write-ValidationSection 'Whitespace check'
Invoke-GitDiffCheck -RepoRoot $repoRoot

Write-Host 'Stage 31 player-facing checks passed.'
