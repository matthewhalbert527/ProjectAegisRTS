[CmdletBinding()]
param(
    [switch]$SkipCoreBuild
)

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

function Invoke-UnityBatchStage20 {
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

function Repair-Stage20GeneratedWhitespace {
    param([string]$RepoRoot)

    $paths = @(
        'docs\STAGE20_PRODUCTION_VISUAL_VALIDATION.md',
        'unity\Assets\Rts\Scenes\Stage20_MvpProductionVisuals.unity',
        'unity\ProjectSettings\EditorBuildSettings.asset'
    )

    foreach ($path in $paths) {
        Remove-TrailingWhitespace -Path (Join-Path $RepoRoot $path)
    }

    Normalize-WhitespaceInTree -Path (Join-Path $RepoRoot 'unity\Assets\Rts\Art\Prefabs\Actors\ProductionProxies') -Include @('*.prefab', '*.meta')
    Normalize-WhitespaceInTree -Path (Join-Path $RepoRoot 'unity\Assets\Rts\Art\Materials') -Include @('stage20_*.mat', 'stage20_*.mat.meta')
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
Write-Host 'Stage 20 scene: Stage20_MvpProductionVisuals.'

if ($SkipCoreBuild) {
    Write-Host 'Skipping Rts.Core DLL build for Unity; caller already built it.'
} else {
    Write-Host 'Building Rts.Core DLL for Unity.'
    & (Join-Path $repoRoot 'tools\build-rts-core-for-unity.ps1')
    if ($LASTEXITCODE -ne 0) {
        throw "build-rts-core-for-unity.ps1 failed with exit code $LASTEXITCODE."
    }
}

Invoke-UnityBatchStage20 -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage20MvpProductionProxyGenerator.GenerateMvpProductionProxiesBatch' -LogPath (Join-Path $logRoot 'stage20-proxy-generation.log') -SuccessPattern 'Stage 20 MVP production proxy prefabs updated: 9' -TimeoutMinutes 20
Invoke-UnityBatchStage20 -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage20ProductionVisualValidator.ValidateStage20ProductionVisualsBatch' -LogPath (Join-Path $logRoot 'stage20-production-visual-validation.log') -SuccessPattern 'Stage 20 production visual validation passed.' -TimeoutMinutes 18
Invoke-UnityBatchStage20 -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage20SceneCreator.CreateStage20SceneBatch' -LogPath (Join-Path $logRoot 'stage20-create.log') -SuccessPattern 'Created Stage 20 scene at Assets/Rts/Scenes/Stage20_MvpProductionVisuals.unity' -TimeoutMinutes 20
Invoke-UnityBatchStage20 -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage20SceneValidator.ValidateStage20SceneBatch' -LogPath (Join-Path $logRoot 'stage20-validate.log') -SuccessPattern 'Stage 20 scene validation passed.' -TimeoutMinutes 20
Invoke-UnityBatchStage20 -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage20PlayModeSmokeValidator.RunStage20PlayModeSmokeBatch' -LogPath (Join-Path $logRoot 'stage20-playmode-smoke.log') -SuccessPattern 'Stage 20 play mode smoke validation passed.' -TimeoutMinutes 30

Repair-Stage20GeneratedWhitespace -RepoRoot $repoRoot
Repair-UnityGeneratedValidationWhitespace -RepoRoot $repoRoot

Write-Host "Stage 20 Unity validation passed. Logs: $logRoot"
Write-Host 'Stage 20 Play Mode smoke verified MVP proxy resolution, PCDesktop sidebar, QuestXR hand-control preservation, and hidden player-facing debug panels.'
