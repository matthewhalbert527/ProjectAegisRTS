[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$dotnet = Find-DotNet
$corePath = Join-Path $repoRoot 'src\Rts.Core'

Write-ValidationSection 'Stage 32.8 medium checks'
Write-Host 'Scope: Rts.Core tests/build, validation recursion audits, direct Stage4/Stage5 hand-control validation, Stage32.8 terrain quality validation, UnityEngine-free scan, and whitespace.'
Write-Host 'This medium tier does not call prior run-stage*-medium-checks.ps1 scripts.'

Write-ValidationSection 'Medium recursion audit'
& (Join-Path $repoRoot 'tools\audit-medium-validation-recursion.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "audit-medium-validation-recursion.ps1 failed with exit code $LASTEXITCODE."
}

if (Test-Path -LiteralPath (Join-Path $repoRoot 'tools\audit-full-validation-recursion.ps1')) {
    Write-ValidationSection 'Full recursion audit'
    & (Join-Path $repoRoot 'tools\audit-full-validation-recursion.ps1')
    if ($LASTEXITCODE -ne 0) {
        throw "audit-full-validation-recursion.ps1 failed with exit code $LASTEXITCODE."
    }
}

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

Write-ValidationSection 'Direct Stage 4 hand-control validation'
& (Join-Path $repoRoot 'tools\run-unity-stage4-validation.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-unity-stage4-validation.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Direct Stage 5 hand-control validation'
& (Join-Path $repoRoot 'tools\run-unity-stage5-validation.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-unity-stage5-validation.ps1 failed with exit code $LASTEXITCODE."
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

Write-Host 'Stage 32.8 medium checks passed.'
