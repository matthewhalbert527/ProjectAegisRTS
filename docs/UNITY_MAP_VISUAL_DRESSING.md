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
4. The generated scene includes compiler layers for base terrain chunks, terrain transition masks, production terrain detail decals, water surfaces, shoreline mud/wetness, roads and tire tracks, topology-driven cliffs, resource fields, modular base pads, and rule-based scatter.
5. The compiler window can capture a local screenshot to `%TEMP%\ProjectAegisRTS\VisualCompilerPreviews\`.

Production preview is the default visual mode. Debug overlays are available through `DebugOverlay` or `Hybrid` mode, but they are not production output.

The visual seed is derived from the map identity and dimensions unless overridden in the compiler window, so the same map and seed produce the same dressing layout.

The batch preview validation uses `sample_ai_medium_forest_2p_river_chokepoint.aegismap.json`, a checked-in deterministic sample with river water, cliff/blocker bands, ore clusters, and two connected player starts. This gives the render check a map that exercises the terrain detail layers visible when zoomed in without overcrowding the preview with route lines.

The art-pack showcase sample is:

`unity/Assets/Rts/MapEditor/Samples/sample_art_pack_showcase_160_forest_river.aegismap.json`

It is a 160x160 forest river composition intended to exercise the imported art pack in one place: concrete base pads, soft roads, river bends, muddy banks, cliff and rock ridges, ore clusters, craters, vegetation, and navigation/region metadata. The batch screenshot method is `ProjectAegisRTS.UnityClient.EditorTools.AegisMapVisualBuilder.RenderArtPackShowcaseForBatch`, which writes `C:\Users\matth\AppData\Local\Temp\ProjectAegisRTS\aegis_art_pack_showcase.png`.

For close-up inspection, `ProjectAegisRTS.UnityClient.EditorTools.AegisMapVisualBuilder.RenderProductionDetailPreviewForBatch` writes `C:\Users\matth\AppData\Local\Temp\ProjectAegisRTS\aegis_visual_quality_detail.png`. Use this capture when judging zoomed-in pad, bridge, resource, road, and shoreline fidelity.

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

to verify production-preview defaults, texture-role bindings, terrain detail decals, merged water strips, production water ribbon meshes, bridge/fording handling for road-water crossings, road/base-pad detail decals, capped resource glints, and non-fallback sample output.

The compiler reads art-pack textures and prefabs without rewriting texture importer metadata during validation. Existing embedded GLB materials are preserved when present; fallback materials are only assigned to missing material slots.

## Roads And Base Pads

The logical map does not store Unity-only road meshes. The visual builder derives deterministic soft road decals from the same generated start-to-center route segments used for the terrain path texture. These overlays add dust, worn shoulders, paired rut decals, and occasional mud-track scuffs without changing pathability.

Base pads are also visual-only dressing on top of player start metadata. Each generated pad uses the imported `base_pad_14x14.glb` when available, then receives concrete panels, transparent panel/trim markings, thin seam decals, hairline cracks, a dusty approach apron facing the map center, construction-wear decals, and deterministic grime marks so start areas read less like flat placeholder slabs. Missing pad mesh or missing concrete texture paths produce validation warnings.

## Terrain Detail Decals

`Production Terrain Detail Decals` is a deterministic visual-only layer placed above the logical terrain surface. It adds low-density grass mottling, roadside dust, wet mud near water, gravel/rubble speckles, and subtle water highlights. These decals are intentionally sparse and soft; they reduce the top-down checkerboard feel without pretending to replace final terrain blending.

Production terrain transitions now use transparent blend roles (`terrain.blend_grass`, `terrain.blend_dirt`, `terrain.blend_gravel`, and `terrain.blend_mud`) with deterministic offset and width variation. Debug mode still shows literal terrain-role transitions. Production preview also suppresses isolated rough cells surrounded by softer terrain so mixed terrain samples read less like checkerboards at close zoom.

## Water Rendering

The logical map still stores water as deterministic terrain cells. In production preview, the base terrain layer does not draw separate water cell quads; the water compiler owns the visible water surface. It converts contiguous water rows into smoothed ribbon meshes and retains strip metrics for validation. Shoreline mud/wetness is now emitted as deterministic left/right bank meshes plus end caps that follow the smoothed water span. Production terrain also remaps rough/cliff river shoulders to lighter dirt so the dark wet-bank mesh reads as edge detail instead of a huge square mud slab. Roads crossing water are split and represented with neutral `bridge_prototype_*` deck/rail pieces until final bridge or ford art exists.

## Current Limits

- The compiler now has layer contracts and summaries, but terrain chunks are still prototype quads rather than a final shader/material-layer terrain.
- Terrain detail decals and transparent transition blends improve the current preview, but true realism still needs a shader-driven terrain blend, height/normal-aware terrain layers, or authored terrain meshes.
- Roads are generated as deterministic routes between player starts and the map center. A later pass should read explicit road/region/path metadata when map documents include it.
- Water is now rendered from smoothed water-cell ribbon meshes with shoreline bank meshes. A later pass can replace this with authored river splines, animated water materials, reeds, foam, and hand-tuned shoreline decals.
- Ore, cliff, vegetation, crater, river-edge, and base-pad props use imported production-proxy art-pack assets when available. A later art pass can add higher-poly sculpted meshes, LODs, collision-free prefab variants, and tuned material overrides.

## Asset Rules

Use the provided reference image only as a quality target. Do not copy protected art, textures, names, UI, map data, or faction identifiers from Command & Conquer, Red Alert, OpenRA, or any other reference project.

Production-quality final art should be original Project Aegis content, generated under clear rights, purchased/licensed, or created in-house.
