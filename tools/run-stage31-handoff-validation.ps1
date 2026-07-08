[CmdletBinding()]
param(
    [switch]$RequireScreenshots
)

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common-validation.ps1')

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')

Write-ValidationSection 'Stage 31 artist handoff package validation'

$requiredDocs = @(
    'docs\STAGE31_REPORT.md',
    'docs\STAGE31_ARTIST_HANDOFF_PACKAGE.md',
    'docs\STAGE31_MVP_ART_REPLACEMENT_GUIDE.md',
    'docs\STAGE31_PER_ACTOR_PRODUCTION_CHECKLIST.md',
    'docs\STAGE31_QUEST_LOD_BUDGETS.md',
    'docs\STAGE31_REFERENCE_PACKAGE.md',
    'docs\OVERNIGHT_VISUAL_QUALITY_REPORT.md'
)

$requiredScripts = @(
    'tools\run-stage31-handoff-validation.ps1',
    'tools\run-stage31-fast-checks.ps1',
    'tools\run-stage31-medium-checks.ps1',
    'tools\run-stage31-player-facing-checks.ps1',
    'tools\run-stage31-checks.ps1'
)

$requiredActorIds = @(
    'fabrication_hub',
    'power_plant',
    'refinery',
    'barracks',
    'war_factory',
    'gun_tower',
    'rifle_infantry',
    'light_tank',
    'harvester'
)

$failures = @()

foreach ($relativePath in $requiredDocs + $requiredScripts) {
    $path = Join-Path $repoRoot $relativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        $failures += "Missing Stage 31 package file: $relativePath"
    }
}

if ($failures.Count -eq 0) {
    $handoff = Get-Content -LiteralPath (Join-Path $repoRoot 'docs\STAGE31_ARTIST_HANDOFF_PACKAGE.md') -Raw
    $replacement = Get-Content -LiteralPath (Join-Path $repoRoot 'docs\STAGE31_MVP_ART_REPLACEMENT_GUIDE.md') -Raw
    $checklist = Get-Content -LiteralPath (Join-Path $repoRoot 'docs\STAGE31_PER_ACTOR_PRODUCTION_CHECKLIST.md') -Raw
    $budgets = Get-Content -LiteralPath (Join-Path $repoRoot 'docs\STAGE31_QUEST_LOD_BUDGETS.md') -Raw
    $reference = Get-Content -LiteralPath (Join-Path $repoRoot 'docs\STAGE31_REFERENCE_PACKAGE.md') -Raw

    $handoffTerms = @(
        'Export And Modeling Brief',
        'Material Naming Rules',
        'Trim-Sheet Guidance',
        'LOD',
        'Quest',
        'Screenshot And Reference Package',
        'Handoff Acceptance'
    )

    foreach ($term in $handoffTerms) {
        if ($handoff -notmatch [regex]::Escape($term)) {
            $failures += "Stage31 handoff package missing required section or term: $term"
        }
    }

    if ($replacement -notmatch 'Replacement Sequence' -or $replacement -notmatch 'Validation Commands') {
        $failures += 'Stage31 MVP art replacement guide must include replacement sequence and validation commands.'
    }

    if ($budgets -notmatch 'LOD0' -or $budgets -notmatch 'Quest Budget Rules') {
        $failures += 'Stage31 Quest LOD budgets must include LOD targets and Quest budget rules.'
    }

    if ($reference -notmatch 'stage29_battlefield_visual_review.png' -or $reference -notmatch 'stage30_visual_readability_qa.png') {
        $failures += 'Stage31 reference package must name the Stage29 and Stage30 screenshot artifacts.'
    }

    foreach ($actorId in $requiredActorIds) {
        if ($checklist -notmatch [regex]::Escape($actorId)) {
            $failures += "Stage31 per-actor checklist missing actor: $actorId"
        }
        if ($replacement -notmatch [regex]::Escape($actorId)) {
            $failures += "Stage31 replacement guide missing actor ID: $actorId"
        }
    }
}

if ($RequireScreenshots) {
    $screenshots = @(
        'build\screenshots\stage29_battlefield_visual_review.png',
        'build\screenshots\stage30_visual_readability_qa.png'
    )
    foreach ($relativePath in $screenshots) {
        $path = Join-Path $repoRoot $relativePath
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
            $failures += "Missing generated screenshot artifact: $relativePath"
        }
    }
}

if ($failures.Count -gt 0) {
    Write-Error "Stage 31 handoff validation failed:`n$($failures -join "`n")"
    exit 1
}

Write-Host 'Stage 31 handoff package validation passed.'
$global:LASTEXITCODE = 0
