# AI-Assisted Procedural Map Generator

## Scope

Project Aegis now has a deterministic prompt-assisted map generator in `src/Rts.Core/Maps/Generation`. It converts structured controls or simple prompt text into an `AegisMapDocument`, keeping `.aegismap.json` as the runtime source of truth.

The parser is deterministic and local. It does not call an external LLM or network service.

## Supported Controls

- Sizes: `small` `100x100`, `medium` `200x200`, `large` `400x400`, or custom `100..400`.
- Size prompt synonyms: `tiny` maps warn and use `100x100`; `huge` maps warn and use `400x400`.
- Players: `2`, `4`, `6`, or `8`, including digit and word phrases such as `two player`, `four player`, `six player`, and `eight player`.
- Biomes: grassland, desert, tundra, volcanic, rocky, forest, wasteland.
- Densities: resources, cliffs, and rockiness from none/very-low/low/medium/high/very-high/extreme style controls.
- Water: none, low, medium, high, including phrases such as `some water` and `lots of water`.
- Symmetry: none, horizontal, vertical, rotational, radial, mirrored, symmetric.
- Profiles: open, balanced, chokepoint, defensive, resource-rich, scarce, tournament.
- Seed: explicit integer seed for deterministic output.

## Prompt Examples

- `small rocky map with lots of ore and high cliffs`
- `large 4 player desert map, medium resources, low cliffs`
- `200 by 200 forest map with high rockiness and regenerating ore`

Unknown words produce warnings and fall back to defaults.

## Output

The generator creates:

- map metadata and generation properties
- `terrain_base`
- blockers for generated cliffs and rocks
- player starts
- resource fields with regeneration metadata
- base-area and build-pad regions
- validation, buildability, generation summary, and balance reports

## Implemented Quality Checks

- Generated maps are validated as `AegisMapDocument` before success is returned.
- Player starts are kept off water, cliffs, blockers, and resources.
- Base areas are cleared before blocker/resource placement.
- Resource fields are placed near starts but outside the protected base pad.
- Buildability analysis exposes clean 1x1 through 5x5 footprint support, rectangular footprints, optional padding checks, and generated build-pad regions.
- Balance analysis reports connected start pairs, unreachable starts, min/max path distance, nearby resource amount by player, blocker count, and resource count.
- The generation summary reports width, height, seed, player count, biome, density settings, water, symmetry, profile, resource field count, total resource amount, blocker/cliff/rock counts, build-pad count, warnings, and validation errors.

Tiled remains an export/import authoring surface. Generated maps can be exported through the existing Tiled JSON exporter where practical.

## Not Implemented Yet

- A real LLM provider bridge.
- Full artistic biome-specific tile palettes.
- Advanced tactical fairness scoring beyond deterministic start connectivity, distance spread, nearby resource balance, resource counts, and build-pad checks.
- Direct Unity editor execution of the core generator assembly. The Unity editor currently creates a compatible procedural shell and documents the direct bridge as future work.
