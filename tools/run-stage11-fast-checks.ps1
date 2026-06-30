[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$corePath = Join-Path $repoRoot 'src\Rts.Core'

Write-ValidationSection 'Stage 11 fast checks'
Write-Host 'Purpose: fast current-stage iteration for Stage 11 fog, radar, minimap, scene, and placeholder visibility visual changes.'
Write-Host 'Scope: Unity DLL build, Stage 11 scene validation, Stage 11 Play Mode smoke or live fallback, Rts.Core UnityEngine scan, and git diff whitespace check.'
Write-Host 'This fast tier intentionally does not run Stage 1 through Stage 10 checks.'

Write-ValidationSection 'Stage 11 Unity validation'
& (Join-Path $repoRoot 'tools\run-unity-stage11-validation.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-unity-stage11-validation.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Rts.Core UnityEngine-free scan'
Test-RtsCoreUnityEngineFree -CorePath $corePath

Write-ValidationSection 'Whitespace check'
Invoke-GitDiffCheck -RepoRoot $repoRoot

Write-Host 'Stage 11 fast checks passed.'
