$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot 'common-validation.ps1')

$repoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..")).Path
$project = Join-Path $repoRoot "src/Rts.Core.Tests"

$dotnet = Find-DotNet

Invoke-DotNetRunNoRestore -DotNetPath $dotnet -ProjectPath $project
exit $LASTEXITCODE
