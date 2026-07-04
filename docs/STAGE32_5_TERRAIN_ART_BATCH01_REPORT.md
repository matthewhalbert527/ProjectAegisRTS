# Stage 32.5 Terrain Art Batch01 Report

Stage 32.5 replaces the low-quality primitive terrain visual path with imported Batch01 source art.

## Imported Source

- Package: `ProjectAegisRTS_TerrainArt_Batch01_SourceAssets.zip`
- Source folder: `unity/Assets/Rts/Art/Source/Terrain/Batch01`
- Manifest: `terrain_batch01_manifest.json`
- Expected source terrain assets: 42
- Expected PNG files: 84, including transparent primary PNGs and `_card` fallbacks.

## Generated Runtime Assets

The Stage32.5 importer generates Unity materials, mesh assets, prefab wrappers, metadata, and a review scene from the JSON manifest. The generated runtime path is:

- Materials: `Assets/Rts/Art/Materials/Terrain/Batch01Imported`
- Meshes: `Assets/Rts/Art/Meshes/Terrain/Batch01Imported`
- Canonical prefabs: `Assets/Rts/Art/Prefabs/Terrain/Batch01Imported`
- Player-facing mapped prefabs: `Assets/Rts/Art/Prefabs/Terrain/Batch01Imported/MappedDefinitions`
- Review scene: `Assets/Rts/Scenes/Stage32_5_TerrainArtBatch01Review.unity`

## Player-Facing Integration

Stage32.5 maps Batch01 source art onto the existing Stage32 terrain-piece definitions used by the Stage16 player-facing battlefield set dressing. Old primitive/proxy terrain remains fallback/debug only.

The importer does not modify `Rts.Core` gameplay and does not add colliders to terrain art prefabs.

## Current Known Limitation

Batch01 is high-fidelity 2D source art on textured mesh planes/cards. It is a major visual improvement over primitive-only placeholders, but it is not final 3D terrain geometry for close Quest inspection. Artist-authored meshes can later replace these prefabs while keeping the same manifest IDs and Stage32 mapping.
