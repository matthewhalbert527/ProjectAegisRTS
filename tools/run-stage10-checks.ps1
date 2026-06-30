[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')

Write-Host 'Stage 10 full acceptance gate: validates Stage 0 through Stage 10.'
Write-Warning 'This is the full acceptance gate. Use run-stage10-fast-checks.ps1 or run-stage10-medium-checks.ps1 for normal iteration.'

& (Join-Path $repoRoot 'tools\run-stage-full-chain-checks.ps1') -Stage 10
if ($LASTEXITCODE -ne 0) {
    throw "run-stage-full-chain-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-Host 'Stage 10 checks passed.'
