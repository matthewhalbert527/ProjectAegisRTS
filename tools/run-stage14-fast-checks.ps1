[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$corePath = Join-Path $repoRoot 'src\Rts.Core'

Write-ValidationSection 'Stage 14 fast checks'
Write-Host 'Purpose: fast current-stage iteration for Stage 14 feedback changes.'
Write-Host 'Scope: Unity DLL build, Stage 14 profile generation, scene validation, Play Mode smoke or live fallback, Rts.Core UnityEngine scan, and git diff whitespace check.'
Write-Host 'This fast tier intentionally does not run Stage 1 through Stage 13 checks.'

Write-ValidationSection 'Stage 14 Unity validation'
& (Join-Path $repoRoot 'tools\run-unity-stage14-validation.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-unity-stage14-validation.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Rts.Core UnityEngine-free scan'
Test-RtsCoreUnityEngineFree -CorePath $corePath

Write-ValidationSection 'Whitespace check'
Invoke-GitDiffCheck -RepoRoot $repoRoot

Write-Host 'Stage 14 fast checks passed.'
