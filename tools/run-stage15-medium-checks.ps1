[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$dotnet = Find-DotNet
$corePath = Join-Path $repoRoot 'src\Rts.Core'

Write-ValidationSection 'Stage 15 medium checks'
Write-Host 'Purpose: pre-commit confidence for Stage 15 changes.'
Write-Host 'Scope: Rts.Core tests, one Unity DLL build, direct Stage 14 Unity validation as the immediate dependency, Stage 15 validation, Stage 15 Play Mode smoke or live fallback, Rts.Core UnityEngine scan, and git diff whitespace check.'
Write-Host 'This medium tier does not call prior medium checks. Use run-stage15-checks.ps1 for final Stage 0-through-Stage 15 acceptance.'

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

Write-ValidationSection 'Stage 14 immediate dependency validation'
& (Join-Path $repoRoot 'tools\run-unity-stage14-validation.ps1') -SkipCoreBuild
if ($LASTEXITCODE -ne 0) {
    throw "run-unity-stage14-validation.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 15 Unity validation'
& (Join-Path $repoRoot 'tools\run-unity-stage15-validation.ps1') -SkipCoreBuild
if ($LASTEXITCODE -ne 0) {
    throw "run-unity-stage15-validation.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Rts.Core UnityEngine-free scan'
Test-RtsCoreUnityEngineFree -CorePath $corePath

Write-ValidationSection 'Whitespace check'
Invoke-GitDiffCheck -RepoRoot $repoRoot

Write-Host 'Stage 15 medium checks passed.'
