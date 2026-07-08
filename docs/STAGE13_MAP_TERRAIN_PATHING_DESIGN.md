# Stage 13 Map Terrain Pathing Design

Stage 13 adds deterministic map metadata and path diagnostics without replacing the existing movement model.

## Core Boundary

- `Rts.Core` owns terrain kind, movement class, passability, movement cost, map validation, and path query diagnostics.
- Unity reads `MapSnapshot` and `PathDebugSnapshot` data only.
- Unity terrain overlays, path lines, HUD buttons, and authoring placeholders do not mutate authoritative terrain or actor state.

## Terrain Model

The default terrain catalog includes clear, road, rough, forest, water, cliff, and ore field cells. Movement classes are infantry, wheeled, tracked, harvester, aircraft, and building. Costs and passability are integer-only and deterministic.

Stage 13 keeps authoring simple: code/demo data can set terrain cells, and future map tooling can build on the `MapAuthoringData` and `MapValidationResult` surfaces.

## Pathing

`GridPathfinder.QueryPath` returns a structured result with success, failure code, movement class, total cost, visited cell count, and path cells. Existing move and harvest orders now use this query path, while clear maps preserve previous behavior.

## Known Limits

- No full map editor UI yet.
- No final terrain art.
- No advanced terrain occlusion, formation movement, or dynamic terrain.
- Unit collision remains intentionally simple for the current prototype.
