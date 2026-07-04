[CmdletBinding()]
param(
    [switch]$SkipCoreBuild
)

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

function Invoke-UnityBatchStage32 {
    param(
        [string]$UnityEditor,
        [string]$UnityProject,
        [string]$ExecuteMethod,
        [string]$LogPath,
        [string]$SuccessPattern,
        [int]$TimeoutMinutes = 75
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

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$unityProject = Join-Path $repoRoot 'unity'
$logRoot = Join-Path $repoRoot 'build\unity-logs'
$unityEditor = Find-UnityEditor

if (-not $unityEditor) {
    throw 'Unity Editor was not found.'
}

New-Item -ItemType Directory -Force -Path $logRoot | Out-Null
Write-Host "Unity Editor: $unityEditor"
Write-Host 'Stage 32 terrain piece / set dressing Unity validation.'

if ($SkipCoreBuild) {
    Write-Host 'Skipping Rts.Core DLL build for Unity; caller already built it.'
} else {
    Write-Host 'Building Rts.Core DLL for Unity.'
    & (Join-Path $repoRoot 'tools\build-rts-core-for-unity.ps1')
    if ($LASTEXITCODE -ne 0) {
        throw "build-rts-core-for-unity.ps1 failed with exit code $LASTEXITCODE."
    }
}

Invoke-UnityBatchStage32 -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage32TerrainPieceGenerator.GenerateStage32TerrainPiecesBatch' -LogPath (Join-Path $logRoot 'stage32-generate-terrain-pieces.log') -SuccessPattern 'Stage 32 terrain piece generation completed.' -TimeoutMinutes 90
Invoke-UnityBatchStage32 -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage32TerrainArtIngestionGenerator.IngestBatch01TerrainArtBatch' -LogPath (Join-Path $logRoot 'stage32-terrain-art-ingestion.log') -SuccessPattern 'Stage 32 terrain art ingestion completed.' -TimeoutMinutes 75
Invoke-UnityBatchStage32 -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage32SceneCreator.CreateStage32SceneBatch' -LogPath (Join-Path $logRoot 'stage32-create-review-scene.log') -SuccessPattern 'Stage 32 terrain set dressing review scene created.' -TimeoutMinutes 90
Invoke-UnityBatchStage32 -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage32TerrainPieceValidator.ValidateStage32TerrainPiecesBatch' -LogPath (Join-Path $logRoot 'stage32-terrain-piece-validation.log') -SuccessPattern 'Stage 32 terrain piece validation passed.' -TimeoutMinutes 90
Invoke-UnityBatchStage32 -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage32SceneValidator.ValidateStage32SceneBatch' -LogPath (Join-Path $logRoot 'stage32-scene-validation.log') -SuccessPattern 'Stage 32 scene validation passed.' -TimeoutMinutes 90
Invoke-UnityBatchStage32 -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage32PlayModeSmokeValidator.RunStage32PlayModeSmokeBatch' -LogPath (Join-Path $logRoot 'stage32-playmode-smoke.log') -SuccessPattern 'Stage 32 play-mode smoke validation passed.' -TimeoutMinutes 90
Invoke-UnityBatchStage32 -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage32ScreenshotCaptureTool.CaptureStage32ScreenshotsBatch' -LogPath (Join-Path $logRoot 'stage32-screenshot-capture.log') -SuccessPattern 'Stage 32 screenshot capture completed.' -TimeoutMinutes 75

Repair-UnityGeneratedValidationWhitespace -RepoRoot $repoRoot

Write-Host "Stage 32 Unity validation passed. Logs: $logRoot"
Write-Host 'Stage 32 verified terrain piece generation, catalogs, review scene, player-facing integration, smoke, and screenshots.'
