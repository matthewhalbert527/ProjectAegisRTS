param(
    [switch]$RequireAiSourceTextures
)

$ErrorActionPreference = "Stop"

$projectRoot = Split-Path -Parent $PSScriptRoot
Set-Location $projectRoot

function Fail($message) {
    Write-Error $message
    exit 1
}

function Require-Path($path, $description) {
    if (-not (Test-Path -LiteralPath $path)) {
        Fail "Missing $description`: $path"
    }
}

function Require-Text($path, $pattern, $description) {
    $match = Select-String -LiteralPath $path -Pattern $pattern -SimpleMatch -Quiet
    if (-not $match) {
        Fail "$description was not found in $path"
    }
}

function Forbid-Text($path, $pattern, $description) {
    $match = Select-String -LiteralPath $path -Pattern $pattern -SimpleMatch -Quiet
    if ($match) {
        Fail "$description should not be present in $path"
    }
}

function Assert-UnityEngineFree($path) {
    if (Get-Command rg -ErrorAction SilentlyContinue) {
        $matches = rg "UnityEngine" $path
        if ($LASTEXITCODE -eq 0) {
            $matches | Write-Host
            Fail "Rts.Core references UnityEngine."
        }
        if ($LASTEXITCODE -gt 1) {
            Fail "ripgrep failed while scanning Rts.Core."
        }
    }
    else {
        $matches = Get-ChildItem $path -Recurse -Include *.cs | Select-String -Pattern "UnityEngine"
        if ($matches) {
            $matches | ForEach-Object { Write-Host $_ }
            Fail "Rts.Core references UnityEngine."
        }
    }
}

$expected = @(
    "fabrication_hub",
    "power_plant",
    "advanced_power_plant",
    "barracks",
    "war_factory",
    "refinery",
    "field_hospital",
    "comm_center",
    "repair_bay",
    "tech_center",
    "cannon_turret",
    "gun_tower",
    "advanced_gun_tower",
    "dual_helipad"
)

$doorBuildings = @("fabrication_hub", "barracks", "war_factory", "field_hospital", "repair_bay")
$productionBuildings = @("fabrication_hub", "barracks", "war_factory", "field_hospital", "dual_helipad")
$turretBuildings = @("cannon_turret", "gun_tower", "advanced_gun_tower")
$signatureMarkers = @{
    "fabrication_hub" = "fabrication hub right L shaped machine annex"
    "power_plant" = "power plant left round generator pod"
    "advanced_power_plant" = "advanced power central tall reactor spine"
    "barracks" = "barracks long dormitory wing"
    "war_factory" = "war factory clear internal vehicle spawn bay floor"
    "refinery" = "refinery tall processor block"
    "field_hospital" = "field hospital central clinic tower"
    "comm_center" = "comm center rear antenna tower base"
    "repair_bay" = "repair bay flat vehicle pull-on repair pad"
    "tech_center" = "tech center central research tower"
    "cannon_turret" = "cannon turret heavy single barrel"
    "gun_tower" = "gun tower twin machine cannon left"
    "advanced_gun_tower" = "advanced gun tower realistic left missile battery"
    "dual_helipad" = "dual helipad center taxi control spine"
}

$prefabDir = "unity/Assets/Rts/Art/UnityAIBuildingSlate/Prefabs"
$materialDir = "unity/Assets/Rts/Art/UnityAIBuildingSlate/Materials"
$textureDir = "unity/Assets/Rts/Art/UnityAIBuildingSlate/Textures"
$sourceTextureDir = "unity/Assets/Rts/Art/UnityAIBuildingSlate/SourceTextures"
$scenePath = "unity/Assets/Rts/Scenes/UnityAI_BuildingSlateReview.unity"
$screenshotPath = "build/screenshots/unity_ai_building_slate_review.png"
$texturePromptDoc = "docs/UNITY_AI_BUILDING_TEXTURE_PROMPTS.md"

