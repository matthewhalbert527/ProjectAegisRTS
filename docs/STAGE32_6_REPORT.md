# Stage 32.6 Report

Stage 32.6 corrects the rejected terrain-card integration by treating Batch01 sheet images as reference-only art direction and using runtime mesh/material prefab assemblies for player-facing terrain dressing.

- Runtime terrain prefabs: 40
- Player-facing mapped wrappers: 44
- Shared runtime materials: 14
- Reference-only images: 4
- Review scene: `Assets/Rts/Scenes/Stage32_6_TerrainArtIntegrationReview.unity`
- Review screenshot: `build/screenshots/stage32_6/terrain_review.png`
- Player-facing screenshot: `build/screenshots/stage32_6/player_facing.png`

## Root Cause
The prior Batch01 ingestion cropped concept/reference sheets into texture materials and saved one-plane prefab cards under `Batch01Imported`. Those technically validated, but they were not usable runtime terrain art.

## Runtime Rule
Runtime terrain prefabs must use Unity meshes and shared materials. Reference sheets may remain in `Assets/Rts/Art/References/Terrain/Stage32_6ArtDirection/`, but they must not be assigned to player-facing materials, terrain prefabs, or Stage16 set dressing.

## Validation Errors
- None
