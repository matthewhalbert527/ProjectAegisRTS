# Unity AI Asset Pipeline

## Purpose

Unity AI-generated assets may support map-editor visualization, but they do not define gameplay rules or runtime map data. The authoritative map content remains `.aegismap.json` imported and validated by `Rts.Core`.

The current procedural map workflow is deterministic prompt-driven generation. It does not require an external LLM, Unity AI package, cloud project, or network API.

The Unity map editor calls the deterministic `Rts.Core` generation bridge when available. Unity AI packages are not part of map generation, validation, save, export, fairness scoring, buildability checks, or ore regeneration.

## Map-Editor Asset Categories

- Terrain proxy materials: clear, road, rough, forest, water, cliff, ore.
- Blocker proxy prefab: simple visible map-authoring blocker.
- Resource proxy visuals: original Project Aegis ore styling.
- Region overlays: editor-only preview aids.
- Visual terrain dressing assets: original cliff rocks, boulders, road pebbles, craters, vegetation, ore chunks, base pads, water/bank materials, and biome material profiles.

The current `Build Visual Terrain From Aegis Map` menu item routes through the Unity visual compiler. The compiler uses deterministic prototype materials, imported prototype art-pack assets where available, and generated fallback geometry so the map can render as a layered RTS battlefield in Unity. Future Unity AI or artist-created assets can replace those role bindings without changing `.aegismap.json` gameplay data.

The dedicated compiler window is available at:

`Project Aegis > Map Editor > Visual Compiler`

It supports theme selection, visual seed control, preview compilation, layer summaries, debug overlay toggles, and local screenshot capture.

## Prompt Export

The Unity menu `Project Aegis > Map Editor > Export Unity AI Asset Prompts` writes prompt notes under:

`unity/Assets/Rts/MapEditor/AssetPrompts/`

Generated assets should use original Project Aegis names and identifiers. Do not generate or import protected Command & Conquer / Red Alert names, logos, faction identifiers, UI, art direction, map data, or file formats.

## Optional Tools

SuperTiled2Unity may be used later as a visual/editor convenience. It is not required for deterministic import/export and should not become a gameplay dependency.

Unity AI packages remain optional for future original visual asset ideation. Any generated asset still needs human review before it is treated as production art.
