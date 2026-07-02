[CmdletBinding()]
param(
    [switch]$SkipCoreBuild
)

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

function Invoke-UnityBatchStage21_5 {
    param(
        [string]$UnityEditor,
        [string]$UnityProject,
        [string]$ExecuteMethod,
        [string]$LogPath,
        [string]$SuccessPattern,
        [int]$TimeoutMinutes = 20
    )

    if (Test-Path -LiteralPath $LogPath) {
        Remove-Item -LiteralPath $LogPath -Force
    }

    $arguments = "-batchmode -quit -projectPath `"$UnityProject`" -executeMethod $ExecuteMethod -logFile `"$LogPath`""
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

function Repair-Stage21_5GeneratedWhitespace {
    param([string]$RepoRoot)

    $paths = @(
        'unity\Assets\Rts\Scenes\Stage16_5_Boot.unity',
        'unity\Assets\Rts\Scenes\Stage16_PlayableVerticalSlice.unity',
        'unity\ProjectSettings\ProjectSettings.asset',
        'unity\ProjectSettings\EditorBuildSettings.asset',
        'docs\STAGE21_5_DISPLAY_SCALING_REPORT.md',
        'docs\STAGE21_5_PLAYER_WINDOW_GUIDE.md'
    )

    foreach ($path in $paths) {
        Remove-TrailingWhitespace -Path (Join-Path $RepoRoot $path)
    }
}

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$unityProject = Join-Path $repoRoot 'unity'
$logRoot = Join-Path $repoRoot 'build\unity-logs'
$unityEditor = Find-UnityEditor

if (-not $unityEditor) {
    throw 'Unity Editor was not found.'
}

New-Item -ItemType Directory -Force -Path $logRoot | Out-Null
Write-Host "Unity Editor: $unityEditor"
Write-Host 'Stage 21.5 display scaling validation.'

if ($SkipCoreBuild) {
    Write-Host 'Skipping Rts.Core DLL build for Unity; caller already built it.'
} else {
    Write-Host 'Building Rts.Core DLL for Unity.'
    & (Join-Path $repoRoot 'tools\build-rts-core-for-unity.ps1')
    if ($LASTEXITCODE -ne 0) {
        throw "build-rts-core-for-unity.ps1 failed with exit code $LASTEXITCODE."
    }
}

Invoke-UnityBatchStage21_5 -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage21_5DisplaySettingsConfigurator.ConfigureDisplaySettingsBatch' -LogPath (Join-Path $logRoot 'stage21-5-display-configure.log') -SuccessPattern 'Stage 21.5 display settings configured.' -TimeoutMinutes 24
Invoke-UnityBatchStage21_5 -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage21_5DisplaySettingsValidator.ValidateDisplaySettingsBatch' -LogPath (Join-Path $logRoot 'stage21-5-display-validate.log') -SuccessPattern 'Stage 21.5 display settings validation passed.' -TimeoutMinutes 24
Invoke-UnityBatchStage21_5 -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage21_5PlayModeSmokeValidator.RunStage21_5PlayModeSmokeBatch' -LogPath (Join-Path $logRoot 'stage21-5-playmode-smoke.log') -SuccessPattern 'Stage 21.5 play mode smoke validation passed.' -TimeoutMinutes 30

Repair-Stage21_5GeneratedWhitespace -RepoRoot $repoRoot
Repair-UnityGeneratedValidationWhitespace -RepoRoot $repoRoot

Write-Host "Stage 21.5 Unity validation passed. Logs: $logRoot"
Write-Host 'Stage 21.5 Play Mode smoke verified Boot options display controls, Stage16 board/sidebar visibility, hidden debug panels, hidden placement UI, PCDesktop mode, and responsive CanvasScaler enforcement.'
