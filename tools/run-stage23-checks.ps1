[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$corePath = Join-Path $repoRoot 'src\Rts.Core'

Write-Host 'Stage 23 full acceptance gate: validates Stage 0 through Stage 22, then base management and player-facing base-command smoke.'
Write-Warning 'This is the slow final acceptance gate. Use run-stage23-fast-checks.ps1 for iteration and run-stage23-medium-checks.ps1 before commit.'

Write-ValidationSection 'Medium recursion audit'
& (Join-Path $repoRoot 'tools\audit-medium-validation-recursion.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "audit-medium-validation-recursion.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 22 full acceptance dependency'
& (Join-Path $repoRoot 'tools\run-stage22-checks.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-stage22-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 23 fast validation'
& (Join-Path $repoRoot 'tools\run-stage23-fast-checks.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-stage23-fast-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 23 player-facing validation'
& (Join-Path $repoRoot 'tools\run-stage23-player-facing-checks.ps1') -SkipCoreBuild -SkipStage22Validation -SkipStage23Validation
if ($LASTEXITCODE -ne 0) {
    throw "run-stage23-player-facing-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Rts.Core UnityEngine-free scan'
Test-RtsCoreUnityEngineFree -CorePath $corePath

Write-ValidationSection 'Whitespace check'
Invoke-GitDiffCheck -RepoRoot $repoRoot

Write-Host 'Stage 23 full checks passed.'
