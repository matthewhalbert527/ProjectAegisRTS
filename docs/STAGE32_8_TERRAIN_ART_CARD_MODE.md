# Stage 32.8 Terrain Art Card Mode

Stage32.8 adds an interim image-backed terrain-card mode for player-facing visual improvement while final authored terrain meshes are still pending.

- Source art: `Assets/Rts/Art/Source/Terrain/Batch01/individual/*_card.png`.
- Prefabs: `Assets/Rts/Art/Prefabs/Terrain/Stage32_8Cards/`.
- Metadata: `TerrainArtCardTag` marks these as visual-only image-backed cards.
- Gameplay: no Rts.Core terrain, pathing, placement, or economy behavior changes.
- Player-facing mappings: 4 existing Stage32 terrain definitions now use card prefabs where safe.

This is deliberately an interim mode. True final terrain still needs authored 3D models, proper UVs, and production textures.
