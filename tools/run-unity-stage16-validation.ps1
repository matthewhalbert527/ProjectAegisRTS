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
        [int]$TimeoutMinutes = 15
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

function Test-Stage16SceneFileLive {
    param([string]$ScenePath)

    if (-not (Test-Path -LiteralPath $ScenePath)) {
        throw "Stage 16 scene does not exist and Unity is already open, so it cannot be generated in batchmode: $ScenePath"
    }

    $lines = Get-Content -LiteralPath $ScenePath
    $required = @(
        'm_Name: RtsGame',
        'm_Name: BoardRoot',
        'm_Name: Main Camera',
        'Assembly-CSharp::ProjectAegisRTS.UnityClient.Scenario.VerticalSliceScenarioController',
        'Assembly-CSharp::ProjectAegisRTS.UnityClient.Scenario.VerticalSliceDebugActions',
        'Assembly-CSharp::ProjectAegisRTS.UnityClient.UI.Common.MatchObjectiveHud',
        'Assembly-CSharp::ProjectAegisRTS.UnityClient.UI.Common.IntegratedSystemsStatusHud'
    )

    foreach ($pattern in $required) {
        if (-not ($lines | Select-String -Pattern ([regex]::Escape($pattern)) -Quiet)) {
            throw "Stage 16 scene live validation failed; missing '$pattern'."
        }
    }
}

function Normalize-Stage16GeneratedFiles {
    param([string]$RepoRoot)

    $paths = @(
        'unity\Assets\Rts\Scenes\Stage16_PlayableVerticalSlice.unity',
        'unity\Assets\Rts\Scenes\Stage16_PlayableVerticalSlice.unity.meta',
        'unity\Assets\Rts\Scripts\Scenario.meta',
        'unity\Assets\Rts\Scripts\Scenario\VerticalSliceScenarioController.cs',
        'unity\Assets\Rts\Scripts\Scenario\VerticalSliceScenarioController.cs.meta',
        'unity\Assets\Rts\Scripts\Scenario\VerticalSliceDebugActions.cs',
        'unity\Assets\Rts\Scripts\Scenario\VerticalSliceDebugActions.cs.meta',
        'unity\Assets\Rts\Scripts\UI\Common\MatchObjectiveHud.cs',
        'unity\Assets\Rts\Scripts\UI\Common\MatchObjectiveHud.cs.meta',
        'unity\Assets\Rts\Scripts\UI\Common\IntegratedSystemsStatusHud.cs',
        'unity\Assets\Rts\Scripts\UI\Common\IntegratedSystemsStatusHud.cs.meta',
        'unity\Assets\Rts\Editor\Stage16SceneCreator.cs',
        'unity\Assets\Rts\Editor\Stage16SceneCreator.cs.meta',
        'unity\Assets\Rts\Editor\Stage16SceneValidator.cs',
        'unity\Assets\Rts\Editor\Stage16SceneValidator.cs.meta',
        'unity\Assets\Rts\Editor\Stage16PlayModeSmokeValidator.cs',
        'unity\Assets\Rts\Editor\Stage16PlayModeSmokeValidator.cs.meta',
        'unity\Assets\XR\Settings\OpenXR Package Settings.asset',
        'unity\Assets\XR\Settings\OpenXR Package Settings.asset.meta',
        'unity\Assets\XR\Settings.meta',
        'unity\Assets\XR.meta',
        'unity\ProjectSettings\EditorBuildSettings.asset'
    )

    foreach ($path in $paths) {
        Remove-TrailingWhitespace -Path (Join-Path $RepoRoot $path)
    }
}

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$unityProject = Join-Path $repoRoot 'unity'
$scenePath = Join-Path $unityProject 'Assets\Rts\Scenes\Stage16_PlayableVerticalSlice.unity'
$logRoot = Join-Path $repoRoot 'build\unity-logs'
$unityEditor = Find-UnityEditor

if (-not $unityEditor) {
    throw 'Unity Editor was not found.'
}

New-Item -ItemType Directory -Force -Path $logRoot | Out-Null
Write-Host "Unity Editor: $unityEditor"
Write-Host "Stage 16 scene: $scenePath"

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
    Write-Warning 'Unity Editor is already open for this project. Using Stage 16 live scene/log validation fallback; full batch Play Mode automation is skipped because Unity owns the project lock.'
    Test-Stage16SceneFileLive -ScenePath $scenePath
    Normalize-Stage16GeneratedFiles -RepoRoot $repoRoot
    $editorLog = Join-Path $unityProject 'Logs\Editor.log'
    if (Test-Path -LiteralPath $editorLog) {
        $error = Select-String -LiteralPath $editorLog -Pattern 'NullReferenceException', 'MissingMethodException', 'TypeLoadException', 'FileNotFoundException', 'Scripts have compiler errors', 'error CS[0-9]+' -Quiet
        if ($error) {
            throw "Unity Editor log contains red-error signatures. See $editorLog"
        }
    }

    Write-Host 'Stage 16 live validation passed. Play Mode smoke used fallback.'
    Write-Host "Scene path: $scenePath"
    return
}

Invoke-UnityBatch -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage16SceneCreator.CreateStage16SceneBatch' -LogPath (Join-Path $logRoot 'stage16-create.log') -SuccessPattern 'Created Stage 16 scene at Assets/Rts/Scenes/Stage16_PlayableVerticalSlice.unity' -TimeoutMinutes 18
Invoke-UnityBatch -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage16SceneValidator.ValidateStage16SceneBatch' -LogPath (Join-Path $logRoot 'stage16-validate.log') -SuccessPattern 'Stage 16 scene validation passed.' -TimeoutMinutes 12
Invoke-UnityBatch -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage16PlayModeSmokeValidator.RunStage16PlayModeSmokeBatch' -LogPath (Join-Path $logRoot 'stage16-playmode-smoke.log') -SuccessPattern 'Stage 16 play mode smoke validation passed.' -TimeoutMinutes 18

Normalize-Stage16GeneratedFiles -RepoRoot $repoRoot

Write-Host "Stage 16 Unity validation passed. Logs: $logRoot"
Write-Host 'Stage 16 Play Mode smoke fully ran in batchmode.'
Write-Host "Scene path: $scenePath"
