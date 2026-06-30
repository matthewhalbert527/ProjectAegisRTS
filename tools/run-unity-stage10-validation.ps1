[CmdletBinding()]
param()

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

function Test-Stage10SceneFileLive {
    param([string]$ScenePath)

    if (-not (Test-Path -LiteralPath $ScenePath)) {
        throw "Stage 10 scene does not exist and Unity is already open, so it cannot be generated in batchmode: $ScenePath"
    }

    $lines = Get-Content -LiteralPath $ScenePath
    $required = @(
        'm_Name: RtsGame',
        'm_Name: BoardRoot',
        'm_Name: Main Camera',
        'm_Name: Directional Light',
        'm_Name: EventSystem',
        'm_Name: Canvas',
        'Assembly-CSharp::ProjectAegisRTS.UnityClient.Rendering.Economy.ResourceFieldRenderSystem',
        'Assembly-CSharp::ProjectAegisRTS.UnityClient.Rendering.Economy.HarvesterCargoVisualController',
        'Assembly-CSharp::ProjectAegisRTS.UnityClient.Rendering.Economy.RefineryDockVisualController',
        'Assembly-CSharp::ProjectAegisRTS.UnityClient.Rendering.Economy.EconomyEventRenderSystem',
        'Assembly-CSharp::ProjectAegisRTS.UnityClient.UI.Common.EconomyDebugHud'
    )

    foreach ($pattern in $required) {
        if (-not ($lines | Select-String -Pattern ([regex]::Escape($pattern)) -Quiet)) {
            throw "Stage 10 scene live validation failed; missing '$pattern'."
        }
    }
}

function Normalize-Stage10GeneratedFiles {
    param([string]$RepoRoot)

    $paths = @(
        'unity\Assets\Rts\Scenes\Stage10_EconomyHarvesting.unity',
        'unity\Assets\Rts\Scenes\Stage10_EconomyHarvesting.unity.meta',
        'unity\Assets\Rts\Scripts\Rendering\Economy.meta',
        'unity\Assets\Rts\Scripts\UI\Common\EconomyDebugHud.cs.meta',
        'unity\Assets\Rts\Editor\Stage10SceneCreator.cs.meta',
        'unity\Assets\Rts\Editor\Stage10SceneValidator.cs.meta',
        'unity\Assets\Rts\Editor\Stage10PlayModeSmokeValidator.cs.meta',
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
        'unity\Assets\Rts\Scripts\Rendering\Economy'
    )

    foreach ($relativeRoot in $roots) {
        $path = Join-Path $RepoRoot $relativeRoot
        if (Test-Path -LiteralPath $path) {
            Get-ChildItem -LiteralPath $path -Recurse -File -Include *.asset,*.meta |
                ForEach-Object { Remove-TrailingWhitespace -Path $_.FullName }
        }
    }
}

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$unityProject = Join-Path $repoRoot 'unity'
$scenePath = Join-Path $unityProject 'Assets\Rts\Scenes\Stage10_EconomyHarvesting.unity'
$logRoot = Join-Path $repoRoot 'build\unity-logs'
$unityEditor = Find-UnityEditor

if (-not $unityEditor) {
    throw 'Unity Editor was not found.'
}

New-Item -ItemType Directory -Force -Path $logRoot | Out-Null
Write-Host "Unity Editor: $unityEditor"
Write-Host "Stage 10 scene: $scenePath"

Write-Host 'Building Rts.Core DLL for Unity.'
& (Join-Path $repoRoot 'tools\build-rts-core-for-unity.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "build-rts-core-for-unity.ps1 failed with exit code $LASTEXITCODE."
}

$openUnity = @(Get-UnityProcessesForProject -ProjectPath $unityProject)
if ($openUnity.Count -gt 0) {
    Write-Warning 'Unity Editor is already open for this project. Using Stage 10 live scene/log validation fallback; full batch Play Mode automation is skipped because Unity owns the project lock.'
    Test-Stage10SceneFileLive -ScenePath $scenePath
    Normalize-Stage10GeneratedFiles -RepoRoot $repoRoot
    $editorLog = Join-Path $unityProject 'Logs\Editor.log'
    if (Test-Path -LiteralPath $editorLog) {
        $error = Select-String -LiteralPath $editorLog -Pattern 'NullReferenceException', 'MissingMethodException', 'TypeLoadException', 'FileNotFoundException', 'Scripts have compiler errors', 'error CS[0-9]+' -Quiet
        if ($error) {
            throw "Unity Editor log contains red-error signatures. See $editorLog"
        }
    }

    Write-Host 'Stage 10 live validation passed. Play Mode smoke used fallback.'
    Write-Host "Scene path: $scenePath"
    return
}

Invoke-UnityBatch -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage10SceneCreator.CreateStage10SceneBatch' -LogPath (Join-Path $logRoot 'stage10-create.log') -SuccessPattern 'Created Stage 10 scene at Assets/Rts/Scenes/Stage10_EconomyHarvesting.unity' -TimeoutMinutes 15
Invoke-UnityBatch -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage10SceneValidator.ValidateStage10SceneBatch' -LogPath (Join-Path $logRoot 'stage10-validate.log') -SuccessPattern 'Stage 10 scene validation passed.' -TimeoutMinutes 12
Invoke-UnityBatch -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage10PlayModeSmokeValidator.RunStage10PlayModeSmokeBatch' -LogPath (Join-Path $logRoot 'stage10-playmode-smoke.log') -SuccessPattern 'Stage 10 play mode smoke validation passed.' -TimeoutMinutes 12

Normalize-Stage10GeneratedFiles -RepoRoot $repoRoot

Write-Host "Stage 10 Unity validation passed. Logs: $logRoot"
Write-Host 'Stage 10 Play Mode smoke fully ran in batchmode.'
Write-Host "Scene path: $scenePath"
