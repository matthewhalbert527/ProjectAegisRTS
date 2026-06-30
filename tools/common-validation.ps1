[CmdletBinding()]
param()

function Write-ValidationSection {
    param([string]$Title)

    Write-Host ''
    Write-Host "== $Title =="
}

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

function Resolve-DotNetProjectFile {
    param([string]$ProjectPath)

    $resolved = (Resolve-Path -LiteralPath $ProjectPath).Path
    if (Test-Path -LiteralPath $resolved -PathType Leaf) {
        return $resolved
    }

    $project = Get-ChildItem -LiteralPath $resolved -Filter '*.csproj' -File | Select-Object -First 1
    if (-not $project) {
        throw "No .csproj file was found under $resolved"
    }

    return $project.FullName
}

function Invoke-DotNetRestoreIfNeeded {
    param(
        [string]$DotNetPath,
        [string]$ProjectPath
    )

    $projectFile = Resolve-DotNetProjectFile -ProjectPath $ProjectPath
    $assetsFile = Join-Path (Split-Path -Parent $projectFile) 'obj\project.assets.json'

    if (Test-Path -LiteralPath $assetsFile) {
        Write-Host "NuGet assets present for $projectFile; using --no-restore."
        return
    }

    Write-Host "NuGet assets missing for $projectFile; running dotnet restore once."
    & $DotNetPath restore $projectFile
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet restore failed with exit code $LASTEXITCODE for $projectFile."
    }
}

function Invoke-DotNetRunNoRestore {
    param(
        [string]$DotNetPath,
        [string]$ProjectPath
    )

    Invoke-DotNetRestoreIfNeeded -DotNetPath $DotNetPath -ProjectPath $ProjectPath
    & $DotNetPath run --no-restore --project $ProjectPath
}

function Invoke-DotNetBuildNoRestore {
    param(
        [string]$DotNetPath,
        [string]$ProjectPath,
        [string]$Configuration = 'Debug'
    )

    Invoke-DotNetRestoreIfNeeded -DotNetPath $DotNetPath -ProjectPath $ProjectPath
    & $DotNetPath build $ProjectPath -c $Configuration --no-restore
}

function Invoke-DotNetPublishNoRestore {
    param(
        [string]$DotNetPath,
        [string]$ProjectPath,
        [string]$Configuration,
        [string[]]$ExtraArguments = @()
    )

    Invoke-DotNetRestoreIfNeeded -DotNetPath $DotNetPath -ProjectPath $ProjectPath
    & $DotNetPath publish $ProjectPath --configuration $Configuration --no-restore @ExtraArguments
}

function Find-UnityEditor {
    $patterns = @(
        'E:\Unity\Hub\Editor\*\Editor\Unity.exe',
        'C:\Program Files\Unity\Hub\Editor\*\Editor\Unity.exe',
        'C:\Program Files\Unity\Editor\Unity.exe',
        'C:\Program Files (x86)\Unity\Editor\Unity.exe'
    )

    $editors = @()
    foreach ($pattern in $patterns) {
        $editors += Get-ChildItem -Path $pattern -ErrorAction SilentlyContinue
    }

    if ($editors.Count -gt 0) {
        return ($editors | Sort-Object FullName -Descending | Select-Object -First 1).FullName
    }

    $command = Get-Command Unity -ErrorAction SilentlyContinue
    if ($command) {
        return $command.Source
    }

    return $null
}

function Find-UnityEngineReferencesWithPowerShell {
    param([string]$CorePath)

    $referenceHits = Get-ChildItem $CorePath -Recurse -Include *.cs |
        Select-String -Pattern 'UnityEngine'
    return @($referenceHits)
}

function Find-UnityEngineReferences {
    param([string]$CorePath)

    $rg = Get-Command rg -ErrorAction SilentlyContinue
    if ($rg) {
        try {
            $rgHits = & $rg.Source -n 'UnityEngine' $CorePath
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

function Test-RtsCoreUnityEngineFree {
    param([string]$CorePath)

    Write-Host 'Checking Rts.Core for UnityEngine references.'
    $unityEngineHits = Find-UnityEngineReferences -CorePath $CorePath
    if ($unityEngineHits.Count -gt 0) {
        $unityEngineHits | ForEach-Object { Write-Host $_ }
        throw 'Rts.Core must remain UnityEngine-free.'
    }

    Write-Host 'Rts.Core UnityEngine reference check passed; no UnityEngine references found.'
}

function Invoke-GitDiffCheck {
    param([string]$RepoRoot)

    Write-Host 'Running git diff whitespace check.'
    git -C $RepoRoot diff --check
    if ($LASTEXITCODE -ne 0) {
        throw "git diff --check failed with exit code $LASTEXITCODE."
    }
}

function Remove-TrailingWhitespace {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        return
    }

    $resolved = (Resolve-Path -LiteralPath $Path).Path
    $text = [System.IO.File]::ReadAllText($resolved)
    if ($text.Length -eq 0) {
        return
    }

    $newline = "`n"
    if ($text.Contains("`r`n")) {
        $newline = "`r`n"
    }

    $normalizedText = $text -replace "`r`n", "`n"
    $normalizedText = $normalizedText -replace "`r", "`n"
    $lines = $normalizedText -split "`n", -1
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
        [System.IO.File]::WriteAllText($resolved, ($lines -join $newline), $utf8NoBom)
    }
}

function Normalize-WhitespaceInTree {
    param(
        [string]$Path,
        [string[]]$Include
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        return
    }

    Get-ChildItem -LiteralPath $Path -Recurse -File -Include $Include |
        ForEach-Object { Remove-TrailingWhitespace -Path $_.FullName }
}
