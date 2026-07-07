[CmdletBinding()]
param(
    [string]$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [int]$TimeoutMinutes = 90
)

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

function Invoke-UnityAiTankBatch {
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
Write-Host 'Unity AI tank visual generation and validation.'

Invoke-UnityAiTankBatch -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.UnityAiTankVisualGenerator.BuildAndCaptureBatch' -LogPath (Join-Path $logRoot 'unity-ai-tank-visual-generate.log') -SuccessPattern 'Unity AI tank visual screenshot captured:' -TimeoutMinutes $TimeoutMinutes
Invoke-UnityAiTankBatch -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.UnityAiTankVisualGenerator.ValidateBatch' -LogPath (Join-Path $logRoot 'unity-ai-tank-visual-validate.log') -SuccessPattern 'Unity AI tank visual validation passed for 3 prefabs.' -TimeoutMinutes $TimeoutMinutes

Repair-UnityGeneratedValidationWhitespace -RepoRoot $repoRoot
Normalize-WhitespaceInTree -Path (Join-Path $repoRoot 'unity\Assets\Rts\Art\UnityAITankSlate') -Include @('*.mat', '*.meta', '*.prefab', '*.asset', '*.unity')
Normalize-WhitespaceInTree -Path (Join-Path $repoRoot 'unity\Assets\Rts\ScriptableObjects\Art\ActorVisualDefinitions') -Include @('light_tank_visual.asset', 'medium_tank_visual.asset', 'heavy_tank_visual.asset')
Remove-TrailingWhitespace -Path (Join-Path $repoRoot 'unity\Assets\Rts\Scenes\UnityAI_TankVisualReview.unity')
Remove-TrailingWhitespace -Path (Join-Path $repoRoot 'unity\Assets\Rts\Scenes\UnityAI_TankVisualReview.unity.meta')

Write-Host "Unity AI tank visual generation passed. Screenshot: $(Join-Path $repoRoot 'build\screenshots\unity_ai_tank_visual_review.png')"
