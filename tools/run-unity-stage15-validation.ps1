[CmdletBinding()]
param(
    [switch]$SkipCoreBuild
)

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

function Get-UnityProcessesForProject {
    param([string]$ProjectPath)

    $slashPath = $ProjectPath -replace '\\', '/'
    Get-CimInstance Win32_Process -Filter "name = 'Unity.exe'" |
        Where-Object {
            $_.CommandLine -and
            $_.CommandLine -match '-projectPath' -and
            ($_.CommandLine.Contains($ProjectPath) -or $_.CommandLine.Contains($slashPath)) -and
            $_.CommandLine -notmatch 'AssetImportWorker' -and
            $_.CommandLine -notmatch '-batchmode' -and
            $_.CommandLine -notmatch '-batchMode'
        }
}

function Invoke-UnityBatch {
    param(
        [string]$UnityEditor,
        [string]$UnityProject,
        [string]$ExecuteMethod,
        [string]$LogPath,
        [string]$SuccessPattern,
        [int]$TimeoutMinutes = 12
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

function Test-Stage15SceneFileLive {
    param([string]$ScenePath)

    if (-not (Test-Path -LiteralPath $ScenePath)) {
        throw "Stage 15 scene does not exist and Unity is already open, so it cannot be generated in batchmode: $ScenePath"
    }

    $lines = Get-Content -LiteralPath $ScenePath
    $required = @(
        'm_Name: RtsGame',
        'm_Name: BoardRoot',
        'm_Name: Main Camera',
        'Assembly-CSharp::ProjectAegisRTS.UnityClient.Performance.ObjectPoolService',
        'Assembly-CSharp::ProjectAegisRTS.UnityClient.Performance.PerformanceBudgetLibrary',
        'Assembly-CSharp::ProjectAegisRTS.UnityClient.Performance.RuntimePerformanceStats',
        'Assembly-CSharp::ProjectAegisRTS.UnityClient.Performance.SceneComplexityReporter',
        'Assembly-CSharp::ProjectAegisRTS.UnityClient.Performance.QualityProfileApplier',
        'Assembly-CSharp::ProjectAegisRTS.UnityClient.Performance.QuestBuildReadinessReporter',
        'Assembly-CSharp::ProjectAegisRTS.UnityClient.Performance.PcBuildReadinessReporter',
        'Assembly-CSharp::ProjectAegisRTS.UnityClient.Performance.RenderStatsHud'
    )

    foreach ($pattern in $required) {
        if (-not ($lines | Select-String -Pattern ([regex]::Escape($pattern)) -Quiet)) {
            throw "Stage 15 scene live validation failed; missing '$pattern'."
        }
    }
}

function Normalize-Stage15GeneratedFiles {
    param([string]$RepoRoot)

    $paths = @(
        'unity\Assets\Rts\Scenes\Stage15_PerformanceBuildReadiness.unity',
        'unity\Assets\Rts\Scenes\Stage15_PerformanceBuildReadiness.unity.meta',
        'unity\Assets\Rts\Scripts\Performance.meta',
        'unity\Assets\Rts\ScriptableObjects\Performance.meta',
        'unity\Assets\Rts\Editor\Stage15PerformanceProfileAssetCreator.cs.meta',
        'unity\Assets\Rts\Editor\Stage15SceneCreator.cs.meta',
        'unity\Assets\Rts\Editor\Stage15SceneValidator.cs.meta',
        'unity\Assets\Rts\Editor\Stage15PlayModeSmokeValidator.cs.meta',
        'unity\Assets\Rts\Editor\Stage15BuildReadinessReporter.cs.meta',
        'unity\ProjectSettings\EditorBuildSettings.asset',
        'unity\Assets\XR\Settings\OpenXR Package Settings.asset',
        'unity\Assets\XR\Settings\OpenXR Package Settings.asset.meta',
        'unity\Assets\XR\Settings.meta',
        'unity\Assets\XR.meta'
    )

    foreach ($path in $paths) {
        Remove-TrailingWhitespace -Path (Join-Path $RepoRoot $path)
    }

    $roots = @(
        'unity\Assets\Rts\Scripts\Performance',
        'unity\Assets\Rts\ScriptableObjects\Performance'
    )

    foreach ($relativeRoot in $roots) {
        $path = Join-Path $RepoRoot $relativeRoot
        if (Test-Path -LiteralPath $path) {
            Get-ChildItem -LiteralPath $path -Recurse -File -Include *.cs,*.asset,*.meta |
                ForEach-Object { Remove-TrailingWhitespace -Path $_.FullName }
        }
    }

    $edited = @(
        'unity\Assets\Rts\Scripts\Bootstrap\RtsGameBootstrapper.cs',
        'unity\Assets\Rts\Scripts\Rendering\Combat\ProjectileRenderSystem.cs',
        'unity\Assets\Rts\Scripts\Feedback\VfxFeedbackController.cs',
        'unity\Assets\Rts\Scripts\Feedback\FeedbackVisualMarker.cs'
    )
    foreach ($path in $edited) {
        Remove-TrailingWhitespace -Path (Join-Path $RepoRoot $path)
    }
}

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$unityProject = Join-Path $repoRoot 'unity'
$scenePath = Join-Path $unityProject 'Assets\Rts\Scenes\Stage15_PerformanceBuildReadiness.unity'
$logRoot = Join-Path $repoRoot 'build\unity-logs'
$unityEditor = Find-UnityEditor

if (-not $unityEditor) {
    throw 'Unity Editor was not found.'
}

New-Item -ItemType Directory -Force -Path $logRoot | Out-Null
Write-Host "Unity Editor: $unityEditor"
Write-Host "Stage 15 scene: $scenePath"

if ($SkipCoreBuild) {
    Write-Host 'Skipping Rts.Core DLL build for Unity; caller already built it.'
} else {
    Write-Host 'Building Rts.Core DLL for Unity.'
    & (Join-Path $repoRoot 'tools\build-rts-core-for-unity.ps1')
    if ($LASTEXITCODE -ne 0) {
        throw "build-rts-core-for-unity.ps1 failed with exit code $LASTEXITCODE."
    }
}

$openUnity = @(Get-UnityProcessesForProject -ProjectPath $unityProject |
    Where-Object {
        $_.CommandLine -notmatch 'AssetImportWorker' -and
        $_.CommandLine -notmatch '-batchmode'
    })
if ($openUnity.Count -gt 0) {
    Write-Warning 'Unity Editor is already open for this project. Using Stage 15 live scene/log validation fallback; full batch Play Mode automation is skipped because Unity owns the project lock.'
    Test-Stage15SceneFileLive -ScenePath $scenePath
    Normalize-Stage15GeneratedFiles -RepoRoot $repoRoot
    $editorLog = Join-Path $unityProject 'Logs\Editor.log'
    if (Test-Path -LiteralPath $editorLog) {
        $error = Select-String -LiteralPath $editorLog -Pattern 'NullReferenceException', 'MissingMethodException', 'TypeLoadException', 'FileNotFoundException', 'Scripts have compiler errors', 'error CS[0-9]+' -Quiet
        if ($error) {
            throw "Unity Editor log contains red-error signatures. See $editorLog"
        }
    }

    Write-Host 'Stage 15 live validation passed. Play Mode smoke used fallback.'
    Write-Host "Scene path: $scenePath"
    return
}

Invoke-UnityBatch -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage15PerformanceProfileAssetCreator.CreatePerformanceProfilesBatch' -LogPath (Join-Path $logRoot 'stage15-performance-profiles.log') -SuccessPattern 'Stage 15 performance profiles updated' -TimeoutMinutes 12
Invoke-UnityBatch -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage15SceneCreator.CreateStage15SceneBatch' -LogPath (Join-Path $logRoot 'stage15-create.log') -SuccessPattern 'Created Stage 15 scene at Assets/Rts/Scenes/Stage15_PerformanceBuildReadiness.unity' -TimeoutMinutes 15
Invoke-UnityBatch -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage15SceneValidator.ValidateStage15SceneBatch' -LogPath (Join-Path $logRoot 'stage15-validate.log') -SuccessPattern 'Stage 15 scene validation passed.' -TimeoutMinutes 12
Invoke-UnityBatch -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage15PlayModeSmokeValidator.RunStage15PlayModeSmokeBatch' -LogPath (Join-Path $logRoot 'stage15-playmode-smoke.log') -SuccessPattern 'Stage 15 play mode smoke validation passed.' -TimeoutMinutes 15
Invoke-UnityBatch -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage15BuildReadinessReporter.RunStage15BuildReadinessReportBatch' -LogPath (Join-Path $logRoot 'stage15-build-readiness.log') -SuccessPattern 'Stage 15 build readiness report passed.' -TimeoutMinutes 12

Normalize-Stage15GeneratedFiles -RepoRoot $repoRoot

Write-Host "Stage 15 Unity validation passed. Logs: $logRoot"
Write-Host 'Stage 15 Play Mode smoke fully ran in batchmode.'
Write-Host 'Stage 15 build readiness audit ran without producing APK or player artifacts.'
Write-Host "Scene path: $scenePath"
