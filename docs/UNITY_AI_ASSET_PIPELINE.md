# Unity AI Asset Pipeline

## Purpose

Unity AI-generated assets may support map-editor visualization, but they do not define gameplay rules or runtime map data. The authoritative map content remains `.aegismap.json` imported and validated by `Rts.Core`.

## Map-Editor Asset Categories

- Terrain proxy materials: clear, road, rough, forest, water, cliff, ore.
- Blocker proxy prefab: simple visible map-authoring blocker.
- Resource proxy visuals: original Project Aegis ore styling.
- Region overlays: editor-only preview aids.

## Prompt Export

The Unity menu `Project Aegis > Map Editor > Export Unity AI Asset Prompts` writes prompt notes under:

`unity/Assets/Rts/MapEditor/AssetPrompts/`

Generated assets should use original Project Aegis names and identifiers. Do not generate or import protected Command & Conquer / Red Alert names, logos, faction identifiers, UI, art direction, map data, or file formats.

## Optional Tools

SuperTiled2Unity may be used later as a visual/editor convenience. It is not required for deterministic import/export and should not become a gameplay dependency.
