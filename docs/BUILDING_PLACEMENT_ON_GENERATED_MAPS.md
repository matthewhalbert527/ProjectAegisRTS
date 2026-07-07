# Building Placement On Generated Maps

## Buildability Rules

Generated maps expose deterministic build spots around each player start through `AegisMapBuildabilityAnalyzer`.

Placement checks reject:

- cells outside map bounds
- blockers
- cliff terrain
- water terrain
- active resource cells
- terrain that does not support construction

The map-level footprint model supports rectangular footprints from `1x1` through `5x5`.

## Base Areas

The generator clears protected base areas around every player start before placing cliffs, rocks, blockers, and resources. Resources are placed near the base, but outside the initial build pad.

Build-pad regions are also written into generated documents as `generated_build_pad` regions so editor previews can show candidate construction locations.

## Validation

Core tests verify 2x2/4x4 style placement surfaces, rejection on blockers/cliffs/resources, clean pads for each start, and large-map buildability performance.
