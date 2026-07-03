[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$forbiddenScriptPattern = 'run-stage(?:8|9|10|11|12|13|14|15|16|17|18|18-5|19|19-5|20|21|21-5|22|23|24|25|26|27|27-1|28|28-1)-medium-checks(?:\.ps1)?'
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

$stage24ScriptName = 'run-stage24-medium-checks.ps1'
$stage24ScriptPath = Join-Path $repoRoot "tools\$stage24ScriptName"
if (-not (Test-Path -LiteralPath $stage24ScriptPath)) {
    $failures += "Missing medium validation script: $stage24ScriptPath"
} else {
    $lines = Get-Content -LiteralPath $stage24ScriptPath
    for ($lineNumber = 0; $lineNumber -lt $lines.Count; $lineNumber++) {
        $line = $lines[$lineNumber]
        $trimmed = $line.Trim()

        if ($line -match $forbiddenScriptPattern) {
            $ownNameInComment = $line -match [regex]::Escape($stage24ScriptName) -and $trimmed.StartsWith('#')
            if (-not $ownNameInComment) {
                $failures += "${stage24ScriptName}:$($lineNumber + 1) contains forbidden medium dependency text: $trimmed"
            }
        }

        if ($line -match $forbiddenTextPattern) {
            $failures += "${stage24ScriptName}:$($lineNumber + 1) contains old medium dependency wording: $trimmed"
        }
    }

    $content = $lines -join "`n"
    if ($content -notmatch [regex]::Escape('run-unity-stage23-validation.ps1')) {
        $failures += "$stage24ScriptName does not call direct Stage 23 Unity validation."
    }
    if ($content -notmatch [regex]::Escape('run-stage23-player-facing-checks.ps1')) {
        $failures += "$stage24ScriptName does not call direct Stage 23 player-facing validation."
    }
    if ($content -notmatch [regex]::Escape('run-unity-stage24-validation.ps1')) {
        $failures += "$stage24ScriptName does not call direct Stage 24 Unity validation."
    }
    if ($content -notmatch [regex]::Escape('run-stage24-player-facing-checks.ps1')) {
        $failures += "$stage24ScriptName does not call direct Stage 24 player-facing validation."
    }
}

$stage25ScriptName = 'run-stage25-medium-checks.ps1'
$stage25ScriptPath = Join-Path $repoRoot "tools\$stage25ScriptName"
if (-not (Test-Path -LiteralPath $stage25ScriptPath)) {
    $failures += "Missing medium validation script: $stage25ScriptPath"
} else {
    $lines = Get-Content -LiteralPath $stage25ScriptPath
    for ($lineNumber = 0; $lineNumber -lt $lines.Count; $lineNumber++) {
        $line = $lines[$lineNumber]
        $trimmed = $line.Trim()

        if ($line -match $forbiddenScriptPattern) {
            $ownNameInComment = $line -match [regex]::Escape($stage25ScriptName) -and $trimmed.StartsWith('#')
            if (-not $ownNameInComment) {
                $failures += "${stage25ScriptName}:$($lineNumber + 1) contains forbidden medium dependency text: $trimmed"
            }
        }

        if ($line -match $forbiddenTextPattern) {
            $failures += "${stage25ScriptName}:$($lineNumber + 1) contains old medium dependency wording: $trimmed"
        }
    }

    $content = $lines -join "`n"
    if ($content -notmatch [regex]::Escape('run-unity-stage24-validation.ps1')) {
        $failures += "$stage25ScriptName does not call direct Stage 24 Unity validation."
    }
    if ($content -notmatch [regex]::Escape('run-stage24-player-facing-checks.ps1')) {
        $failures += "$stage25ScriptName does not call direct Stage 24 player-facing validation."
    }
    if ($content -notmatch [regex]::Escape('run-unity-stage25-validation.ps1')) {
        $failures += "$stage25ScriptName does not call direct Stage 25 Unity validation."
    }
    if ($content -notmatch [regex]::Escape('run-stage25-player-facing-checks.ps1')) {
        $failures += "$stage25ScriptName does not call direct Stage 25 player-facing validation."
    }
}

$stage26ScriptName = 'run-stage26-medium-checks.ps1'
$stage26ScriptPath = Join-Path $repoRoot "tools\$stage26ScriptName"
if (-not (Test-Path -LiteralPath $stage26ScriptPath)) {
    $failures += "Missing medium validation script: $stage26ScriptPath"
} else {
    $lines = Get-Content -LiteralPath $stage26ScriptPath
    for ($lineNumber = 0; $lineNumber -lt $lines.Count; $lineNumber++) {
        $line = $lines[$lineNumber]
        $trimmed = $line.Trim()

        if ($line -match $forbiddenScriptPattern) {
            $ownNameInComment = $line -match [regex]::Escape($stage26ScriptName) -and $trimmed.StartsWith('#')
            if (-not $ownNameInComment) {
                $failures += "${stage26ScriptName}:$($lineNumber + 1) contains forbidden medium dependency text: $trimmed"
            }
        }

        if ($line -match $forbiddenTextPattern) {
            $failures += "${stage26ScriptName}:$($lineNumber + 1) contains old medium dependency wording: $trimmed"
        }
    }

    $content = $lines -join "`n"
    if ($content -notmatch [regex]::Escape('run-unity-stage25-validation.ps1')) {
        $failures += "$stage26ScriptName does not call direct Stage 25 Unity validation."
    }
    if ($content -notmatch [regex]::Escape('run-stage25-player-facing-checks.ps1')) {
        $failures += "$stage26ScriptName does not call direct Stage 25 player-facing validation."
    }
    if ($content -notmatch [regex]::Escape('run-unity-stage26-validation.ps1')) {
        $failures += "$stage26ScriptName does not call direct Stage 26 Unity validation."
    }
    if ($content -notmatch [regex]::Escape('run-stage26-player-facing-checks.ps1')) {
        $failures += "$stage26ScriptName does not call direct Stage 26 player-facing validation."
    }
}

$stage27ScriptName = 'run-stage27-medium-checks.ps1'
$stage27ScriptPath = Join-Path $repoRoot "tools\$stage27ScriptName"
if (-not (Test-Path -LiteralPath $stage27ScriptPath)) {
    $failures += "Missing medium validation script: $stage27ScriptPath"
} else {
    $lines = Get-Content -LiteralPath $stage27ScriptPath
    for ($lineNumber = 0; $lineNumber -lt $lines.Count; $lineNumber++) {
        $line = $lines[$lineNumber]
        $trimmed = $line.Trim()

        if ($line -match $forbiddenScriptPattern) {
            $ownNameInComment = $line -match [regex]::Escape($stage27ScriptName) -and $trimmed.StartsWith('#')
            if (-not $ownNameInComment) {
                $failures += "${stage27ScriptName}:$($lineNumber + 1) contains forbidden medium dependency text: $trimmed"
            }
        }

        if ($line -match $forbiddenTextPattern) {
            $failures += "${stage27ScriptName}:$($lineNumber + 1) contains old medium dependency wording: $trimmed"
        }
    }

    $content = $lines -join "`n"
    if ($content -notmatch [regex]::Escape('run-unity-stage26-validation.ps1')) {
        $failures += "$stage27ScriptName does not call direct Stage 26 Unity validation."
    }
    if ($content -notmatch [regex]::Escape('run-stage26-player-facing-checks.ps1')) {
        $failures += "$stage27ScriptName does not call direct Stage 26 player-facing validation."
    }
    if ($content -notmatch [regex]::Escape('run-unity-stage27-validation.ps1')) {
        $failures += "$stage27ScriptName does not call direct Stage 27 Unity validation."
    }
    if ($content -notmatch [regex]::Escape('run-stage27-player-facing-checks.ps1')) {
        $failures += "$stage27ScriptName does not call direct Stage 27 player-facing validation."
    }
}

$stage27_1ScriptName = 'run-stage27-1-medium-checks.ps1'
$stage27_1ScriptPath = Join-Path $repoRoot "tools\$stage27_1ScriptName"
if (-not (Test-Path -LiteralPath $stage27_1ScriptPath)) {
    $failures += "Missing medium validation script: $stage27_1ScriptPath"
} else {
    $lines = Get-Content -LiteralPath $stage27_1ScriptPath
    for ($lineNumber = 0; $lineNumber -lt $lines.Count; $lineNumber++) {
        $line = $lines[$lineNumber]
        $trimmed = $line.Trim()

        if ($line -match $forbiddenScriptPattern) {
            $ownNameInComment = $line -match [regex]::Escape($stage27_1ScriptName) -and $trimmed.StartsWith('#')
            if (-not $ownNameInComment) {
                $failures += "${stage27_1ScriptName}:$($lineNumber + 1) contains forbidden medium dependency text: $trimmed"
            }
        }

        if ($line -match $forbiddenTextPattern) {
            $failures += "${stage27_1ScriptName}:$($lineNumber + 1) contains old medium dependency wording: $trimmed"
        }
    }

    $content = $lines -join "`n"
    if ($content -notmatch [regex]::Escape('run-stage27-player-facing-checks.ps1')) {
        $failures += "$stage27_1ScriptName does not call direct Stage 27 player-facing validation."
    }
    if ($content -notmatch [regex]::Escape('run-unity-stage4-validation.ps1')) {
        $failures += "$stage27_1ScriptName does not call direct Stage 4 hand-control validation."
    }
    if ($content -notmatch [regex]::Escape('run-unity-stage5-validation.ps1')) {
        $failures += "$stage27_1ScriptName does not call direct Stage 5 hand-control validation."
    }
    if ($content -notmatch [regex]::Escape('run-unity-stage27-1-validation.ps1')) {
        $failures += "$stage27_1ScriptName does not call direct Stage 27.1 Unity validation."
    }
    if ($content -notmatch [regex]::Escape('run-stage27-1-player-facing-checks.ps1')) {
        $failures += "$stage27_1ScriptName does not call direct Stage 27.1 player-facing validation."
    }
}

$stage28ScriptName = 'run-stage28-medium-checks.ps1'
$stage28ScriptPath = Join-Path $repoRoot "tools\$stage28ScriptName"
if (-not (Test-Path -LiteralPath $stage28ScriptPath)) {
    $failures += "Missing medium validation script: $stage28ScriptPath"
} else {
    $lines = Get-Content -LiteralPath $stage28ScriptPath
    for ($lineNumber = 0; $lineNumber -lt $lines.Count; $lineNumber++) {
        $line = $lines[$lineNumber]
        $trimmed = $line.Trim()

        if ($line -match $forbiddenScriptPattern) {
            $ownNameInComment = $line -match [regex]::Escape($stage28ScriptName) -and $trimmed.StartsWith('#')
            if (-not $ownNameInComment) {
                $failures += "${stage28ScriptName}:$($lineNumber + 1) contains forbidden medium dependency text: $trimmed"
            }
        }

        if ($line -match $forbiddenTextPattern) {
            $failures += "${stage28ScriptName}:$($lineNumber + 1) contains old medium dependency wording: $trimmed"
        }
    }

    $content = $lines -join "`n"
    if ($content -notmatch [regex]::Escape('run-unity-stage27-1-validation.ps1')) {
        $failures += "$stage28ScriptName does not call direct Stage 27.1 Unity validation."
    }
    if ($content -notmatch [regex]::Escape('run-stage27-1-player-facing-checks.ps1')) {
        $failures += "$stage28ScriptName does not call direct Stage 27.1 player-facing validation."
    }
    if ($content -notmatch [regex]::Escape('run-unity-stage4-validation.ps1')) {
        $failures += "$stage28ScriptName does not call direct Stage 4 hand-control validation."
    }
    if ($content -notmatch [regex]::Escape('run-unity-stage5-validation.ps1')) {
        $failures += "$stage28ScriptName does not call direct Stage 5 hand-control validation."
    }
    if ($content -notmatch [regex]::Escape('run-unity-stage28-validation.ps1')) {
        $failures += "$stage28ScriptName does not call direct Stage 28 Unity validation."
    }
    if ($content -notmatch [regex]::Escape('run-stage28-player-facing-checks.ps1')) {
        $failures += "$stage28ScriptName does not call direct Stage 28 player-facing validation."
    }
}

$stage28_1ScriptName = 'run-stage28-1-medium-checks.ps1'
$stage28_1ScriptPath = Join-Path $repoRoot "tools\$stage28_1ScriptName"
if (-not (Test-Path -LiteralPath $stage28_1ScriptPath)) {
    $failures += "Missing medium validation script: $stage28_1ScriptPath"
} else {
    $lines = Get-Content -LiteralPath $stage28_1ScriptPath
    for ($lineNumber = 0; $lineNumber -lt $lines.Count; $lineNumber++) {
        $line = $lines[$lineNumber]
        $trimmed = $line.Trim()

        if ($line -match $forbiddenScriptPattern) {
            $ownNameInComment = $line -match [regex]::Escape($stage28_1ScriptName) -and $trimmed.StartsWith('#')
            if (-not $ownNameInComment) {
                $failures += "${stage28_1ScriptName}:$($lineNumber + 1) contains forbidden medium dependency text: $trimmed"
            }
        }

        if ($line -match $forbiddenTextPattern) {
            $failures += "${stage28_1ScriptName}:$($lineNumber + 1) contains old medium dependency wording: $trimmed"
        }
    }

    $content = $lines -join "`n"
    if ($content -notmatch [regex]::Escape('run-unity-stage28-validation.ps1')) {
        $failures += "$stage28_1ScriptName does not call direct Stage 28 Unity validation."
    }
    if ($content -notmatch [regex]::Escape('run-unity-stage27-1-validation.ps1')) {
        $failures += "$stage28_1ScriptName does not call direct Stage 27.1 Unity validation."
    }
    if ($content -notmatch [regex]::Escape('run-unity-stage4-validation.ps1')) {
        $failures += "$stage28_1ScriptName does not call direct Stage 4 hand-control validation."
    }
    if ($content -notmatch [regex]::Escape('run-unity-stage5-validation.ps1')) {
        $failures += "$stage28_1ScriptName does not call direct Stage 5 hand-control validation."
    }
    if ($content -notmatch [regex]::Escape('run-unity-stage28-1-validation.ps1')) {
        $failures += "$stage28_1ScriptName does not call direct Stage 28.1 Unity validation."
    }
    if ($content -notmatch [regex]::Escape('run-stage28-1-player-facing-checks.ps1')) {
        $failures += "$stage28_1ScriptName does not call direct Stage 28.1 player-facing validation."
    }
}

if ($failures.Count -gt 0) {
    Write-Error "Medium validation recursion audit failed:`n$($failures -join "`n")"
    exit 1
}

Write-Host 'Medium validation recursion audit passed: Stage 9-28.1 medium scripts use direct Unity validation dependencies only.'
$global:LASTEXITCODE = 0
