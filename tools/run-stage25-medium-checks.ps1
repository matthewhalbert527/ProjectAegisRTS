[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$dotnet = Find-DotNet
$corePath = Join-Path $repoRoot 'src\Rts.Core'

Write-ValidationSection 'Stage 25 medium checks'
Write-Host 'Purpose: pre-commit confidence for engineer capture/repair and transports.'
Write-Host 'Scope: Rts.Core tests/build, direct Stage24 Unity/player-facing validation, direct Stage4/Stage5 hand-control validation, Stage25 Unity validation and smoke, Stage25 player-facing checks, medium recursion audit, UnityEngine-free scan, and git diff whitespace check.'
Write-Host 'This medium tier does not call prior medium checks. Use run-stage25-checks.ps1 for the slow full final acceptance gate.'

Write-ValidationSection 'Medium recursion audit'
& (Join-Path $repoRoot 'tools\audit-medium-validation-recursion.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "audit-medium-validation-recursion.ps1 failed with exit code $LASTEXITCODE."
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

Write-ValidationSection 'Direct Stage 24 Unity validation'
& (Join-Path $repoRoot 'tools\run-unity-stage24-validation.ps1') -SkipCoreBuild
if ($LASTEXITCODE -ne 0) {
    throw "run-unity-stage24-validation.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Direct Stage 24 player-facing validation'
& (Join-Path $repoRoot 'tools\run-stage24-player-facing-checks.ps1') -SkipPlayerBuild -SkipPlayerLog -SkipCoreBuild -SkipStage23Validation -SkipStage24Validation
if ($LASTEXITCODE -ne 0) {
    throw "run-stage24-player-facing-checks.ps1 failed with exit code $LASTEXITCODE."
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

Write-ValidationSection 'Stage 25 Unity validation'
& (Join-Path $repoRoot 'tools\run-unity-stage25-validation.ps1') -SkipCoreBuild
if ($LASTEXITCODE -ne 0) {
    throw "run-unity-stage25-validation.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 25 player-facing validation'
& (Join-Path $repoRoot 'tools\run-stage25-player-facing-checks.ps1') -SkipPlayerBuild -SkipPlayerLog -SkipCoreBuild -SkipStage24Validation -SkipStage25Validation
if ($LASTEXITCODE -ne 0) {
    throw "run-stage25-player-facing-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Rts.Core UnityEngine-free scan'
Test-RtsCoreUnityEngineFree -CorePath $corePath

Write-ValidationSection 'Normalize Unity-generated whitespace'
Repair-UnityGeneratedValidationWhitespace -RepoRoot $repoRoot

Write-ValidationSection 'Whitespace check'
Invoke-GitDiffCheck -RepoRoot $repoRoot

Write-Host 'Stage 25 medium checks passed.'
