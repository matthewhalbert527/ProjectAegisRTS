[CmdletBinding()]
param(
    [switch]$SkipPlayerBuild,
    [switch]$SkipPlayerLog,
    [switch]$SkipCoreBuild,
    [switch]$SkipStage18Validation,
    [switch]$SkipStage18_5Validation
)

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

function Test-Stage18_5LogForRedErrors {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        throw "Expected log was not found: $Path"
    }

    $patterns = @(
        'error CS',
        'NullReferenceException',
        'MissingReferenceException',
        'MissingComponentException',
        'MissingMethodException',
        'TypeLoadException',
        'FileNotFoundException',
        'ArgumentException',
        'InvalidOperationException',
        'Scripts have compiler errors',
        'Script Compilation Error'
    )

    $hit = Select-String -LiteralPath $Path -Pattern $patterns -CaseSensitive:$false | Select-Object -First 1
    if ($hit) {
        throw "Red-error signature found in $Path at line $($hit.LineNumber): $($hit.Line.Trim())"
    }

    Write-Host "Log inspection passed: $Path"
}

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$dotnet = Find-DotNet
$logRoot = Join-Path $repoRoot 'build\unity-logs'
$corePath = Join-Path $repoRoot 'src\Rts.Core'

New-Item -ItemType Directory -Force -Path $logRoot | Out-Null

Write-ValidationSection 'Stage 18.5 player-facing checks'
Write-Host 'Scope: Stage 18 player-facing dependency, Stage 18.5 validation and smoke, optional Windows player build/log inspection through Stage 18, UnityEngine-free scan, and git diff whitespace check.'

if ($SkipCoreBuild) {
    Write-Host 'Skipping Rts.Core tests/build; caller already ran them.'
} else {
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
}

Write-ValidationSection 'Medium recursion audit'
& (Join-Path $repoRoot 'tools\audit-medium-validation-recursion.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "audit-medium-validation-recursion.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Stage 18 player-facing dependency'
$stage18Args = @{
    SkipCoreBuild = $true
}
if ($SkipPlayerBuild) {
    $stage18Args['SkipPlayerBuild'] = $true
}
if ($SkipPlayerLog) {
    $stage18Args['SkipPlayerLog'] = $true
}
if ($SkipStage18Validation) {
    $stage18Args['SkipStage18Validation'] = $true
}
$stage18PlayerFacingScript = Join-Path $repoRoot 'tools\run-stage18-player-facing-checks.ps1'
& $stage18PlayerFacingScript @stage18Args
if ($LASTEXITCODE -ne 0) {
    throw "run-stage18-player-facing-checks.ps1 failed with exit code $LASTEXITCODE."
}

if ($SkipStage18_5Validation) {
    Write-Host 'Skipping Stage 18.5 Unity validation; caller already ran it.'
} else {
    Write-ValidationSection 'Stage 18.5 Unity validation'
    & (Join-Path $repoRoot 'tools\run-unity-stage18-5-validation.ps1') -SkipCoreBuild
    if ($LASTEXITCODE -ne 0) {
        throw "run-unity-stage18-5-validation.ps1 failed with exit code $LASTEXITCODE."
    }
}

Write-ValidationSection 'Stage 18.5 Unity log inspection'
$logsToInspect = @(
    (Join-Path $logRoot 'stage18-5-configure.log'),
    (Join-Path $logRoot 'stage18-5-validate.log'),
    (Join-Path $logRoot 'stage18-5-playmode-smoke.log')
)
foreach ($log in $logsToInspect) {
    Test-Stage18_5LogForRedErrors -Path $log
}

Write-ValidationSection 'Rts.Core UnityEngine-free scan'
Test-RtsCoreUnityEngineFree -CorePath $corePath

Write-ValidationSection 'Normalize Unity-generated whitespace'
Repair-UnityGeneratedValidationWhitespace -RepoRoot $repoRoot

Write-ValidationSection 'Whitespace check'
Invoke-GitDiffCheck -RepoRoot $repoRoot

Write-Host 'Stage 18.5 player-facing checks passed.'
