# Map Visual Compiler Plan

## Implemented In This Pass

- `AegisMapVisualCompiler` orchestrates layer compilation from `.aegismap.json`.
- Runtime visual contracts define themes, compile context, compile result, and layer summaries.
- Visual render settings define `ProductionPreview`, `DebugOverlay`, and `Hybrid`; production preview is the default.
- Prototype themes define semantic roles for debug, forest, and desert visual rules.
- Terrain builds as production chunks below the old 16x16 debug behavior; mixed chunks are split into semantic patches with explicit transition masks.
- Water and shoreline compile as merged visual strips instead of raw per-cell corridors.
- Roads compile as deterministic path segments with body, tire-track overlays, and prototype bridges for water crossings.
- Cliffs compile from exposed topology edges with straight/corner/endcap counts while raw blocker fill is hidden in production mode.
- Resources compile as stable visual fields with amount-based density, dust/stain decals, and capped glints.
- Base pads compile as imported or procedural textured panel, trim, corner, seam, grime, and wear pieces.
- Scatter compiles as clustered semantic placements and skips water, roads, resources, and start-protected cells.
- `Project Aegis > Map Editor > Visual Compiler` opens a dedicated compiler window.
- `Project Aegis > Map Editor > Validate Visual Quality Gate` runs production-preview gate checks.
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
- Replace `bridge_prototype_*` generated geometry with original authored bridge or ford assets.
- Add final prefabs and LODs for cliffs, rocks, resources, pads, vegetation, rubble, and shore props.
- Add debug overlay rendering for pathability/fairness summaries once the Unity preview UX is settled.
- Add automated image-diff or pixel histogram QA for canonical preview scenarios.

## Non-Goals

- Do not move visual code into `src/Rts.Core`.
- Do not make Unity scene output authoritative map data.
- Do not add network AI/API dependencies.
- Do not copy protected RTS art, names, UI, map data, or implementation code.
