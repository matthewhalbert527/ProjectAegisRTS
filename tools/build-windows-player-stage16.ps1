[CmdletBinding()]
param(
    [int]$WindowWidth = 1600,
    [int]$WindowHeight = 900,
    [switch]$Fullscreen,
    [switch]$Windowed
)

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

function Invoke-UnityPlayerBuildBatch {
    param(
        [string]$UnityEditor,
        [string]$UnityProject,
        [string]$LogPath,
        [string]$OutputPath
    )

    if (Test-Path -LiteralPath $LogPath) {
        Remove-Item -LiteralPath $LogPath -Force
    }

    $arguments = "-batchmode -quit -projectPath `"$UnityProject`" -executeMethod ProjectAegisRTS.UnityClient.EditorTools.Stage16_5BuildFlowConfigurator.BuildWindowsPlayerBatch -stage16WindowsPlayerPath `"$OutputPath`" -logFile `"$LogPath`""
    Write-Host 'Running Stage 16.5 Windows player build.'
    Write-Host "Log: $LogPath"
    $process = Start-Process -FilePath $UnityEditor -ArgumentList $arguments -WindowStyle Hidden -PassThru

    $deadline = (Get-Date).AddMinutes(30)
    $sawCompletion = $false
    do {
        Start-Sleep -Seconds 2
        if (Test-Path -LiteralPath $LogPath) {
            $compilerError = Select-String -LiteralPath $LogPath -Pattern 'Scripts have compiler errors', 'error CS', 'Script Compilation Error', 'Exception:' -Quiet
            if ($compilerError) {
                break
            }

            $sawCompletion = Select-String -LiteralPath $LogPath -Pattern 'Stage 16.5 Windows player build succeeded:' -Quiet
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
        throw "Unity did not create a build log. Expected: $LogPath"
    }

    $compilerErrorAfter = Select-String -LiteralPath $LogPath -Pattern 'Scripts have compiler errors', 'error CS', 'Script Compilation Error', 'Exception:' -Quiet
    if ($compilerErrorAfter) {
        throw "Unity logged errors. See $LogPath"
    }

    if (-not $sawCompletion) {
        throw "Unity did not log the Stage 16.5 Windows player success marker. See $LogPath"
    }

    if ($process.ExitCode -ne 0) {
        throw "Unity process exited with code $($process.ExitCode). See $LogPath"
    }
}

function Invoke-Stage21_5DisplayConfigBatch {
    param(
        [string]$UnityEditor,
        [string]$UnityProject,
        [string]$LogPath,
        [int]$Width,
        [int]$Height,
        [string]$FullscreenMode
    )

    if (Test-Path -LiteralPath $LogPath) {
        Remove-Item -LiteralPath $LogPath -Force
    }

    $arguments = "-batchmode -quit -projectPath `"$UnityProject`" -executeMethod ProjectAegisRTS.UnityClient.EditorTools.Stage21_5DisplaySettingsConfigurator.ConfigureDisplaySettingsBatch -stage21_5WindowWidth $Width -stage21_5WindowHeight $Height -stage21_5FullscreenMode $FullscreenMode -logFile `"$LogPath`""
    $process = Start-Process -FilePath $UnityEditor -ArgumentList $arguments -WindowStyle Hidden -Wait -PassThru
    if ($process.ExitCode -ne 0 -or -not (Select-String -LiteralPath $LogPath -Pattern 'Stage 21.5 display settings configured.' -Quiet)) {
        throw "Stage 21.5 display settings configuration failed. See $LogPath"
    }
}

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$unityEditor = Find-UnityEditor
$unityProject = Join-Path $repoRoot 'unity'
$logRoot = Join-Path $repoRoot 'build\unity-logs'
$outputDir = Join-Path $repoRoot 'build\windows-player-stage16'
$outputExe = Join-Path $outputDir 'ProjectAegisRTS.exe'

if (-not $unityEditor) {
    throw 'Unity Editor was not found.'
}

if ($Fullscreen -and $Windowed) {
    throw 'Use either -Fullscreen or -Windowed, not both.'
}

$WindowWidth = [Math]::Max($WindowWidth, 1280)
$WindowHeight = [Math]::Max($WindowHeight, 720)
$fullscreenMode = 'Windowed'
if ($Fullscreen) {
    $fullscreenMode = 'FullScreenWindow'
}
if ($Windowed) {
    $fullscreenMode = 'Windowed'
}

New-Item -ItemType Directory -Force -Path $logRoot | Out-Null
New-Item -ItemType Directory -Force -Path $outputDir | Out-Null

Write-ValidationSection 'Build Rts.Core for Unity'
& (Join-Path $repoRoot 'tools\build-rts-core-for-unity.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "build-rts-core-for-unity.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Configure Stage 16.5 build flow'
$configureLog = Join-Path $logRoot 'stage16-5-configure-for-player-build.log'
if (Test-Path -LiteralPath $configureLog) {
    Remove-Item -LiteralPath $configureLog -Force
}
$configureArgs = "-batchmode -quit -projectPath `"$unityProject`" -executeMethod ProjectAegisRTS.UnityClient.EditorTools.Stage16_5BuildFlowConfigurator.ConfigureBuildFlowBatch -logFile `"$configureLog`""
$configureProcess = Start-Process -FilePath $unityEditor -ArgumentList $configureArgs -WindowStyle Hidden -Wait -PassThru
if ($configureProcess.ExitCode -ne 0 -or -not (Select-String -LiteralPath $configureLog -Pattern 'Stage 16.5 player build flow configured.' -Quiet)) {
    throw "Stage 16.5 build flow configuration failed. See $configureLog"
}

Write-ValidationSection 'Configure Stage 21.5 display settings'
$displayConfigureLog = Join-Path $logRoot 'stage21-5-configure-for-player-build.log'
Invoke-Stage21_5DisplayConfigBatch -UnityEditor $unityEditor -UnityProject $unityProject -LogPath $displayConfigureLog -Width $WindowWidth -Height $WindowHeight -FullscreenMode $fullscreenMode

Write-ValidationSection 'Stage 16 medium validation'
& (Join-Path $repoRoot 'tools\run-stage16-medium-checks.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-stage16-medium-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Re-configure Stage 16.5 build flow'
$reconfigureLog = Join-Path $logRoot 'stage16-5-reconfigure-for-player-build.log'
if (Test-Path -LiteralPath $reconfigureLog) {
    Remove-Item -LiteralPath $reconfigureLog -Force
}
$reconfigureArgs = "-batchmode -quit -projectPath `"$unityProject`" -executeMethod ProjectAegisRTS.UnityClient.EditorTools.Stage16_5BuildFlowConfigurator.ConfigureBuildFlowBatch -logFile `"$reconfigureLog`""
$reconfigureProcess = Start-Process -FilePath $unityEditor -ArgumentList $reconfigureArgs -WindowStyle Hidden -Wait -PassThru
if ($reconfigureProcess.ExitCode -ne 0 -or -not (Select-String -LiteralPath $reconfigureLog -Pattern 'Stage 16.5 player build flow configured.' -Quiet)) {
    throw "Stage 16.5 build flow reconfiguration failed. See $reconfigureLog"
}

Write-ValidationSection 'Re-configure Stage 21.5 display settings'
$displayReconfigureLog = Join-Path $logRoot 'stage21-5-reconfigure-for-player-build.log'
Invoke-Stage21_5DisplayConfigBatch -UnityEditor $unityEditor -UnityProject $unityProject -LogPath $displayReconfigureLog -Width $WindowWidth -Height $WindowHeight -FullscreenMode $fullscreenMode

Write-ValidationSection 'Windows player build'
Invoke-UnityPlayerBuildBatch -UnityEditor $unityEditor -UnityProject $unityProject -LogPath (Join-Path $logRoot 'stage16-5-windows-player.log') -OutputPath $outputExe

if (-not (Test-Path -LiteralPath $outputExe)) {
    throw "Expected Windows player EXE was not created: $outputExe"
}

Write-Host "Windows player build passed."
Write-Host "EXE: $outputExe"
Write-Host 'Suggested test commands:'
Write-Host "  Normal: & `"$outputExe`""
Write-Host "  1920x1080 windowed: & `"$outputExe`" -screen-width 1920 -screen-height 1080 -screen-fullscreen 0"
Write-Host "  Fullscreen window: & `"$outputExe`" -screen-fullscreen 1"

Write-ValidationSection 'Normalize Unity-generated whitespace'
Repair-UnityGeneratedValidationWhitespace -RepoRoot $repoRoot
