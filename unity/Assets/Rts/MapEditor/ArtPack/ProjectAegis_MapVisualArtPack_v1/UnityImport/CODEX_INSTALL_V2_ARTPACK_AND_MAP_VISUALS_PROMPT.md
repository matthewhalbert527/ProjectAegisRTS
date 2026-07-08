You are working locally in ProjectAegisRTS at:

E:\OpenRA Mod\ProjectAegisRTS

This task installs the rebuilt Project Aegis Unity map art packet and wires it into the existing map visual compiler so generated maps look better and remain playable.

Current known main-branch architecture:
- `src/Rts.Core/Maps` owns deterministic map data, generation, validation, and Tiled export.
- `unity/Assets/Rts/Scripts/MapEditor/Editor/Visuals` contains the Unity-side visual compiler layers.
- `unity/Assets/Rts/Scripts/MapEditor/Runtime/Visuals` contains visual theme/context/summary runtime data.
- `AegisMapArtPack.Root` currently points to `Assets/Rts/MapEditor/ArtPack/ProjectAegis_MapVisualArtPack_v1`.
- The rebuilt zip keeps that folder name intentionally, even though the contents are v2 production-proxy art.

Use or create branch:

codex/install-v2-map-artpack

Hard restrictions:
- Do not push to main.
- Do not pull in Stage 1 packages.
- Do not use C&C / Red Alert / OpenRA art, code, maps, UI, names, faction identifiers, or implementation files.
- Do not copy OpenRA implementation code.
- Do not add network AI/API dependencies.
- Do not stage `.vs/`, `build/`, zip files, `unity-compile.log`, `*.local-export.tiled.json`, Unity cache folders, or broad generated screenshots.
- Keep `src/Rts.Core` deterministic and UnityEngine/UnityEditor-free.
- `.aegismap.json` / `AegisMapDocument` remains the runtime source of truth.
- Tiled remains an external authoring/export format, not the runtime source of truth.

PHASE 1 — Install the rebuilt art pack

1. Locate the extracted rebuilt art pack folder:
   `ProjectAegis_MapVisualArtPack_v1/`

2. Copy or replace it into:
   `unity/Assets/Rts/MapEditor/ArtPack/ProjectAegis_MapVisualArtPack_v1/`

3. Do not rename the folder unless you also update `AegisMapArtPack.Root`. The safer path is to keep the v1 folder name because the current code expects it.

4. Verify the following files exist after install:
   - `unity/Assets/Rts/MapEditor/ArtPack/ProjectAegis_MapVisualArtPack_v1/manifest.json`
   - `unity/Assets/Rts/MapEditor/ArtPack/ProjectAegis_MapVisualArtPack_v1/Materials/semantic_materials.json`
   - `unity/Assets/Rts/MapEditor/ArtPack/ProjectAegis_MapVisualArtPack_v1/Terrain/forest_grass_albedo.png`
   - `unity/Assets/Rts/MapEditor/ArtPack/ProjectAegis_MapVisualArtPack_v1/Terrain/forest_grass_normal.png`
   - `unity/Assets/Rts/MapEditor/ArtPack/ProjectAegis_MapVisualArtPack_v1/Meshes/Cliffs/cliff_straight_01.glb`
   - `unity/Assets/Rts/MapEditor/ArtPack/ProjectAegis_MapVisualArtPack_v1/Meshes/BasePads/base_pad_14x14.glb`

PHASE 2 — Wire semantic textures into visual themes

The current visual theme may still use color-only rules for most terrain roles. Update `AegisBiomeVisualTheme` so the following roles include albedo/normal/mask paths from `Materials/semantic_materials.json`:

- `terrain.grass` -> `Terrain/forest_grass_*`
- `terrain.dark_grass` -> `Terrain/forest_grass_dark_patch_*`
- `terrain.dirt` -> `Terrain/dirt_path_*`
- `terrain.gravel` -> `Terrain/gravel_path_*`
- `terrain.mud` -> `Terrain/muddy_bank_*`
- `terrain.shallow_water` -> `Terrain/shallow_water_*`
- `terrain.deep_water` -> `Terrain/deep_water_*`
- `terrain.cliff_ground` -> `Terrain/cliff_ground_*`
- `terrain.ore_stained_soil` -> `Terrain/ore_stained_soil_*`
- `terrain.concrete_base_pad` -> `Terrain/concrete_base_pad_*`
- `road.dirt` -> `Terrain/dirt_path_*`
- `road.gravel` -> `Terrain/gravel_path_*`
- `river.water` -> `Terrain/shallow_water_*`
- `river.shoreline` -> `Terrain/muddy_bank_*`
- `basepad.panel` -> `Terrain/concrete_panel_*`
- `basepad.trim` -> `Terrain/concrete_trim_*`
- `basepad.corner` -> `Terrain/concrete_trim_*`
- `blocker.rock` and cliff roles -> `Terrain/cliff_ground_*` or `Terrain/rough_ground_*`

Do this by passing `albedo`, `normal`, and `mask` arguments to theme `.Add(...)` calls, or by adding a helper that loads `Materials/semantic_materials.json` and applies those paths to the theme. Keep it deterministic and editor-safe.

