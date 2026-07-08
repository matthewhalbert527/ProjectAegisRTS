[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

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

function Find-UnityEngineReferences {
    param([string]$CorePath)

    $rg = Get-Command rg -ErrorAction SilentlyContinue
    if ($rg) {
        try {
            $rgHits = & $rg.Source -n "UnityEngine" $CorePath
        } catch {
            Write-Warning "rg was found at $($rg.Source) but could not be executed; using built-in PowerShell Select-String fallback."
            return Find-UnityEngineReferencesWithPowerShell -CorePath $CorePath
        }

        if ($LASTEXITCODE -eq 0) {
            return @($rgHits)
        }
        if ($LASTEXITCODE -gt 1) {
            throw "rg failed while checking UnityEngine references with exit code $LASTEXITCODE."
        }

        return @()
    }

    Write-Host 'rg not found; using built-in PowerShell Select-String fallback.'
    return Find-UnityEngineReferencesWithPowerShell -CorePath $CorePath
}

function Find-UnityEngineReferencesWithPowerShell {
    param([string]$CorePath)

    $referenceHits = Get-ChildItem $CorePath -Recurse -Include *.cs |
        Select-String -Pattern "UnityEngine"
    return @($referenceHits)
}

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$dotnet = Find-DotNet
$corePath = Join-Path $repoRoot 'src\Rts.Core'

Write-Host 'Stage 8 full acceptance gate: validates Stage 0 through Stage 8.'
Write-Warning 'This is the slow full acceptance gate and can take a long time. Use run-stage8-fast-checks.ps1 or run-stage8-medium-checks.ps1 for normal iteration.'

Write-Host 'Running Rts.Core tests.'
Invoke-DotNetRunNoRestore -DotNetPath $dotnet -ProjectPath (Join-Path $repoRoot 'src\Rts.Core.Tests')
if ($LASTEXITCODE -ne 0) {
    throw "Rts.Core.Tests failed with exit code $LASTEXITCODE."
}

Write-Host 'Building and copying Rts.Core.dll for Unity.'
& (Join-Path $repoRoot 'tools\build-rts-core-for-unity.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "build-rts-core-for-unity.ps1 failed with exit code $LASTEXITCODE."
}

$checks = @(
    'run-stage1-checks.ps1',
    'run-stage2-checks.ps1',
    'run-stage2-playmode-smoke.ps1',
    'run-stage3-checks.ps1',
    'run-stage4-checks.ps1',
    'run-stage5-checks.ps1',
    'run-stage6-checks.ps1',
    'run-stage7-checks.ps1',
    'run-unity-stage8-validation.ps1'
)

foreach ($check in $checks) {
    Write-Host "Running $check."
    & (Join-Path $repoRoot "tools\$check")
    if ($LASTEXITCODE -ne 0) {
        throw "$check failed with exit code $LASTEXITCODE."
    }
}

Write-Host 'Checking Rts.Core for UnityEngine references.'
$unityEngineHits = Find-UnityEngineReferences -CorePath $corePath
if ($unityEngineHits.Count -gt 0) {
    $unityEngineHits | ForEach-Object { Write-Host $_ }
    throw 'Rts.Core must remain UnityEngine-free.'
}
Write-Host 'Rts.Core UnityEngine reference check passed; no UnityEngine references found.'

Write-Host 'Running git diff whitespace check.'
git -C $repoRoot diff --check
if ($LASTEXITCODE -ne 0) {
    throw "git diff --check failed with exit code $LASTEXITCODE."
}

Write-Host 'Stage 8 checks passed.'
