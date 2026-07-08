[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$corePath = Join-Path $repoRoot 'src\Rts.Core'

Write-ValidationSection 'Stage 8 fast checks'
Write-Host 'Purpose: fast current-stage iteration for Stage 8 art, prefab, and scene changes.'
Write-Host 'Scope: Rts.Core Unity build, Stage 8 generation/validation, Stage 8 Play Mode smoke or live fallback, Rts.Core UnityEngine scan, and git diff whitespace check.'
Write-Host 'This fast tier intentionally does not run Stage 1 through Stage 7 checks.'

Write-ValidationSection 'Stage 8 Unity validation'
& (Join-Path $repoRoot 'tools\run-unity-stage8-validation.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-unity-stage8-validation.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Rts.Core UnityEngine-free scan'
Test-RtsCoreUnityEngineFree -CorePath $corePath

Write-ValidationSection 'Whitespace check'
Invoke-GitDiffCheck -RepoRoot $repoRoot

Write-Host 'Stage 8 fast checks passed.'