PHASE 3 — Confirm mesh filename compatibility

Verify `AegisMapArtPack` can still find these referenced meshes:

Cliffs:
- `Meshes/Cliffs/cliff_straight_01.glb`
- `Meshes/Cliffs/cliff_straight_02.glb`
- `Meshes/Cliffs/cliff_wall_tall_01.glb`
- `Meshes/Cliffs/cliff_wall_low_01.glb`
- `Meshes/Cliffs/cliff_endcap_01.glb`
- `Meshes/Cliffs/cliff_spire_cluster_01.glb`
- `Meshes/Cliffs/cliff_spire_cluster_02.glb`
- `Meshes/Cliffs/cliff_corner_inner_01.glb`
- `Meshes/Cliffs/cliff_corner_outer_01.glb`

Rocks/resources/vegetation/river/craters/base pads should also resolve through existing `AegisMapArtPack` arrays or through the visual compiler rules.

PHASE 4 — Make generated maps actually use the visual compiler

1. Confirm `Project Aegis > Map Editor > Visual Compiler` exists.
2. Confirm `Project Aegis > Map Editor > Open Map Editor` can generate an `.aegismap.json` map.
3. Confirm generated maps can be compiled into Unity preview visuals.
4. Confirm the preview uses imported art-pack textures/meshes rather than only fallback colors/cubes.
5. If necessary, update `AegisMapVisualBuilder` so it delegates to `AegisMapVisualCompiler` by default, not the old flat-stamp path.
6. Keep old builder code only as a fallback compatibility path.

PHASE 5 — Fix the visible quality problems

Focus on process, not random art stamps:
- terrain should be chunk/theme driven, not one stretched debug texture
- roads should use road body + edge wear + tire ruts, not foggy smears
- rivers should use water + shoreline mud/wetness + reeds/rocks near banks
- cliffs should come from topology, not random gray blobs
- resources should render as field clusters with dust/glints and amount-based density
- base pads should use the modular 14x14 pad mesh and trim/seam/grime decals
- scatter should avoid roads, water, start pads, and buildable base zones

PHASE 6 — Add a local art-pack validation command or editor menu

Add or update a validation menu/tool that reports:
- art pack root exists
- manifest exists
- semantic_materials.json exists
- required terrain textures exist
- required GLB meshes exist
- required decals exist
- themes have texture paths for core terrain roles
- visual compiler can compile at least one sample map without using only fallback geometry

Suggested menu:
`Project Aegis > Map Editor > Validate Visual Art Pack`

PHASE 7 — Generate one curated preview scene/screenshot locally

Create or update a preview capture workflow for one sample generated map. Put generated screenshots in an ignored local folder unless intentionally curated.

Do not stage broad generated screenshot folders.

PHASE 8 — Validation

Run from repo root:

`dotnet restore src/Rts.Core.Tests/Rts.Core.Tests.csproj`

`dotnet build src/Rts.Core.Tests/Rts.Core.Tests.csproj --no-restore`

`dotnet run --project src/Rts.Core.Tests/Rts.Core.Tests.csproj`

Run Tiled export validation:

`"C:\Program Files\Tiled\tiled.exe" --export-map json --embed-tilesets --resolve-types-and-properties "unity/Assets/Rts/Maps/Tiled/sample_small_100.tmx" "unity/Assets/Rts/Maps/Generated/sample_small_100.local-export.tiled.json"`

Then remove:

`Remove-Item "unity/Assets/Rts/Maps/Generated/sample_small_100.local-export.tiled.json" -ErrorAction SilentlyContinue`

Run Unity batch compile:

`& "E:\Unity\Hub\Editor\6000.5.1f1\Editor\Unity.exe" -batchmode -quit -projectPath "E:\OpenRA Mod\ProjectAegisRTS\unity" -logFile "E:\OpenRA Mod\ProjectAegisRTS\unity-compile.log"`

Inspect the log tail and remove the log before staging:

`Get-Content "E:\OpenRA Mod\ProjectAegisRTS\unity-compile.log" -Tail 200`

`Remove-Item "unity-compile.log" -ErrorAction SilentlyContinue`

PHASE 9 — Git hygiene

Stage only intended files:
- art pack files under `unity/Assets/Rts/MapEditor/ArtPack/ProjectAegis_MapVisualArtPack_v1/`
- Unity `.meta` files generated for the art pack
- visual theme/compiler updates under `unity/Assets/Rts/Scripts/MapEditor/`
- docs if updated
- `.gitignore` if updated

Do not stage:
- `.vs/`
- `build/`
- zip files
- `unity-compile.log`
- `*.local-export.tiled.json`
- broad generated screenshot folders
- C&C / Red Alert files
- OpenRA copied code
- Stage 1 zip/package files

Commit only if validation passes.

Commit message:

`Install rebuilt map visual art pack`

Push branch:

`git push -u origin codex/install-v2-map-artpack`

Final report must include:
- branch
- commit hash
- push result
- .NET test result
- Tiled export result
- Unity compile result
- art pack validation result
- whether themes now use texture paths
- whether maps compile with imported art instead of fallback-only visuals
- remaining visual quality issues
- confirmation no protected IP was used
- confirmation `src/Rts.Core` remains Unity-free
