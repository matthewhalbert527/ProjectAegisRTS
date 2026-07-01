# Stage 18.5 Fine Placement Grid Design

## Purpose

Stage 18.5 doubles building placement resolution without changing the visible board size or shrinking/growing existing buildings. A building that previously occupied a 2 x 2 coarse footprint still covers the same board area, but it now occupies a 4 x 4 fine placement footprint.

This gives placement room for future 1 x 1, 2 x 1, 3 x 3, and other small fine-cell structures while preserving the Stage 0-18 gameplay feel.

## Coordinate Model

- Coarse cells remain the map, terrain, movement, fog, resources, and high-level command grid.
- Fine placement cells are authoritative for building placement and building occupancy.
- `PlacementGridScale` is `2`.
- One coarse cell maps to a 2 x 2 block of fine placement cells.
- A coarse 32 x 32 map exposes a 64 x 64 placement grid.

Conversion examples:

| Legacy footprint | Fine placement footprint | Physical size |
| --- | --- | --- |
| 1 x 1 coarse | 2 x 2 fine | unchanged |
| 2 x 2 coarse | 4 x 4 fine | unchanged |
| 3 x 2 coarse | 6 x 4 fine | unchanged |
| 3 x 3 coarse | 6 x 6 fine | unchanged |

## Rts.Core Authority

`Rts.Core` owns the fine placement model through `PlacementGridMetrics`, `BuildingDefinition.PlacementFootprintCells`, `ActorState.PlacementTopLeftCell`, and fine-cell occupancy in `GridMap`.

Building placement commands now interpret `TopLeftCell` as a fine placement coordinate. Existing bootstrap/demo paths that create actors at coarse cells still work because they convert through the placement grid helpers.

The core validates:

- fine-cell bounds,
- fine-cell footprint size,
- fine-cell terrain/buildability,
- fine-cell building occupancy,
- build radius converted to placement cells,
- partial overlaps at fine offsets.

The coarse occupancy projection remains available so movement and pathing continue to treat building-covered coarse cells as blocked.

## Unity Presentation

`BoardCoordinateMapper` keeps the physical board scale stable by halving the placement cell size relative to the coarse cell size. `BoardRenderer` draws thin fine-grid lines and emphasizes every second line as a coarse boundary. Placement previews render each fine footprint cell, so a 4 x 4 fine power plant preview covers the same board area as the old 2 x 2 coarse preview.

Mouse and hand-ray placement snap to fine cells while normal selection, move, attack, and other command targeting continue to use coarse command cells.

## Compatibility Notes

- Pathfinding remains coarse for Stage 18.5, but it respects fine building occupancy through the coarse blocked projection.
- Existing actor and building definitions keep their legacy coarse footprints and receive converted fine footprints automatically.
- Explicit future fine footprints can be added without changing the board scale.
- Unity visual definitions and generated blockout prefabs remain presentation-only and do not alter authoritative placement.

## Player-Facing Behavior

Players should see a denser placement grid and finer snapping during building placement. Buildings should not look smaller. The desktop placement panel and XR placement panel show fine footprint details only while placement is active, keeping the default HUD clean.

## Stage 20 Visual Relationship

Stage 20 production proxies keep the same physical footprint relationship established here. Foundations align to the authoritative fine-grid footprint, while upper visual detail can be inset, raised, tiered, or beveled for readability. The proxy art must not imply a larger gameplay footprint than the placement preview validates.
