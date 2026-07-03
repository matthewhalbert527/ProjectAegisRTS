[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$corePath = Join-Path $repoRoot 'src\Rts.Core'

Write-Host 'Stage 30 full acceptance gate: delegates to the flattened Stage 29 full gate, then adds Stage 30 readability QA, player-facing/log coverage, UnityEngine-free scan, and whitespace.'
Write-Host 'Expected runtime: slow. Use run-stage30-fast-checks.ps1 for readability iteration and run-stage30-medium-checks.ps1 before commit.'

Write-ValidationSection 'Full recursion audit'
& (Join-Path $repoRoot 'tools\audit-full-validation-recursion.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "audit-full-validation-recursion.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Flattened Stage 29 full gate'
& (Join-Path $repoRoot 'tools\run-stage29-checks.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-stage29-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 30 Unity readability validation'
& (Join-Path $repoRoot 'tools\run-unity-stage30-validation.ps1') -SkipCoreBuild
if ($LASTEXITCODE -ne 0) {
    throw "run-unity-stage30-validation.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 30 player-facing validation'
& (Join-Path $repoRoot 'tools\run-stage30-player-facing-checks.ps1') -SkipCoreBuild -SkipStage29Validation -SkipStage30Validation
if ($LASTEXITCODE -ne 0) {
    throw "run-stage30-player-facing-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Rts.Core UnityEngine-free scan'
Test-RtsCoreUnityEngineFree -CorePath $corePath

Write-ValidationSection 'Normalize Unity-generated whitespace'
Repair-UnityGeneratedValidationWhitespace -RepoRoot $repoRoot

Write-ValidationSection 'Whitespace check'
Invoke-GitDiffCheck -RepoRoot $repoRoot

Write-Host 'Stage 30 checks passed.'
