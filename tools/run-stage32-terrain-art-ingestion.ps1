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
$logPath = Join-Path $logs 'stage32-terrain-art-ingestion.log'
if (Test-Path -LiteralPath $logPath) {
    Remove-Item -LiteralPath $logPath -Force
}

Write-Host 'Running Stage32 terrain art ingestion.'
Write-Host "Log: $logPath"
$arguments = "-batchmode -quit -projectPath `"$ProjectPath`" -executeMethod ProjectAegisRTS.UnityClient.EditorTools.Stage32TerrainArtIngestionGenerator.IngestBatch01TerrainArtBatch -logFile `"$logPath`""
$process = Start-Process -FilePath $UnityExe -ArgumentList $arguments -WindowStyle Hidden -PassThru
$deadline = (Get-Date).AddMinutes(75)
$sawSuccess = $false

do {
    Start-Sleep -Seconds 2
    if (Test-Path -LiteralPath $logPath) {
        $compilerError = Select-String -LiteralPath $logPath -Pattern 'Scripts have compiler errors', 'error CS', 'Script Compilation Error', 'Exception:' -Quiet
        if ($compilerError) {
            break
        }

        $sawSuccess = Select-String -LiteralPath $logPath -Pattern 'Stage 32 terrain art ingestion completed.' -Quiet
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

$compilerErrorAfter = Select-String -LiteralPath $logPath -Pattern 'Scripts have compiler errors', 'error CS', 'Script Compilation Error', 'Exception:' -Quiet
if ($compilerErrorAfter) {
    throw "Unity terrain art ingestion logged errors. See $logPath"
}

if (-not $sawSuccess) {
    throw "Unity did not log the Stage32 terrain art ingestion success marker. See $logPath"
}

if ($process.ExitCode -ne 0) {
    throw "Unity process exited with code $($process.ExitCode). See $logPath"
}

Repair-UnityGeneratedValidationWhitespace -RepoRoot $repoRoot
Write-Host 'Stage32 terrain art ingestion passed.'
