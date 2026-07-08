[CmdletBinding()]
param(
    [switch]$SkipPlayerBuild,
    [switch]$SkipCoreBuild,
    [switch]$SkipStage17Validation
)

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

function Invoke-UnityBatchForStage17PlayerFacing {
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
    param(
        [string]$Path,
        [switch]$AllowMissing
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        if ($AllowMissing) {
            Write-Host "Log not found; skipping optional inspection: $Path"
            return
        }

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

function Find-PlayerLog {
    $candidates = @(
        (Join-Path $env:USERPROFILE 'AppData\LocalLow\DefaultCompany\ProjectAegisRTS\Player.log'),
        (Join-Path $env:USERPROFILE 'AppData\LocalLow\ProjectAegisRTS\ProjectAegisRTS\Player.log'),
        (Join-Path $env:USERPROFILE 'AppData\LocalLow\ProjectAegisRTS\Project Aegis RTS\Player.log')
    )

    foreach ($candidate in $candidates) {
        if (Test-Path -LiteralPath $candidate) {
            return $candidate
        }
    }

    $root = Join-Path $env:USERPROFILE 'AppData\LocalLow'
    if (Test-Path -LiteralPath $root) {
        $found = Get-ChildItem -LiteralPath $root -Recurse -Filter 'Player.log' -ErrorAction SilentlyContinue |
            Where-Object { $_.FullName -match 'ProjectAegis|Project Aegis|DefaultCompany' } |
            Sort-Object LastWriteTime -Descending |
            Select-Object -First 1
        if ($found) {
            return $found.FullName
        }
    }

    return $null
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

Write-ValidationSection 'Stage 17 player-facing checks'
Write-Host 'Scope: boot/build settings, Stage 17 validation and smoke, player-facing defaults, log inspection, optional Windows player build, UnityEngine-free scan, and git diff whitespace check.'

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

if ($SkipStage17Validation) {
    Write-Host 'Skipping Stage 17 Unity validation; caller already ran it.'
} else {
    Write-ValidationSection 'Stage 17 Unity validation'
    & (Join-Path $repoRoot 'tools\run-unity-stage17-validation.ps1') -SkipCoreBuild
    if ($LASTEXITCODE -ne 0) {
        throw "run-unity-stage17-validation.ps1 failed with exit code $LASTEXITCODE."
    }
}

Write-ValidationSection 'Direct player-facing build-flow smoke'
Invoke-UnityBatchForStage17PlayerFacing -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage16_5PlayModeSmokeValidator.RunBuildFlowSmokeBatch' -LogPath (Join-Path $logRoot 'stage17-player-facing-build-flow-smoke.log') -SuccessPattern 'Stage 16.5 build flow smoke validation passed.' -TimeoutMinutes 18

if ($SkipPlayerBuild) {
    Write-Host 'Skipping Windows player build by request.'
} else {
    Write-ValidationSection 'Windows player build'
    $quotedPlayerPath = '-stage16WindowsPlayerPath "' + $playerExe + '"'
    Invoke-UnityBatchForStage17PlayerFacing -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage16_5BuildFlowConfigurator.BuildWindowsPlayerBatch' -LogPath (Join-Path $logRoot 'stage17-windows-player.log') -SuccessPattern 'Stage 16.5 Windows player build succeeded:' -TimeoutMinutes 30 -ExtraArguments @($quotedPlayerPath)
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
    (Join-Path $logRoot 'stage17-player-facing-build-flow-smoke.log')
)
if (-not $SkipPlayerBuild) {
    $logsToInspect += (Join-Path $logRoot 'stage17-windows-player.log')
}
foreach ($log in $logsToInspect) {
    Test-LogForRedErrors -Path $log
}

if (-not $SkipPlayerBuild) {
    Write-ValidationSection 'Player.log inspection'
    $playerLog = Find-PlayerLog
    if ($playerLog) {
        Test-LogForRedErrors -Path $playerLog
        Write-Host "Player.log inspected: $playerLog"
    } else {
        Write-Warning 'Player.log was not found. The player build succeeded, but the EXE has not produced a fresh Player.log in LocalLow yet.'
    }
}

Write-ValidationSection 'Rts.Core UnityEngine-free scan'
Test-RtsCoreUnityEngineFree -CorePath $corePath

Write-ValidationSection 'Normalize Unity-generated whitespace'
Repair-UnityGeneratedValidationWhitespace -RepoRoot $repoRoot

Write-ValidationSection 'Whitespace check'
Invoke-GitDiffCheck -RepoRoot $repoRoot

Write-Host 'Stage 17 player-facing checks passed.'
