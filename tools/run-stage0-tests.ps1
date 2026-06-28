$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..")
$project = Join-Path $repoRoot "src/Rts.Core.Tests"

$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if ($null -eq $dotnet) {
    throw "dotnet SDK is required to run Stage 0 tests. Install or expose a .NET SDK on PATH, then rerun this script."
}

& $dotnet.Source run --project $project
exit $LASTEXITCODE
