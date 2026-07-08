# Unity Map Visual Dressing

## Purpose

Project Aegis maps remain authored and validated as `.aegismap.json`. The Unity visual dressing pass is now organized as an editor-side visual compiler that turns that logical document into a layered RTS battlefield scene for inspection and iteration.

The visual pass does not change gameplay data. It reads player starts, terrain, blockers, resources, and map metadata from the selected `.aegismap.json` file and creates deterministic scene objects.

## Implemented Now

Use either:

`Project Aegis > Map Editor > Build Visual Terrain From Aegis Map`

or:

`Project Aegis > Map Editor > Visual Compiler`

Workflow:

1. Select a `.aegismap.json` asset in Unity.
2. Run the compatibility build menu item, or open the visual compiler window and click `Compile Preview`.
3. Unity creates an `Aegis Visual Map - <mapId>` scene object with `AegisMapVisualScene` summary metadata.
4. The generated scene includes compiler layers for base terrain chunks, terrain transition masks, water surfaces, shoreline mud/wetness, roads and tire tracks, topology-driven cliffs, resource fields, modular base pads, and rule-based scatter.
5. The compiler window can capture a local screenshot to `%TEMP%\ProjectAegisRTS\VisualCompilerPreviews\`.

Production preview is the default visual mode. Debug overlays are available through `DebugOverlay` or `Hybrid` mode, but they are not production output.

The visual seed is derived from the map identity and dimensions unless overridden in the compiler window, so the same map and seed produce the same dressing layout.

The batch preview validation uses `sample_ai_medium_forest_2p_river_chokepoint.aegismap.json`, a checked-in deterministic sample with river water, cliff/blocker bands, ore clusters, and two connected player starts. This gives the render check a map that exercises the terrain detail layers visible when zoomed in without overcrowding the preview with route lines.

The art-pack showcase sample is:

`unity/Assets/Rts/MapEditor/Samples/sample_art_pack_showcase_160_forest_river.aegismap.json`

It is a 160x160 forest river composition intended to exercise the imported art pack in one place: concrete base pads, soft roads, river bends, muddy banks, cliff and rock ridges, ore clusters, craters, vegetation, and navigation/region metadata. The batch screenshot method is `ProjectAegisRTS.UnityClient.EditorTools.AegisMapVisualBuilder.RenderArtPackShowcaseForBatch`, which writes `C:\Users\matth\AppData\Local\Temp\ProjectAegisRTS\aegis_art_pack_showcase.png`.

## Biome Profiles

The builder includes original Project Aegis color/material profiles for:

- forest / grassland
- desert
- tundra
- volcanic
- rocky / wasteland

Profiles currently drive terrain colors, mud banks, water tones, cliff colors, path colors, ore-stained soil, vegetation, concrete, pebble roughness, and crater materials.

## Imported Art Pack

`ProjectAegis_MapVisualArtPack_v1` is checked in under:

`unity/Assets/Rts/MapEditor/ArtPack/ProjectAegis_MapVisualArtPack_v1/`

The visual compiler reads the pack directly from this folder. It uses the original PNG terrain/material textures and transparent decals where practical and instantiates the pack's `.glb` meshes for prototype cliffs, boulders, ore clusters, vegetation, river-edge props, craters, and base-pad pieces where available.

This branch uses Unity glTFast (`com.unity.cloud.gltfast`) so those `.glb` files import directly in Unity. If the package is removed or unavailable on a future machine, the compiler keeps working by falling back to deterministic generated geometry for those props. The installed contents are the rebuilt v2 production-proxy pack inside the compatibility `ProjectAegis_MapVisualArtPack_v1` folder.

Use:

`Project Aegis > Map Editor > Validate Visual Art Pack`

to verify the root, manifest, semantic material map, required terrain textures, GLB meshes, decals, theme texture paths, and sample compiler output.

Use:

`Project Aegis > Map Editor > Validate Visual Quality Gate`

to verify production-preview defaults, texture-role bindings, merged water strips, bridge/fording handling for road-water crossings, capped resource glints, and non-fallback sample output.

## Roads And Base Pads

The logical map does not store Unity-only road meshes. The visual builder derives deterministic soft road decals from the same generated start-to-center route segments used for the terrain path texture. These overlays add dust, paired rut strips, and occasional gravel scuffs without changing pathability.

Base pads are also visual-only dressing on top of player start metadata. Each generated pad uses the imported `base_pad_14x14.glb` when available, then receives concrete panels, thin seam decals, trim strips, a dusty approach apron facing the map center, and deterministic grime marks so start areas read less like flat placeholder slabs. Missing pad mesh or missing concrete texture paths produce validation warnings.

## Water Rendering

The logical map still stores water as deterministic terrain cells. The visual compiler merges water cells into wider strips and emits softened shoreline mud/wetness strips so production rivers no longer show raw cell borders. Roads crossing water are split and represented with neutral `bridge_prototype_*` deck/rail/shadow pieces until final bridge or ford art exists.

## Current Limits

- The compiler now has layer contracts and summaries, but terrain chunks are still prototype quads rather than a final shader/material-layer terrain.
- Roads are generated as deterministic routes between player starts and the map center. A later pass should read explicit road/region/path metadata when map documents include it.
- Water is rendered from water-cell topology with shoreline masks. A later pass can replace this with spline meshes, animated water materials, reeds, foam, and authored shoreline decals.
- Ore, cliff, vegetation, crater, river-edge, and base-pad props use imported production-proxy art-pack assets when available. A later art pass can add higher-poly sculpted meshes, LODs, collision-free prefab variants, and tuned material overrides.

## Asset Rules

Use the provided reference image only as a quality target. Do not copy protected art, textures, names, UI, map data, or faction identifiers from Command & Conquer, Red Alert, OpenRA, or any other reference project.

Production-quality final art should be original Project Aegis content, generated under clear rights, purchased/licensed, or created in-house.
