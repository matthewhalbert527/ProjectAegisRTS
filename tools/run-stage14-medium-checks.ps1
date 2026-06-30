[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$dotnet = Find-DotNet
$corePath = Join-Path $repoRoot 'src\Rts.Core'

Write-ValidationSection 'Stage 14 medium checks'
Write-Host 'Purpose: pre-commit confidence for Stage 14 changes.'
Write-Host 'Scope: Rts.Core tests, Unity DLL build, direct Stage 13 medium validation as the immediate dependency, Stage 14 validation, Stage 14 Play Mode smoke or live fallback, Rts.Core UnityEngine scan, and git diff whitespace check.'
Write-Host 'This medium tier avoids replaying the full Stage 0-through-Stage 14 acceptance chain. Use run-stage14-checks.ps1 for final acceptance.'

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

Write-ValidationSection 'Stage 13 immediate dependency validation'
& (Join-Path $repoRoot 'tools\run-stage13-medium-checks.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-stage13-medium-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 14 Unity validation'
& (Join-Path $repoRoot 'tools\run-unity-stage14-validation.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-unity-stage14-validation.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Rts.Core UnityEngine-free scan'
Test-RtsCoreUnityEngineFree -CorePath $corePath

Write-ValidationSection 'Whitespace check'
Invoke-GitDiffCheck -RepoRoot $repoRoot

Write-Host 'Stage 14 medium checks passed.'
