[CmdletBinding()]
param()

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

function Find-UnityEngineReferences {
    param([string]$CorePath)

    $rg = Get-Command rg -ErrorAction SilentlyContinue
    if ($rg) {
        $rgHits = & $rg.Source -n "UnityEngine" $CorePath
        if ($LASTEXITCODE -eq 0) {
            return @($rgHits)
        }
        if ($LASTEXITCODE -gt 1) {
            throw "rg failed while checking UnityEngine references with exit code $LASTEXITCODE."
        }

        return @()
    }

    Write-Host 'rg not found; using built-in PowerShell Select-String fallback.'
    $referenceHits = Get-ChildItem $CorePath -Recurse -Include *.cs |
        Select-String -Pattern "UnityEngine"
    return @($referenceHits)
}

function Remove-TrailingWhitespace {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        return
    }

    $resolved = (Resolve-Path -LiteralPath $Path).Path
    $lines = [System.IO.File]::ReadAllLines($resolved)
    $changed = $false
    for ($i = 0; $i -lt $lines.Length; $i++) {
        $trimmed = $lines[$i].TrimEnd()
        if ($trimmed.Length -ne $lines[$i].Length) {
            $changed = $true
            $lines[$i] = $trimmed
        }
    }

    if ($changed) {
        $utf8NoBom = New-Object System.Text.UTF8Encoding -ArgumentList $false
        [System.IO.File]::WriteAllLines($resolved, [string[]]$lines, $utf8NoBom)
    }
}

function Normalize-UnityGeneratedFiles {
    param([string]$RepoRoot)

    $paths = @(
        'unity\Assets\Rts\Scenes\Stage1_DesktopBoard.unity',
        'unity\Assets\Rts\Scenes\Stage2_PCSidebar.unity',
        'unity\Assets\Rts\Scenes\Stage3_XRBoardPlacement.unity',
        'unity\Assets\Rts\Scenes\Stage4_LeftHandBuildSelection.unity',
        'unity\Assets\XR\Settings\OpenXR Package Settings.asset',
        'unity\Assets\XR\Settings\OpenXR Package Settings.asset.meta',
        'unity\Assets\XR\Settings.meta',
        'unity\Assets\XR.meta'
    )

    foreach ($path in $paths) {
        Remove-TrailingWhitespace -Path (Join-Path $RepoRoot $path)
    }
}

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$dotnet = Find-DotNet
$corePath = Join-Path $repoRoot 'src\Rts.Core'

Write-Host 'Running Rts.Core tests.'
& $dotnet run --project (Join-Path $repoRoot 'src\Rts.Core.Tests')
if ($LASTEXITCODE -ne 0) {
    throw "Rts.Core.Tests failed with exit code $LASTEXITCODE."
}

Write-Host 'Building and copying Rts.Core.dll for Unity.'
& (Join-Path $repoRoot 'tools\build-rts-core-for-unity.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "build-rts-core-for-unity.ps1 failed with exit code $LASTEXITCODE."
}

Write-Host 'Running Stage 1 live/file validation.'
& (Join-Path $repoRoot 'tools\run-unity-stage1-validation.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-unity-stage1-validation.ps1 failed with exit code $LASTEXITCODE."
}

Write-Host 'Running Stage 2 Unity validation.'
& (Join-Path $repoRoot 'tools\run-unity-stage2-validation.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-unity-stage2-validation.ps1 failed with exit code $LASTEXITCODE."
}

Write-Host 'Running Stage 2 Play Mode smoke validation.'
& (Join-Path $repoRoot 'tools\run-stage2-playmode-smoke.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-stage2-playmode-smoke.ps1 failed with exit code $LASTEXITCODE."
}

Write-Host 'Running Stage 3 Unity validation.'
& (Join-Path $repoRoot 'tools\run-unity-stage3-validation.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-unity-stage3-validation.ps1 failed with exit code $LASTEXITCODE."
}

Write-Host 'Running Stage 3 checks.'
& (Join-Path $repoRoot 'tools\run-stage3-checks.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-stage3-checks.ps1 failed with exit code $LASTEXITCODE."
}

Write-Host 'Running Stage 4 Unity validation.'
& (Join-Path $repoRoot 'tools\run-unity-stage4-validation.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "run-unity-stage4-validation.ps1 failed with exit code $LASTEXITCODE."
}

Write-Host 'Checking Rts.Core for UnityEngine references.'
$unityEngineHits = Find-UnityEngineReferences -CorePath $corePath
if ($unityEngineHits.Count -gt 0) {
    $unityEngineHits | ForEach-Object { Write-Host $_ }
    throw 'Rts.Core must remain UnityEngine-free.'
}
Write-Host 'Rts.Core UnityEngine reference check passed; no UnityEngine references found.'

Write-Host 'Normalizing Unity-generated whitespace.'
Normalize-UnityGeneratedFiles -RepoRoot $repoRoot

Write-Host 'Running git diff whitespace check.'
git -C $repoRoot diff --check
if ($LASTEXITCODE -ne 0) {
    throw "git diff --check failed with exit code $LASTEXITCODE."
}

Write-Host 'Stage 4 checks passed.'
