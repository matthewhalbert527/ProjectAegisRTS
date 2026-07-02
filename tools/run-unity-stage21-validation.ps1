[CmdletBinding()]
param(
    [switch]$SkipCoreBuild
)

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

function Invoke-UnityBatchStage21 {
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

function Repair-Stage21GeneratedWhitespace {
    param([string]$RepoRoot)

    $paths = @(
        'docs\STAGE21_ARTIST_MODEL_IMPORT_STATUS.md',
        'docs\STAGE21_MVP_VISUAL_QA.md',
        'unity\Assets\Rts\Art\Models\Imported\MVP.meta',
        'unity\Assets\Rts\Art\Models\Source\MVP.meta',
        'unity\Assets\Rts\Art\Prefabs\Actors\Production\MVP.meta',
        'unity\Assets\Rts\Scenes\Stage21_MvpVisualQaReview.unity',
        'unity\Assets\Rts\Scenes\Stage21_MvpVisualQaReview.unity.meta',
        'unity\ProjectSettings\EditorBuildSettings.asset',
        'unity\Assets\Rts\ScriptableObjects\Art\ProductionSpecs\stage21_artist_model_import_manifest.asset',
        'unity\Assets\Rts\ScriptableObjects\Art\ProductionSpecs\stage21_artist_model_import_manifest.asset.meta'
    )

    foreach ($path in $paths) {
        Remove-TrailingWhitespace -Path (Join-Path $RepoRoot $path)
    }

    Normalize-WhitespaceInTree -Path (Join-Path $RepoRoot 'unity\Assets\Rts\Art\Models\Source\MVP') -Include @('*.md', '*.meta')
    Normalize-WhitespaceInTree -Path (Join-Path $RepoRoot 'unity\Assets\Rts\Art\Models\Imported\MVP') -Include @('*.md', '*.meta')
    Normalize-WhitespaceInTree -Path (Join-Path $RepoRoot 'unity\Assets\Rts\Art\Prefabs\Actors\Production\MVP') -Include @('*.md', '*.prefab', '*.meta')
    Normalize-WhitespaceInTree -Path (Join-Path $RepoRoot 'unity\Assets\Rts\Art\Prefabs\Actors\ProductionProxies') -Include @('*.prefab', '*.meta')
    Normalize-WhitespaceInTree -Path (Join-Path $RepoRoot 'unity\Assets\Rts\ScriptableObjects\Art\ActorVisualDefinitions') -Include @('*.asset', '*.meta')
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
Write-Host 'Stage 21 scene: Stage21_MvpVisualQaReview.'

if ($SkipCoreBuild) {
    Write-Host 'Skipping Rts.Core DLL build for Unity; caller already built it.'
} else {
    Write-Host 'Building Rts.Core DLL for Unity.'
    & (Join-Path $repoRoot 'tools\build-rts-core-for-unity.ps1')
    if ($LASTEXITCODE -ne 0) {
        throw "build-rts-core-for-unity.ps1 failed with exit code $LASTEXITCODE."
    }
}

Invoke-UnityBatchStage21 -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage21ArtistModelImportScanner.ScanMvpArtistModelsBatch' -LogPath (Join-Path $logRoot 'stage21-artist-import-scan.log') -SuccessPattern 'Stage 21 artist model import scan completed.' -TimeoutMinutes 18
Invoke-UnityBatchStage21 -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage21MvpVisualQaValidator.ValidateMvpVisualQaBatch' -LogPath (Join-Path $logRoot 'stage21-mvp-visual-qa.log') -SuccessPattern 'Stage 21 MVP visual QA validation passed.' -TimeoutMinutes 24
Invoke-UnityBatchStage21 -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage21MvpSocketPivotScaleValidator.ValidateMvpSocketPivotScaleBatch' -LogPath (Join-Path $logRoot 'stage21-socket-pivot-scale.log') -SuccessPattern 'Stage 21 socket/pivot/scale validation passed.' -TimeoutMinutes 20
Invoke-UnityBatchStage21 -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage21SceneCreator.CreateStage21SceneBatch' -LogPath (Join-Path $logRoot 'stage21-create.log') -SuccessPattern 'Created Stage 21 scene at Assets/Rts/Scenes/Stage21_MvpVisualQaReview.unity' -TimeoutMinutes 24
Invoke-UnityBatchStage21 -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage21SceneValidator.ValidateStage21SceneBatch' -LogPath (Join-Path $logRoot 'stage21-validate.log') -SuccessPattern 'Stage 21 scene validation passed.' -TimeoutMinutes 24
Invoke-UnityBatchStage21 -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage21PlayModeSmokeValidator.RunStage21PlayModeSmokeBatch' -LogPath (Join-Path $logRoot 'stage21-playmode-smoke.log') -SuccessPattern 'Stage 21 play mode smoke validation passed.' -TimeoutMinutes 35

Repair-Stage21GeneratedWhitespace -RepoRoot $repoRoot
Repair-UnityGeneratedValidationWhitespace -RepoRoot $repoRoot

Write-Host "Stage 21 Unity validation passed. Logs: $logRoot"
Write-Host 'Stage 21 Play Mode smoke verified MVP visual QA, player-facing proxy resolution, PCDesktop sidebar, QuestXR hand controls, hidden debug panels, and placement preview safety.'
