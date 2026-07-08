# Stage 32 Terrain Asset Replacement

Stage32.6 correction: the original Batch01 cropped texture/card output is no longer an accepted player-facing replacement path. Batch01 art sheets are reference-only, `Batch01Imported` runtime folders are removed, and Stage16/Stage32 player-facing set dressing maps to Stage32.6 mesh/material prefab assemblies.

Stage 32 now includes two terrain replacement paths in addition to the existing terrain-piece/set-dressing catalog.

- `Batch01` source-art ingestion imports externally supplied texture/model/prefab terrain assets and replaces player-facing Stage16/Stage32 set dressing when real source assets exist.
- The generated terrain-kit overlay remains a debug/review/fallback path for library inspection and future artist handoff. It is not the player-facing replacement path when Batch01 source art is present.

The player-facing validation now fails if Batch01 source art exists but the Stage16 rendered terrain dressing is still using primitive-only generated proxies for the core replacement batch.

The current Batch01 ingestion seed uses the Stage 31 terrain source sheets copied into:

```text
unity/Assets/Rts/Art/Source/Terrain/Batch01
```

The generated proxy review kit still reads the Stage 31 terrain source references from:

```text
unity/Assets/Rts/Art/References/Terrain/Stage31TerrainSource
```

The sheets drive the replacement priorities for roads, base pads, curbs, retaining walls, cliffs, minerals, craters, wreckage, barriers, fences, and foliage.

## Batch01 Ingestion Outputs

- Manifest: `unity/Assets/Rts/ScriptableObjects/Art/TerrainPieces/stage32_terrain_art_manifest.asset`
- Source-art materials: `unity/Assets/Rts/Art/Materials/Terrain/Batch01Imported/`
- Source-art meshes: `unity/Assets/Rts/Art/Meshes/Terrain/Batch01Imported/`
- Source-art prefabs: `unity/Assets/Rts/Art/Prefabs/Terrain/Batch01Imported/`
- Direct runner: `tools/run-stage32-terrain-art-ingestion.ps1`

Each source-art replacement prefab carries `TerrainArtSourceTag` plus the existing `TerrainPieceValidationTag` contract. Stage32 validation requires at least 32 player-facing source-art replacements and checks the actual rendered Stage16 terrain root for source-art tags.

## Debug/Fallback Generated Kit Outputs

- Prefabs: `unity/Assets/Rts/Art/Prefabs/Terrain/Stage32Generated/`
- Materials: `unity/Assets/Rts/Art/Materials/Terrain/Stage32Generated/`
- Review scene: `unity/Assets/Rts/Scenes/Stage32_TerrainAssetReplacementReview.unity`
- Generation report: `docs/STAGE32_GENERATED_TERRAIN_KIT_REPORT.md`
- QA report: `docs/STAGE32_TERRAIN_QA_REPORT.md`

The reference-driven overlay generation produces 62 prefabs and 20 shared materials. Every prefab has a `Stage32TerrainPieceTag`, mesh renderers, LOD metadata, fine-grid size metadata, and buildability/passability/category flags for later mapping into the authored terrain pipeline. These generated prefabs are fallback/debug assets, not the player-facing replacement path when imported source art is available.

The in-game Stage32 terrain-piece generator keeps stable IDs. It first generates the fallback proxy definitions, then applies Batch01 source-art replacements to matching terrain definitions when source assets exist. Stage16 placement, pathing, resources, and passability still come from existing systems; terrain art remains visual-only.

## Regeneration

From the repo root:

```powershell
.\tools\run-stage32-terrain-art-ingestion.ps1
.\tools\run-stage32-terrain-kit-generator.ps1
```

The script discovers the installed Unity editor, runs the generator, runs the validator, normalizes Unity-generated whitespace, and writes logs under `build/unity-logs/`.

Unity menu equivalents:

- `ProjectAegisRTS > Stage 32 > Generate High Quality Terrain Kit`
- `ProjectAegisRTS > Stage 32 > Validate Terrain Kit`

## Artist Replacement Path

For player-facing terrain replacement, add real source files to `unity/Assets/Rts/Art/Source/Terrain/Batch01/`, run `.\tools\run-stage32-terrain-art-ingestion.ps1`, then run `.\tools\run-unity-stage32-validation.ps1`. Do not claim player-facing visual replacement is complete unless `docs/STAGE32_VISUAL_QA_REPORT.md` reports Batch01 source-art replacements and player-facing source-art placements, and the latest `build/screenshots/stage32_player_facing_terrain_view.png` shows imported source art.

Artist-authored replacements should keep the terrain-piece ID and validation-tag contract stable unless a deliberate library migration is planned. Replace source art with imported meshes or texture assets while preserving:

- origin/pivot centered on the fine-grid footprint,
- top-down readability at the Stage16 camera distance,
- source-reference category intent from the Stage31 terrain sheets,
- `fineGridSize`, `blocksMovement`, `buildable`, `road`, `water`, and `resourceField` metadata,
- a simple LODGroup and Quest-safe renderer/material count,
- no gameplay colliders unless a later gameplay stage explicitly adopts them.

After replacement, rerun `.\tools\run-stage32-terrain-kit-generator.ps1` only if the procedural library should be regenerated. For hand-authored replacements, run the validator menu or keep a future artist-intake validator from overwriting the artist meshes.

## Known Limits

- The generated `Stage32Generated` kit is proxy art, not final terrain art.
- The generated review scene is a library/inspection scene; Stage16 uses Batch01 source-art replacements when available.
- Current Batch01 source art is sheet/crop based. It proves the ingestion/replacement path and removes primitive-only player-facing set dressing; it is not final authored terrain modeling.
- The terrain tag is Unity presentation metadata only. `Rts.Core` terrain, placement, movement, resources, AI, fog, and combat remain authoritative and UnityEngine-free.
- The kit does not import protected Red Alert or Command & Conquer assets, names, UI, or trade dress.
