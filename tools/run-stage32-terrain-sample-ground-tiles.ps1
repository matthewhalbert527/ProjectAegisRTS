[CmdletBinding()]
param(
    [switch]$SkipCoreBuild
)

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

function Invoke-UnityBatchStage32TerrainSample {
    param(
        [string]$UnityEditor,
        [string]$UnityProject,
        [string]$ExecuteMethod,
        [string]$LogPath,
        [string]$SuccessPattern,
        [int]$TimeoutMinutes = 90
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

Write-ValidationSection 'Stage 32 Terrain Sample ground tiles'
Write-Host 'Scope: imported Unity Terrain Sample Asset Pack, generated ground-tile prefabs/materials, ground definition replacement, UnityEngine-free scan, and whitespace.'

if ($SkipCoreBuild) {
    Write-Host 'Skipping Rts.Core DLL build for Unity; caller already built it.'
} else {
    Write-ValidationSection 'Build Rts.Core for Unity'
    & (Join-Path $repoRoot 'tools\build-rts-core-for-unity.ps1')
    if ($LASTEXITCODE -ne 0) {
        throw "build-rts-core-for-unity.ps1 failed with exit code $LASTEXITCODE."
    }
}

New-Item -ItemType Directory -Force -Path $logRoot | Out-Null
Write-Host "Unity Editor: $unityEditor"

Invoke-UnityBatchStage32TerrainSample -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage32TerrainSampleGroundTileIntegrator.IntegrateGroundTilesBatch' -LogPath (Join-Path $logRoot 'stage32-terrain-sample-ground-tile-integration.log') -SuccessPattern 'Terrain Sample ground tile integration completed.' -TimeoutMinutes 90
Invoke-UnityBatchStage32TerrainSample -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage32TerrainSampleGroundTileIntegrator.ValidateGroundTilesBatch' -LogPath (Join-Path $logRoot 'stage32-terrain-sample-ground-tile-validation.log') -SuccessPattern 'Terrain Sample ground tile validation passed.' -TimeoutMinutes 90

Write-ValidationSection 'Rts.Core UnityEngine-free scan'
Test-RtsCoreUnityEngineFree -CorePath (Join-Path $repoRoot 'src\Rts.Core')

Write-ValidationSection 'Normalize Unity-generated whitespace'
Repair-UnityGeneratedValidationWhitespace -RepoRoot $repoRoot
Normalize-WhitespaceInTree -Path (Join-Path $repoRoot 'unity\Assets\Rts\Art\Materials\Terrain\TerrainSamplePack') -Include @('*.mat', '*.meta')
Normalize-WhitespaceInTree -Path (Join-Path $repoRoot 'unity\Assets\Rts\Art\Meshes\Terrain\TerrainSampleGroundTiles') -Include @('*.asset', '*.meta')
Normalize-WhitespaceInTree -Path (Join-Path $repoRoot 'unity\Assets\Rts\Art\Prefabs\Terrain\TerrainSampleGroundTiles') -Include @('*.prefab', '*.meta')
Remove-TrailingWhitespace -Path (Join-Path $repoRoot 'docs\TERRAIN_SAMPLE_GROUND_TILES.md')

Write-ValidationSection 'Whitespace check'
Invoke-GitDiffCheck -RepoRoot $repoRoot

Write-Host 'Stage 32 Terrain Sample ground tile checks passed.'
