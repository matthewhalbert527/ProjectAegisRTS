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
- Unity editor menus: `unity/Assets/Rts/Scripts/MapEditor/Editor`
- Unity runtime helper paths/templates: `unity/Assets/Rts/Scripts/MapEditor/Runtime`

## Unity Menu Items

- `Project Aegis > Map Editor > Open Map Editor`
- `Project Aegis > Map Editor > Import Tiled JSON as Aegis Map`
- `Project Aegis > Map Editor > Export Selected Aegis Map to Tiled JSON`
- `Project Aegis > Map Editor > Create Tiled Starter Tileset`
- `Project Aegis > Map Editor > Build Proxy Materials and Prefabs`
- `Project Aegis > Map Editor > Export Unity AI Asset Prompts`

## Next Useful Work

- Add a dedicated command-line converter for batch import/export.
- Add Unity preview rendering from `.aegismap.json` once Unity compile validation is available.
- Expand Tiled tileset metadata if more terrain or resource types are added.
- Add optional SuperTiled2Unity documentation for visual-only workflows.
