[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')

Write-Host 'Stage 18 full acceptance gate: validates Stage 0 through Stage 18 and the player-facing Windows build flow.'
Write-Warning 'This is the slow final acceptance gate. Use run-stage18-fast-checks.ps1 for iteration and run-stage18-medium-checks.ps1 before commit.'

$global:LASTEXITCODE = 0
& (Join-Path $repoRoot 'tools\audit-medium-validation-recursion.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "audit-medium-validation-recursion.ps1 failed with exit code $LASTEXITCODE."
}

& (Join-Path $repoRoot 'tools\run-stage-full-chain-checks.ps1') -Stage 18
if ($LASTEXITCODE -ne 0) {
    throw "run-stage-full-chain-checks.ps1 failed with exit code $LASTEXITCODE."
}

& (Join-Path $repoRoot 'tools\run-stage18-player-facing-checks.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-stage18-player-facing-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-Host 'Stage 18 checks passed.'
