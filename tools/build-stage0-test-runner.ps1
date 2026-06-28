param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..")
$project = Join-Path $repoRoot "src/Rts.Core.Tests/Rts.Core.Tests.csproj"
$output = Join-Path $repoRoot "build/stage0-test-runner"
$desktop = [Environment]::GetFolderPath("DesktopDirectory")
$shortcutPath = Join-Path $desktop "ProjectAegisRTS Stage 0 Tests.lnk"

$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if ($null -eq $dotnet) {
    throw "dotnet SDK is required to publish the Stage 0 test runner. Install or expose a .NET SDK on PATH, then rerun this script."
}

New-Item -ItemType Directory -Force -Path $output | Out-Null
& $dotnet.Source publish $project --configuration $Configuration --runtime win-x64 --self-contained false -p:PublishSingleFile=true --output $output
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$exe = Join-Path $output "Rts.Core.Tests.exe"
if (!(Test-Path -LiteralPath $exe)) {
    throw "Publish completed but expected executable was not found: $exe"
}

$shell = New-Object -ComObject WScript.Shell
$shortcut = $shell.CreateShortcut($shortcutPath)
$shortcut.TargetPath = $exe
$shortcut.WorkingDirectory = $repoRoot
$shortcut.Description = "Run ProjectAegisRTS Stage 0 deterministic core tests"
$shortcut.Save()

Write-Host "Published $exe"
Write-Host "Updated shortcut $shortcutPath"
