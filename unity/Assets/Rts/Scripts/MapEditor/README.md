# Project Aegis Map Editor Scripts

The Unity scripts in this folder provide editor menus and file helpers for the Tiled-to-Aegis map pipeline.

- Runtime maps use `.aegismap.json`.
- Tiled JSON is an authoring/interchange format, not the runtime authority.
- Deterministic import/export lives in `src/Rts.Core/Maps`.
- Deterministic procedural generation lives in `src/Rts.Core/Maps/Generation`.
- The Unity menus are editor conveniences for folder setup, starter tilesets, proxy assets, and preview-oriented shell files.
- `Project Aegis > Map Editor > Open Map Editor` opens a window with an `AI / Procedural Generate` section for prompt-assisted map creation.
- The Unity window does not call network AI services; it is a local procedural prompt workflow.
- The Unity window includes prompt, size, player count, biome, resources, cliffs, rockiness, water, symmetry, seed, profile, ore regeneration rate/delay, overlay toggles, validation, save/export, prompt examples, warning/error output, and a generated summary panel.
- `AegisUnityMapGenerationBridge` calls the deterministic `Rts.Core` generation bridge through reflection when the Unity `Rts.Core.dll` plugin is current.
- If the bridge is unavailable, Unity falls back to a compatible `.aegismap.json` shell and shows a warning.
- `Project Aegis > Map Editor > Build Visual Terrain From Aegis Map` reads the selected `.aegismap.json` and creates a deterministic dressed scene with blended terrain, soft dirt routes, generated watercourses, muddy water banks, shore pebbles/bank grass, cliff/ore/rock/vegetation/base-pad/crater dressing, and detailed concrete base pads.
- The visual builder reads the original art packet at `Assets/Rts/MapEditor/ArtPack/ProjectAegis_MapVisualArtPack_v1/` directly. PNG textures and decals are used for materials immediately; `.glb` meshes are imported through Unity glTFast (`com.unity.cloud.gltfast`) and otherwise fall back to deterministic generated proxy meshes if that package is unavailable.
- SuperTiled2Unity is optional for visual authoring convenience and is not required for deterministic import tests.
