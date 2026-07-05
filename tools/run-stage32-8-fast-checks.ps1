[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$corePath = Join-Path $repoRoot 'src\Rts.Core'

Write-ValidationSection 'Stage 32.8 fast checks'
Write-Host 'Scope: build/copy Rts.Core DLL, Stage32.8 Unity validation, UnityEngine-free scan, and whitespace.'

Write-ValidationSection 'Build Rts.Core for Unity'
& (Join-Path $repoRoot 'tools\build-rts-core-for-unity.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "build-rts-core-for-unity.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 32.8 Unity validation'
& (Join-Path $repoRoot 'tools\run-unity-stage32-8-validation.ps1') -SkipCoreBuild
if ($LASTEXITCODE -ne 0) {
    throw "run-unity-stage32-8-validation.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Rts.Core UnityEngine-free scan'
Test-RtsCoreUnityEngineFree -CorePath $corePath

Write-ValidationSection 'Whitespace check'
Invoke-GitDiffCheck -RepoRoot $repoRoot

Write-Host 'Stage 32.8 fast checks passed.'
