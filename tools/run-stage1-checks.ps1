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

function Find-UnityEditor {
    $patterns = @(
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

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$dotnet = Find-DotNet
$unityProject = Join-Path $repoRoot 'unity'
$unityLog = Join-Path $repoRoot 'build\stage1-unity-batchmode.log'

Write-Host 'Running Rts.Core tests.'
& $dotnet run --project (Join-Path $repoRoot 'src\Rts.Core.Tests')
if ($LASTEXITCODE -ne 0) {
    throw "Rts.Core.Tests failed with exit code $LASTEXITCODE."
}

Write-Host 'Building and copying Rts.Core.dll for Unity.'
& (Join-Path $repoRoot 'tools\build-rts-core-for-unity.ps1') -Configuration $Configuration
if ($LASTEXITCODE -ne 0) {
    throw "build-rts-core-for-unity.ps1 failed with exit code $LASTEXITCODE."
}

$unityEditor = Find-UnityEditor
if (-not $unityEditor) {
    Write-Warning 'Unity Editor executable was not found. Unity script compilation and play validation must be checked manually in Unity Hub.'
    exit 0
}

New-Item -ItemType Directory -Force -Path (Split-Path -Parent $unityLog) | Out-Null
Write-Host "Running Unity batchmode scene creation and compile check with $unityEditor"
& $unityEditor -batchmode -quit -projectPath $unityProject -executeMethod ProjectAegisRTS.UnityClient.EditorTools.Stage1SceneCreator.CreateStage1SceneBatch -logFile $unityLog
if ($LASTEXITCODE -ne 0) {
    throw "Unity batchmode failed with exit code $LASTEXITCODE. See $unityLog"
}

Write-Host "Unity batchmode completed successfully. Log: $unityLog"
