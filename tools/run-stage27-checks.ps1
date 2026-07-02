[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$corePath = Join-Path $repoRoot 'src\Rts.Core'

Write-Host 'Stage 27 full acceptance gate: validates Stage 0 through Stage 26, then skirmish AI pressure, difficulty controls, and player-facing smoke.'
Write-Warning 'This is the slow final acceptance gate. Use run-stage27-fast-checks.ps1 for iteration and run-stage27-medium-checks.ps1 before commit.'

Write-ValidationSection 'Medium recursion audit'
& (Join-Path $repoRoot 'tools\audit-medium-validation-recursion.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "audit-medium-validation-recursion.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 26 full acceptance dependency'
& (Join-Path $repoRoot 'tools\run-stage26-checks.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-stage26-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 27 fast validation'
& (Join-Path $repoRoot 'tools\run-stage27-fast-checks.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-stage27-fast-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 27 player-facing validation'
& (Join-Path $repoRoot 'tools\run-stage27-player-facing-checks.ps1') -SkipCoreBuild -SkipStage26Validation -SkipStage27Validation
if ($LASTEXITCODE -ne 0) {
    throw "run-stage27-player-facing-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Rts.Core UnityEngine-free scan'
Test-RtsCoreUnityEngineFree -CorePath $corePath

Write-ValidationSection 'Whitespace check'
Invoke-GitDiffCheck -RepoRoot $repoRoot

Write-Host 'Stage 27 full checks passed.'
