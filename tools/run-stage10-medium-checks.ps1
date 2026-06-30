[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$dotnet = Find-DotNet
$corePath = Join-Path $repoRoot 'src\Rts.Core'

Write-ValidationSection 'Stage 10 medium checks'
Write-Host 'Purpose: pre-commit confidence for Stage 10 changes.'
Write-Host 'Scope: Rts.Core tests, Unity DLL build, direct Stage 9 medium validation as the immediate dependency, Stage 10 validation, Stage 10 Play Mode smoke or live fallback, Rts.Core UnityEngine scan, and git diff whitespace check.'
Write-Host 'This medium tier avoids replaying the full Stage 0-through-Stage 10 acceptance chain. Use run-stage10-checks.ps1 for final acceptance.'

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

Write-ValidationSection 'Stage 9 immediate dependency validation'
& (Join-Path $repoRoot 'tools\run-stage9-medium-checks.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-stage9-medium-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 10 Unity validation'
& (Join-Path $repoRoot 'tools\run-unity-stage10-validation.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-unity-stage10-validation.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Rts.Core UnityEngine-free scan'
Test-RtsCoreUnityEngineFree -CorePath $corePath

Write-ValidationSection 'Whitespace check'
Invoke-GitDiffCheck -RepoRoot $repoRoot

Write-Host 'Stage 10 medium checks passed.'
