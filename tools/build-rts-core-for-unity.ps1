[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug'
)

$ErrorActionPreference = 'Stop'

function Find-DotNet {
    $command = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($command) {
        return $command.Source
    }

    $defaultPath = 'C:\Program Files\dotnet\dotnet.exe'
    if (Test-Path -LiteralPath $defaultPath) {
        return $defaultPath
    }

    throw 'dotnet was not found on PATH or at C:\Program Files\dotnet\dotnet.exe.'
}

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$dotnet = Find-DotNet
$coreProject = Join-Path $repoRoot 'src\Rts.Core\Rts.Core.csproj'
$outputDir = Join-Path $repoRoot 'unity\Assets\Rts\Plugins\RtsCore'
$sourceDir = Join-Path $repoRoot "src\Rts.Core\bin\$Configuration\netstandard2.1"
$sourceDll = Join-Path $sourceDir 'Rts.Core.dll'
$sourcePdb = Join-Path $sourceDir 'Rts.Core.pdb'

Write-Host "Building Rts.Core for Unity with $dotnet"
& $dotnet build $coreProject -c $Configuration
if ($LASTEXITCODE -ne 0) {
    throw "dotnet build failed with exit code $LASTEXITCODE."
}

if (-not (Test-Path -LiteralPath $sourceDll)) {
    throw "Expected build output was not found: $sourceDll"
}

New-Item -ItemType Directory -Force -Path $outputDir | Out-Null
Copy-Item -LiteralPath $sourceDll -Destination (Join-Path $outputDir 'Rts.Core.dll') -Force

if (Test-Path -LiteralPath $sourcePdb) {
    Copy-Item -LiteralPath $sourcePdb -Destination (Join-Path $outputDir 'Rts.Core.pdb') -Force
}

Write-Host "Copied Rts.Core.dll to $outputDir"
