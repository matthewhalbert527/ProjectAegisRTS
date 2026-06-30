[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$corePath = Join-Path $repoRoot 'src\Rts.Core'

Write-ValidationSection 'Stage 10 fast checks'
Write-Host 'Purpose: fast current-stage iteration for Stage 10 economy, harvesting, refinery, scene, and placeholder resource visual changes.'
Write-Host 'Scope: Unity DLL build, Stage 10 scene validation, Stage 10 Play Mode smoke or live fallback, Rts.Core UnityEngine scan, and git diff whitespace check.'
Write-Host 'This fast tier intentionally does not run Stage 1 through Stage 9 checks.'

Write-ValidationSection 'Stage 10 Unity validation'
& (Join-Path $repoRoot 'tools\run-unity-stage10-validation.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-unity-stage10-validation.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Rts.Core UnityEngine-free scan'
Test-RtsCoreUnityEngineFree -CorePath $corePath

Write-ValidationSection 'Whitespace check'
Invoke-GitDiffCheck -RepoRoot $repoRoot

Write-Host 'Stage 10 fast checks passed.'