Write-Host "Checking Unity AI building slate assets."
Require-Path $prefabDir "building slate prefab directory"
Require-Path $materialDir "building slate material directory"
Require-Path $textureDir "building slate texture directory"
Require-Path $sourceTextureDir "building slate Unity AI source texture directory"
Require-Path $scenePath "building slate review scene"
Require-Path $screenshotPath "building slate screenshot"
Require-Path $texturePromptDoc "Unity AI building texture prompt guide"
Require-Text "unity/Assets/Rts/Editor/UnityAiBuildingSlateGenerator.cs" "SourceTextureFolder" "Unity AI source texture folder support"
Require-Text "unity/Assets/Rts/Editor/UnityAiBuildingSlateGenerator.cs" "FindPromotedTexturePath" "promoted Unity AI texture lookup"

$prefabs = Get-ChildItem -LiteralPath $prefabDir -Filter "*_unity_ai_building.prefab"
if ($prefabs.Count -lt $expected.Count) {
    Fail "Expected at least $($expected.Count) building prefabs, found $($prefabs.Count)."
}

foreach ($id in $expected) {
    $prefab = Join-Path $prefabDir "$($id)_unity_ai_building.prefab"
    $profile = "unity/Assets/Rts/ScriptableObjects/BuildingProfiles/$($id)_building_visual.asset"
    $definition = "unity/Assets/Rts/ScriptableObjects/Art/ActorVisualDefinitions/$($id)_visual.asset"

    Require-Path $prefab "$id prefab"
    Require-Path $profile "$id building profile"
    Require-Path $definition "$id actor visual definition"

    Require-Text $prefab "VisualRoot" "$id VisualRoot socket"
    Require-Text $prefab "BodyRoot" "$id BodyRoot socket"
    Require-Text $prefab "SelectionAnchor" "$id SelectionAnchor socket"
    Require-Text $prefab "HealthBarAnchor" "$id HealthBarAnchor socket"
    Require-Text $prefab "UiAnchor" "$id UiAnchor socket"
    Require-Text $prefab "VfxSmoke" "$id smoke socket"
    Require-Text $prefab "VfxExplosion" "$id explosion socket"
    Require-Text $prefab "powered status light" "$id powered lights"
    Require-Text $definition "Unity AI building slate production prefab" "$id production visual definition"
    Require-Text $prefab $signatureMarkers[$id] "$id distinctive silhouette marker"
    Forbid-Text $prefab "front foundation lip" "$id full-width front foundation lip"

    if ($turretBuildings -contains $id) {
        Require-Text $prefab "Turret sweep assembly" "$id turret rig"
        Require-Text $prefab "default yaw 2 oclock" "$id default turret yaw"
        Require-Text $prefab "MuzzlePrimary" "$id muzzle socket"
        Require-Text $prefab "high contrast" "$id high contrast turret silhouette"
        Require-Text $prefab "textured turret top armor" "$id textured turret top armor"
        Require-Text $prefab "turret top tactical panel seam" "$id turret top tactical seams"
        Require-Text $prefab "raised turret top command hatch" "$id raised turret top hatch"
        if ($id -eq "advanced_gun_tower") {
            Require-Text $prefab "advanced gun tower realistic left missile battery" "$id left missile battery"
            Require-Text $prefab "advanced gun tower realistic right missile battery" "$id right missile battery"
            Require-Text $prefab "missile battery textured turret top armor hatch" "$id missile battery top armor texture"
            Require-Text $prefab "missile launch tube" "$id missile launch tubes"
            Require-Text $prefab "high visibility missile nose cap" "$id missile nose caps"
            Forbid-Text $prefab "advanced gun tower high visibility muzzle tip bridge" "$id orphaned central muzzle bridge"
        }
        else {
            Require-Text $prefab "high visibility muzzle" "$id high visibility muzzle tips"
        }
    }
    elseif ($id -eq "dual_helipad") {
        Require-Text $prefab "center-only airfield machinery rotor" "$id center-only airfield machinery"
        Require-Text $prefab "center-only helipad tower roof panel" "$id center-only roof detail"
        Require-Text $prefab "center-only airfield powered status light" "$id center-only powered status lights"
        Require-Text $prefab "center-only airfield front spine rail" "$id center-only foundation rail"
        Require-Text $prefab "flush clear helipad landing paint disk" "$id flush landing pads"
        Require-Text $prefab "flush painted helipad H stripe" "$id flush landing pad paint"
        Require-Text $prefab "dual helipad center air control tower base" "$id center tower"
        Require-Text $prefab "mauve dual helipad center tower identity band" "$id center tower mauve identity"
        Forbid-Text $prefab "dual helipad low left equipment locker" "$id left pad raised equipment"
        Forbid-Text $prefab "dual helipad low right equipment locker" "$id right pad raised equipment"
        Forbid-Text $prefab "mauve dual helipad large H identity crossbar" "$id cross-pad raised identity stripe"
        Forbid-Text $prefab "front foundation lip" "$id full-width front foundation lip"
        Forbid-Text $prefab "rear foundation lip" "$id full-width rear foundation lip"
        Forbid-Text $prefab "left foundation lip" "$id full-depth left foundation lip"
        Forbid-Text $prefab "right foundation lip" "$id full-depth right foundation lip"
        Forbid-Text $prefab "helipad landing disk" "$id raised landing disk"
        Forbid-Text $prefab "helipad stripe" "$id raised landing stripe"
        Forbid-Text $prefab "helipad cross stripe" "$id raised landing cross stripe"
        Forbid-Text $prefab "left service panels" "$id left landing area service panels"
        Forbid-Text $prefab "right service panels" "$id right landing area service panels"
        Forbid-Text $prefab "vertical chamfer armor" "$id landing area vertical armor"
        Forbid-Text $prefab "roof vent" "$id landing area roof vents"
    }
    else {
        Require-Text $prefab "powered machinery rotor" "$id powered machinery"
    }

    if ($doorBuildings -contains $id) {
        Require-Text $prefab "DoorRoot" "$id door socket"
        if ($id -eq "barracks") {
            Require-Text $prefab "doorOpenLocalDirection: {x: -1" "$id door opens left to reveal interior"
            Require-Text $profile "doorOpenDistance: 0.62" "$id door slides fully clear"
            Require-Text $prefab "barracks ground-level dark infantry doorway opening" "$id ground-level dark doorway opening"
            Require-Text $prefab "barracks bright blue interior floor tile" "$id distinct interior floor visible when open"
            Require-Text $prefab "barracks mauve interior floor stripe" "$id interior floor guide stripe"
            Require-Text $prefab "barracks cyan interior floor seam" "$id interior floor cross seam"
            Require-Text $prefab "barracks full-height closed metal personnel door panel" "$id full-height closed personnel door"
            Require-Text $prefab "barracks personnel door lower floor-contact edge" "$id door reaches floor"
            Require-Text $prefab "barracks visible open door mauve left edge trim" "$id visible open door left edge trim"
            Require-Text $prefab "barracks visible open door mauve right edge trim" "$id visible open door right edge trim"
            Require-Text $prefab "barracks visible open door mauve top edge trim" "$id visible open door top edge trim"
            Require-Text $prefab "barracks outside asphalt infantry exit threshold" "$id outside infantry threshold"
            Require-Text $prefab "barracks personnel door upper slide rail" "$id personnel door slide rail"
            Require-Text $prefab "barracks personnel door visible open parking rail upper" "$id visible upper parking rail for open door"
            Require-Text $prefab "barracks personnel door visible open parking rail lower" "$id visible lower parking rail for open door"
            Forbid-Text $prefab "barracks small normal sliding personnel door" "$id old short floating personnel door"
            Forbid-Text $prefab "barracks visible interior floor inside doorway" "$id old ambiguous interior floor"
            Forbid-Text $prefab "barracks dark interior tile floor visible when door opens" "$id old too-subtle interior floor"
            Forbid-Text $prefab "barracks blue gray interior steel tile floor visible when door opens" "$id old low-contrast interior floor"
            Forbid-Text $prefab "barracks bright blue interior steel tile floor visible when door opens" "$id old overlong interior floor name"
            Forbid-Text $prefab "barracks full-height sliding personnel door panel" "$id old ambiguous door panel"
            Forbid-Text $prefab "barracks personnel door left sliding pocket" "$id old occluding door pocket"
            Forbid-Text $prefab "barracks personnel door right sliding pocket" "$id old wrong-side door pocket"
            Forbid-Text $prefab "ribbed production door" "$id old oversized production door"
        }
        elseif ($id -eq "war_factory") {
            Require-Text $prefab "war factory full-height overhead sliding vehicle bay door assembly" "$id full-height overhead vehicle bay door assembly"
            Require-Text $prefab "war factory full-height overhead sliding vehicle bay door panel" "$id full-height overhead vehicle bay door panel"
            Require-Text $prefab "war factory moving door lower floor-contact edge" "$id moving bay door floor-contact edge"
            Require-Text $prefab "war factory moving ribbed door slat" "$id moving bay door readable slats"
            Require-Text $prefab "war factory door bottom floor seal" "$id vehicle bay door reaches floor"
            Require-Text $prefab "war factory unobstructed vehicle exit lane" "$id unobstructed vehicle exit lane"
            Require-Text $prefab "factory clear vehicle spawn marker inside bay" "$id internal vehicle spawn marker"
            Require-Text $prefab "factory left side conveyor clear of exit lane" "$id left conveyor outside exit lane"
            Require-Text $prefab "factory right side conveyor clear of exit lane" "$id right conveyor outside exit lane"
            Forbid-Text $prefab "factory dark vehicle bay interior" "$id old raised vehicle bay block"
            Forbid-Text $prefab "ribbed production door" "$id old generic production door"
        }
        elseif ($id -eq "field_hospital") {
            Require-Text $prefab "doorOpenLocalDirection: {x: -1" "$id clinic door opens left to reveal interior"
            Require-Text $profile "doorOpenDistance: 0.62" "$id clinic door slides fully clear"
            Require-Text $prefab "field hospital ground-level dark clinic doorway opening" "$id ground-level clinic doorway opening"
            Require-Text $prefab "field hospital bright teal interior floor tile" "$id distinct clinic interior floor"
            Require-Text $prefab "field hospital green triage interior floor stripe" "$id clinic interior triage stripe"
            Require-Text $prefab "field hospital cyan interior floor seam" "$id clinic interior floor seam"
            Require-Text $prefab "field hospital full-height closed metal clinic personnel door panel" "$id barracks-style full-height clinic door"
            Require-Text $prefab "field hospital clinic personnel door lower floor-contact edge" "$id clinic door reaches floor"
            Require-Text $prefab "green field hospital closed door face stripe" "$id green clinic door face stripe"
            Require-Text $prefab "field hospital visible open door green left edge trim" "$id visible open door left edge trim"
            Require-Text $prefab "field hospital visible open door green right edge trim" "$id visible open door right edge trim"
            Require-Text $prefab "field hospital visible open door green top edge trim" "$id visible open door top edge trim"
            Require-Text $prefab "field hospital clean outside infantry entry threshold" "$id clinic outside infantry threshold"
            Require-Text $prefab "field hospital clinic door upper slide rail" "$id clinic door slide rail"
            Require-Text $prefab "field hospital clinic door visible open parking rail upper" "$id visible upper parking rail for open door"
            Require-Text $prefab "field hospital clinic door visible open parking rail lower" "$id visible lower parking rail for open door"
            Forbid-Text $prefab "field hospital visible clean interior floor inside doorway" "$id old low-contrast clinic interior floor"
            Forbid-Text $prefab "field hospital bright sterile teal interior tile floor visible when door" "$id old truncated clinic floor name"
            Forbid-Text $prefab "field hospital full-height sliding clinic door assembly" "$id old hidden-door clinic assembly"
            Forbid-Text $prefab "field hospital full-height sliding clinic door panel" "$id old ambiguous clinic door panel"
            Forbid-Text $prefab "field hospital clinic door left sliding pocket" "$id old occluding clinic pocket"
            Forbid-Text $prefab "field hospital clinic door right sliding pocket" "$id old wrong-side clinic pocket"
            Forbid-Text $prefab "field hospital clean infantry entry threshold" "$id old oversized clinic threshold"
            Forbid-Text $prefab "ribbed production door" "$id old generic production door"
        }
        else {
            Require-Text $prefab "ribbed production door" "$id animated door"
        }
    }

    if ($id -eq "repair_bay") {
        Require-Text $prefab "animated articulated repair worker arm left" "$id left articulated repair arm"
        Require-Text $prefab "animated articulated repair worker arm right" "$id right articulated repair arm"
        Require-Text $prefab "left repair arm round shoulder joint" "$id left shoulder joint"
        Require-Text $prefab "right repair arm round shoulder joint" "$id right shoulder joint"
        Require-Text $prefab "left repair arm upper worker boom" "$id left upper worker boom"
        Require-Text $prefab "right repair arm upper worker boom" "$id right upper worker boom"
        Require-Text $prefab "left repair arm round elbow joint" "$id left elbow joint"
        Require-Text $prefab "right repair arm round elbow joint" "$id right elbow joint"
        Require-Text $prefab "left repair arm angled forearm link" "$id left angled forearm"
        Require-Text $prefab "right repair arm angled forearm link" "$id right angled forearm"
        Require-Text $prefab "left repair arm compact wrist joint" "$id left wrist joint"
        Require-Text $prefab "right repair arm compact wrist joint" "$id right wrist joint"
        Require-Text $prefab "green left repair worker arm precision welding nozzle" "$id green left precision nozzle"
        Require-Text $prefab "green right repair worker arm precision welding nozzle" "$id green right precision nozzle"
        Require-Text $prefab "green left repair worker arm emitter lens" "$id green left emitter lens"
        Require-Text $prefab "green right repair worker arm emitter lens" "$id green right emitter lens"
        Require-Text $prefab "left repair arm slim left tool finger" "$id left slim tool finger"
        Require-Text $prefab "right repair arm slim right tool finger" "$id right slim tool finger"
        Forbid-Text $prefab "left small repair arm reaches over pad" "$id old simple left beam arm"
        Forbid-Text $prefab "right small repair arm reaches over pad" "$id old simple right beam arm"
        Forbid-Text $prefab "green left repair narrow welding nozzle over vehicle pad" "$id old left simple nozzle"
        Forbid-Text $prefab "green right repair narrow welding nozzle over vehicle pad" "$id old right simple nozzle"
        Forbid-Text $prefab "green left repair welding tool over vehicle pad" "$id old blocky left repair tool"
        Forbid-Text $prefab "green right repair welding tool over vehicle pad" "$id old blocky right repair tool"
    }

    if ($id -eq "fabrication_hub" -or $id -eq "barracks" -or $id -eq "war_factory" -or $id -eq "dual_helipad") {
        Require-Text $prefab "mauve" "$id mauve production highlight"
    }

    if ($id -eq "fabrication_hub") {
        Require-Text $prefab "fabrication hub oversized tall rear fabrication hall" "$id oversized fabrication hall"
        Require-Text $prefab "fabrication hub broad low front assembly bay" "$id broad assembly bay"
        Require-Text $definition "footprintWidth: 4" "$id visual footprint larger than war factory"
    }

    if ($id -eq "advanced_power_plant") {
        Require-Text $prefab "advanced power wide left reactor wing" "$id wide left reactor wing"
        Require-Text $prefab "advanced power extra wide reinforced top crossbar" "$id extra wide top crossbar"
        Require-Text $prefab "advanced power left top spinning turbine blade assembly" "$id left top spinning turbine"
        Require-Text $prefab "advanced power right top spinning turbine blade assembly" "$id right top spinning turbine"
        Require-Text $prefab "advanced power left spinner armored bearing" "$id left spinner bearing"
        Require-Text $prefab "advanced power right spinner armored bearing" "$id right spinner bearing"
        Require-Text $prefab "TurbineRoot Left" "$id left turbine socket"
        Require-Text $prefab "TurbineRoot Right" "$id right turbine socket"
        Forbid-Text $prefab "animated turbine assembly" "$id old center-only turbine assembly"
        Require-Text $definition "footprintWidth: 3" "$id wider visual footprint than regular power plant"
    }

    if ($id -eq "power_plant") {
        Require-Text $prefab "power plant top spinning turbine blade assembly" "$id top spinning turbine"
        Require-Text $prefab "power plant top spinner armored bearing" "$id top spinner bearing"
        Require-Text $prefab "TurbineRoot" "$id turbine socket"
        Forbid-Text $prefab "animated turbine assembly" "$id old generic turbine assembly"
    }

    if ($id -eq "comm_center") {
        Require-Text $prefab "animated comm center satellite dish" "$id satellite dish rig"
        Require-Text $prefab "large comm center satellite dish bright reflector face" "$id satellite reflector"
        Require-Text $prefab "satellite dish receiver node" "$id satellite receiver node"
    }

    if ($id -eq "tech_center") {
        Require-Text $prefab "tech center taller glass command crown" "$id taller tech crown"
        Require-Text $prefab "tech center sealed front sensor platform" "$id sealed sensor platform instead of path"
        Require-Text $prefab "tech center sealed front quantum array block" "$id sealed front research facade"
        Require-Text $prefab "tech center cyan front analysis screen" "$id front analysis screen"
        Require-Text $prefab "tech center sealed front data facade not a door" "$id non-door front facade"
        Require-Text $prefab "tech center forward micro sensor turret" "$id forward sensor turret"
        Require-Text $prefab "tech center roof quantum ring front" "$id roof quantum ring"
        Require-Text $prefab "tech center high spire antenna" "$id high tech spire"
        Require-Text $prefab "tech center secondary spire antenna left" "$id secondary left spire"
        Require-Text $prefab "tech center secondary spire antenna right" "$id secondary right spire"
        Require-Text $prefab "tech center glowing apex node" "$id glowing apex node"
        Forbid-Text $prefab "tech clean concrete entry apron" "$id old front path/apron cue"
        Forbid-Text $prefab "deep front facade shadow" "$id old generic door-like front shadow"
    }

    if ($productionBuildings -contains $id) {
        Require-Text $prefab "ProductionExit" "$id production exit socket"
    }
}

