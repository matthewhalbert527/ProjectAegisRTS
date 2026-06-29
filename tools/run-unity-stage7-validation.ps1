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
            ($_.CommandLine.Contains($ProjectPath) -or $_.CommandLine.Contains($slashPath))
        }
}

function Invoke-UnityBatch {
    param(
        [string]$UnityEditor,
        [string]$UnityProject,
        [string]$ExecuteMethod,
        [string]$LogPath,
        [string]$SuccessPattern,
        [int]$TimeoutMinutes = 10
    )

    if (Test-Path -LiteralPath $LogPath) {
        Remove-Item -LiteralPath $LogPath -Force
    }

    $arguments = "-batchmode -quit -projectPath `"$UnityProject`" -executeMethod $ExecuteMethod -logFile `"$LogPath`""
    Write-Host "Running Unity batch method: $ExecuteMethod"
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

function Test-Stage7SceneFileLive {
    param([string]$ScenePath)

    if (-not (Test-Path -LiteralPath $ScenePath)) {
        throw "Stage 7 scene does not exist and Unity is already open, so it cannot be generated in batchmode: $ScenePath"
    }

    $lines = Get-Content -LiteralPath $ScenePath
    $required = @(
        'm_Name: RtsGame',
        'm_Name: BoardRoot',
        'm_Name: Main Camera',
        'm_Name: Directional Light',
        'm_Name: EventSystem',
        'm_Name: Canvas',
        'm_Name: Stage7 Placeholder Buildings',
        'Assembly-CSharp::ProjectAegisRTS.UnityClient.Rendering.Buildings.BuildingVisualProfileLibrary',
        'Assembly-CSharp::ProjectAegisRTS.UnityClient.Rendering.Buildings.BuildingPowerDemoController',
        'Assembly-CSharp::ProjectAegisRTS.UnityClient.Rendering.Buildings.BuildingVisualStateController',
        'Assembly-CSharp::ProjectAegisRTS.UnityClient.UI.Common.BuildingAnimationDebugHud'
    )

    foreach ($pattern in $required) {
        if (-not ($lines | Select-String -Pattern ([regex]::Escape($pattern)) -Quiet)) {
            throw "Stage 7 scene live validation failed; missing '$pattern'."
        }
    }
}

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$unityProject = Join-Path $repoRoot 'unity'
$scenePath = Join-Path $unityProject 'Assets\Rts\Scenes\Stage7_BuildingPowerProduction.unity'
$profilePath = Join-Path $unityProject 'Assets\Rts\ScriptableObjects\BuildingProfiles'
$logRoot = Join-Path $repoRoot 'build\unity-logs'
$profilesLog = Join-Path $logRoot 'stage7-profiles.log'
$createLog = Join-Path $logRoot 'stage7-create.log'
$validateLog = Join-Path $logRoot 'stage7-validate.log'
$smokeLog = Join-Path $logRoot 'stage7-playmode-smoke.log'
$unityEditor = Find-UnityEditor

if (-not $unityEditor) {
    throw 'Unity Editor was not found.'
}

New-Item -ItemType Directory -Force -Path $logRoot | Out-Null
Write-Host "Unity Editor: $unityEditor"
Write-Host "Stage 7 scene: $scenePath"

Write-Host 'Building Rts.Core DLL for Unity.'
& (Join-Path $repoRoot 'tools\build-rts-core-for-unity.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "build-rts-core-for-unity.ps1 failed with exit code $LASTEXITCODE."
}

$openUnity = @(Get-UnityProcessesForProject -ProjectPath $unityProject | Where-Object { $_.CommandLine -notmatch 'AssetImportWorker' })
if ($openUnity.Count -gt 0) {
    Write-Warning 'Unity Editor is already open for this project. Using Stage 7 live scene/log validation fallback; full batch Play Mode automation is skipped because Unity owns the project lock.'
    Test-Stage7SceneFileLive -ScenePath $scenePath
    Remove-TrailingWhitespace -Path $scenePath
    $editorLog = Join-Path $unityProject 'Logs\Editor.log'
    if (Test-Path -LiteralPath $editorLog) {
        $error = Select-String -LiteralPath $editorLog -Pattern 'NullReferenceException', 'MissingMethodException', 'TypeLoadException', 'FileNotFoundException', 'Scripts have compiler errors', 'error CS[0-9]+' -Quiet
        if ($error) {
            throw "Unity Editor log contains red-error signatures. See $editorLog"
        }
    }

    Write-Host 'Stage 7 live validation passed. Play Mode smoke used fallback.'
    Write-Host "Scene path: $scenePath"
    return
}

Invoke-UnityBatch -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage7BuildingProfileAssetCreator.CreateBuildingProfilesBatch' -LogPath $profilesLog -SuccessPattern 'Stage 7 building visual profile assets updated at Assets/Rts/ScriptableObjects/BuildingProfiles' -TimeoutMinutes 10
Invoke-UnityBatch -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage7SceneCreator.CreateStage7SceneBatch' -LogPath $createLog -SuccessPattern 'Created Stage 7 scene at Assets/Rts/Scenes/Stage7_BuildingPowerProduction.unity' -TimeoutMinutes 10
Invoke-UnityBatch -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage7SceneValidator.ValidateStage7SceneBatch' -LogPath $validateLog -SuccessPattern 'Stage 7 scene validation passed.' -TimeoutMinutes 10
Invoke-UnityBatch -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage7PlayModeSmokeValidator.RunStage7PlayModeSmokeBatch' -LogPath $smokeLog -SuccessPattern 'Stage 7 play mode smoke validation passed.' -TimeoutMinutes 10

Remove-TrailingWhitespace -Path $scenePath
if (Test-Path -LiteralPath "$scenePath.meta") {
    Remove-TrailingWhitespace -Path "$scenePath.meta"
}
if (Test-Path -LiteralPath $profilePath) {
    Get-ChildItem -LiteralPath $profilePath -Filter '*.asset' -ErrorAction SilentlyContinue |
        ForEach-Object { Remove-TrailingWhitespace -Path $_.FullName }
    Get-ChildItem -LiteralPath $profilePath -Filter '*.asset.meta' -ErrorAction SilentlyContinue |
        ForEach-Object { Remove-TrailingWhitespace -Path $_.FullName }
}

Write-Host "Stage 7 Unity validation passed. Logs: $profilesLog ; $createLog ; $validateLog ; $smokeLog"
Write-Host 'Stage 7 Play Mode smoke fully ran in batchmode.'
Write-Host "Scene path: $scenePath"
