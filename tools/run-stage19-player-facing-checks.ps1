[CmdletBinding()]
param(
    [switch]$SkipPlayerBuild,
    [switch]$SkipPlayerLog,
    [switch]$SkipCoreBuild,
    [switch]$SkipStage18_5Validation,
    [switch]$SkipStage19Validation
)

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

function Test-Stage19LogForRedErrors {
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

Write-ValidationSection 'Stage 19 player-facing checks'
Write-Host 'Scope: core tests/build, medium recursion audit, direct Stage 18.5 player-facing/build-flow validation, Stage 19 Unity validation, optional Windows player build/log inspection, UnityEngine-free scan, and git diff whitespace check.'

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

if ($SkipStage18_5Validation) {
    Write-Host 'Skipping Stage 18.5 player-facing validation; caller already ran it.'
} else {
    Write-ValidationSection 'Stage 18.5 player-facing dependency'
    $stage18_5Args = @{
        SkipCoreBuild = $true
    }
    if ($SkipPlayerBuild) {
        $stage18_5Args['SkipPlayerBuild'] = $true
    }
    if ($SkipPlayerLog) {
        $stage18_5Args['SkipPlayerLog'] = $true
    }
    $stage18_5Args['SkipStage18Dependency'] = $true
    $stage18_5PlayerFacingScript = Join-Path $repoRoot 'tools\run-stage18-5-player-facing-checks.ps1'
    & $stage18_5PlayerFacingScript @stage18_5Args
    if ($LASTEXITCODE -ne 0) {
        throw "run-stage18-5-player-facing-checks.ps1 failed with exit code $LASTEXITCODE."
    }
}

if ($SkipStage19Validation) {
    Write-Host 'Skipping Stage 19 Unity validation; caller already ran it.'
} else {
    Write-ValidationSection 'Stage 19 Unity validation'
    & (Join-Path $repoRoot 'tools\run-unity-stage19-validation.ps1') -SkipCoreBuild
    if ($LASTEXITCODE -ne 0) {
        throw "run-unity-stage19-validation.ps1 failed with exit code $LASTEXITCODE."
    }
}

Write-ValidationSection 'Stage 19 Unity log inspection'
$logsToInspect = @(
    (Join-Path $logRoot 'stage19-configure.log'),
    (Join-Path $logRoot 'stage19-validate.log'),
    (Join-Path $logRoot 'stage19-playmode-smoke.log')
)
foreach ($log in $logsToInspect) {
    Test-Stage19LogForRedErrors -Path $log
}

if ($SkipPlayerBuild) {
    Write-Host 'Skipping Windows player build.'
} else {
    Write-ValidationSection 'Windows player build'
    & (Join-Path $repoRoot 'tools\build-windows-player-stage16.ps1')
    if ($LASTEXITCODE -ne 0) {
        throw "build-windows-player-stage16.ps1 failed with exit code $LASTEXITCODE."
    }
}

if ($SkipPlayerLog) {
    Write-Host 'Skipping Player.log inspection.'
} else {
    Write-ValidationSection 'Player.log inspection'
    & (Join-Path $repoRoot 'tools\inspect-latest-player-log.ps1') -CopyToDebugLogs
    if ($LASTEXITCODE -ne 0) {
        throw "inspect-latest-player-log.ps1 failed with exit code $LASTEXITCODE."
    }
}

Write-ValidationSection 'Rts.Core UnityEngine-free scan'
Test-RtsCoreUnityEngineFree -CorePath $corePath

Write-ValidationSection 'Normalize Unity-generated whitespace'
Repair-UnityGeneratedValidationWhitespace -RepoRoot $repoRoot

Write-ValidationSection 'Whitespace check'
Invoke-GitDiffCheck -RepoRoot $repoRoot

Write-Host 'Stage 19 player-facing checks passed.'
