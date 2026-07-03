[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$corePath = Join-Path $repoRoot 'src\Rts.Core'

Write-Host 'Stage 32 full acceptance gate: delegates to the Stage 31 final gate, then adds Stage 32 terrain-piece/set-dressing generation, terrain-kit generation, review/player-facing validation, player build/log coverage, UnityEngine-free scan, and whitespace.'
Write-Host 'Expected runtime: slow. Use run-stage32-fast-checks.ps1 for terrain iteration and run-stage32-medium-checks.ps1 before commit.'

Write-ValidationSection 'Full recursion audit'
& (Join-Path $repoRoot 'tools\audit-full-validation-recursion.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "audit-full-validation-recursion.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 31 full gate'
& (Join-Path $repoRoot 'tools\run-stage31-checks.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-stage31-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 32 Unity validation'
& (Join-Path $repoRoot 'tools\run-unity-stage32-validation.ps1') -SkipCoreBuild
if ($LASTEXITCODE -ne 0) {
    throw "run-unity-stage32-validation.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 32 terrain-kit generation and validation'
& (Join-Path $repoRoot 'tools\run-stage32-terrain-kit-generator.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-stage32-terrain-kit-generator.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 32 player-facing validation'
& (Join-Path $repoRoot 'tools\run-stage32-player-facing-checks.ps1') -SkipCoreBuild -SkipStage32Validation -SkipSafetyDependencies -SkipTerrainKit
if ($LASTEXITCODE -ne 0) {
    throw "run-stage32-player-facing-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Rts.Core UnityEngine-free scan'
Test-RtsCoreUnityEngineFree -CorePath $corePath

Write-ValidationSection 'Normalize Unity-generated whitespace'
Repair-UnityGeneratedValidationWhitespace -RepoRoot $repoRoot

Write-ValidationSection 'Whitespace check'
Invoke-GitDiffCheck -RepoRoot $repoRoot

Write-Host 'Stage 32 checks passed.'
