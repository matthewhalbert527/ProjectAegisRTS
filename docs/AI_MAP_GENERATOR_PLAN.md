# AI-Assisted Procedural Map Generator

## Scope

Project Aegis now has a deterministic prompt-assisted map generator in `src/Rts.Core/Maps/Generation`. It converts structured controls or simple prompt text into an `AegisMapDocument`, keeping `.aegismap.json` as the runtime source of truth.

The parser is deterministic and local. It does not call an external LLM or network service.

## Supported Controls

- Sizes: `small` `100x100`, `medium` `200x200`, `large` `400x400`, or custom `100..400`.
- Players: `2`, `4`, `6`, or `8`.
- Biomes: grassland, desert, tundra, volcanic, rocky, forest, wasteland.
- Densities: resources, cliffs, and rockiness from none/low/medium/high/extreme style controls.
- Water: none, low, medium, high.
- Symmetry: none, horizontal, vertical, rotational, radial.
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
- validation, buildability, and balance reports

Tiled remains an export/import authoring surface. Generated maps can be exported through the existing Tiled JSON exporter where practical.

## Not Implemented Yet

- A real LLM provider bridge.
- Full artistic biome-specific tile palettes.
- Advanced tactical fairness scoring beyond deterministic start connectivity, resource counts, and build-pad checks.
