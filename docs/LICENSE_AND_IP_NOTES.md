# License And IP Notes

## OpenRA

OpenRA is GPL-licensed. The copied `external/openra` tree is reference material for Stage 0. If future work copies, modifies, or derives code from OpenRA, preserve GPL headers, keep license notices, and treat the resulting game code as GPL-compatible unless legal review says otherwise.

The Aegis generated-map implementation in `src/Rts.Core/MapGeneration` is a clean, original implementation. It follows the architectural pattern of "settings in, normal deterministic map/world out" after reviewing OpenRA behavior, but it does not copy OpenRA source, YAML, tile definitions, names, assets, or generator code.

## Red Alert Reference

`external/redalert_reference` is historical reference only. Do not port code, assets, names, or behaviors from it into ProjectAegisRTS. Do not copy EA/Westwood art, audio, names, faction branding, or protected assets.

## Original Assets

Concept art and future production assets should remain under `art/` or a separate asset pipeline. Keeping art separate from code helps manage different licensing models later.

## Protected Names

Do not use Command & Conquer, Red Alert, EA, GDI, Nod, Soviet, Allied, or similar protected names in new game code, docs intended for shipping, UI, or asset IDs.

## Working Labels Requiring Review

The concept archive includes working labels. `Orca Lifter`, `Skyraider`, and `MASH` are flagged for release-name review in the asset registry. Code uses safer working IDs such as `heavy_lifter_aircraft`, `attack_aircraft`, and `field_hospital`.
