# Map Visual Art Direction

## Direction

Project Aegis maps should read like grounded tactical battlefields: clear bases, readable routes, visible resource fields, sharp movement blockers, and terrain detail that supports decisions rather than hiding them.

The installed `ProjectAegis_MapVisualArtPack_v1` folder now contains the rebuilt v2 production-proxy art packet. The folder name intentionally remains `ProjectAegis_MapVisualArtPack_v1` because the Unity compiler root points there. Treat these assets as production-proxy art: good enough for map readability and compiler validation, but still subject to final art review before they become final shipping art.

## Current Texture Baseline

The terrain texture baseline now uses original procedural production-proxy maps for forest grass, dark grass, dirt paths, gravel/rough ground, muddy banks, compacted road soil, muddy river water, and weathered bridge decking/rails. These textures are more detailed than the initial flat color tiles: they include multi-scale tonal variation, compacted soil grain, gravel speckles, sharper grass blade strokes, suspended silt, weathered deck grain, regenerated normal maps, and roughness/AO masks. This keeps close-up previews from relying only on transparent decals to hide a flat terrain plane, flat blue water, or single-color bridge slab.

Terrain detail decals remain a secondary layer. They add small deterministic grass, dry-grass, shadow, soil, wet-bank, and rubble marks on top of the terrain textures, but the texture set itself now carries the first layer of realism.

## Readability Priorities

- Player starts must be immediately legible through modular base pads and clean surrounding construction space.
- Resources must read as fields with density and depletion state, not isolated sparkly rocks.
- Cliffs must read as continuous height/blocker edges with straight pieces, corners, and endcaps.
- Roads must guide movement without becoming noisy brown fog; close-up roads should include worn shoulders, rutting, grass encroachment, and exposed pebble/bare-soil breakup rather than clean opaque strips.
- Rivers must show a merged muddy water body, internal silt/deep-pool/ripple variation, shoreline wetness, eroded bank undercuts, sparse wet pebble scatter, broken water-edge depth/ripple details, and readable weathered bridge or shallow ford hints with bank contact props where gameplay allows crossing.
- Scatter must support biome and topology: trees on suitable terrain, rocks near cliffs, rubble near craters, and no clutter in start/build zones.

## Biomes

Implemented prototype themes:

- `DebugVisualTheme`
- `ForestPrototypeVisualTheme`
- `DesertPrototypeVisualTheme`

Future themes should keep the same semantic roles and only change asset/material bindings.

## Current Limits

- The compiler now has chunk/layer semantics, but terrain rendering is still a prototype scene-object implementation rather than a final shader-layer terrain.
- Production preview now hides debug overlays by default and uses smaller/mixed terrain chunks, slower world-continuous terrain UVs, macro and micro terrain detail overlays, strip-merged water with patch-based edge detail, weathered bridge prototypes with segmented rails, abutments, deck dust, bank contact dressing, road-edge breakup overlays, capped resource glints, procedural terrain textures, layered surface detail, and clustered scatter.
- The installed v2 art pack provides semantic terrain textures, decals, and GLB meshes, but it is still production-proxy content rather than final hand-authored shipping art.
- Cliff, river, and resource assets need final sculpted/prefab variants.
- Camera capture is functional QA, not final promotional rendering.

## IP Guardrails

Use references only as quality targets. Do not copy protected art, UI, map data, faction labels, file formats, or implementation code from Command & Conquer, Red Alert, OpenRA, or other RTS projects.
