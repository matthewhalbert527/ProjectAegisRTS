[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$corePath = Join-Path $repoRoot 'src\Rts.Core'

Write-Host 'Stage 19.5 full acceptance gate: validates Stage 0 through Stage 19, player-facing build flow, and the Stage 19.5 PC sidebar/pause menu rework.'
Write-Warning 'This is the slow final acceptance gate. Use run-stage19-5-fast-checks.ps1 for iteration and run-stage19-5-medium-checks.ps1 before commit.'

Write-ValidationSection 'Medium recursion audit'
& (Join-Path $repoRoot 'tools\audit-medium-validation-recursion.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "audit-medium-validation-recursion.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 19 full acceptance dependency'
& (Join-Path $repoRoot 'tools\run-stage19-checks.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-stage19-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 19 player-facing dependency'
& (Join-Path $repoRoot 'tools\run-stage19-player-facing-checks.ps1') -SkipCoreBuild -SkipStage18_5Validation
if ($LASTEXITCODE -ne 0) {
    throw "run-stage19-player-facing-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 19.5 player-facing validation'
& (Join-Path $repoRoot 'tools\run-stage19-5-player-facing-checks.ps1') -SkipCoreBuild -SkipStage19Validation
if ($LASTEXITCODE -ne 0) {
    throw "run-stage19-5-player-facing-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Rts.Core UnityEngine-free scan'
Test-RtsCoreUnityEngineFree -CorePath $corePath

Write-ValidationSection 'Normalize Unity-generated whitespace'
Repair-UnityGeneratedValidationWhitespace -RepoRoot $repoRoot

Write-ValidationSection 'Whitespace check'
Invoke-GitDiffCheck -RepoRoot $repoRoot

Write-Host 'Stage 19.5 checks passed.'
