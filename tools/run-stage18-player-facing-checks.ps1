[CmdletBinding()]
param(
    [switch]$SkipPlayerBuild,
    [switch]$SkipPlayerLog,
    [switch]$SkipCoreBuild,
    [switch]$SkipStage18Validation
)

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

function Invoke-UnityBatchForStage18PlayerFacing {
    param(
        [string]$UnityEditor,
        [string]$UnityProject,
        [string]$ExecuteMethod,
        [string]$LogPath,
        [string]$SuccessPattern,
        [int]$TimeoutMinutes = 15,
        [string[]]$ExtraArguments = @()
    )

    if (Test-Path -LiteralPath $LogPath) {
        Remove-Item -LiteralPath $LogPath -Force
    }

    $arguments = "-batchmode -quit -projectPath `"$UnityProject`" -executeMethod $ExecuteMethod -logFile `"$LogPath`""
    foreach ($extra in $ExtraArguments) {
        $arguments += " $extra"
    }

    Write-Host "Running Unity batch method: $ExecuteMethod"
    Write-Host "Log: $LogPath"
    $process = Start-Process -FilePath $UnityEditor -ArgumentList $arguments -WindowStyle Hidden -PassThru

    $deadline = (Get-Date).AddMinutes($TimeoutMinutes)
    $sawCompletion = $false
    do {
        Start-Sleep -Seconds 2
        if (Test-Path -LiteralPath $LogPath) {
            $compilerError = Select-String -LiteralPath $LogPath -Pattern 'Scripts have compiler errors', 'error CS', 'Script Compilation Error', 'Exception:' -Quiet
            if ($compilerError) {
                break
            }

            $sawCompletion = Select-String -LiteralPath $LogPath -Pattern $SuccessPattern -Quiet
            if ($sawCompletion) {
                break
            }
        }
        if ($process.HasExited) {
            break
        }
    } while ((Get-Date) -lt $deadline)

    if (-not $process.HasExited) {
        if (-not $process.WaitForExit(120000)) {
            throw "Unity logged no final exit within the grace period. See $LogPath"
        }
    }

    if (-not (Test-Path -LiteralPath $LogPath)) {
        throw "Unity did not create a log within the timeout. Expected: $LogPath"
    }

    $compilerErrorAfter = Select-String -LiteralPath $LogPath -Pattern 'Scripts have compiler errors', 'error CS', 'Script Compilation Error', 'Exception:' -Quiet
    if ($compilerErrorAfter) {
        throw "Unity logged errors. See $LogPath"
    }

    if (-not $sawCompletion) {
        throw "Unity did not log expected success marker '$SuccessPattern'. See $LogPath"
    }

    if ($process.ExitCode -ne 0) {
        throw "Unity process exited with code $($process.ExitCode). See $LogPath"
    }
}

function Test-LogForRedErrors {
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
$unityEditor = Find-UnityEditor
$unityProject = Join-Path $repoRoot 'unity'
$logRoot = Join-Path $repoRoot 'build\unity-logs'
$corePath = Join-Path $repoRoot 'src\Rts.Core'
$playerExe = Join-Path $repoRoot 'build\windows-player-stage16\ProjectAegisRTS.exe'

if (-not $unityEditor) {
    throw 'Unity Editor was not found.'
}

New-Item -ItemType Directory -Force -Path $logRoot | Out-Null

Write-ValidationSection 'Stage 18 player-facing checks'
Write-Host 'Scope: Stage 17 player-facing dependency, Stage 18 validation and smoke, player-facing defaults, log inspection, optional Windows player build, UnityEngine-free scan, and git diff whitespace check.'

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

Write-ValidationSection 'Direct Stage 17 player-facing dependency'
& (Join-Path $repoRoot 'tools\run-stage17-player-facing-checks.ps1') -SkipPlayerBuild -SkipCoreBuild
if ($LASTEXITCODE -ne 0) {
    throw "run-stage17-player-facing-checks.ps1 failed with exit code $LASTEXITCODE."
}

if ($SkipStage18Validation) {
    Write-Host 'Skipping Stage 18 Unity validation; caller already ran it.'
} else {
    Write-ValidationSection 'Stage 18 Unity validation'
    & (Join-Path $repoRoot 'tools\run-unity-stage18-validation.ps1') -SkipCoreBuild
    if ($LASTEXITCODE -ne 0) {
        throw "run-unity-stage18-validation.ps1 failed with exit code $LASTEXITCODE."
    }
}

if ($SkipPlayerBuild) {
    Write-Host 'Skipping Windows player build by request.'
} else {
    Write-ValidationSection 'Windows player build'
    $quotedPlayerPath = '-stage16WindowsPlayerPath "' + $playerExe + '"'
    Invoke-UnityBatchForStage18PlayerFacing -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage16_5BuildFlowConfigurator.BuildWindowsPlayerBatch' -LogPath (Join-Path $logRoot 'stage18-windows-player.log') -SuccessPattern 'Stage 16.5 Windows player build succeeded:' -TimeoutMinutes 30 -ExtraArguments @($quotedPlayerPath)
    if (-not (Test-Path -LiteralPath $playerExe)) {
        throw "Expected Windows player EXE was not created: $playerExe"
    }
    Write-Host "Windows player EXE: $playerExe"
}

Write-ValidationSection 'Unity log inspection'
$logsToInspect = @(
    (Join-Path $logRoot 'stage17-configure.log'),
    (Join-Path $logRoot 'stage17-validate.log'),
    (Join-Path $logRoot 'stage17-playmode-smoke.log'),
    (Join-Path $logRoot 'stage17-player-facing-build-flow-smoke.log'),
    (Join-Path $logRoot 'stage18-configure.log'),
    (Join-Path $logRoot 'stage18-validate.log'),
    (Join-Path $logRoot 'stage18-playmode-smoke.log')
)
if (-not $SkipPlayerBuild) {
    $logsToInspect += (Join-Path $logRoot 'stage18-windows-player.log')
}
foreach ($log in $logsToInspect) {
    Test-LogForRedErrors -Path $log
}

if (-not $SkipPlayerLog) {
    Write-ValidationSection 'Player.log inspection'
    & (Join-Path $repoRoot 'tools\inspect-latest-player-log.ps1') -CopyToDebugLogs
    if ($LASTEXITCODE -ne 0) {
        throw "inspect-latest-player-log.ps1 failed with exit code $LASTEXITCODE."
    }
} else {
    Write-Host 'Skipping Player.log inspection by request.'
}

Write-ValidationSection 'Rts.Core UnityEngine-free scan'
Test-RtsCoreUnityEngineFree -CorePath $corePath

Write-ValidationSection 'Normalize Unity-generated whitespace'
Repair-UnityGeneratedValidationWhitespace -RepoRoot $repoRoot

Write-ValidationSection 'Whitespace check'
Invoke-GitDiffCheck -RepoRoot $repoRoot

Write-Host 'Stage 18 player-facing checks passed.'
