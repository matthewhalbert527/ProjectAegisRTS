# Stage 32.5 Terrain Art Batch01 Integration

Stage 32.5 integrates `ProjectAegisRTS_TerrainArt_Batch01_SourceAssets.zip` as real source art for the Stage 32 terrain pipeline.

Unlike the rejected primitive-only terrain pass, Batch01 is intentionally image-backed:

- Primary runtime source: `unity/Assets/Rts/Art/Source/Terrain/Batch01/individual/*.png`
- Fallback/reference source: `unity/Assets/Rts/Art/Source/Terrain/Batch01/individual/*_card.png`
- Metadata: `unity/Assets/Rts/Art/Source/Terrain/Batch01/terrain_batch01_manifest.json`

The importer reads the JSON manifest and creates:

- One transparent source-art material per listed terrain asset.
- One canonical textured mesh prefab per listed terrain asset.
- Mapped Stage 32 replacement prefabs for existing player-facing terrain definitions.
- A Unity `TerrainArtManifest` entry for canonical assets and mapped replacements.
- `Assets/Rts/Scenes/Stage32_5_TerrainArtBatch01Review.unity`.

The transparent PNG is always preferred for runtime art. The `_card.png` file remains a fallback/reference path and is recorded in metadata, but the generated runtime material uses the transparent PNG.

## Runtime Boundary

Stage 32.5 is visual-only. `Rts.Core` remains authoritative for terrain, passability, resources, movement, and building placement.

Generated prefabs must not contain colliders. The passable/buildable/fine-grid metadata from the manifest is stored on Unity tags so validation and review tools can see what each art asset claims visually.

## Replacement Policy

Existing Stage 32 primitive terrain pieces remain available as fallback/debug assets. Player-facing set dressing should use Batch01 source-art prefabs anywhere the manifest provides a matching source asset.

The validator fails if:

- Fewer than 40 manifest terrain assets are imported.
- Materials or prefabs are missing.
- Prefabs are not image-backed by Batch01 individual PNG textures.
- Review/player-facing scenes fall back to primitive-only terrain while Batch01 source art exists.
- Player-facing terrain uses too few Batch01 replacements.

## Commands

```powershell
.\tools\run-unity-stage32-5-validation.ps1
.\tools\run-stage32-5-player-facing-checks.ps1 -SkipPlayerBuild
.\tools\run-stage32-5-medium-checks.ps1
```
