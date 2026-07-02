[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$corePath = Join-Path $repoRoot 'src\Rts.Core'

Write-Host 'Stage 28 full acceptance gate: validates Stage 0 through Stage 27.1, then Stage 28 integrated feature regression, player-facing smoke, Quest hand-control preservation, Windows player build/log, and UnityEngine-free core boundaries.'
Write-Warning 'This is the slow final acceptance gate. Use run-stage28-fast-checks.ps1 for iteration and run-stage28-medium-checks.ps1 before local commits.'

Write-ValidationSection 'Medium recursion audit'
& (Join-Path $repoRoot 'tools\audit-medium-validation-recursion.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "audit-medium-validation-recursion.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 27.1 full acceptance dependency'
& (Join-Path $repoRoot 'tools\run-stage27-1-checks.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-stage27-1-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 28 fast validation'
& (Join-Path $repoRoot 'tools\run-stage28-fast-checks.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-stage28-fast-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 28 player-facing validation'
& (Join-Path $repoRoot 'tools\run-stage28-player-facing-checks.ps1') -SkipCoreBuild -SkipStage27_1Validation -SkipStage28Validation
if ($LASTEXITCODE -ne 0) {
    throw "run-stage28-player-facing-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Rts.Core UnityEngine-free scan'
Test-RtsCoreUnityEngineFree -CorePath $corePath

Write-ValidationSection 'Whitespace check'
Invoke-GitDiffCheck -RepoRoot $repoRoot

Write-Host 'Stage 28 full checks passed.'
