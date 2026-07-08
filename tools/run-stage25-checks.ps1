[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$corePath = Join-Path $repoRoot 'src\Rts.Core'

Write-Host 'Stage 25 full acceptance gate: validates Stage 0 through Stage 24, then engineer/capture/transport mechanics and player-facing smoke.'
Write-Warning 'This is the slow final acceptance gate. Use run-stage25-fast-checks.ps1 for iteration and run-stage25-medium-checks.ps1 before commit.'

Write-ValidationSection 'Medium recursion audit'
& (Join-Path $repoRoot 'tools\audit-medium-validation-recursion.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "audit-medium-validation-recursion.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 24 full acceptance dependency'
& (Join-Path $repoRoot 'tools\run-stage24-checks.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-stage24-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 25 fast validation'
& (Join-Path $repoRoot 'tools\run-stage25-fast-checks.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-stage25-fast-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 25 player-facing validation'
& (Join-Path $repoRoot 'tools\run-stage25-player-facing-checks.ps1') -SkipCoreBuild -SkipStage24Validation -SkipStage25Validation
if ($LASTEXITCODE -ne 0) {
    throw "run-stage25-player-facing-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Rts.Core UnityEngine-free scan'
Test-RtsCoreUnityEngineFree -CorePath $corePath

Write-ValidationSection 'Whitespace check'
Invoke-GitDiffCheck -RepoRoot $repoRoot

Write-Host 'Stage 25 full checks passed.'
