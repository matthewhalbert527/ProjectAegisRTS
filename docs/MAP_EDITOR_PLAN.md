# Map Editor Plan

## Goals

- Keep deterministic gameplay and map conversion in `src/Rts.Core`.
- Treat Tiled as an authoring tool, not the runtime format.
- Use `.aegismap.json` as the Project Aegis runtime map document.
- Keep Unity scripts focused on editor menus, preview assets, and file workflow helpers.

## Current Implementation

- Core runtime map document: `src/Rts.Core/Maps/AegisMapDocument.cs`
- Core validator: `src/Rts.Core/Maps/AegisMapDocumentValidator.cs`
- Runtime world factory: `src/Rts.Core/Maps/AegisMapDocumentWorldFactory.cs`
- Tiled importer/exporter: `src/Rts.Core/Maps/Tiled`
- Procedural prompt generator, parser, buildability analyzer, resource planner, and balance analyzer: `src/Rts.Core/Maps/Generation`
- Unity editor menus: `unity/Assets/Rts/Scripts/MapEditor/Editor`
- Unity runtime helper paths/templates: `unity/Assets/Rts/Scripts/MapEditor/Runtime`

## Procedural Generator Status

Implemented in `Rts.Core`:

- Prompt phrases for size, player count, resources, cliffs, rockiness, water, biome, symmetry, profile, seed, and ore regeneration.
- Deterministic 100x100 through 400x400 generation for 2/4/6/8 players.
- Buildability checks for rectangular 1x1 through 5x5 footprints, optional padding, and generated build-pad regions.
- Pathability/fairness metrics for connected start pairs, unreachable starts, path distance spread, nearby resources by player, bottleneck estimate, blocker density, and a 0-100 fairness score.
- Ore depletion/regeneration metadata for generated resource fields.
- Bridge API for editor tooling: `AegisMapGenerationBridge` returns validated `.aegismap.json`, Tiled JSON, summary text, warnings/errors, and fairness score.

Implemented in Unity:

- `Project Aegis > Map Editor > Open Map Editor` opens an editor window with procedural controls, prompt text, seed controls, validation, save/export buttons, prompt examples, overlay toggles, warnings/errors, and a generated summary panel.
- The Unity window calls the deterministic `Rts.Core` bridge through `AegisUnityMapGenerationBridge` when the Unity plugin DLL is current.
- If the bridge is unavailable, Unity writes a compatible `.aegismap.json` shell and displays a warning instead of failing silently.

Future bridge:

- Add scene-view overlay rendering for starts, resources, blockers, cliffs, and buildability.
- Add a command-line converter/generator for batch automation outside Unity.

## Unity Menu Items

- `Project Aegis > Map Editor > Open Map Editor`
- `Project Aegis > Map Editor > Import Tiled JSON as Aegis Map`
- `Project Aegis > Map Editor > Export Selected Aegis Map to Tiled JSON`
- `Project Aegis > Map Editor > Create Tiled Starter Tileset`
- `Project Aegis > Map Editor > Build Proxy Materials and Prefabs`
- `Project Aegis > Map Editor > Export Unity AI Asset Prompts`

## Next Useful Work

- Add a dedicated command-line converter for batch import/export/generation.
- Add Unity preview rendering from `.aegismap.json` once Unity compile validation is available.
- Expand Tiled tileset metadata if more terrain or resource types are added.
- Add optional SuperTiled2Unity documentation for visual-only workflows.
