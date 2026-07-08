[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$corePath = Join-Path $repoRoot 'src\Rts.Core'

Write-Host 'Stage 31 full acceptance gate: delegates to the flattened Stage 30 full gate, then adds Stage 31 artist handoff/package validation, player-facing/log coverage, UnityEngine-free scan, and whitespace.'
Write-Host 'Expected runtime: slow. Use run-stage31-fast-checks.ps1 for handoff-doc iteration and run-stage31-medium-checks.ps1 before commit.'

Write-ValidationSection 'Full recursion audit'
& (Join-Path $repoRoot 'tools\audit-full-validation-recursion.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "audit-full-validation-recursion.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Flattened Stage 30 full gate'
& (Join-Path $repoRoot 'tools\run-stage30-checks.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-stage30-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 31 handoff validation'
& (Join-Path $repoRoot 'tools\run-stage31-handoff-validation.ps1') -RequireScreenshots
if ($LASTEXITCODE -ne 0) {
    throw "run-stage31-handoff-validation.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 31 player-facing validation'
& (Join-Path $repoRoot 'tools\run-stage31-player-facing-checks.ps1') -SkipCoreBuild -SkipStage30PlayerFacing -SkipStage31HandoffValidation
if ($LASTEXITCODE -ne 0) {
    throw "run-stage31-player-facing-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Rts.Core UnityEngine-free scan'
Test-RtsCoreUnityEngineFree -CorePath $corePath

Write-ValidationSection 'Normalize Unity-generated whitespace'
Repair-UnityGeneratedValidationWhitespace -RepoRoot $repoRoot

Write-ValidationSection 'Whitespace check'
Invoke-GitDiffCheck -RepoRoot $repoRoot

Write-Host 'Stage 31 checks passed.'
