[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$forbiddenScriptPattern = 'run-stage(?:8|9|10|11|12|13|14|15)-medium-checks(?:\.ps1)?'
$forbiddenTextPattern = 'medium validation as the immediate dependency'
$failures = @()

foreach ($stage in 9..15) {
    $scriptName = "run-stage$stage-medium-checks.ps1"
    $scriptPath = Join-Path $repoRoot "tools\$scriptName"
    if (-not (Test-Path -LiteralPath $scriptPath)) {
        $failures += "Missing medium validation script: $scriptPath"
        continue
    }

    $lines = Get-Content -LiteralPath $scriptPath
    for ($lineNumber = 0; $lineNumber -lt $lines.Count; $lineNumber++) {
        $line = $lines[$lineNumber]
        $trimmed = $line.Trim()

        if ($line -match $forbiddenScriptPattern) {
            $ownNameInComment = $line -match [regex]::Escape($scriptName) -and $trimmed.StartsWith('#')
            if (-not $ownNameInComment) {
                $failures += "${scriptName}:$($lineNumber + 1) contains forbidden medium dependency text: $trimmed"
            }
        }

        if ($line -match $forbiddenTextPattern) {
            $failures += "${scriptName}:$($lineNumber + 1) contains old medium dependency wording: $trimmed"
        }
    }

    $content = $lines -join "`n"
    $previousStage = $stage - 1
    $requiredPriorUnity = "run-unity-stage$previousStage-validation.ps1"
    $requiredCurrentUnity = "run-unity-stage$stage-validation.ps1"
    if ($content -notmatch [regex]::Escape($requiredPriorUnity)) {
        $failures += "$scriptName does not call direct prior Unity validation: $requiredPriorUnity"
    }
    if ($content -notmatch [regex]::Escape($requiredCurrentUnity)) {
        $failures += "$scriptName does not call direct current Unity validation: $requiredCurrentUnity"
    }
}

if ($failures.Count -gt 0) {
    Write-Error "Medium validation recursion audit failed:`n$($failures -join "`n")"
    exit 1
}

Write-Host 'Medium validation recursion audit passed: Stage 9-15 medium scripts use direct Unity validation dependencies only.'
