# Project Aegis Map Editor Scripts

The Unity scripts in this folder provide editor menus and file helpers for the Tiled-to-Aegis map pipeline.

- Runtime maps use `.aegismap.json`.
- Tiled JSON is an authoring/interchange format, not the runtime authority.
- Deterministic import/export lives in `src/Rts.Core/Maps`.
- The Unity menus are editor conveniences for folder setup, starter tilesets, proxy assets, and preview-oriented shell files.
- SuperTiled2Unity is optional for visual authoring convenience and is not required for deterministic import tests.
