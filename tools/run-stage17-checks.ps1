[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')

Write-Host 'Stage 17 full acceptance gate: validates Stage 0 through Stage 17 and the player-facing Windows build flow.'
Write-Warning 'This is the slow final acceptance gate. Use run-stage17-fast-checks.ps1 for iteration and run-stage17-medium-checks.ps1 before commit.'

$global:LASTEXITCODE = 0
& (Join-Path $repoRoot 'tools\audit-medium-validation-recursion.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "audit-medium-validation-recursion.ps1 failed with exit code $LASTEXITCODE."
}

& (Join-Path $repoRoot 'tools\run-stage-full-chain-checks.ps1') -Stage 17
if ($LASTEXITCODE -ne 0) {
    throw "run-stage-full-chain-checks.ps1 failed with exit code $LASTEXITCODE."
}

& (Join-Path $repoRoot 'tools\run-stage17-player-facing-checks.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-stage17-player-facing-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-Host 'Stage 17 checks passed.'
