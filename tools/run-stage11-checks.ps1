[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')

Write-Host 'Stage 11 full acceptance gate: validates Stage 0 through Stage 11.'
Write-Warning 'This is the full acceptance gate. Use run-stage11-fast-checks.ps1 or run-stage11-medium-checks.ps1 for normal iteration.'

& (Join-Path $repoRoot 'tools\run-stage-full-chain-checks.ps1') -Stage 11
if ($LASTEXITCODE -ne 0) {
    throw "run-stage-full-chain-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-Host 'Stage 11 checks passed.'
