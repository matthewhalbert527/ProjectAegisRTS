[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateRange(1, 17)]
    [int]$Stage
)

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

function Invoke-ValidationTool {
    param(
        [string]$RepoRoot,
        [string]$RelativePath
    )

    $scriptPath = Join-Path $RepoRoot $RelativePath
    if (-not (Test-Path -LiteralPath $scriptPath)) {
        throw "Validation script was not found: $scriptPath"
    }

    $global:LASTEXITCODE = 0
    & $scriptPath
    if ($LASTEXITCODE -ne 0) {
        throw "$RelativePath failed with exit code $LASTEXITCODE."
    }
}

function Get-StageValidationPlan {
    param([int]$HighestStage)

    $plan = @()
    for ($stageNumber = 1; $stageNumber -le $HighestStage; $stageNumber++) {
        $plan += [pscustomobject]@{
            Title = "Stage $stageNumber Unity validation"
            Script = "tools\run-unity-stage$stageNumber-validation.ps1"
        }

        if ($stageNumber -eq 2) {
            $plan += [pscustomobject]@{
                Title = 'Stage 2 Play Mode smoke validation'
                Script = 'tools\run-stage2-playmode-smoke.ps1'
            }
        }
    }

    return $plan
}

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$dotnet = Find-DotNet
$corePath = Join-Path $repoRoot 'src\Rts.Core'

Write-Host "Stage $Stage flattened full acceptance gate: validates Stage 0 through Stage $Stage."
Write-Warning 'This is still the full acceptance gate. It avoids recursive replay, but Unity batchmode validation across many stages can still take a long time.'

Write-ValidationSection 'Rts.Core tests'
Invoke-DotNetRunNoRestore -DotNetPath $dotnet -ProjectPath (Join-Path $repoRoot 'src\Rts.Core.Tests')
if ($LASTEXITCODE -ne 0) {
    throw "Rts.Core.Tests failed with exit code $LASTEXITCODE."
}

Write-ValidationSection 'Build Rts.Core for Unity'
Invoke-ValidationTool -RepoRoot $repoRoot -RelativePath 'tools\build-rts-core-for-unity.ps1'

foreach ($item in Get-StageValidationPlan -HighestStage $Stage) {
    Write-ValidationSection $item.Title
    Invoke-ValidationTool -RepoRoot $repoRoot -RelativePath $item.Script
}

Write-ValidationSection 'Rts.Core UnityEngine-free scan'
Test-RtsCoreUnityEngineFree -CorePath $corePath

Write-ValidationSection 'Whitespace check'
Invoke-GitDiffCheck -RepoRoot $repoRoot

Write-Host "Stage $Stage flattened full acceptance gate passed."
$global:LASTEXITCODE = 0
