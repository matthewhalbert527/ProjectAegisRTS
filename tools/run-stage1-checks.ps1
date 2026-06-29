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

function Get-UnityProcessesForProject {
    param([string]$ProjectPath)

    $slashPath = $ProjectPath -replace '\\', '/'
    Get-CimInstance Win32_Process -Filter "name = 'Unity.exe'" |
        Where-Object {
            $_.CommandLine -and
            $_.CommandLine -match '-projectPath' -and
            ($_.CommandLine.Contains($ProjectPath) -or $_.CommandLine.Contains($slashPath))
        }
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

$openUnity = @(Get-UnityProcessesForProject -ProjectPath $unityProject | Where-Object { $_.CommandLine -notmatch 'AssetImportWorker' })
if ($openUnity.Count -gt 0) {
    Write-Warning 'Unity Editor is already open for this project, so batchmode cannot safely take the project lock.'
    Write-Host 'Running live Stage 1 Unity validation instead.'
    $validationScript = Join-Path $repoRoot 'tools\run-unity-stage1-validation.ps1'
    & $validationScript
    if ($LASTEXITCODE -ne 0) {
        throw "run-unity-stage1-validation.ps1 failed with exit code $LASTEXITCODE."
    }
    exit 0
}

New-Item -ItemType Directory -Force -Path (Split-Path -Parent $unityLog) | Out-Null
if (Test-Path -LiteralPath $unityLog) {
    Remove-Item -LiteralPath $unityLog -Force
}

Write-Host "Running Unity batchmode scene creation and compile check with $unityEditor"
$arguments = "-batchmode -quit -projectPath `"$unityProject`" -executeMethod ProjectAegisRTS.UnityClient.EditorTools.Stage1SceneCreator.CreateStage1SceneBatch -logFile `"$unityLog`""
Start-Process -FilePath $unityEditor -ArgumentList $arguments -WindowStyle Hidden | Out-Null

$deadline = (Get-Date).AddMinutes(5)
$sawCompletion = $false
do {
    Start-Sleep -Seconds 2

    if (Test-Path -LiteralPath $unityLog) {
        $compilerErrorDuringRun = Select-String -LiteralPath $unityLog -Pattern 'Scripts have compiler errors', 'error CS', 'Script Compilation Error' -Quiet
        if ($compilerErrorDuringRun) {
            break
        }

        $sawCompletion = Select-String -LiteralPath $unityLog -Pattern 'Created Stage 1 scene at Assets/Rts/Scenes/Stage1_DesktopBoard.unity' -Quiet
        if ($sawCompletion) {
            break
        }
    }
} while ((Get-Date) -lt $deadline)

if (-not (Test-Path -LiteralPath $unityLog)) {
    throw "Unity batchmode did not create a log within the timeout. Expected log: $unityLog"
}

$compilerError = Select-String -LiteralPath $unityLog -Pattern 'Scripts have compiler errors', 'error CS', 'Script Compilation Error' -Quiet
if ($compilerError) {
    throw "Unity batchmode logged compiler errors. See $unityLog"
}

if (-not $sawCompletion) {
    throw "Unity batchmode did not log Stage 1 scene creation before the timeout. See $unityLog"
}

$stage1Scene = Join-Path $unityProject 'Assets\Rts\Scenes\Stage1_DesktopBoard.unity'
if (-not (Test-Path -LiteralPath $stage1Scene)) {
    throw "Unity batchmode completed but did not create the Stage 1 scene: $stage1Scene. See $unityLog"
}

Write-Host "Unity batchmode completed successfully. Log: $unityLog"
