[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$dotnet = Find-DotNet
$corePath = Join-Path $repoRoot 'src\Rts.Core'

Write-ValidationSection 'Stage 18 medium checks'
Write-Host 'Purpose: pre-commit confidence for Stage 18 tester playability polish.'
Write-Host 'Scope: Rts.Core tests, one Unity DLL build, direct Stage 17 Unity/player-facing validation, Stage 18 validation and smoke, player-facing checks without rebuilding the player, medium recursion audit, Rts.Core UnityEngine scan, and git diff whitespace check.'
Write-Host 'This medium tier does not call prior medium checks. Use run-stage18-checks.ps1 for full Stage 0-through-Stage 18 acceptance.'

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

Write-ValidationSection 'Direct Stage 17 Unity validation'
& (Join-Path $repoRoot 'tools\run-unity-stage17-validation.ps1') -SkipCoreBuild
if ($LASTEXITCODE -ne 0) {
    throw "run-unity-stage17-validation.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Direct Stage 17 player-facing validation'
& (Join-Path $repoRoot 'tools\run-stage17-player-facing-checks.ps1') -SkipPlayerBuild -SkipCoreBuild -SkipStage17Validation
if ($LASTEXITCODE -ne 0) {
    throw "run-stage17-player-facing-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 18 Unity validation'
& (Join-Path $repoRoot 'tools\run-unity-stage18-validation.ps1') -SkipCoreBuild
if ($LASTEXITCODE -ne 0) {
    throw "run-unity-stage18-validation.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 18 player-facing checks'
& (Join-Path $repoRoot 'tools\run-stage18-player-facing-checks.ps1') -SkipPlayerBuild -SkipPlayerLog -SkipCoreBuild -SkipStage18Validation
if ($LASTEXITCODE -ne 0) {
    throw "run-stage18-player-facing-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Rts.Core UnityEngine-free scan'
Test-RtsCoreUnityEngineFree -CorePath $corePath

Write-ValidationSection 'Normalize Unity-generated whitespace'
Repair-UnityGeneratedValidationWhitespace -RepoRoot $repoRoot

Write-ValidationSection 'Whitespace check'
Invoke-GitDiffCheck -RepoRoot $repoRoot

Write-Host 'Stage 18 medium checks passed.'
