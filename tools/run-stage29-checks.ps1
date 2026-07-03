[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$corePath = Join-Path $repoRoot 'src\Rts.Core'

Write-Host 'Stage 29 full acceptance gate: delegates to the flattened Stage 28.1 full gate, then adds Stage 29 visual QA, player-facing/log coverage, UnityEngine-free scan, and whitespace.'
Write-Host 'Expected runtime: slow. Use run-stage29-fast-checks.ps1 for visual iteration and run-stage29-medium-checks.ps1 before commit.'

Write-ValidationSection 'Full recursion audit'
& (Join-Path $repoRoot 'tools\audit-full-validation-recursion.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "audit-full-validation-recursion.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Flattened Stage 28.1 full gate'
& (Join-Path $repoRoot 'tools\run-stage28-1-checks.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-stage28-1-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 29 Unity visual validation'
& (Join-Path $repoRoot 'tools\run-unity-stage29-validation.ps1') -SkipCoreBuild
if ($LASTEXITCODE -ne 0) {
    throw "run-unity-stage29-validation.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 29 player-facing validation'
& (Join-Path $repoRoot 'tools\run-stage29-player-facing-checks.ps1') -SkipCoreBuild -SkipStage28_1Validation -SkipStage29Validation
if ($LASTEXITCODE -ne 0) {
    throw "run-stage29-player-facing-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Rts.Core UnityEngine-free scan'
Test-RtsCoreUnityEngineFree -CorePath $corePath

Write-ValidationSection 'Normalize Unity-generated whitespace'
Repair-UnityGeneratedValidationWhitespace -RepoRoot $repoRoot

Write-ValidationSection 'Whitespace check'
Invoke-GitDiffCheck -RepoRoot $repoRoot

Write-Host 'Stage 29 checks passed.'
