[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$dotnet = Find-DotNet
$corePath = Join-Path $repoRoot 'src\Rts.Core'

Write-ValidationSection 'Stage 8 medium checks'
Write-Host 'Purpose: pre-commit confidence for Stage 8 changes.'
Write-Host 'Scope: Rts.Core tests, Unity DLL build, direct Stage 7 Unity validation, Stage 8 validation, Stage 8 Play Mode smoke or live fallback, Rts.Core UnityEngine scan, and git diff whitespace check.'
Write-Host 'This medium tier validates the immediate Stage 7 dependency without replaying Stage 1 through Stage 6. Use run-stage8-checks.ps1 for the full recursive acceptance chain.'

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

Write-ValidationSection 'Stage 7 immediate dependency validation'
& (Join-Path $repoRoot 'tools\run-unity-stage7-validation.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-unity-stage7-validation.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 8 Unity validation'
& (Join-Path $repoRoot 'tools\run-unity-stage8-validation.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-unity-stage8-validation.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Rts.Core UnityEngine-free scan'
Test-RtsCoreUnityEngineFree -CorePath $corePath

Write-ValidationSection 'Whitespace check'
Invoke-GitDiffCheck -RepoRoot $repoRoot

Write-Host 'Stage 8 medium checks passed.'
