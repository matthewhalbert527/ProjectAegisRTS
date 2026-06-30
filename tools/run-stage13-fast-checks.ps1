[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$corePath = Join-Path $repoRoot 'src\Rts.Core'

Write-ValidationSection 'Stage 13 fast checks'
Write-Host 'Purpose: fast current-stage iteration for Stage 13 map terrain pathing changes.'
Write-Host 'Scope: Unity DLL build, Stage 13 scene validation, Stage 13 Play Mode smoke or live fallback, Rts.Core UnityEngine scan, and git diff whitespace check.'
Write-Host 'This fast tier intentionally does not run Stage 1 through Stage 12 checks.'

Write-ValidationSection 'Stage 13 Unity validation'
& (Join-Path $repoRoot 'tools\run-unity-stage13-validation.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-unity-stage13-validation.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Rts.Core UnityEngine-free scan'
Test-RtsCoreUnityEngineFree -CorePath $corePath

Write-ValidationSection 'Whitespace check'
Invoke-GitDiffCheck -RepoRoot $repoRoot

Write-Host 'Stage 13 fast checks passed.'
