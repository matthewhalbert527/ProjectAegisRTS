[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$corePath = Join-Path $repoRoot 'src\Rts.Core'

Write-Host 'Stage 19 full acceptance gate: validates Stage 0 through Stage 18.5, player-facing build flow, and Stage 19 mission flow tuning.'
Write-Warning 'This is the slow final acceptance gate. Use run-stage19-fast-checks.ps1 for iteration and run-stage19-medium-checks.ps1 before commit.'

Write-ValidationSection 'Medium recursion audit'
& (Join-Path $repoRoot 'tools\audit-medium-validation-recursion.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "audit-medium-validation-recursion.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 18.5 full acceptance dependency'
& (Join-Path $repoRoot 'tools\run-stage18-5-checks.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-stage18-5-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 18.5 player-facing dependency'
& (Join-Path $repoRoot 'tools\run-stage18-5-player-facing-checks.ps1') -SkipCoreBuild
if ($LASTEXITCODE -ne 0) {
    throw "run-stage18-5-player-facing-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 19 player-facing validation'
& (Join-Path $repoRoot 'tools\run-stage19-player-facing-checks.ps1') -SkipCoreBuild -SkipStage18_5Validation
if ($LASTEXITCODE -ne 0) {
    throw "run-stage19-player-facing-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Rts.Core UnityEngine-free scan'
Test-RtsCoreUnityEngineFree -CorePath $corePath

Write-ValidationSection 'Normalize Unity-generated whitespace'
Repair-UnityGeneratedValidationWhitespace -RepoRoot $repoRoot

Write-ValidationSection 'Whitespace check'
Invoke-GitDiffCheck -RepoRoot $repoRoot

Write-Host 'Stage 19 checks passed.'
