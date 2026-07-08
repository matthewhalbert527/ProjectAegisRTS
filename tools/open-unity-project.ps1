[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

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

function Find-UnityHub {
    $paths = @(
        'C:\Program Files\Unity Hub\Unity Hub.exe',
        "$env:LOCALAPPDATA\Programs\Unity Hub\Unity Hub.exe"
    )

    foreach ($path in $paths) {
        if (Test-Path -LiteralPath $path) {
            return $path
        }
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
$unityProject = Join-Path $repoRoot 'unity'
$unityEditor = Find-UnityEditor

$openUnity = @(Get-UnityProcessesForProject -ProjectPath $unityProject |
    Where-Object {
        $_.CommandLine -notmatch 'AssetImportWorker' -and
        $_.CommandLine -notmatch '-batchmode'
    })
if ($openUnity.Count -gt 0) {
    Write-Host "Unity project is already open:"
    Write-Host $unityProject
    return
}

if ($unityEditor) {
    Write-Host "Opening Unity project with $unityEditor"
    Start-Process -FilePath $unityEditor -ArgumentList "-projectPath `"$unityProject`""
    return
}

$unityHub = Find-UnityHub
if ($unityHub) {
    Write-Host "Unity Editor was not found, but Unity Hub is installed at:"
    Write-Host $unityHub
    Write-Host "Open Unity Hub, choose Add/Open project, and select:"
    Write-Host $unityProject
    return
}

Write-Host "Unity Editor was not found."
Write-Host "Install or locate Unity through Unity Hub, then open this project folder:"
Write-Host $unityProject