$materials = Get-ChildItem -LiteralPath $materialDir -Filter "*.mat"
if ($materials.Count -lt 14) {
    Fail "Expected at least 14 generated building materials, found $($materials.Count)."
}
Require-Path (Join-Path $materialDir "production_mauve_accent.mat") "production mauve accent material"
Require-Path (Join-Path $materialDir "bright_blue_interior_floor_tiles.mat") "high-contrast interior floor material"
Require-Path (Join-Path $materialDir "medical_green_light.mat") "medical/repair green accent material"
Require-Path (Join-Path $materialDir "powered_cyan_glow.mat") "electric blue power accent material"
Require-Path (Join-Path $materialDir "turret_high_contrast_armor.mat") "high contrast turret armor material"
Require-Path (Join-Path $materialDir "turret_top_tactical_panel_texture.mat") "turret top tactical panel material"
Require-Path (Join-Path $materialDir "turret_bright_gunmetal.mat") "bright turret barrel material"
Require-Path (Join-Path $materialDir "turret_high_visibility_muzzle.mat") "high visibility turret muzzle material"
Require-Path (Join-Path $textureDir "turret_top_tactical_panel_texture.png") "turret top tactical panel texture"

$textures = Get-ChildItem -LiteralPath $textureDir -Filter "*.png"
if ($textures.Count -lt 14) {
    Fail "Expected at least 14 generated building textures, found $($textures.Count)."
}

