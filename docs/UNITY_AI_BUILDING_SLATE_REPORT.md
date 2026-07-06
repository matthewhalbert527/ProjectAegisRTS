# Unity AI Building Slate Report

## Current Pass

The Unity AI building slate generates a full MVP building set in:

- `unity/Assets/Rts/Art/UnityAIBuildingSlate/Prefabs/`
- `unity/Assets/Rts/Art/UnityAIBuildingSlate/Materials/`
- `unity/Assets/Rts/Art/UnityAIBuildingSlate/Textures/`
- `unity/Assets/Rts/Scenes/UnityAI_BuildingSlateReview.unity`

The review screenshot is written to:

- `build/screenshots/unity_ai_building_slate_review.png`

Unity AI-authored source textures are supported through:

- `unity/Assets/Rts/Art/UnityAIBuildingSlate/SourceTextures/`
- `docs/UNITY_AI_BUILDING_TEXTURE_PROMPTS.md`

When promoted Unity AI textures exist in that folder, generated materials prefer them over the procedural fallback. Accepted map names use common Unity texture extensions such as `.png`, `.jpg`, `.jpeg`, `.tga`, or `.psd`:

- `<material_id>_ai_albedo.<ext>`
- `<material_id>_ai_normal.<ext>`
- `<material_id>_ai_occlusion.<ext>`

The fallback textures now include stronger panel seams, scuffs, rust, concrete cracks, asphalt aggregate, grime, and edge wear, but they remain a fallback. The target quality path is to generate/promote material textures through Unity AI and keep those files in `SourceTextures`.

The latest correction pass also gives the generated buildings more identifiable silhouettes. The slate now uses L, T, U, H, cross, octagonal, and offset base layouts instead of one repeated rectangular foundation. Each building adds a stronger top-down identity marker, such as construction L markings, power/reactor T bars, barracks command chevrons, a deep War Factory bay, refinery ore lanes, communications signal strips, repair-bay service markings, tech-lab cyan markers, and distinct turret weapon profiles.

Generated buildings:

- Fabrication Hub
- Power Plant
- Advanced Power Plant
- Barracks
- War Factory
- Refinery
- Field Hospital
- Comm Center
- Repair Bay
- Tech Center
- Cannon Turret
- Gun Tower
- Advanced Gun Tower
- Dual Helipad

## Runtime Wiring

Each generated building prefab is wired through the existing Unity-side building visual system. The generated prefab contains a `BuildingVisualRig`, and `BuildingVisualStateController` now consumes that rig before falling back to placeholder parts.

Powered movement hooks include:

- Powered status lights
- Machinery rotor
- Power turbine
- Radar dish
- Construction crane
- Refinery dock pump
- Repair arms
- Turret idle sweep
- Helipad beacon

Production hooks include:

- Production exit sockets
- Powered production indicator
- Roll-up door movement for production doors
- Barracks and War Factory doors open while producing

Visual direction:

- Military concrete and weathered green-gray metal base palette
- Dark inset bays and facade shadow strips for top-down readability
- Distinct footprint families: L-shaped construction/refinery/barracks, T-shaped advanced power/tech, U-shaped vehicle bays, cross-shaped hospital, H-shaped helipad, and octagonal defenses
- Category-aware classic red command accents on construction, production, refinery, tech, repair, and airfield buildings
- Caution accents on power and medical/support buildings
- Darker red-brown armor accents on defenses
- Promoted Unity AI albedo textures are tinted by material intent, then paired with derived normal/occlusion maps when Unity AI has not supplied those maps directly.

## Validation

Focused validation:

```powershell
.\tools\run-unity-ai-building-slate-checks.ps1
```

This verifies:

- 14 generated building prefabs exist.
- Generated materials and textures exist.
- The Unity AI source texture folder and prompt guide exist.
- The generator can prefer promoted Unity AI albedo/normal/occlusion maps.
- Required sockets are present.
- Powered/production/door/turret hooks are present where expected.
- `UnityAI_BuildingSlateReview.unity` references the generated buildings.
- The review screenshot exists and has a plausible file size.
- `src/Rts.Core` has no `UnityEngine` references.
- Building-slate files pass scoped `git diff --check`.

Latest focused result: passed.

Latest Unity AI source generation:

- Model: `hand-painted-textures-2-0`
- Generated AI materials: 12
- Promoted AI albedo maps: 12
- Generated at: 2026-07-06 09:27 America/Chicago

Latest generated counts:

- Prefabs: 14
- Materials: 18
- Textures: 42
- Derived normal maps: 12
- Derived occlusion maps: 12
- Screenshot size: 1,297,880 bytes

Promoted Unity AI texture enforcement:

```powershell
.\tools\run-unity-ai-building-slate-checks.ps1 -RequireAiSourceTextures
```

Use this for a visual-quality pass after promoting Unity AI materials. The default validation remains deterministic and passes without network/AI generation.

Core result:

- `dotnet run --no-restore --project src/Rts.Core.Tests`
- Passed 115/115.

Unity console result:

- Unity MCP ran `ProjectAegisRTS/Unity AI/Build And Capture Building Slate`.
- Unity console query found 0 errors and 0 exceptions.

Stage4/Stage5 note:

- Stage4/Stage5 checks progressed through core tests and live Unity fallback validation.
- They then failed on broad repo-wide `git diff --check` because the existing dirty worktree contains unrelated trailing whitespace in older Unity-generated material, prefab, and project files.
- The building-slate scoped whitespace check passes.

## Next Polish Targets

The current slate is usable and integrated, but the next visual-quality pass should keep improving one building at a time:

- Add stronger Red Alert-style faction color accents.
- Replace the remaining primitive-authored massing with artist-authored or Unity AI-authored mesh pieces.
- Give Power Plant and Advanced Power Plant taller bespoke reactor geometry.
- Give Barracks a more recognizable infantry-yard layout and animated doorway treatment.
- Give War Factory a larger vehicle ramp, overhead gantry, and deeper bay model.
- Give Refinery more ore handling machinery and animated dock detail.
- Give Tech Center and Comm Center taller antenna/sensor profiles.
- Add focused per-building screenshots in addition to the full slate screenshot.
- Promote Unity AI material outputs for the shared building materials, starting with `worn_green_gray_metal`, `weathered_concrete`, `ribbed_bay_door`, `worn_asphalt_ramp`, `classic_red_command_accent`, and `gunmetal_mechanics`.
