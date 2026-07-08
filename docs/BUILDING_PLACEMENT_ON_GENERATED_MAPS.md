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

`AegisMapBuildabilityAnalyzer.CanPlace` also has an optional padding argument. Padding cells must remain in bounds and buildable, which lets editor tools reserve clearance around larger structures without changing the default placement behavior.

## Base Areas

The generator clears protected base areas around every player start before placing cliffs, rocks, blockers, and resources. Resources are placed near the base, but outside the initial build pad.

Build-pad regions are also written into generated documents as `generated_build_pad` regions so editor previews can show candidate construction locations.

## Validation

Core tests verify 2x2, 3x3, 4x4, and rectangular padded placement surfaces, rejection on blockers/cliffs/resources/out-of-bounds cases, clean pads for each start, and large-map buildability performance.

## Current Limits

Buildability is deterministic map-level analysis. It does not yet reserve dynamic unit traffic lanes, production exit arcs, or final art footprint sockets; those belong in later gameplay/editor passes.
