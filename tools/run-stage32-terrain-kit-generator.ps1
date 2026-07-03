[CmdletBinding()]
param(
    [string]$UnityExe,
    [string]$ProjectPath
)

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
if (-not $UnityExe) {
    $UnityExe = Find-UnityEditor
}
if (-not $UnityExe -or -not (Test-Path -LiteralPath $UnityExe)) {
    throw 'Unity Editor was not found. Pass -UnityExe or install Unity through Unity Hub.'
}
if (-not $ProjectPath) {
    $ProjectPath = Join-Path $repoRoot 'unity'
}
$ProjectPath = (Resolve-Path $ProjectPath).Path

$logs = Join-Path $repoRoot 'build\unity-logs'
New-Item -ItemType Directory -Force -Path $logs | Out-Null

function Invoke-Stage32TerrainKitUnityMethod {
    param(
        [string]$MethodName,
        [string]$LogName,
        [string]$SuccessPattern,
        [string]$FailureMessage,
        [int]$TimeoutMinutes = 90
    )

    $logPath = Join-Path $logs $LogName
    if (Test-Path -LiteralPath $logPath) {
        Remove-Item -LiteralPath $logPath -Force
    }

    for ($attempt = 1; $attempt -le 2; $attempt++) {
        Write-Host "Running Unity batch method $MethodName"
        Write-Host "Log: $logPath"
        $arguments = "-batchmode -quit -projectPath `"$ProjectPath`" -executeMethod $MethodName -logFile `"$logPath`""
        $process = Start-Process -FilePath $UnityExe -ArgumentList $arguments -WindowStyle Hidden -PassThru
        $deadline = (Get-Date).AddMinutes($TimeoutMinutes)
        $sawSuccess = $false

        do {
            Start-Sleep -Seconds 2
            if (Test-Path -LiteralPath $logPath) {
                $compilerError = Select-String -LiteralPath $logPath -Pattern 'Scripts have compiler errors', 'error CS', 'Script Compilation Error', 'Exception:', 'Aborting batchmode due to fatal error' -Quiet
                if ($compilerError) {
                    break
                }

                $sawSuccess = Select-String -LiteralPath $logPath -Pattern $SuccessPattern -Quiet
                if ($sawSuccess) {
                    break
                }
            }
            if ($process.HasExited) {
                break
            }
        } while ((Get-Date) -lt $deadline)

        if (-not $process.HasExited) {
            if (-not $process.WaitForExit(120000)) {
                throw "Unity logged no final exit within the grace period. See $logPath"
            }
        }

        if (-not (Test-Path -LiteralPath $logPath)) {
            throw "Unity did not create a log within the timeout. Expected: $logPath"
        }

        $compilerErrorAfter = Select-String -LiteralPath $logPath -Pattern 'Scripts have compiler errors', 'error CS', 'Script Compilation Error', 'Exception:', 'Aborting batchmode due to fatal error' -Quiet
        if ($compilerErrorAfter) {
            throw "$FailureMessage See log: $logPath"
        }

        if ($sawSuccess -and $process.ExitCode -eq 0) {
            return
        }

        $compileRefresh = Select-String -LiteralPath $logPath -Pattern 'Requested script compilation', 'script compilation time', 'Tundra requires additional run' -Quiet
        if ($attempt -eq 1 -and $compileRefresh -and -not $sawSuccess) {
            Write-Host 'Unity refreshed scripts before running the batch method; retrying once.'
            continue
        }

        if (-not $sawSuccess) {
            throw "Unity did not log expected success marker '$SuccessPattern'. See $logPath"
        }

        throw "$FailureMessage Unity exited with code $($process.ExitCode). See log: $logPath"
    }
}

Invoke-Stage32TerrainKitUnityMethod `
    -MethodName 'ProjectAegisRTS.UnityClient.EditorTools.Stage32TerrainKitGenerator.GenerateTerrainKitBatch' `
    -LogName 'stage32-terrain-kit-generate.log' `
    -SuccessPattern 'Stage 32 terrain kit generated' `
    -FailureMessage 'Unity terrain kit generation failed.'

Invoke-Stage32TerrainKitUnityMethod `
    -MethodName 'ProjectAegisRTS.UnityClient.EditorTools.Stage32TerrainKitValidator.ValidateTerrainKitBatch' `
    -LogName 'stage32-terrain-kit-validate.log' `
    -SuccessPattern 'Stage32 terrain validation passed' `
    -FailureMessage 'Unity terrain kit validation failed.'

Repair-UnityGeneratedValidationWhitespace -RepoRoot $repoRoot

Write-Host "Stage32 terrain kit generated and validated. Logs: $logs"
