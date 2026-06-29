[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

function Require-File {
    param(
        [string]$Path,
        [string]$Description
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        throw "$Description was not found: $Path"
    }
}

function Require-SceneText {
    param(
        [string[]]$Lines,
        [string]$Pattern,
        [string]$Description
    )

    if (-not ($Lines | Select-String -Pattern $Pattern -Quiet)) {
        throw "$Description was not found in Stage1_DesktopBoard.unity."
    }
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
$scenePath = Join-Path $unityProject 'Assets\Rts\Scenes\Stage1_DesktopBoard.unity'
$coreDll = Join-Path $unityProject 'Assets\Rts\Plugins\RtsCore\Rts.Core.dll'
$editorLog = Join-Path $unityProject 'Logs\Editor.log'

Write-Host 'Validating Stage 1 Unity project files.'
Require-File -Path $scenePath -Description 'Stage 1 scene'
Require-File -Path $coreDll -Description 'Unity Rts.Core plugin DLL'

$sceneLines = Get-Content -LiteralPath $scenePath
Require-SceneText -Lines $sceneLines -Pattern 'm_Name: BoardRoot' -Description 'BoardRoot'
Require-SceneText -Lines $sceneLines -Pattern 'm_Name: RtsGame' -Description 'RtsGame'
Require-SceneText -Lines $sceneLines -Pattern 'm_Name: Main Camera' -Description 'Main Camera'
Require-SceneText -Lines $sceneLines -Pattern 'm_Name: Directional Light' -Description 'Directional Light'
Require-SceneText -Lines $sceneLines -Pattern 'Assembly-CSharp::ProjectAegisRTS\.UnityClient\.Bootstrap\.RtsGameBootstrapper' -Description 'RtsGameBootstrapper component'
Require-SceneText -Lines $sceneLines -Pattern 'Assembly-CSharp::ProjectAegisRTS\.UnityClient\.Rendering\.BoardRenderer' -Description 'BoardRenderer component'
Require-SceneText -Lines $sceneLines -Pattern 'Assembly-CSharp::ProjectAegisRTS\.UnityClient\.CoreBridge\.BoardCoordinateMapper' -Description 'BoardCoordinateMapper component'
Require-SceneText -Lines $sceneLines -Pattern 'Assembly-CSharp::ProjectAegisRTS\.UnityClient\.Rendering\.ActorRenderSystem' -Description 'ActorRenderSystem component'
Require-SceneText -Lines $sceneLines -Pattern 'Assembly-CSharp::ProjectAegisRTS\.UnityClient\.UI\.RtsDebugHud' -Description 'RtsDebugHud component'
Require-SceneText -Lines $sceneLines -Pattern 'orthographic: 1' -Description 'orthographic camera setting'
Require-SceneText -Lines $sceneLines -Pattern 'orthographic size: 28' -Description 'orthographic camera size'
Require-SceneText -Lines $sceneLines -Pattern 'near clip plane: 0\.1' -Description 'camera near clip plane'
Require-SceneText -Lines $sceneLines -Pattern 'far clip plane: 1000' -Description 'camera far clip plane'
Require-SceneText -Lines $sceneLines -Pattern 'm_LocalPosition: \{x: 16, y: 38, z: -26\}' -Description 'camera framing position'

Write-Host 'Scene file validation passed.'

$unityProcesses = @(Get-UnityProcessesForProject -ProjectPath $unityProject)
if ($unityProcesses.Count -gt 0) {
    Write-Host "Unity process count for this project: $($unityProcesses.Count)"
} else {
    Write-Warning 'Unity does not appear to be open for this project. Open Unity and press Play for the manual interaction checklist.'
}

if (Test-Path -LiteralPath $editorLog) {
    Write-Host 'Checking live Unity Editor log for red-error signatures.'
    $errorPatterns = @(
        'NullReferenceException',
        'MissingMethodException',
        'TypeLoadException',
        'FileNotFoundException',
        'Scripts have compiler errors',
        'Script Compilation Error',
        'error CS[0-9]+',
        'Could not load.*Rts\.Core',
        'Rts\.Core.*not found'
    )

    $matches = Select-String -LiteralPath $editorLog -Pattern $errorPatterns
    if ($matches) {
        $sample = ($matches | Select-Object -First 8 | ForEach-Object { "line $($_.LineNumber): $($_.Line)" }) -join "`n"
        throw "Unity Editor log contains red-error signatures:`n$sample"
    }

    Write-Host 'Unity Editor log validation passed.'
} else {
    Write-Warning "Unity Editor log was not found: $editorLog"
}

Write-Host 'Stage 1 Unity validation passed.'
