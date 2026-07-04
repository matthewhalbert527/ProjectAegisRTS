# Stage 32.6 Terrain Art Integration Correction

Stage 32.6 corrects the rejected Stage32 Batch01 terrain-art pass.

## Rejection Cause

The previous integration treated art-direction sheets as runtime assets. It cropped the sheets into texture materials, generated one-plane prefabs under `Batch01Imported`, and allowed those prefabs into player-facing set dressing. That produced flat image cards on top of the board instead of actual terrain art.

## Correction

- Batch01 concept/reference images move to `unity/Assets/Rts/Art/References/Terrain/Stage32_6ArtDirection/`.
- The Batch01 source folder must not contain concept images after Stage32.6 validation.
- Legacy `Batch01Imported` runtime texture/material/mesh/prefab folders are deleted by the Stage32.6 generator.
- Runtime terrain is generated as Unity mesh/material prefab assemblies under `unity/Assets/Rts/Art/Prefabs/Terrain/Stage32_6Runtime/`.
- Player-facing Stage32 set dressing keeps existing piece IDs through mapped wrapper prefabs under `Stage32_6Runtime/MappedDefinitions/`.

## Runtime Pieces

Stage32.6 generates 40 canonical runtime terrain prefabs covering:

- ground pieces
- roads and base pads
- resource clusters
- rock/cliff/crater pieces
- battlefield props, vegetation, crates, and barrels

The prefabs are visual-only. They do not add colliders and do not modify `Rts.Core`.

## Known Limitations

- These are Unity-generated modular assemblies, not final artist-authored meshes.
- Material detail is shader/color/geometry based; the rejected concept-sheet crops are not used as runtime textures.
- The player-facing scene uses a restrained subset so gameplay readability and safe-area behavior remain unchanged.

## Adding Artist-Made Terrain Later

Replace a generated prefab with an imported model/prefab that keeps the same asset name, pivot, grid footprint, visual-only policy, and `Stage32_6RuntimeTerrainTag`. Imported materials must avoid reference-sheet textures and should remain in the Stage32.6 runtime material family or a future reviewed material family.

## Review

Open `Assets/Rts/Scenes/Stage32_6_TerrainArtIntegrationReview.unity` to inspect the runtime terrain kit and sample battlefield composition.

Screenshots are captured to:

- `build/screenshots/stage32_6/terrain_review.png`
- `build/screenshots/stage32_6/player_facing.png`
