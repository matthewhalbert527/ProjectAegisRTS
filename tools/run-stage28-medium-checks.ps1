[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$dotnet = Find-DotNet
$corePath = Join-Path $repoRoot 'src\Rts.Core'

Write-ValidationSection 'Stage 28 medium checks'
Write-Host 'Purpose: pre-commit confidence for integrated playtest stabilization without recursive medium chaining.'
Write-Host 'Scope: Rts.Core tests/build, direct Stage27.1 Unity/player-facing validation, direct Stage4/Stage5 hand-control validation, Stage28 Unity validation, Stage28 player-facing checks with Windows build skipped, medium recursion audit, UnityEngine-free scan, and git diff whitespace check.'

Write-ValidationSection 'Medium recursion audit'
& (Join-Path $repoRoot 'tools\audit-medium-validation-recursion.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "audit-medium-validation-recursion.ps1 failed with exit code $LASTEXITCODE."
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

Write-ValidationSection 'Direct Stage 27.1 Unity validation'
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

Write-ValidationSection 'Direct Stage 27.1 player-facing validation'
& (Join-Path $repoRoot 'tools\run-stage27-1-player-facing-checks.ps1') -SkipPlayerBuild -SkipPlayerLog -SkipCoreBuild -SkipStage27Validation -SkipStage27_1Validation -SkipHandControlValidation
if ($LASTEXITCODE -ne 0) {
    throw "run-stage27-1-player-facing-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 28 Unity validation'
& (Join-Path $repoRoot 'tools\run-unity-stage28-validation.ps1') -SkipCoreBuild
if ($LASTEXITCODE -ne 0) {
    throw "run-unity-stage28-validation.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 28 player-facing validation'
& (Join-Path $repoRoot 'tools\run-stage28-player-facing-checks.ps1') -SkipPlayerBuild -SkipPlayerLog -SkipCoreBuild -SkipStage27_1Validation -SkipHandControlValidation -SkipStage28Validation
if ($LASTEXITCODE -ne 0) {
    throw "run-stage28-player-facing-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Rts.Core UnityEngine-free scan'
Test-RtsCoreUnityEngineFree -CorePath $corePath

Write-ValidationSection 'Normalize Unity-generated whitespace'
Repair-UnityGeneratedValidationWhitespace -RepoRoot $repoRoot

Write-ValidationSection 'Whitespace check'
Invoke-GitDiffCheck -RepoRoot $repoRoot

Write-Host 'Stage 28 medium checks passed.'
