[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$forbiddenScriptPattern = 'run-stage(?:8|9|10|11|12|13|14|15|16|17|18|18-5|19|19-5|20|21|21-5|22|23)-medium-checks(?:\.ps1)?'
$forbiddenTextPattern = 'medium validation as the immediate dependency'
$failures = @()

foreach ($stage in 9..18) {
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
    $requiredCurrentUnity = "run-unity-stage$stage-validation.ps1"
    if ($stage -eq 17) {
        $requiredPriorUnity = 'ProjectAegisRTS.UnityClient.EditorTools.Stage16_5BuildFlowValidator.ValidateBuildFlowBatch'
        if ($content -notmatch [regex]::Escape($requiredPriorUnity)) {
            $failures += "$scriptName does not call direct Stage 16.5 build-flow validation: $requiredPriorUnity"
        }
    } else {
        $previousStage = $stage - 1
        $requiredPriorUnity = "run-unity-stage$previousStage-validation.ps1"
        if ($content -notmatch [regex]::Escape($requiredPriorUnity)) {
            $failures += "$scriptName does not call direct prior Unity validation: $requiredPriorUnity"
        }
    }

    if ($content -notmatch [regex]::Escape($requiredCurrentUnity)) {
        $failures += "$scriptName does not call direct current Unity validation: $requiredCurrentUnity"
    }
}

$stage18_5ScriptName = 'run-stage18-5-medium-checks.ps1'
$stage18_5ScriptPath = Join-Path $repoRoot "tools\$stage18_5ScriptName"
if (-not (Test-Path -LiteralPath $stage18_5ScriptPath)) {
    $failures += "Missing medium validation script: $stage18_5ScriptPath"
} else {
    $lines = Get-Content -LiteralPath $stage18_5ScriptPath
    for ($lineNumber = 0; $lineNumber -lt $lines.Count; $lineNumber++) {
        $line = $lines[$lineNumber]
        $trimmed = $line.Trim()

        if ($line -match $forbiddenScriptPattern) {
            $ownNameInComment = $line -match [regex]::Escape($stage18_5ScriptName) -and $trimmed.StartsWith('#')
            if (-not $ownNameInComment) {
                $failures += "${stage18_5ScriptName}:$($lineNumber + 1) contains forbidden medium dependency text: $trimmed"
            }
        }

        if ($line -match $forbiddenTextPattern) {
            $failures += "${stage18_5ScriptName}:$($lineNumber + 1) contains old medium dependency wording: $trimmed"
        }
    }

    $content = $lines -join "`n"
    if ($content -notmatch [regex]::Escape('run-unity-stage18-validation.ps1')) {
        $failures += "$stage18_5ScriptName does not call direct Stage 18 Unity validation."
    }
    if ($content -notmatch [regex]::Escape('run-unity-stage18-5-validation.ps1')) {
        $failures += "$stage18_5ScriptName does not call direct Stage 18.5 Unity validation."
    }
}

$stage19ScriptName = 'run-stage19-medium-checks.ps1'
$stage19ScriptPath = Join-Path $repoRoot "tools\$stage19ScriptName"
if (-not (Test-Path -LiteralPath $stage19ScriptPath)) {
    $failures += "Missing medium validation script: $stage19ScriptPath"
} else {
    $lines = Get-Content -LiteralPath $stage19ScriptPath
    for ($lineNumber = 0; $lineNumber -lt $lines.Count; $lineNumber++) {
        $line = $lines[$lineNumber]
        $trimmed = $line.Trim()

        if ($line -match $forbiddenScriptPattern) {
            $ownNameInComment = $line -match [regex]::Escape($stage19ScriptName) -and $trimmed.StartsWith('#')
            if (-not $ownNameInComment) {
                $failures += "${stage19ScriptName}:$($lineNumber + 1) contains forbidden medium dependency text: $trimmed"
            }
        }

        if ($line -match $forbiddenTextPattern) {
            $failures += "${stage19ScriptName}:$($lineNumber + 1) contains old medium dependency wording: $trimmed"
        }
    }

    $content = $lines -join "`n"
    if ($content -notmatch [regex]::Escape('run-unity-stage18-5-validation.ps1')) {
        $failures += "$stage19ScriptName does not call direct Stage 18.5 Unity validation."
    }
    if ($content -notmatch [regex]::Escape('run-stage18-5-player-facing-checks.ps1')) {
        $failures += "$stage19ScriptName does not call direct Stage 18.5 player-facing validation."
    }
    if ($content -notmatch [regex]::Escape('-SkipStage18Dependency')) {
        $failures += "$stage19ScriptName does not skip the older Stage 18 player-facing dependency for its targeted Stage 18.5 player-facing check."
    }
    if ($content -notmatch [regex]::Escape('run-unity-stage19-validation.ps1')) {
        $failures += "$stage19ScriptName does not call direct Stage 19 Unity validation."
    }
}

$stage19_5ScriptName = 'run-stage19-5-medium-checks.ps1'
$stage19_5ScriptPath = Join-Path $repoRoot "tools\$stage19_5ScriptName"
if (-not (Test-Path -LiteralPath $stage19_5ScriptPath)) {
    $failures += "Missing medium validation script: $stage19_5ScriptPath"
} else {
    $lines = Get-Content -LiteralPath $stage19_5ScriptPath
    for ($lineNumber = 0; $lineNumber -lt $lines.Count; $lineNumber++) {
        $line = $lines[$lineNumber]
        $trimmed = $line.Trim()

        if ($line -match $forbiddenScriptPattern) {
            $ownNameInComment = $line -match [regex]::Escape($stage19_5ScriptName) -and $trimmed.StartsWith('#')
            if (-not $ownNameInComment) {
                $failures += "${stage19_5ScriptName}:$($lineNumber + 1) contains forbidden medium dependency text: $trimmed"
            }
        }

        if ($line -match $forbiddenTextPattern) {
            $failures += "${stage19_5ScriptName}:$($lineNumber + 1) contains old medium dependency wording: $trimmed"
        }
    }

    $content = $lines -join "`n"
    if ($content -notmatch [regex]::Escape('run-unity-stage19-validation.ps1')) {
        $failures += "$stage19_5ScriptName does not call direct Stage 19 Unity validation."
    }
    if ($content -notmatch [regex]::Escape('run-stage19-player-facing-checks.ps1')) {
        $failures += "$stage19_5ScriptName does not call direct Stage 19 player-facing validation."
    }
    if ($content -notmatch [regex]::Escape('-SkipStage19Validation')) {
        $failures += "$stage19_5ScriptName does not skip the older Stage 19 dependency for its targeted Stage 19.5 player-facing check."
    }
    if ($content -notmatch [regex]::Escape('run-unity-stage19-5-validation.ps1')) {
        $failures += "$stage19_5ScriptName does not call direct Stage 19.5 Unity validation."
    }
}

$stage20ScriptName = 'run-stage20-medium-checks.ps1'
$stage20ScriptPath = Join-Path $repoRoot "tools\$stage20ScriptName"
if (-not (Test-Path -LiteralPath $stage20ScriptPath)) {
    $failures += "Missing medium validation script: $stage20ScriptPath"
} else {
    $lines = Get-Content -LiteralPath $stage20ScriptPath
    for ($lineNumber = 0; $lineNumber -lt $lines.Count; $lineNumber++) {
        $line = $lines[$lineNumber]
        $trimmed = $line.Trim()

        if ($line -match $forbiddenScriptPattern) {
            $ownNameInComment = $line -match [regex]::Escape($stage20ScriptName) -and $trimmed.StartsWith('#')
            if (-not $ownNameInComment) {
                $failures += "${stage20ScriptName}:$($lineNumber + 1) contains forbidden medium dependency text: $trimmed"
            }
        }

        if ($line -match $forbiddenTextPattern) {
            $failures += "${stage20ScriptName}:$($lineNumber + 1) contains old medium dependency wording: $trimmed"
        }
    }

    $content = $lines -join "`n"
    if ($content -notmatch [regex]::Escape('run-unity-stage19-5-validation.ps1')) {
        $failures += "$stage20ScriptName does not call direct Stage 19.5 Unity validation."
    }
    if ($content -notmatch [regex]::Escape('run-stage19-5-player-facing-checks.ps1')) {
        $failures += "$stage20ScriptName does not call direct Stage 19.5 player-facing validation."
    }
    if ($content -notmatch [regex]::Escape('run-unity-stage20-validation.ps1')) {
        $failures += "$stage20ScriptName does not call direct Stage 20 Unity validation."
    }
    if ($content -notmatch [regex]::Escape('run-stage20-player-facing-checks.ps1')) {
        $failures += "$stage20ScriptName does not call direct Stage 20 player-facing validation."
    }
}

$stage21ScriptName = 'run-stage21-medium-checks.ps1'
$stage21ScriptPath = Join-Path $repoRoot "tools\$stage21ScriptName"
if (-not (Test-Path -LiteralPath $stage21ScriptPath)) {
    $failures += "Missing medium validation script: $stage21ScriptPath"
} else {
    $lines = Get-Content -LiteralPath $stage21ScriptPath
    for ($lineNumber = 0; $lineNumber -lt $lines.Count; $lineNumber++) {
        $line = $lines[$lineNumber]
        $trimmed = $line.Trim()

        if ($line -match $forbiddenScriptPattern) {
            $ownNameInComment = $line -match [regex]::Escape($stage21ScriptName) -and $trimmed.StartsWith('#')
            if (-not $ownNameInComment) {
                $failures += "${stage21ScriptName}:$($lineNumber + 1) contains forbidden medium dependency text: $trimmed"
            }
        }

        if ($line -match $forbiddenTextPattern) {
            $failures += "${stage21ScriptName}:$($lineNumber + 1) contains old medium dependency wording: $trimmed"
        }
    }

    $content = $lines -join "`n"
    if ($content -notmatch [regex]::Escape('run-unity-stage20-validation.ps1')) {
        $failures += "$stage21ScriptName does not call direct Stage 20 Unity validation."
    }
    if ($content -notmatch [regex]::Escape('run-stage20-player-facing-checks.ps1')) {
        $failures += "$stage21ScriptName does not call direct Stage 20 player-facing validation."
    }
    if ($content -notmatch [regex]::Escape('run-unity-stage21-validation.ps1')) {
        $failures += "$stage21ScriptName does not call direct Stage 21 Unity validation."
    }
    if ($content -notmatch [regex]::Escape('run-stage21-player-facing-checks.ps1')) {
        $failures += "$stage21ScriptName does not call direct Stage 21 player-facing validation."
    }
}

$stage21_5ScriptName = 'run-stage21-5-medium-checks.ps1'
$stage21_5ScriptPath = Join-Path $repoRoot "tools\$stage21_5ScriptName"
if (-not (Test-Path -LiteralPath $stage21_5ScriptPath)) {
    $failures += "Missing medium validation script: $stage21_5ScriptPath"
} else {
    $lines = Get-Content -LiteralPath $stage21_5ScriptPath
    for ($lineNumber = 0; $lineNumber -lt $lines.Count; $lineNumber++) {
        $line = $lines[$lineNumber]
        $trimmed = $line.Trim()

        if ($line -match $forbiddenScriptPattern) {
            $ownNameInComment = $line -match [regex]::Escape($stage21_5ScriptName) -and $trimmed.StartsWith('#')
            if (-not $ownNameInComment) {
                $failures += "${stage21_5ScriptName}:$($lineNumber + 1) contains forbidden medium dependency text: $trimmed"
            }
        }

        if ($line -match $forbiddenTextPattern) {
            $failures += "${stage21_5ScriptName}:$($lineNumber + 1) contains old medium dependency wording: $trimmed"
        }
    }

    $content = $lines -join "`n"
    if ($content -notmatch [regex]::Escape('run-unity-stage21-validation.ps1')) {
        $failures += "$stage21_5ScriptName does not call direct Stage 21 Unity validation."
    }
    if ($content -notmatch [regex]::Escape('run-stage21-player-facing-checks.ps1')) {
        $failures += "$stage21_5ScriptName does not call direct Stage 21 player-facing validation."
    }
    if ($content -notmatch [regex]::Escape('run-unity-stage21-5-validation.ps1')) {
        $failures += "$stage21_5ScriptName does not call direct Stage 21.5 Unity validation."
    }
    if ($content -notmatch [regex]::Escape('run-stage21-5-player-facing-checks.ps1')) {
        $failures += "$stage21_5ScriptName does not call direct Stage 21.5 player-facing validation."
    }
}

$stage22ScriptName = 'run-stage22-medium-checks.ps1'
$stage22ScriptPath = Join-Path $repoRoot "tools\$stage22ScriptName"
if (-not (Test-Path -LiteralPath $stage22ScriptPath)) {
    $failures += "Missing medium validation script: $stage22ScriptPath"
} else {
    $lines = Get-Content -LiteralPath $stage22ScriptPath
    for ($lineNumber = 0; $lineNumber -lt $lines.Count; $lineNumber++) {
        $line = $lines[$lineNumber]
        $trimmed = $line.Trim()

        if ($line -match $forbiddenScriptPattern) {
            $ownNameInComment = $line -match [regex]::Escape($stage22ScriptName) -and $trimmed.StartsWith('#')
            if (-not $ownNameInComment) {
                $failures += "${stage22ScriptName}:$($lineNumber + 1) contains forbidden medium dependency text: $trimmed"
            }
        }

        if ($line -match $forbiddenTextPattern) {
            $failures += "${stage22ScriptName}:$($lineNumber + 1) contains old medium dependency wording: $trimmed"
        }
    }

    $content = $lines -join "`n"
    if ($content -notmatch [regex]::Escape('run-unity-stage21-5-validation.ps1')) {
        $failures += "$stage22ScriptName does not call direct Stage 21.5 Unity validation."
    }
    if ($content -notmatch [regex]::Escape('run-stage21-5-player-facing-checks.ps1')) {
        $failures += "$stage22ScriptName does not call direct Stage 21.5 player-facing validation."
    }
    if ($content -notmatch [regex]::Escape('run-unity-stage22-validation.ps1')) {
        $failures += "$stage22ScriptName does not call direct Stage 22 Unity validation."
    }
    if ($content -notmatch [regex]::Escape('run-stage22-player-facing-checks.ps1')) {
        $failures += "$stage22ScriptName does not call direct Stage 22 player-facing validation."
    }
}

$stage23ScriptName = 'run-stage23-medium-checks.ps1'
$stage23ScriptPath = Join-Path $repoRoot "tools\$stage23ScriptName"
if (-not (Test-Path -LiteralPath $stage23ScriptPath)) {
    $failures += "Missing medium validation script: $stage23ScriptPath"
} else {
    $lines = Get-Content -LiteralPath $stage23ScriptPath
    for ($lineNumber = 0; $lineNumber -lt $lines.Count; $lineNumber++) {
        $line = $lines[$lineNumber]
        $trimmed = $line.Trim()

        if ($line -match $forbiddenScriptPattern) {
            $ownNameInComment = $line -match [regex]::Escape($stage23ScriptName) -and $trimmed.StartsWith('#')
            if (-not $ownNameInComment) {
                $failures += "${stage23ScriptName}:$($lineNumber + 1) contains forbidden medium dependency text: $trimmed"
            }
        }

        if ($line -match $forbiddenTextPattern) {
            $failures += "${stage23ScriptName}:$($lineNumber + 1) contains old medium dependency wording: $trimmed"
        }
    }

    $content = $lines -join "`n"
    if ($content -notmatch [regex]::Escape('run-unity-stage22-validation.ps1')) {
        $failures += "$stage23ScriptName does not call direct Stage 22 Unity validation."
    }
    if ($content -notmatch [regex]::Escape('run-stage22-player-facing-checks.ps1')) {
        $failures += "$stage23ScriptName does not call direct Stage 22 player-facing validation."
    }
    if ($content -notmatch [regex]::Escape('run-unity-stage23-validation.ps1')) {
        $failures += "$stage23ScriptName does not call direct Stage 23 Unity validation."
    }
    if ($content -notmatch [regex]::Escape('run-stage23-player-facing-checks.ps1')) {
        $failures += "$stage23ScriptName does not call direct Stage 23 player-facing validation."
    }
}

if ($failures.Count -gt 0) {
    Write-Error "Medium validation recursion audit failed:`n$($failures -join "`n")"
    exit 1
}

Write-Host 'Medium validation recursion audit passed: Stage 9-23 medium scripts use direct Unity validation dependencies only.'
$global:LASTEXITCODE = 0