$sourceTextures = @()
if (Test-Path -LiteralPath $sourceTextureDir) {
    $sourceTextures = Get-ChildItem -LiteralPath $sourceTextureDir -File -Include *.png,*.jpg,*.jpeg,*.tga,*.psd -Recurse
}

$aiAlbedoTextures = @($sourceTextures | Where-Object {
    $_.BaseName -match "_ai_albedo$" -or $_.BaseName -match "_albedo$"
})

if ($RequireAiSourceTextures -and $aiAlbedoTextures.Count -lt 6) {
    Fail "Expected at least 6 promoted Unity AI albedo source textures in $sourceTextureDir when -RequireAiSourceTextures is used, found $($aiAlbedoTextures.Count)."
}

$derivedNormalTextures = @(Get-ChildItem -LiteralPath $textureDir -File -Filter "*_derived_normal.png" -ErrorAction SilentlyContinue)
$derivedOcclusionTextures = @(Get-ChildItem -LiteralPath $textureDir -File -Filter "*_derived_occlusion.png" -ErrorAction SilentlyContinue)
if ($RequireAiSourceTextures -and $derivedNormalTextures.Count -lt $aiAlbedoTextures.Count) {
    Fail "Expected derived normal maps for promoted Unity AI albedo textures, found $($derivedNormalTextures.Count) normal maps for $($aiAlbedoTextures.Count) albedo maps."
}
if ($RequireAiSourceTextures -and $derivedOcclusionTextures.Count -lt $aiAlbedoTextures.Count) {
    Fail "Expected derived occlusion maps for promoted Unity AI albedo textures, found $($derivedOcclusionTextures.Count) occlusion maps for $($aiAlbedoTextures.Count) albedo maps."
}

