[CmdletBinding()]
param(
    [switch]$SkipPlayerBuild
)

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

function Invoke-UnityBatchForStage16_5 {
    param(
        [string]$UnityEditor,
        [string]$UnityProject,
        [string]$ExecuteMethod,
        [string]$LogPath,
        [string]$SuccessPattern,
        [int]$TimeoutMinutes = 15,
        [string[]]$ExtraArguments = @()
    )

    if (Test-Path -LiteralPath $LogPath) {
        Remove-Item -LiteralPath $LogPath -Force
    }

    $arguments = "-batchmode -quit -projectPath `"$UnityProject`" -executeMethod $ExecuteMethod -logFile `"$LogPath`""
    foreach ($extra in $ExtraArguments) {
        $arguments += " $extra"
    }

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
$dotnet = Find-DotNet
$unityEditor = Find-UnityEditor
$unityProject = Join-Path $repoRoot 'unity'
$logRoot = Join-Path $repoRoot 'build\unity-logs'
$corePath = Join-Path $repoRoot 'src\Rts.Core'
$playerExe = Join-Path $repoRoot 'build\windows-player-stage16\ProjectAegisRTS.exe'

if (-not $unityEditor) {
    throw 'Unity Editor was not found.'
}

New-Item -ItemType Directory -Force -Path $logRoot | Out-Null

Write-ValidationSection 'Stage 16 player build checks'
Write-Host 'Scope: Rts.Core tests, Unity DLL build, medium recursion audit, Stage 16 medium validation, Stage 16.5 build-flow validation, optional Windows player build, UnityEngine-free scan, and git diff whitespace check.'

Write-ValidationSection 'Rts.Core tests'
Invoke-DotNetRunNoRestore -DotNetPath $dotnet -ProjectPath (Join-Path $repoRoot 'src\Rts.Core.Tests')
if ($LASTEXITCODE -ne 0) {
    throw "Rts.Core.Tests failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Build Rts.Core for Unity'
& (Join-Path $repoRoot 'tools\build-rts-core-for-unity.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "build-rts-core-for-unity.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Medium recursion audit'
& (Join-Path $repoRoot 'tools\audit-medium-validation-recursion.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "audit-medium-validation-recursion.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Configure Stage 16.5 build flow'
Invoke-UnityBatchForStage16_5 -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage16_5BuildFlowConfigurator.ConfigureBuildFlowBatch' -LogPath (Join-Path $logRoot 'stage16-5-configure.log') -SuccessPattern 'Stage 16.5 player build flow configured.' -TimeoutMinutes 18

Write-ValidationSection 'Stage 16 medium validation'
& (Join-Path $repoRoot 'tools\run-stage16-medium-checks.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-stage16-medium-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Re-configure Stage 16.5 build flow'
Invoke-UnityBatchForStage16_5 -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage16_5BuildFlowConfigurator.ConfigureBuildFlowBatch' -LogPath (Join-Path $logRoot 'stage16-5-reconfigure.log') -SuccessPattern 'Stage 16.5 player build flow configured.' -TimeoutMinutes 18

Write-ValidationSection 'Stage 16.5 build-flow validation'
Invoke-UnityBatchForStage16_5 -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage16_5BuildFlowValidator.ValidateBuildFlowBatch' -LogPath (Join-Path $logRoot 'stage16-5-validate.log') -SuccessPattern 'Stage 16.5 build flow validation passed.' -TimeoutMinutes 12
Invoke-UnityBatchForStage16_5 -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage16_5PlayModeSmokeValidator.RunBuildFlowSmokeBatch' -LogPath (Join-Path $logRoot 'stage16-5-smoke.log') -SuccessPattern 'Stage 16.5 build flow smoke validation passed.' -TimeoutMinutes 18

if ($SkipPlayerBuild) {
    Write-Host 'Skipping Windows player build by request.'
} else {
    Write-ValidationSection 'Windows player build'
    $quotedPlayerPath = '-stage16WindowsPlayerPath "' + $playerExe + '"'
    Invoke-UnityBatchForStage16_5 -UnityEditor $unityEditor -UnityProject $unityProject -ExecuteMethod 'ProjectAegisRTS.UnityClient.EditorTools.Stage16_5BuildFlowConfigurator.BuildWindowsPlayerBatch' -LogPath (Join-Path $logRoot 'stage16-5-windows-player.log') -SuccessPattern 'Stage 16.5 Windows player build succeeded:' -TimeoutMinutes 30 -ExtraArguments @($quotedPlayerPath)
    if (-not (Test-Path -LiteralPath $playerExe)) {
        throw "Expected Windows player EXE was not created: $playerExe"
    }
    Write-Host "Windows player EXE: $playerExe"
}

Write-ValidationSection 'Rts.Core UnityEngine-free scan'
Test-RtsCoreUnityEngineFree -CorePath $corePath

Write-ValidationSection 'Normalize Unity-generated whitespace'
Repair-UnityGeneratedValidationWhitespace -RepoRoot $repoRoot

Write-ValidationSection 'Whitespace check'
Invoke-GitDiffCheck -RepoRoot $repoRoot

Write-Host 'Stage 16 player build checks passed.'
