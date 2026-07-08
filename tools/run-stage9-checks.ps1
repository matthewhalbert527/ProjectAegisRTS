[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')

Write-Host 'Stage 9 full acceptance gate: validates Stage 0 through Stage 9.'
Write-Warning 'This is the full acceptance gate. Use run-stage9-fast-checks.ps1 or run-stage9-medium-checks.ps1 for normal iteration.'

& (Join-Path $repoRoot 'tools\run-stage-full-chain-checks.ps1') -Stage 9
if ($LASTEXITCODE -ne 0) {
    throw "run-stage-full-chain-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-Host 'Stage 9 checks passed.'
