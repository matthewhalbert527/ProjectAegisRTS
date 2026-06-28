$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..")).Path
$project = Join-Path $repoRoot "src/Rts.Core.Tests"

$dotnetCommand = Get-Command dotnet -ErrorAction SilentlyContinue
$dotnet = if ($null -ne $dotnetCommand) { $dotnetCommand.Source } elseif (Test-Path -LiteralPath "C:\Program Files\dotnet\dotnet.exe") { "C:\Program Files\dotnet\dotnet.exe" } else { $null }
if ($null -eq $dotnet) {
    throw "dotnet SDK is required to run Stage 0 tests. Install or expose a .NET SDK on PATH, then rerun this script."
}

& $dotnet run --project $project
exit $LASTEXITCODE
