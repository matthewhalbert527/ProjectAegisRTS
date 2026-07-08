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

to verify production-preview defaults, texture-role bindings, denser terrain detail decals, organic terrain-transition feather meshes, merged water strips, production water ribbon meshes, muddy water-surface detail, shoreline bank meshes plus water-edge and eroded-bank detail patches, weathered bridge texture bindings, bridge/fording handling for road-water crossings, road/base-pad detail decals, capped resource glints, and non-fallback sample output.

The compiler reads art-pack textures and prefabs without rewriting texture importer metadata during validation. Existing embedded GLB materials are preserved when present; fallback materials are only assigned to missing material slots.

## Roads And Base Pads

The logical map does not store Unity-only road meshes. The visual builder derives deterministic soft road visuals from the same generated start-to-center route segments used for the terrain path texture. In production preview, road-adjacent dirt terrain is visually naturalized back into grass so the road compiler owns the visible path surface. Production road bodies use narrower, lighter organic meshes with stronger edge jitter and dimension-scaled road/dust UVs rather than opaque rectangular strips, then layer worn shoulders, paired rut decals, and occasional mud-track scuffs without changing pathability. Road-water crossings add deterministic bridge deck seams, side posts, and soft under-shadows on top of the prototype deck and rail pieces.

Base pads are also visual-only dressing on top of player start metadata. Each generated pad uses the imported `base_pad_14x14.glb` when available, then receives concrete panels, transparent panel/trim markings, thin seam decals, hairline cracks, a dusty approach apron facing the map center, construction-wear decals, and deterministic grime marks so start areas read less like flat placeholder slabs. Missing pad mesh or missing concrete texture paths produce validation warnings.

Bridge crossings are still production-proxy visuals, but they now use original weathered deck and rail texture sets instead of the older concrete placeholder material. Crossings are split into deck spans, segmented side rails, posts, abutment blocks, under-shadows, edge-wear decals, and wet approach grime so close-up previews read as built crossings rather than single flat slabs.

## Terrain Detail Decals

`Production Terrain Detail Decals` is a deterministic visual-only layer placed above the logical terrain surface. It adds grass mottling, fine grass marks, dry-grass flecks, low-opacity surface shadow, roadside dust, wet mud near water, gravel/rubble speckles, and subtle water highlights. These decals are still bounded by a deterministic placement cap, but they are denser and smaller than the first production pass so close-up grass and dirt read less uniform. Production detail decals use small irregular mesh silhouettes instead of plain rectangular quads so their transparent texture edges break up more naturally at close zoom.

The art-pack terrain texture baseline has also been regenerated with original procedural detail for forest grass, dark grass, dirt paths, gravel/rough ground, muddy banks, compacted roads, and muddy river water. The forest grass and dark-grass sets now use sharper deterministic blade strokes with matching normal/roughness maps, while the road and river sets carry compacted soil grain, gravel speckles, mud streaking, suspended water silt, subtle ripple response, and broader tonal variation before any decals are applied. This is still production-proxy terrain art, but it removes the old flat green-plane, tight texture grid, and flat-blue river read at tactical zoom.

Production terrain transitions now use transparent blend roles (`terrain.blend_grass`, `terrain.blend_dirt`, `terrain.blend_gravel`, and `terrain.blend_mud`) with deterministic offset, width variation, and organic feather meshes. Debug mode still shows literal terrain-role transitions. Production preview naturalizes rough/cliff terrain to softer grass/dirt base surfaces and uses a single softened base role for mixed chunks. Production terrain chunks also use slower world-continuous UVs, so tileable grass and dirt textures flow across chunk boundaries without resetting every patch or repeating as a visible tactical-grid pattern. Roughness reads through rubble speckles, rocks, wet banks, and cliff props rather than checkerboard terrain cells at close zoom.

