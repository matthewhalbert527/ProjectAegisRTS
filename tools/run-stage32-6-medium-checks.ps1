[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$dotnet = Find-DotNet
$corePath = Join-Path $repoRoot 'src\Rts.Core'

Write-ValidationSection 'Stage 32.6 medium checks'
Write-Host 'Scope: Rts.Core tests/build, validation recursion audits, direct Stage4/Stage5 hand-control validation, Stage32.6 terrain validation, player-facing terrain checks with player build/log skipped, UnityEngine-free scan, and whitespace.'

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

Write-ValidationSection 'Stage 32.6 Unity validation'
& (Join-Path $repoRoot 'tools\run-unity-stage32-6-validation.ps1') -SkipCoreBuild
if ($LASTEXITCODE -ne 0) {
    throw "run-unity-stage32-6-validation.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 32.6 final terrain mesh Batch01 validation'
& (Join-Path $repoRoot 'tools\run-unity-stage32-6-final-terrain-mesh-validation.ps1') -SkipCoreBuild
if ($LASTEXITCODE -ne 0) {
    throw "run-unity-stage32-6-final-terrain-mesh-validation.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 32.6 player-facing validation'
& (Join-Path $repoRoot 'tools\run-stage32-6-player-facing-checks.ps1') -SkipCoreBuild -SkipStage32_6Validation -SkipPlayerBuild -SkipPlayerLog
if ($LASTEXITCODE -ne 0) {
    throw "run-stage32-6-player-facing-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Rts.Core UnityEngine-free scan'
Test-RtsCoreUnityEngineFree -CorePath $corePath

Write-ValidationSection 'Normalize Unity-generated whitespace'
Repair-UnityGeneratedValidationWhitespace -RepoRoot $repoRoot

Write-ValidationSection 'Whitespace check'
Invoke-GitDiffCheck -RepoRoot $repoRoot

Write-Host 'Stage 32.6 medium checks passed.'
