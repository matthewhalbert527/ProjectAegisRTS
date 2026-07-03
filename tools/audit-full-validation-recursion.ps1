[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$failures = @()
$reports = @()
$fullDependencyPattern = 'run-stage(?:9|10|11|12|13|14|15|16|17|18|18-5|19|19-5|20|21|21-5|22|23|24|25|26|27|27-1|28|28-1|29|30)-checks(?:\.ps1)?'
$stage28DisallowedPattern = 'run-stage(?:9|10|11|12|13|14|15|16|17|18|18-5|19|19-5|20|21|21-5|22|23|24|25|26|27|27-1)-checks(?:\.ps1)?'
$stage28_1DisallowedPattern = 'run-stage(?:9|10|11|12|13|14|15|16|17|18|18-5|19|19-5|20|21|21-5|22|23|24|25|26|27|27-1)-checks(?:\.ps1)?'
$stage29DisallowedPattern = 'run-stage(?:9|10|11|12|13|14|15|16|17|18|18-5|19|19-5|20|21|21-5|22|23|24|25|26|27|27-1|28)-checks(?:\.ps1)?'
$stage30DisallowedPattern = 'run-stage(?:9|10|11|12|13|14|15|16|17|18|18-5|19|19-5|20|21|21-5|22|23|24|25|26|27|27-1|28|28-1)-checks(?:\.ps1)?'

$scripts = Get-ChildItem -LiteralPath (Join-Path $repoRoot 'tools') -Filter 'run-stage*-checks.ps1' -File |
    Where-Object {
        $_.Name -notmatch '-(?:fast|medium|player-facing)-checks\.ps1$' -and
        $_.Name -notmatch 'full-chain'
    } |
    Sort-Object Name

foreach ($script in $scripts) {
    $lines = Get-Content -LiteralPath $script.FullName
    for ($lineNumber = 0; $lineNumber -lt $lines.Count; $lineNumber++) {
        $line = $lines[$lineNumber]
        $trimmed = $line.Trim()
        if ($trimmed.StartsWith('#')) {
            continue
        }

        $isExecutableReference =
            $trimmed.StartsWith('&') -or
            $trimmed.StartsWith('.\') -or
            $trimmed.StartsWith('./')
        if (-not $isExecutableReference) {
            continue
        }

        $matches = [regex]::Matches($line, $fullDependencyPattern)
        foreach ($match in $matches) {
            $dependency = $match.Value
            if ($dependency -eq $script.Name -or $dependency -eq ($script.BaseName + '.ps1')) {
                continue
            }

            $reports += "$($script.Name):$($lineNumber + 1) references full gate $dependency"

            if ($script.Name -eq 'run-stage28-checks.ps1' -and $dependency -match $stage28DisallowedPattern) {
                $failures += "$($script.Name):$($lineNumber + 1) must not recursively call prior full gate $dependency"
            }

            if ($script.Name -eq 'run-stage28-1-checks.ps1' -and $dependency -match $stage28_1DisallowedPattern) {
                $failures += "$($script.Name):$($lineNumber + 1) must not recursively call legacy full gate $dependency"
            }

            if ($script.Name -eq 'run-stage29-checks.ps1' -and $dependency -match $stage29DisallowedPattern) {
                $failures += "$($script.Name):$($lineNumber + 1) must not recursively call legacy full gate $dependency"
            }

            if ($script.Name -eq 'run-stage30-checks.ps1' -and $dependency -match $stage30DisallowedPattern) {
                $failures += "$($script.Name):$($lineNumber + 1) must not recursively call legacy full gate $dependency"
            }
        }
    }
}

$stage28Path = Join-Path $repoRoot 'tools\run-stage28-checks.ps1'
if (-not (Test-Path -LiteralPath $stage28Path)) {
    $failures += "Missing full validation script: $stage28Path"
} else {
    $stage28Content = Get-Content -LiteralPath $stage28Path -Raw
    $requiredStage28Coverage = @(
        'run-unity-stage16-validation.ps1',
        'run-unity-stage19-5-validation.ps1',
        'run-unity-stage21-5-validation.ps1',
        'run-unity-stage27-1-validation.ps1',
        'run-unity-stage28-validation.ps1',
        'run-unity-stage28-1-validation.ps1',
        'run-stage28-1-player-facing-checks.ps1'
    )
    foreach ($required in $requiredStage28Coverage) {
        if ($stage28Content -notmatch [regex]::Escape($required)) {
            $failures += "run-stage28-checks.ps1 does not include direct flattened coverage: $required"
        }
    }
}

$stage28_1Path = Join-Path $repoRoot 'tools\run-stage28-1-checks.ps1'
if (Test-Path -LiteralPath $stage28_1Path) {
    $stage28_1Content = Get-Content -LiteralPath $stage28_1Path -Raw
    if ($stage28_1Content -notmatch [regex]::Escape('run-stage28-checks.ps1')) {
        $failures += 'run-stage28-1-checks.ps1 should delegate to the flattened Stage 28 full gate.'
    }
}

$stage29Path = Join-Path $repoRoot 'tools\run-stage29-checks.ps1'
if (-not (Test-Path -LiteralPath $stage29Path)) {
    $failures += "Missing full validation script: $stage29Path"
} else {
    $stage29Content = Get-Content -LiteralPath $stage29Path -Raw
    $requiredStage29Coverage = @(
        'run-stage28-1-checks.ps1',
        'run-unity-stage29-validation.ps1',
        'run-stage29-player-facing-checks.ps1'
    )
    foreach ($required in $requiredStage29Coverage) {
        if ($stage29Content -notmatch [regex]::Escape($required)) {
            $failures += "run-stage29-checks.ps1 does not include flattened Stage 29 coverage: $required"
        }
    }
}

$stage30Path = Join-Path $repoRoot 'tools\run-stage30-checks.ps1'
if (-not (Test-Path -LiteralPath $stage30Path)) {
    $failures += "Missing full validation script: $stage30Path"
} else {
    $stage30Content = Get-Content -LiteralPath $stage30Path -Raw
    $requiredStage30Coverage = @(
        'run-stage29-checks.ps1',
        'run-unity-stage30-validation.ps1',
        'run-stage30-player-facing-checks.ps1'
    )
    foreach ($required in $requiredStage30Coverage) {
        if ($stage30Content -notmatch [regex]::Escape($required)) {
            $failures += "run-stage30-checks.ps1 does not include flattened Stage 30 coverage: $required"
        }
    }
}

if ($reports.Count -gt 0) {
    Write-Host 'Full validation dependency references:'
    $reports | ForEach-Object { Write-Host "  $_" }
}

if ($failures.Count -gt 0) {
    Write-Error "Full validation recursion audit failed:`n$($failures -join "`n")"
    exit 1
}

Write-Host 'Full validation recursion audit passed: Stage 28+ full gates avoid recursive legacy full-chain replay.'
$global:LASTEXITCODE = 0
