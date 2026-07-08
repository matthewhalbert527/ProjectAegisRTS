[CmdletBinding()]
param(
    [string]$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [int]$TimeoutMinutes = 90
)

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

function Invoke-UnityBatchStage33 {
    param(
        [string]$UnityEditor,
        [string]$UnityProject,
        [string]$ExecuteMethod,
        [string]$LogPath,
        [string]$SuccessPattern,
        [int]$TimeoutMinutes
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

$repoRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
$unityProject = Join-Path $repoRoot 'unity'
$logRoot = Join-Path $repoRoot 'build\unity-logs'
$unityEditor = Find-UnityEditor

if (-not $unityEditor) {
    throw 'Unity Editor was not found.'
}

New-Item -ItemType Directory -Force -Path $logRoot | Out-Null
Write-Host "Unity Editor: $unityEditor"
Write-Host 'Stage 33 tank source prefab generation and validation.'

Invoke-UnityBatchStage33 -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.Art.Production.Stage33TankSourceGenerator.GenerateTankSourcePrefabsBatch' -LogPath (Join-Path $logRoot 'stage33-tank-source-generate.log') -SuccessPattern 'Stage33 tank source prefabs generated: 3' -TimeoutMinutes $TimeoutMinutes
Invoke-UnityBatchStage33 -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.Art.Production.Stage33TankSourceGenerator.ValidateTankSourcePrefabsBatch' -LogPath (Join-Path $logRoot 'stage33-tank-source-validate.log') -SuccessPattern 'Stage33 tank source validation passed for 3 prefabs.' -TimeoutMinutes $TimeoutMinutes

Repair-UnityGeneratedValidationWhitespace -RepoRoot $repoRoot
Normalize-WhitespaceInTree -Path (Join-Path $repoRoot 'unity\Assets\Rts\Art\Materials\Production\Tanks') -Include @('*.mat', '*.meta')
Normalize-WhitespaceInTree -Path (Join-Path $repoRoot 'unity\Assets\Rts\Art\Prefabs\Actors\Production\MVP\Tanks') -Include @('*.prefab', '*.meta')
Remove-TrailingWhitespace -Path (Join-Path $repoRoot 'unity\Assets\Rts\Scenes\Stage33_TankSourceReview.unity')
Remove-TrailingWhitespace -Path (Join-Path $repoRoot 'unity\Assets\Rts\Scenes\Stage33_TankSourceReview.unity.meta')
Remove-TrailingWhitespace -Path (Join-Path $repoRoot 'docs\STAGE33_TANK_SOURCE_REPORT.md')

Write-Host "Stage33 tank source generation passed. Logs: $logRoot"
