# Map Visual Art Direction

## Direction

Project Aegis maps should read like grounded tactical battlefields: clear bases, readable routes, visible resource fields, sharp movement blockers, and terrain detail that supports decisions rather than hiding them.

The installed `ProjectAegis_MapVisualArtPack_v1` folder now contains the rebuilt v2 production-proxy art packet. The folder name intentionally remains `ProjectAegis_MapVisualArtPack_v1` because the Unity compiler root points there. Treat these assets as production-proxy art: good enough for map readability and compiler validation, but still subject to final art review before they become final shipping art.

## Readability Priorities

- Player starts must be immediately legible through modular base pads and clean surrounding construction space.
- Resources must read as fields with density and depletion state, not isolated sparkly rocks.
- Cliffs must read as continuous height/blocker edges with straight pieces, corners, and endcaps.
- Roads must guide movement without becoming noisy brown fog.
- Rivers must show a merged water body, shoreline wetness, and shallow ford or bridge hints where gameplay allows crossing.
- Scatter must support biome and topology: trees on suitable terrain, rocks near cliffs, rubble near craters, and no clutter in start/build zones.

## Biomes

Implemented prototype themes:

- `DebugVisualTheme`
- `ForestPrototypeVisualTheme`
- `DesertPrototypeVisualTheme`

Future themes should keep the same semantic roles and only change asset/material bindings.

## Current Limits

- The compiler now has chunk/layer semantics, but terrain rendering is still a prototype scene-object implementation rather than a final shader-layer terrain.
- Production preview now hides debug overlays by default and uses smaller/mixed terrain chunks, strip-merged water, bridge prototypes, capped resource glints, and clustered scatter.
- The installed v2 art pack provides semantic terrain textures, decals, and GLB meshes, but it is still production-proxy content rather than final hand-authored shipping art.
- Cliff, river, and resource assets need final sculpted/prefab variants.
- Camera capture is functional QA, not final promotional rendering.

## IP Guardrails

Use references only as quality targets. Do not copy protected art, UI, map data, faction labels, file formats, or implementation code from Command & Conquer, Red Alert, OpenRA, or other RTS projects.
