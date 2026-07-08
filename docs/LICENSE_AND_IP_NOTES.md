# License And IP Notes

## OpenRA

OpenRA is GPL-licensed. The copied `external/openra` tree is reference material for Stage 0. If future work copies, modifies, or derives code from OpenRA, preserve GPL headers, keep license notices, and treat the resulting game code as GPL-compatible unless legal review says otherwise.

The Aegis generated-map implementation in `src/Rts.Core/MapGeneration` is a clean, original implementation. It follows the architectural pattern of "settings in, normal deterministic map/world out" after reviewing OpenRA behavior, but it does not copy OpenRA source, YAML, tile definitions, names, assets, or generator code.

## Red Alert Reference

`external/redalert_reference` is historical reference only. Do not port code, assets, names, or behaviors from it into ProjectAegisRTS. Do not copy EA/Westwood art, audio, names, faction branding, or protected assets.

## Original Assets

Concept art and future production assets should remain under `art/` or a separate asset pipeline. Keeping art separate from code helps manage different licensing models later.

`unity/Assets/Rts/MapEditor/ArtPack/ProjectAegis_MapVisualArtPack_v1/` contains original procedural Project Aegis map-visual assets delivered for this repository. Its included `LICENSE_AND_ORIGIN.md` states that the textures, decals, meshes, previews, manifest data, and helper code were created for Project Aegis and do not copy or derive from Command & Conquer, Red Alert, OpenRA, Warcraft, StarCraft, Total Annihilation, or other RTS assets.

The Unity visual builder uses this pack as editor-side visual mapping for `.aegismap.json` terrain, resources, blockers, roads, craters, base pads, and biome decoration. The pack does not make Unity or Tiled the runtime source of truth.

For current map-visual compiler work, treat `ProjectAegis_MapVisualArtPack_v1` as prototype-only unless individual assets are explicitly promoted by art review. The semantic compiler contract is the durable integration point; prototype asset paths can be replaced without changing deterministic gameplay data.

## Tiled Map Editor Integration

The Stage 0 Tiled map-editor integration is original Project Aegis code. Tiled is used as an external authoring/interchange tool, and `.aegismap.json` is the Project Aegis runtime map format.

No Command & Conquer / Red Alert implementation files, names, faction identifiers, UI, art, map data, or file formats are used by the map-editor integration. No OpenRA code was copied into `src/Rts.Core/Maps` or the Unity map-editor scripts.

## Protected Names

Do not use Command & Conquer, Red Alert, EA, GDI, Nod, Soviet, Allied, or similar protected names in new game code, docs intended for shipping, UI, or asset IDs.

## Working Labels Requiring Review

The concept archive includes working labels. `Orca Lifter`, `Skyraider`, and `MASH` are flagged for release-name review in the asset registry. Code uses safer working IDs such as `heavy_lifter_aircraft`, `attack_aircraft`, and `field_hospital`.
