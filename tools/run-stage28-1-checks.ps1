[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')

Write-Host 'Stage 28.1 full acceptance gate: delegates to the flattened Stage 28 full gate, which includes Stage 28.1 safe-area layout validation and full-recursion auditing.'
Write-Host 'Expected runtime: minutes, not the old recursive Stage27->Stage26 replay chain.'

Write-ValidationSection 'Full recursion audit'
& (Join-Path $repoRoot 'tools\audit-full-validation-recursion.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "audit-full-validation-recursion.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Flattened Stage 28 full gate'
& (Join-Path $repoRoot 'tools\run-stage28-checks.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-stage28-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-Host 'Stage 28.1 checks passed.'