foreach ($id in $expected) {
    Require-Text $scenePath $id "$id review scene instance"
}

$screenshot = Get-Item -LiteralPath $screenshotPath
if ($screenshot.Length -lt 100000) {
    Fail "Building slate screenshot looks too small to be a valid capture: $($screenshot.Length) bytes."
}

Write-Host "Checking Rts.Core for UnityEngine references."
Assert-UnityEngineFree ".\src\Rts.Core"
Write-Host "Rts.Core UnityEngine-free check passed."

Write-Host "Running git diff whitespace check for building slate files."
git diff --check -- `
    unity/Assets/Rts/Editor/UnityAiBuildingSlateGenerator.cs `
    unity/Assets/Rts/Scripts/Rendering/Buildings/BuildingVisualRig.cs `
    unity/Assets/Rts/Scripts/Rendering/Buildings/BuildingArtShowcaseController.cs `
    unity/Assets/Rts/Scripts/Rendering/Buildings/BuildingVisualStateController.cs `
    unity/Assets/Rts/Scripts/Rendering/Buildings/BuildingPlaceholderPartFactory.cs `
    unity/Assets/Rts/Scripts/Rendering/Buildings/BuildingDoorVisualController.cs `
    unity/Assets/Rts/Art/UnityAIBuildingSlate `
    unity/Assets/Rts/Scenes/UnityAI_BuildingSlateReview.unity `
    docs/UNITY_AI_BUILDING_TEXTURE_PROMPTS.md `
    docs/UNITY_AI_BUILDING_SLATE_REPORT.md
if ($LASTEXITCODE -ne 0) {
    Fail "git diff --check failed for building slate files."
}

Write-Host "Unity AI building slate validation passed."
Write-Host "Prefabs: $($expected.Count)"
Write-Host "Materials: $($materials.Count)"
Write-Host "Textures: $($textures.Count)"
Write-Host "Promoted Unity AI source textures: $($aiAlbedoTextures.Count) albedo candidates"
Write-Host "Derived detail maps: $($derivedNormalTextures.Count) normal, $($derivedOcclusionTextures.Count) occlusion"
Write-Host "Screenshot: $($screenshot.FullName) ($($screenshot.Length) bytes)"
