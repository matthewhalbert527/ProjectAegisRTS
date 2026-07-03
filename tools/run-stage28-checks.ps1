[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$dotnet = Find-DotNet
$corePath = Join-Path $repoRoot 'src\Rts.Core'

Write-Host 'Stage 28 flattened full acceptance gate.'
Write-Host 'Coverage: one Rts.Core test run, one Unity DLL build, medium/full recursion audits, direct Stage4/5 hand controls, Stage16/16.5 boot flow, Stage19.5 sidebar/pause, Stage21.5 display scaling, Stage27.1 placement HUD split, Stage28 feature regression, Stage28.1 safe-area layout, player-facing Windows build/log, UnityEngine-free scan, and whitespace.'
Write-Host 'Expected runtime: materially shorter than the old recursive full-chain replay. This gate intentionally does not call run-stage27-checks.ps1, run-stage26-checks.ps1, or earlier legacy full gates.'

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

Write-ValidationSection 'Direct Stage 4 hand-control validation'
& (Join-Path $repoRoot 'tools\run-unity-stage4-validation.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-unity-stage4-validation.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Direct Stage 5 hand-control validation'
& (Join-Path $repoRoot 'tools\run-unity-stage5-validation.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-unity-stage5-validation.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Direct Stage 16 boot/player-facing validation'
& (Join-Path $repoRoot 'tools\run-unity-stage16-validation.ps1') -SkipCoreBuild
if ($LASTEXITCODE -ne 0) {
    throw "run-unity-stage16-validation.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Direct Stage 19.5 PC sidebar/pause validation'
& (Join-Path $repoRoot 'tools\run-unity-stage19-5-validation.ps1') -SkipCoreBuild
if ($LASTEXITCODE -ne 0) {
    throw "run-unity-stage19-5-validation.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Direct Stage 21.5 display/window validation'
& (Join-Path $repoRoot 'tools\run-unity-stage21-5-validation.ps1') -SkipCoreBuild
if ($LASTEXITCODE -ne 0) {
    throw "run-unity-stage21-5-validation.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Direct Stage 27.1 placement HUD separation validation'
& (Join-Path $repoRoot 'tools\run-unity-stage27-1-validation.ps1') -SkipCoreBuild
if ($LASTEXITCODE -ne 0) {
    throw "run-unity-stage27-1-validation.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Direct Stage 28 feature regression validation'
& (Join-Path $repoRoot 'tools\run-unity-stage28-validation.ps1') -SkipCoreBuild
if ($LASTEXITCODE -ne 0) {
    throw "run-unity-stage28-validation.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Direct Stage 28.1 PC safe-area layout validation'
& (Join-Path $repoRoot 'tools\run-unity-stage28-1-validation.ps1') -SkipCoreBuild
if ($LASTEXITCODE -ne 0) {
    throw "run-unity-stage28-1-validation.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 28.1 player-facing validation'
& (Join-Path $repoRoot 'tools\run-stage28-1-player-facing-checks.ps1') -SkipCoreBuild -SkipStage28Validation -SkipStage28_1Validation -SkipPlayerBuild -SkipPlayerLog
if ($LASTEXITCODE -ne 0) {
    throw "run-stage28-1-player-facing-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Windows player build'
& (Join-Path $repoRoot 'tools\build-windows-player-stage16.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "build-windows-player-stage16.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Player.log inspection'
& (Join-Path $repoRoot 'tools\inspect-latest-player-log.ps1') -CopyToDebugLogs -RequireDisplayStartup
if ($LASTEXITCODE -ne 0) {
    throw "inspect-latest-player-log.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Rts.Core UnityEngine-free scan'
Test-RtsCoreUnityEngineFree -CorePath $corePath

Write-ValidationSection 'Normalize Unity-generated whitespace'
Repair-UnityGeneratedValidationWhitespace -RepoRoot $repoRoot

Write-ValidationSection 'Whitespace check'
Invoke-GitDiffCheck -RepoRoot $repoRoot

Write-Host 'Stage 28 flattened full checks passed.'
