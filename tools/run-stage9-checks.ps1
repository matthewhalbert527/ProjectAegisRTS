[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$dotnet = Find-DotNet
$corePath = Join-Path $repoRoot 'src\Rts.Core'

Write-Host 'Stage 9 full acceptance gate: validates Stage 0 through Stage 9.'
Write-Warning 'This is the slow full acceptance gate and can take a long time. Use run-stage9-fast-checks.ps1 or run-stage9-medium-checks.ps1 for normal iteration.'

Write-ValidationSection 'Rts.Core tests'
Invoke-DotNetRunNoRestore -DotNetPath $dotnet -ProjectPath (Join-Path $repoRoot 'src\Rts.Core.Tests')
if ($LASTEXITCODE -ne 0) {
    throw "Rts.Core.Tests failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Build Rts.Core for Unity'
& (Join-Path $repoRoot 'tools\build-rts-core-for-unity.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "build-rts-core-for-unity.ps1 failed with exit code $LASTEXITCODE."
}

$checks = @(
    'run-stage1-checks.ps1',
    'run-stage2-checks.ps1',
    'run-stage2-playmode-smoke.ps1',
    'run-stage3-checks.ps1',
    'run-stage4-checks.ps1',
    'run-stage5-checks.ps1',
    'run-stage6-checks.ps1',
    'run-stage7-checks.ps1',
    'run-stage8-medium-checks.ps1',
    'run-unity-stage9-validation.ps1'
)

foreach ($check in $checks) {
    Write-ValidationSection $check
    & (Join-Path $repoRoot "tools\$check")
    if ($LASTEXITCODE -ne 0) {
        throw "$check failed with exit code $LASTEXITCODE."
    }
}

Write-ValidationSection 'Rts.Core UnityEngine-free scan'
Test-RtsCoreUnityEngineFree -CorePath $corePath

Write-ValidationSection 'Whitespace check'
Invoke-GitDiffCheck -RepoRoot $repoRoot

Write-Host 'Stage 9 checks passed.'
