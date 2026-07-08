# Map Visual Compiler Plan

## Implemented In This Pass

- `AegisMapVisualCompiler` orchestrates layer compilation from `.aegismap.json`.
- Runtime visual contracts define themes, compile context, compile result, and layer summaries.
- Prototype themes define semantic roles for debug, forest, and desert visual rules.
- Terrain builds as chunks with explicit transition masks rather than one authoritative mega-texture.
- Water and shoreline compile as separate visual layers.
- Roads compile as deterministic path segments with body and tire-track overlays.
- Cliffs compile from exposed topology edges with straight/corner/endcap counts.
- Resources compile as stable visual fields with density-based instances.
- Base pads compile as modular panel, trim, corner, seam, grime, and wear pieces.
- Scatter compiles from semantic rules and skips water, roads, resources, and start-protected cells.
- `Project Aegis > Map Editor > Visual Compiler` opens a dedicated compiler window.
- Preview capture writes to a local temp folder outside the repo.

## Compatibility

`AegisMapVisualBuilder` remains as the existing menu and batch wrapper. Its `BuildScene` path now calls `AegisMapVisualCompiler`, so existing menu items and batch preview methods use the new compiler without changing `.aegismap.json`.

## Layer Order

1. base terrain surface
2. terrain transitions
3. water surface
4. shoreline mud/wetness
5. roads and tire tracks
6. cliffs and elevation edges
7. blockers/rocks
8. resources
9. base pads
10. craters/scorch/rubble
11. vegetation scatter
12. debug overlays

The current compiler combines blockers/rocks, craters/rubble, and vegetation into the topology/scatter layers. The summary model already has the metrics needed to split them into separate compilers later.

## Future Work

- Replace prototype chunk quads with shader-driven terrain layers or Terrain/mesh tiles.
- Add authored road spline metadata to `.aegismap.json` if design needs explicit roads.
- Add authored river spline metadata or flow direction if water quality becomes a blocker.
- Add final prefabs and LODs for cliffs, rocks, resources, pads, vegetation, rubble, and shore props.
- Add debug overlay rendering for pathability/fairness summaries once the Unity preview UX is settled.
- Add automated image-diff or pixel histogram QA for canonical preview scenarios.

## Non-Goals

- Do not move visual code into `src/Rts.Core`.
- Do not make Unity scene output authoritative map data.
- Do not add network AI/API dependencies.
- Do not copy protected RTS art, names, UI, map data, or implementation code.
