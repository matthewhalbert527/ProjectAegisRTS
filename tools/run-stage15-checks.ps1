[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')

Write-Host 'Stage 15 full acceptance gate: validates Stage 0 through Stage 15.'
Write-Warning 'This is the full acceptance gate. Use run-stage15-fast-checks.ps1 or run-stage15-medium-checks.ps1 for normal iteration.'

& (Join-Path $repoRoot 'tools\run-stage-full-chain-checks.ps1') -Stage 15
if ($LASTEXITCODE -ne 0) {
    throw "run-stage-full-chain-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-Host 'Stage 15 checks passed.'
