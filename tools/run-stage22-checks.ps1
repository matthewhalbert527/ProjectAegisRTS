[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$corePath = Join-Path $repoRoot 'src\Rts.Core'

Write-Host 'Stage 22 full acceptance gate: validates Stage 0 through Stage 21.5, then classic RTS command controls and player-facing command-matrix smoke.'
Write-Warning 'This is the slow final acceptance gate. Use run-stage22-fast-checks.ps1 for iteration and run-stage22-medium-checks.ps1 before commit.'

Write-ValidationSection 'Medium recursion audit'
& (Join-Path $repoRoot 'tools\audit-medium-validation-recursion.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "audit-medium-validation-recursion.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 21.5 full acceptance dependency'
& (Join-Path $repoRoot 'tools\run-stage21-5-checks.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-stage21-5-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 22 fast validation'
& (Join-Path $repoRoot 'tools\run-stage22-fast-checks.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-stage22-fast-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 22 player-facing validation'
& (Join-Path $repoRoot 'tools\run-stage22-player-facing-checks.ps1') -SkipCoreBuild -SkipStage21_5Validation -SkipStage22Validation
if ($LASTEXITCODE -ne 0) {
    throw "run-stage22-player-facing-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Rts.Core UnityEngine-free scan'
Test-RtsCoreUnityEngineFree -CorePath $corePath

Write-ValidationSection 'Whitespace check'
Invoke-GitDiffCheck -RepoRoot $repoRoot

Write-Host 'Stage 22 full checks passed.'