The detail overlay now has two scales: low-alpha macro terrain variation patches break up broad grass and dirt fields, then smaller grass, dry-grass, shadow, pebble, wet-bank, and water highlight decals add local surface detail. The macro roles are intentionally subtle so they add ground variation without turning the map into a cloudy wash.

Cliff edges now receive deterministic talus dressing in production preview: transparent rubble decals at exposed cliff feet plus occasional imported pebble-cluster meshes selected from the art pack. Battlefield crater and rubble scatter use textured decals or crater meshes instead of color-only generated primitives, keeping damaged/rocky areas from reading as flat gray blocks.

Road rendering uses layered deterministic visual-only overlays: soft dust sits under a narrower textured road core, and tire ruts/worn edges sit above the core. Non-water road runs now render as gently curved ribbon meshes with deterministic centerline drift and variable width, so long generated roads no longer read as perfect straight rectangular bands. The road core uses a dedicated `road_compacted` albedo/normal/roughness set with lower road-specific UV tiling, which keeps close-up road surfaces closer to granular compacted soil and avoids the over-repeated plank-like read from broader dirt textures. Bridge crossings remain production-proxy geometry, but the preview uses a weathered deck material, segmented rails, abutments, deck seams, posts, tapered approach-dust/road-wear ribbons, edge-wear patches, and wet-road bank smears instead of one flat rectangular road slab.

## Water Rendering

The logical map still stores water as deterministic terrain cells. In production preview, the base terrain layer does not draw separate water cell quads; the water compiler owns the visible water surface. It converts contiguous water rows into smoothed ribbon meshes and retains strip metrics for validation. The water surface now uses a dedicated `river_muddy_water` texture set and overlays deterministic deep-pool, silt-flow, and midstream ripple detail patches so close-up rivers read closer to muddy battlefield streams than flat cyan topology. Shoreline mud/wetness is emitted as deterministic left/right bank meshes plus end caps that follow the smoothed water span. The shoreline now uses a stronger wet core and a wider lighter outer feather, both using transparent river decal textures instead of tiled muddy terrain albedo, so the river edge reads less like a hard dark trench. Water-edge depth/shallow/ripple detail is emitted as broken deterministic patch decals rather than continuous contour ribbons, and eroded-bank decals add muddy undercuts plus sparse wet-pebble scatter on both sides of the stream. This keeps close-up rivers from showing artificial outline bands or clean polygon cuts. Production terrain also remaps rough/cliff river shoulders to lighter dirt so wet-bank meshes read as edge detail instead of huge square mud slabs. Roads crossing water are split and represented with neutral `bridge_prototype_*` deck/rail pieces until final bridge or ford art exists.

## Current Limits

- The compiler now has layer contracts and summaries, but terrain chunks are still prototype quads rather than a final shader/material-layer terrain.
- Terrain detail decals and transparent transition blends now use organic mesh silhouettes, but true realism still needs a shader-driven terrain blend, height/normal-aware terrain layers, or authored terrain meshes.
- The current generated texture sets are original production-proxy terrain assets. Final shipping quality should still replace or refine them with approved art-directed shader layers and material blending.
- Roads are generated as deterministic routes between player starts and the map center. A later pass should read explicit road/region/path metadata when map documents include it.
- Water is now rendered from smoothed water-cell ribbon meshes with a muddy-water material, internal surface detail patches, shoreline bank meshes, eroded-bank decals, and wet pebble scatter. A later pass can replace this with authored river splines, animated water materials, reeds, foam, and hand-tuned shoreline decals.
- Ore, cliff, vegetation, crater, river-edge, and base-pad props use imported production-proxy art-pack assets when available. A later art pass can add higher-poly sculpted meshes, LODs, collision-free prefab variants, and tuned material overrides.

## Asset Rules

Use the provided reference image only as a quality target. Do not copy protected art, textures, names, UI, map data, or faction identifiers from Command & Conquer, Red Alert, OpenRA, or any other reference project.

Production-quality final art should be original Project Aegis content, generated under clear rights, purchased/licensed, or created in-house.
