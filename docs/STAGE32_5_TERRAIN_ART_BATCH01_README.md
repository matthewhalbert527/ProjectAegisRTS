# ProjectAegisRTS Terrain Art Batch 01 Source Assets

This package contains individual image-backed source assets cropped from the approved high-fidelity terrain art sheets. Use these as source art for Stage32.5/Stage33 terrain replacement, not as Codex-generated primitive placeholders.

The package includes:

- `individual/*.png`: transparent-background source images where possible.
- `individual/*_card.png`: original cropped cards with dark background, useful as fallback/reference.
- `sheets/*.png`: source sheets used for visual reference.
- `terrain_batch01_manifest.json`: terrain IDs, categories, grid footprint hints, passability/buildability flags.

Integration intent:

1. Import these images into Unity under `Assets/Rts/Art/Source/Terrain/Batch01`.
2. Create image-backed prefab cards/decals or textured mesh planes for the first pass.
3. Preserve these as external source art; do not ask Codex to recreate them from primitives.
4. Replace low-quality placeholder terrain only when one of these source assets exists.
5. Keep existing Stage27.1 placement HUD separation, PCDesktop sidebar, QuestXR controls, and validation audits.

Important limitation: these are high-quality art source cards, not final 3D meshes. For 360-degree close Quest inspection, later replace them with artist-authored meshes that match these references.
