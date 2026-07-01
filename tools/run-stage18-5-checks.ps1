[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$corePath = Join-Path $repoRoot 'src\Rts.Core'

Write-Host 'Stage 18.5 full acceptance gate: validates Stage 0 through Stage 18, player-facing build flow, and the Stage 18.5 fine placement grid.'
Write-Warning 'This is the slow final acceptance gate. Use run-stage18-5-fast-checks.ps1 for iteration and run-stage18-5-medium-checks.ps1 before commit.'

Write-ValidationSection 'Medium recursion audit'
& (Join-Path $repoRoot 'tools\audit-medium-validation-recursion.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "audit-medium-validation-recursion.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 0 through Stage 18 chain'
& (Join-Path $repoRoot 'tools\run-stage-full-chain-checks.ps1') -Stage 18
if ($LASTEXITCODE -ne 0) {
    throw "run-stage-full-chain-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 18 player-facing checks'
& (Join-Path $repoRoot 'tools\run-stage18-player-facing-checks.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-stage18-player-facing-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 18.5 Unity validation'
& (Join-Path $repoRoot 'tools\run-unity-stage18-5-validation.ps1') -SkipCoreBuild
if ($LASTEXITCODE -ne 0) {
    throw "run-unity-stage18-5-validation.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Rts.Core UnityEngine-free scan'
Test-RtsCoreUnityEngineFree -CorePath $corePath

Write-ValidationSection 'Normalize Unity-generated whitespace'
Repair-UnityGeneratedValidationWhitespace -RepoRoot $repoRoot

Write-ValidationSection 'Whitespace check'
Invoke-GitDiffCheck -RepoRoot $repoRoot

Write-Host 'Stage 18.5 checks passed.'
