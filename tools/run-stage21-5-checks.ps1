[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$corePath = Join-Path $repoRoot 'src\Rts.Core'

Write-Host 'Stage 21.5 full acceptance gate: validates Stage 0 through Stage 21, then Windows player resolution/UI scaling and player-facing launch/log diagnostics.'
Write-Warning 'This is the slow final acceptance gate. Use run-stage21-5-fast-checks.ps1 for iteration and run-stage21-5-medium-checks.ps1 before commit.'

Write-ValidationSection 'Medium recursion audit'
& (Join-Path $repoRoot 'tools\audit-medium-validation-recursion.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "audit-medium-validation-recursion.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 21 full acceptance dependency'
& (Join-Path $repoRoot 'tools\run-stage21-checks.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-stage21-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 21.5 fast validation'
& (Join-Path $repoRoot 'tools\run-stage21-5-fast-checks.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-stage21-5-fast-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 21.5 player-facing validation'
& (Join-Path $repoRoot 'tools\run-stage21-5-player-facing-checks.ps1') -SkipCoreBuild -SkipStage21Validation -SkipStage21_5Validation
if ($LASTEXITCODE -ne 0) {
    throw "run-stage21-5-player-facing-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Rts.Core UnityEngine-free scan'
Test-RtsCoreUnityEngineFree -CorePath $corePath

Write-ValidationSection 'Whitespace check'
Invoke-GitDiffCheck -RepoRoot $repoRoot

Write-Host 'Stage 21.5 full checks passed.'
