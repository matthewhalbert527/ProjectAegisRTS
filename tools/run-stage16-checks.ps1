[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')

Write-Host 'Stage 16 full acceptance gate: validates Stage 0 through Stage 16.'
Write-Warning 'This is the full acceptance gate. Use run-stage16-fast-checks.ps1 or run-stage16-medium-checks.ps1 for normal iteration.'

$global:LASTEXITCODE = 0
& (Join-Path $repoRoot 'tools\audit-medium-validation-recursion.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "audit-medium-validation-recursion.ps1 failed with exit code $LASTEXITCODE."
}

& (Join-Path $repoRoot 'tools\run-stage-full-chain-checks.ps1') -Stage 16
if ($LASTEXITCODE -ne 0) {
    throw "run-stage-full-chain-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-Host 'Stage 16 checks passed.'
