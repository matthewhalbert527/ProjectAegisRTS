[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$dotnet = Find-DotNet
$corePath = Join-Path $repoRoot 'src\Rts.Core'

Write-Host 'Stage 13 full acceptance gate: validates Stage 0 through Stage 13.'
Write-Host 'WARNING: This is the slow full acceptance gate and can take a long time. Use run-stage13-fast-checks.ps1 or run-stage13-medium-checks.ps1 for normal iteration.'

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

Write-ValidationSection 'Stage 12 full dependency validation'
& (Join-Path $repoRoot 'tools\run-stage12-checks.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-stage12-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 13 Unity validation'
& (Join-Path $repoRoot 'tools\run-unity-stage13-validation.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-unity-stage13-validation.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Rts.Core UnityEngine-free scan'
Test-RtsCoreUnityEngineFree -CorePath $corePath

Write-ValidationSection 'Whitespace check'
Invoke-GitDiffCheck -RepoRoot $repoRoot

Write-Host 'Stage 13 checks passed.'
