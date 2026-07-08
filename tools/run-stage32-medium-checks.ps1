[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$dotnet = Find-DotNet
$corePath = Join-Path $repoRoot 'src\Rts.Core'

Write-ValidationSection 'Stage 32 medium checks'
Write-Host 'Purpose: pre-commit confidence for terrain set dressing while keeping medium validation non-recursive.'
Write-Host 'Scope: Rts.Core tests/build, direct Stage31/28.1/27.1/Stage4/Stage5 preservation, Stage32 validation/player-facing checks, terrain-kit generation/validation, audits, UnityEngine-free scan, and whitespace.'

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

Write-ValidationSection 'Direct Stage 31 handoff validation'
& (Join-Path $repoRoot 'tools\run-stage31-handoff-validation.ps1') -RequireScreenshots
if ($LASTEXITCODE -ne 0) {
    throw "run-stage31-handoff-validation.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Direct Stage 31 player-facing preservation'
& (Join-Path $repoRoot 'tools\run-stage31-player-facing-checks.ps1') -SkipCoreBuild -SkipPlayerBuild -SkipPlayerLog -SkipStage30PlayerFacing -SkipStage31HandoffValidation
if ($LASTEXITCODE -ne 0) {
    throw "run-stage31-player-facing-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Direct Stage 28.1 layout validation'
& (Join-Path $repoRoot 'tools\run-unity-stage28-1-validation.ps1') -SkipCoreBuild
if ($LASTEXITCODE -ne 0) {
    throw "run-unity-stage28-1-validation.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Direct Stage 28.1 player-facing preservation'
& (Join-Path $repoRoot 'tools\run-stage28-1-player-facing-checks.ps1') -SkipCoreBuild -SkipPlayerBuild -SkipPlayerLog -SkipStage28Validation -SkipStage28_1Validation
if ($LASTEXITCODE -ne 0) {
    throw "run-stage28-1-player-facing-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Direct Stage 27.1 placement validation'
& (Join-Path $repoRoot 'tools\run-unity-stage27-1-validation.ps1') -SkipCoreBuild
if ($LASTEXITCODE -ne 0) {
    throw "run-unity-stage27-1-validation.ps1 failed with exit code $LASTEXITCODE."
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

Write-ValidationSection 'Stage 32 Unity validation'
& (Join-Path $repoRoot 'tools\run-unity-stage32-validation.ps1') -SkipCoreBuild
if ($LASTEXITCODE -ne 0) {
    throw "run-unity-stage32-validation.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 32 terrain-kit generation and validation'
& (Join-Path $repoRoot 'tools\run-stage32-terrain-kit-generator.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-stage32-terrain-kit-generator.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 32 player-facing validation'
& (Join-Path $repoRoot 'tools\run-stage32-player-facing-checks.ps1') -SkipCoreBuild -SkipPlayerBuild -SkipPlayerLog -SkipStage32Validation -SkipSafetyDependencies -SkipTerrainKit
if ($LASTEXITCODE -ne 0) {
    throw "run-stage32-player-facing-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Rts.Core UnityEngine-free scan'
Test-RtsCoreUnityEngineFree -CorePath $corePath

Write-ValidationSection 'Normalize Unity-generated whitespace'
Repair-UnityGeneratedValidationWhitespace -RepoRoot $repoRoot

Write-ValidationSection 'Whitespace check'
Invoke-GitDiffCheck -RepoRoot $repoRoot

Write-Host 'Stage 32 medium checks passed.'
