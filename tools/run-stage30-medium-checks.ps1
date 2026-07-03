[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$dotnet = Find-DotNet
$corePath = Join-Path $repoRoot 'src\Rts.Core'

Write-ValidationSection 'Stage 30 medium checks'
Write-Host 'Purpose: pre-commit confidence for Stage 30 visual readability QA without recursive medium chaining.'
Write-Host 'Scope: Rts.Core tests/build, direct Stage29/28.1 preservation, Stage4/Stage5 hand controls, Stage30 readability validation, Stage30 player-facing checks with Windows build skipped, recursion audits, UnityEngine-free scan, and git diff whitespace check.'

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

Write-ValidationSection 'Direct Stage 29 Unity validation'
& (Join-Path $repoRoot 'tools\run-unity-stage29-validation.ps1') -SkipCoreBuild
if ($LASTEXITCODE -ne 0) {
    throw "run-unity-stage29-validation.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Direct Stage 28.1 Unity validation'
& (Join-Path $repoRoot 'tools\run-unity-stage28-1-validation.ps1') -SkipCoreBuild
if ($LASTEXITCODE -ne 0) {
    throw "run-unity-stage28-1-validation.ps1 failed with exit code $LASTEXITCODE."
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

Write-ValidationSection 'Stage 30 Unity validation'
& (Join-Path $repoRoot 'tools\run-unity-stage30-validation.ps1') -SkipCoreBuild
if ($LASTEXITCODE -ne 0) {
    throw "run-unity-stage30-validation.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 30 player-facing validation'
& (Join-Path $repoRoot 'tools\run-stage30-player-facing-checks.ps1') -SkipCoreBuild -SkipPlayerBuild -SkipPlayerLog -SkipStage29Validation -SkipStage30Validation
if ($LASTEXITCODE -ne 0) {
    throw "run-stage30-player-facing-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Rts.Core UnityEngine-free scan'
Test-RtsCoreUnityEngineFree -CorePath $corePath

Write-ValidationSection 'Normalize Unity-generated whitespace'
Repair-UnityGeneratedValidationWhitespace -RepoRoot $repoRoot

Write-ValidationSection 'Whitespace check'
Invoke-GitDiffCheck -RepoRoot $repoRoot

Write-Host 'Stage 30 medium checks passed.'
