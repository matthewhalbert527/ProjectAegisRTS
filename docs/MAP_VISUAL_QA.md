# Map Visual QA

## Why The Previous Screenshot Failed

The previous preview communicated that the map generator worked, but close-up quality suffered because terrain, roads, ore staining, and riverbanks were mostly flattened into a single surface treatment with stamps placed on top. The result was readable from far away but weak at tactical zoom.

## Readable RTS Map Criteria

- Bases are clear and construction zones are not cluttered.
- Roads guide movement and connect meaningful areas.
- Resources read as gatherable fields with visible density.
- Cliffs and rocks clearly communicate blocked terrain.
- Rivers show body, shore, and ford/crossing intent.
- Scatter supports biome identity without hiding gameplay information.

## Terrain Blending QA

- Check whether terrain chunks have coherent biome identity.
- Check whether transition masks appear at semantic boundaries.
- Grass-to-road, grass-to-water, road-to-mud, base-pad-to-dirt, and cliff-to-ground transitions should be visible without noisy repetition.
- Block merge if terrain roles are indistinguishable or if debug-like grid artifacts dominate.

## Cliff QA

- Cliff pieces should align with exposed blocker/high-ground edges.
- Straight pieces should follow long edges.
- Corners should appear at turns.
- Endcaps should appear where cliff segments terminate.
- Block merge if cliffs are random piles unrelated to map topology.

## Base Pad QA

- Each player start should have one modular pad.
- Panels, trims, corners, seams, grime, and approach wear should be visible.
- Pads should not be hidden under resources or scatter.
- Block merge if starts are hard to identify or build zones are cluttered.

## Resource QA

- Resources should render as fields, not independent random blobs.
- Field density should scale with amount/fill.
- Depleted fields should not show full visual richness.
- Resources should avoid base pads and start cleanup zones.
- Block merge if resource ownership, density, or depletion state is unreadable.

## Roads And Rivers QA

- Roads should have a body, direction, and tire tracks.
- Road scuffs should be sparse and aligned to route direction.
- Road-water crossings must render as bridge/ford segments or fail/report a warning; road material must not paint directly through river material.
- Rivers should have water body and shoreline wetness.
- Production rivers should be merged/softened enough that raw cell edges are hidden.
- Fords should be shallow/crossing hints only.
- Block merge if roads/rivers look like uniform soft stains with no structure.

## Canonical Preview Captures

Suggested scenarios:

- `forest_river_cliffs`
- `base_pad_ore`
- `road_crater_scatter`
- `tournament_4p`
- `rocky_chokepoint`

Run the compiler window:

`Project Aegis > Map Editor > Visual Compiler`

Or use batch capture methods through Unity batchmode. Preview captures should write to a local ignored temp folder such as:

`%TEMP%\ProjectAegisRTS\VisualCompilerPreviews\`

The curated benchmark capture path is:

`%TEMP%\ProjectAegisRTS\ArtDirectedPreviews\`

Use `Project Aegis > Map Editor > Capture Art-Directed Preview` for the benchmark screenshot. Treat this screenshot as a human visual gate: the automated validators can prove that roads, bridge/fording, river ribbon, base-pad details, and resource dust exist, but they cannot prove the image has final terrain art direction.

The batch method `AegisMapVisualBuilder.RenderProductionAndDebugPreviewsForBatch` renders production and debug-overlay captures separately. Production captures should not show debug/helper layers.

Run `Project Aegis > Map Editor > Validate Visual Quality Gate` or `AegisMapVisualQualityGate.ValidateSampleForBatch` before merging renderer changes.

Do not stage broad screenshot output.

## Merge Blockers

- `src/Rts.Core` references Unity APIs.
- Visual compiler changes make `.aegismap.json` no longer authoritative.
- Tiled export validation fails.
- Unity batch compile fails.
- Protected IP names/assets/code are introduced.
- Preview layers compile without summaries or warnings/errors.
- Topology-driven cliffs, resource fields, or modular base pads regress into random stamp placement.
- Production preview defaults back to debug overlays.
- Roads cross water without bridge/ford handling.
- Resource glints exceed capped field density.
