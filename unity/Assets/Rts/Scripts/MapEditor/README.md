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
- `Project Aegis > Map Editor > Build Visual Terrain From Aegis Map` remains the compatibility entry point and now routes through the Unity visual compiler.
- `Project Aegis > Map Editor > Visual Compiler` opens the compiler window for theme selection, visual mode, visual seed control, preview compilation, layer summaries, debug overlay toggles, and local screenshot capture.
- The compiler defaults to `ProductionPreview`; `DebugOverlay` and `Hybrid` are opt-in and are not production output.
- The compiler creates deterministic visual layers for production terrain chunks, transition masks, muddy merged water with internal silt/deep-pool details and broken edge details, softened shorelines, compacted-soil roads with grass-edge breakup and bridge prototypes over water, topology-driven cliffs, amount-based resource fields, modular base pads, and rule-based clustered scatter.
- River shorelines include deterministic eroded-bank undercut decals and sparse wet-pebble scatter on top of the smoothed bank meshes.
- The visual builder reads the original art packet at `Assets/Rts/MapEditor/ArtPack/ProjectAegis_MapVisualArtPack_v1/` directly. PNG textures and decals are used for materials immediately; `.glb` meshes are imported through Unity glTFast (`com.unity.cloud.gltfast`) and otherwise fall back to deterministic generated proxy meshes if that package is unavailable.
- The folder `ProjectAegis_MapVisualArtPack_v1` now contains the rebuilt v2 production-proxy map art packet while preserving the compatibility folder name.
- The terrain texture baseline now includes regenerated original procedural grass, dark-grass, dirt, gravel/rough-ground, and muddy-bank albedo/normal/roughness sets for closer tactical zoom previews. Grass and dark-grass textures now carry sharper blade strokes, and the compiler uses slower world-continuous terrain UVs plus subtle macro terrain variation patches to reduce visible tile repetition.
- Bridge crossings now use original weathered deck and rail texture sets plus segmented rails, posts, abutments, deck seams, edge-wear decals, tracked deck dust, wet deck grime, and pebble/rubble/grass contact dressing at the banks. These remain production-proxy bridge visuals, not final shipping bridge art.
- `Project Aegis > Map Editor > Validate Visual Art Pack` verifies the art-pack manifest, semantic material map, terrain texture paths, required GLB meshes, decals, visual theme texture mappings, and sample compiler output.
- `Project Aegis > Map Editor > Validate Visual Quality Gate` verifies production-preview defaults, no default debug overlays, texture-role bindings, merged water strips, muddy water-surface details, water-edge detail patches, bridge-bank contact dressing, road edge-breakup coverage, road-water crossing handling, capped resource glints, and non-fallback sample output.
- The showcase map `Assets/Rts/MapEditor/Samples/sample_art_pack_showcase_160_forest_river.aegismap.json` is intended for quick art-pack validation and renders through the batch method `AegisMapVisualBuilder.RenderArtPackShowcaseForBatch`.
- SuperTiled2Unity is optional for visual authoring convenience and is not required for deterministic import tests.
