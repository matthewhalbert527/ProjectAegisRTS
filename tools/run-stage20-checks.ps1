[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$corePath = Join-Path $repoRoot 'src\Rts.Core'

Write-Host 'Stage 20 full acceptance gate: validates Stage 0 through Stage 19.5, then Stage 20 MVP production proxy visuals and platform UI split.'
Write-Warning 'This is the slow final acceptance gate. Use run-stage20-fast-checks.ps1 for iteration and run-stage20-medium-checks.ps1 before commit.'

Write-ValidationSection 'Medium recursion audit'
& (Join-Path $repoRoot 'tools\audit-medium-validation-recursion.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "audit-medium-validation-recursion.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 19.5 full acceptance dependency'
& (Join-Path $repoRoot 'tools\run-stage19-5-checks.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-stage19-5-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 20 fast validation'
& (Join-Path $repoRoot 'tools\run-stage20-fast-checks.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-stage20-fast-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 20 player-facing validation'
& (Join-Path $repoRoot 'tools\run-stage20-player-facing-checks.ps1') -SkipCoreBuild -SkipStage19_5Validation -SkipStage20Validation
if ($LASTEXITCODE -ne 0) {
    throw "run-stage20-player-facing-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Rts.Core UnityEngine-free scan'
Test-RtsCoreUnityEngineFree -CorePath $corePath

Write-ValidationSection 'Whitespace check'
Invoke-GitDiffCheck -RepoRoot $repoRoot

Write-Host 'Stage 20 full checks passed.'
