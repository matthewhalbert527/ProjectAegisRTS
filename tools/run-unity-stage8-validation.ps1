[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

function Find-UnityEditor {
    $patterns = @(
        'E:\Unity\Hub\Editor\*\Editor\Unity.exe',
        'C:\Program Files\Unity\Hub\Editor\*\Editor\Unity.exe',
        'C:\Program Files\Unity\Editor\Unity.exe',
        'C:\Program Files (x86)\Unity\Editor\Unity.exe'
    )

    $editors = @()
    foreach ($pattern in $patterns) {
        $editors += Get-ChildItem -Path $pattern -ErrorAction SilentlyContinue
    }

    if ($editors.Count -gt 0) {
        return ($editors | Sort-Object FullName -Descending | Select-Object -First 1).FullName
    }

    $command = Get-Command Unity -ErrorAction SilentlyContinue
    if ($command) {
        return $command.Source
    }

    return $null
}

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

function Remove-TrailingWhitespace {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        return
    }

    $resolved = (Resolve-Path -LiteralPath $Path).Path
    $lines = [System.IO.File]::ReadAllLines($resolved)
    $changed = $false
    for ($i = 0; $i -lt $lines.Length; $i++) {
        $trimmed = $lines[$i].TrimEnd()
        if ($trimmed.Length -ne $lines[$i].Length) {
            $changed = $true
            $lines[$i] = $trimmed
        }
    }

    if ($changed) {
        $utf8NoBom = New-Object System.Text.UTF8Encoding -ArgumentList $false
        [System.IO.File]::WriteAllLines($resolved, [string[]]$lines, $utf8NoBom)
    }
}

function Test-Stage8SceneFileLive {
    param([string]$ScenePath)

    if (-not (Test-Path -LiteralPath $ScenePath)) {
        throw "Stage 8 scene does not exist and Unity is already open, so it cannot be generated in batchmode: $ScenePath"
    }

    $lines = Get-Content -LiteralPath $ScenePath
    $required = @(
        'm_Name: RtsGame',
        'm_Name: BoardRoot',
        'm_Name: Main Camera',
        'm_Name: Directional Light',
        'm_Name: EventSystem',
        'm_Name: Canvas',
        'm_Name: Stage8 Art Pipeline Showcase',
        'Assembly-CSharp::ProjectAegisRTS.UnityClient.Art.ActorVisualDefinitionLibrary',
        'Assembly-CSharp::ProjectAegisRTS.UnityClient.Art.ActorVisualPrefabResolver',
        'Assembly-CSharp::ProjectAegisRTS.UnityClient.Art.ConceptArtReferenceLibrary',
        'Assembly-CSharp::ProjectAegisRTS.UnityClient.Art.ArtPipelineShowcaseController',
        'Assembly-CSharp::ProjectAegisRTS.UnityClient.UI.Common.ArtPipelineDebugHud'
    )

    foreach ($pattern in $required) {
        if (-not ($lines | Select-String -Pattern ([regex]::Escape($pattern)) -Quiet)) {
            throw "Stage 8 scene live validation failed; missing '$pattern'."
        }
    }
}

function Normalize-Stage8GeneratedFiles {
    param([string]$RepoRoot)

    $paths = @(
        'unity\Assets\Rts\Scenes\Stage8_ArtPipelineShowcase.unity',
        'unity\Assets\Rts\Scenes\Stage8_ArtPipelineShowcase.unity.meta',
        'unity\Assets\XR\Settings\OpenXR Package Settings.asset',
        'unity\Assets\XR\Settings\OpenXR Package Settings.asset.meta',
        'docs\STAGE8_PREFAB_VALIDATION.md'
    )

    foreach ($path in $paths) {
        Remove-TrailingWhitespace -Path (Join-Path $RepoRoot $path)
    }

    $roots = @(
        'unity\Assets\Rts\ScriptableObjects\Art',
        'unity\Assets\Rts\Art\Validation',
        'unity\Assets\Rts\Art\Materials',
        'unity\Assets\Rts\Art\Prefabs\Actors\GeneratedBlockouts'
    )

    foreach ($relativeRoot in $roots) {
        $path = Join-Path $RepoRoot $relativeRoot
        if (Test-Path -LiteralPath $path) {
            Get-ChildItem -LiteralPath $path -Recurse -File -Include *.asset,*.json,*.mat,*.meta,*.prefab |
                ForEach-Object { Remove-TrailingWhitespace -Path $_.FullName }
        }
    }
}

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$unityProject = Join-Path $repoRoot 'unity'
$scenePath = Join-Path $unityProject 'Assets\Rts\Scenes\Stage8_ArtPipelineShowcase.unity'
$logRoot = Join-Path $repoRoot 'build\unity-logs'
$unityEditor = Find-UnityEditor

