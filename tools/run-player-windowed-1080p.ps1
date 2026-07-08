[CmdletBinding()]
param(
    [int]$SmokeSeconds = 0,
    [switch]$Hidden
)

$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$exePath = Join-Path $repoRoot 'build\windows-player-stage16\ProjectAegisRTS.exe'

if (-not (Test-Path -LiteralPath $exePath)) {
    throw "Windows player EXE was not found: $exePath"
}

$arguments = @('-screen-width', '1920', '-screen-height', '1080', '-screen-fullscreen', '0')
Write-Host "Launching 1920x1080 windowed player: $exePath"
Write-Host "Arguments: $($arguments -join ' ')"

if ($Hidden) {
    $process = Start-Process -FilePath $exePath -ArgumentList $arguments -WindowStyle Hidden -PassThru
} else {
    $process = Start-Process -FilePath $exePath -ArgumentList $arguments -PassThru
}

Write-Host "Process Id: $($process.Id)"
Write-Host "Started: $($process.StartTime)"

if ($SmokeSeconds -gt 0) {
    Start-Sleep -Seconds $SmokeSeconds
    if (-not $process.HasExited) {
        Stop-Process -Id $process.Id -Force
        Start-Sleep -Seconds 2
        Write-Host "Stopped 1920x1080 windowed smoke process after $SmokeSeconds seconds."
    } else {
        Write-Host "1920x1080 windowed smoke process exited with code $($process.ExitCode)."
    }
}
