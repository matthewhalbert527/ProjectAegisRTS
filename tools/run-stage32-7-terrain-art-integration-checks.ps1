[CmdletBinding()]
param(
    [switch]$SkipCoreBuild
)

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

function Invoke-UnityBatchStage32_7 {
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

function Get-UnityGuid {
    param([string]$MetaPath)

    $line = Select-String -LiteralPath $MetaPath -Pattern '^guid:\s*(\S+)' | Select-Object -First 1
    if (-not $line) {
        throw "Could not read Unity GUID from $MetaPath"
    }
    return $line.Matches[0].Groups[1].Value
}

function Assert-PngSignature {
    param([string]$Path)

    $bytes = [System.IO.File]::ReadAllBytes($Path)
    $expected = [byte[]](0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A)
    if ($bytes.Length -lt $expected.Length) {
        throw "Final mesh source PNG is too short to be valid: $Path"
    }

    for ($i = 0; $i -lt $expected.Length; $i++) {
        if ($bytes[$i] -ne $expected[$i]) {
            $signature = ($bytes[0..7] | ForEach-Object { $_.ToString('X2') }) -join ' '
            throw "Final mesh source PNG has an invalid signature ($signature): $Path"
        }
    }
}

function Assert-FinalMeshPngSources {
    param([string]$SourceRoot)

    $pngs = @(Get-ChildItem -LiteralPath $SourceRoot -Recurse -File -Filter *.png)
    if ($pngs.Count -eq 0) {
        throw "No final mesh source PNG textures were found under $SourceRoot"
    }

    foreach ($png in $pngs) {
        Assert-PngSignature -Path $png.FullName
    }
}

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$unityProject = Join-Path $repoRoot 'unity'
$logRoot = Join-Path $repoRoot 'build\unity-logs'
$unityEditor = Find-UnityEditor

if (-not $unityEditor) {
    throw 'Unity Editor was not found.'
}

Write-ValidationSection 'Stage 32.7 terrain art integration checks'
Write-Host 'Scope: final mesh source/prefab/material validation, focused review scene validation, placeholder exclusion, UnityEngine-free scan, and whitespace.'

$requiredFiles = @(
    'unity\Assets\Rts\Scenes\Stage32_6_FinalTerrainMeshReview.unity',
    'unity\Assets\Rts\Art\Source\Terrain\FinalMeshBatch01\ground_grass_dirt_01\ground_grass_dirt_01.obj',
    'unity\Assets\Rts\Art\Source\Terrain\FinalMeshBatch01\resource_cluster_blue_01\resource_cluster_blue_01.obj',
    'unity\Assets\Rts\Art\Prefabs\Terrain\FinalMeshBatch01\ground_grass_dirt_01.prefab',
    'unity\Assets\Rts\Art\Prefabs\Terrain\FinalMeshBatch01\resource_cluster_blue_01.prefab'
)
foreach ($relativePath in $requiredFiles) {
    $absolutePath = Join-Path $repoRoot $relativePath
    if (-not (Test-Path -LiteralPath $absolutePath)) {
        throw "Required final terrain mesh file is missing: $relativePath"
    }
}
Assert-FinalMeshPngSources -SourceRoot (Join-Path $repoRoot 'unity\Assets\Rts\Art\Source\Terrain\FinalMeshBatch01')

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

Invoke-UnityBatchStage32_7 -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage32_6FinalTerrainMeshBatch01Importer.CreateFinalMeshReviewSceneBatch' -LogPath (Join-Path $logRoot 'stage32-7-final-mesh-review-scene.log') -SuccessPattern 'Stage 32.6 final terrain mesh Batch01 review scene created.' -TimeoutMinutes 90
Invoke-UnityBatchStage32_7 -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage32_6FinalTerrainMeshBatch01Importer.ValidateStage32_7TerrainArtIntegrationBatch' -LogPath (Join-Path $logRoot 'stage32-7-terrain-art-integration-validation.log') -SuccessPattern 'Stage 32.7 terrain art integration validation passed.' -TimeoutMinutes 90

$scenePath = Join-Path $repoRoot 'unity\Assets\Rts\Scenes\Stage32_6_FinalTerrainMeshReview.unity'
$sceneText = Get-Content -LiteralPath $scenePath -Raw
$groundGuid = Get-UnityGuid (Join-Path $repoRoot 'unity\Assets\Rts\Art\Prefabs\Terrain\FinalMeshBatch01\ground_grass_dirt_01.prefab.meta')
$resourceGuid = Get-UnityGuid (Join-Path $repoRoot 'unity\Assets\Rts\Art\Prefabs\Terrain\FinalMeshBatch01\resource_cluster_blue_01.prefab.meta')

if ($sceneText -notmatch [regex]::Escape($groundGuid)) {
    throw 'Final review scene does not reference ground_grass_dirt_01 final mesh prefab.'
}
if ($sceneText -notmatch [regex]::Escape($resourceGuid)) {
    throw 'Final review scene does not reference resource_cluster_blue_01 final mesh prefab.'
}
if ($sceneText -match 'before placeholder|Placeholder Comparison|Batch01Imported') {
    throw 'Final review scene still contains old placeholder comparison or Batch01Imported content.'
}

foreach ($material in @(
    'unity\Assets\Rts\Art\Materials\Terrain\FinalMeshBatch01\ground_grass_dirt_01_ground_surface.mat',
    'unity\Assets\Rts\Art\Materials\Terrain\FinalMeshBatch01\resource_cluster_blue_01_resource_ground.mat'
)) {
    $materialText = Get-Content -LiteralPath (Join-Path $repoRoot $material) -Raw
    if ($materialText -notmatch 'm_Texture:\s*\{fileID:\s*2800000, guid:') {
        throw "Primary final mesh material has no texture references: $material"
    }
}

Write-ValidationSection 'Rts.Core UnityEngine-free scan'
Test-RtsCoreUnityEngineFree -CorePath (Join-Path $repoRoot 'src\Rts.Core')

Write-ValidationSection 'Normalize Unity-generated whitespace'
Repair-UnityGeneratedValidationWhitespace -RepoRoot $repoRoot
Normalize-WhitespaceInTree -Path (Join-Path $repoRoot 'unity\Assets\Rts\Art\Materials\Terrain\FinalMeshBatch01') -Include @('*.mat', '*.meta')
Normalize-WhitespaceInTree -Path (Join-Path $repoRoot 'unity\Assets\Rts\Art\Prefabs\Terrain\FinalMeshBatch01') -Include @('*.prefab', '*.meta')
Remove-TrailingWhitespace -Path $scenePath
Remove-TrailingWhitespace -Path ($scenePath + '.meta')
Assert-FinalMeshPngSources -SourceRoot (Join-Path $repoRoot 'unity\Assets\Rts\Art\Source\Terrain\FinalMeshBatch01')

Write-ValidationSection 'Whitespace check'
Invoke-GitDiffCheck -RepoRoot $repoRoot

Write-Host 'Stage 32.7 terrain art integration checks passed.'