if (-not $unityEditor) {
    throw 'Unity Editor was not found.'
}

New-Item -ItemType Directory -Force -Path $logRoot | Out-Null
Write-Host "Unity Editor: $unityEditor"
Write-Host "Stage 8 scene: $scenePath"

Write-Host 'Building Rts.Core DLL for Unity.'
& (Join-Path $repoRoot 'tools\build-rts-core-for-unity.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "build-rts-core-for-unity.ps1 failed with exit code $LASTEXITCODE."
}

$openUnity = @(Get-UnityProcessesForProject -ProjectPath $unityProject |
    Where-Object {
        $_.CommandLine -notmatch 'AssetImportWorker' -and
        $_.CommandLine -notmatch '-batchmode'
    })
if ($openUnity.Count -gt 0) {
    Write-Warning 'Unity Editor is already open for this project. Using Stage 8 live scene/log validation fallback; full batch Play Mode automation is skipped because Unity owns the project lock.'
    Test-Stage8SceneFileLive -ScenePath $scenePath
    Normalize-Stage8GeneratedFiles -RepoRoot $repoRoot
    $editorLog = Join-Path $unityProject 'Logs\Editor.log'
    if (Test-Path -LiteralPath $editorLog) {
        $error = Select-String -LiteralPath $editorLog -Pattern 'NullReferenceException', 'MissingMethodException', 'TypeLoadException', 'FileNotFoundException', 'Scripts have compiler errors', 'error CS[0-9]+' -Quiet
        if ($error) {
            throw "Unity Editor log contains red-error signatures. See $editorLog"
        }
    }

    Write-Host 'Stage 8 live validation passed. Play Mode smoke used fallback.'
    Write-Host "Scene path: $scenePath"
    return
}

Invoke-UnityBatch -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage8ConceptArtImporter.ImportConceptArtBatch' -LogPath (Join-Path $logRoot 'stage8-concept-import.log') -SuccessPattern 'Stage 8 concept art references imported: 27' -TimeoutMinutes 12
Invoke-UnityBatch -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage8BlockoutPrefabGenerator.GenerateBlockoutsBatch' -LogPath (Join-Path $logRoot 'stage8-blockouts.log') -SuccessPattern 'Stage 8 generated blockout prefabs updated: 27' -TimeoutMinutes 12
Invoke-UnityBatch -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage8ActorVisualDefinitionGenerator.CreateDefinitionsBatch' -LogPath (Join-Path $logRoot 'stage8-definitions.log') -SuccessPattern 'Stage 8 actor visual definitions updated: 27' -TimeoutMinutes 12
Invoke-UnityBatch -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage8IconGenerator.GenerateIconsBatch' -LogPath (Join-Path $logRoot 'stage8-icons.log') -SuccessPattern 'Stage 8 actor icons updated: 27' -TimeoutMinutes 12
Invoke-UnityBatch -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage8PrefabSocketValidator.ValidatePrefabsBatch' -LogPath (Join-Path $logRoot 'stage8-prefab-validation.log') -SuccessPattern 'Stage 8 prefab validation passed.' -TimeoutMinutes 12
Invoke-UnityBatch -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage8SceneCreator.CreateStage8SceneBatch' -LogPath (Join-Path $logRoot 'stage8-create.log') -SuccessPattern 'Created Stage 8 scene at Assets/Rts/Scenes/Stage8_ArtPipelineShowcase.unity' -TimeoutMinutes 15
Invoke-UnityBatch -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage8SceneValidator.ValidateStage8SceneBatch' -LogPath (Join-Path $logRoot 'stage8-validate.log') -SuccessPattern 'Stage 8 scene validation passed.' -TimeoutMinutes 12
Invoke-UnityBatch -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage8PlayModeSmokeValidator.RunStage8PlayModeSmokeBatch' -LogPath (Join-Path $logRoot 'stage8-playmode-smoke.log') -SuccessPattern 'Stage 8 play mode smoke validation passed.' -TimeoutMinutes 12

Normalize-Stage8GeneratedFiles -RepoRoot $repoRoot

Write-Host "Stage 8 Unity validation passed. Logs: $logRoot"
Write-Host 'Stage 8 Play Mode smoke fully ran in batchmode.'
Write-Host "Scene path: $scenePath"
