# Stage 32 Terrain Asset Replacement

Stage 32 now includes a terrain-kit overlay pass in addition to the existing terrain-piece/set-dressing catalog. The overlay installs Unity-side generator and validator tools, then produces a first modular terrain replacement library made from Quest-safe primitive proxy meshes and shared materials.

## Generated Outputs

- Prefabs: `unity/Assets/Rts/Art/Prefabs/Terrain/Stage32Generated/`
- Materials: `unity/Assets/Rts/Art/Materials/Terrain/Stage32Generated/`
- Review scene: `unity/Assets/Rts/Scenes/Stage32_TerrainAssetReplacementReview.unity`
- Generation report: `docs/STAGE32_GENERATED_TERRAIN_KIT_REPORT.md`
- QA report: `docs/STAGE32_TERRAIN_QA_REPORT.md`

The first overlay generation produces 47 prefabs and 15 shared materials. Every prefab has a `Stage32TerrainPieceTag`, mesh renderers, LOD metadata, fine-grid size metadata, and buildability/passability/category flags for later mapping into the authored terrain pipeline.

## Regeneration

From the repo root:

```powershell
.\tools\run-stage32-terrain-kit-generator.ps1
```

The script discovers the installed Unity editor, runs the generator, runs the validator, normalizes Unity-generated whitespace, and writes logs under `build/unity-logs/`.

Unity menu equivalents:

- `ProjectAegisRTS > Stage 32 > Generate High Quality Terrain Kit`
- `ProjectAegisRTS > Stage 32 > Validate Terrain Kit`

## Artist Replacement Path

Artist-authored replacements should keep the prefab path and `Stage32TerrainPieceTag` contract stable unless a deliberate library migration is planned. Replace the primitive child geometry with imported meshes while preserving:

- origin/pivot centered on the fine-grid footprint,
- top-down readability at the Stage16 camera distance,
- `fineGridSize`, `blocksMovement`, `buildable`, `road`, `water`, and `resourceField` metadata,
- a simple LODGroup and Quest-safe renderer/material count,
- no gameplay colliders unless a later gameplay stage explicitly adopts them.

After replacement, rerun `.\tools\run-stage32-terrain-kit-generator.ps1` only if the procedural library should be regenerated. For hand-authored replacements, run the validator menu or keep a future artist-intake validator from overwriting the artist meshes.

## Known Limits

- The generated kit is proxy art, not final terrain art.
- The generated review scene is a library/inspection scene; Stage16 continues to use the existing Stage32 set-dressing integration.
- The terrain tag is Unity presentation metadata only. `Rts.Core` terrain, placement, movement, resources, AI, fog, and combat remain authoritative and UnityEngine-free.
- The kit does not import protected Red Alert or Command & Conquer assets, names, UI, or trade dress.
