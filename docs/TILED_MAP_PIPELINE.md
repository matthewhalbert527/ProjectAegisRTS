# Tiled Map Pipeline

## Ownership

Project Aegis owns the runtime map format. Tiled is an external authoring tool and interchange surface, while runtime maps are stored as `.aegismap.json`.

The deterministic implementation is in `src/Rts.Core/Maps`:

- `AegisMapDocument` defines the runtime document.
- `AegisMapDocumentValidator` enforces map size and authoring validity.
- `AegisMapDocumentWorldFactory` creates `RtsWorld` instances from Aegis map documents.
- `Maps/Tiled` imports and exports Tiled-compatible JSON.

## Supported Sizes

- Small: `100x100`
- Medium: `200x200`
- Large: `400x400`
- Minimum: `100x100`
- Maximum: `400x400`

Maps smaller than `100x100` or larger than `400x400` are rejected.

## Tiled Support

The importer supports finite orthogonal Tiled JSON maps only.

Supported Tiled layers:

- `terrain_base`
- `terrain_overlay`
- `blockers`
- `resources`
- `player_starts`
- `actor_placements`
- `regions`
- `nav_overrides`

Tile layers may use plain JSON arrays or CSV data. Compressed data and unsupported encodings are rejected with validation errors. Infinite maps and non-orthogonal orientations are rejected.

Object layers are used for player starts, resource fields, actor placements, regions, blockers, and navigation overrides. The importer maps object pixel positions to Aegis grid cells using the Tiled tile size.

## Runtime Flow

1. Author or export a finite orthogonal Tiled JSON map.
2. Import it through `AegisTiledMapImporter`.
3. Validate the produced `AegisMapDocument`.
4. Save the runtime map as `.aegismap.json`.
5. Build an `RtsWorld` with `AegisMapDocumentWorldFactory`.

Tiled JSON is not loaded as the authoritative runtime map format.

## Samples

- `unity/Assets/Rts/Maps/Tiled/sample_small_100.tmx`
- `unity/Assets/Rts/Maps/Tiled/sample_small_100.tsx`
- `unity/Assets/Rts/Maps/Generated/sample_small_100.tiled.json`
- `unity/Assets/Rts/MapEditor/Samples/sample_small_100.aegismap.json`
- `unity/Assets/Rts/MapEditor/Samples/sample_large_400_shell.aegismap.json`

## Notes

The authoritative implementation brief is checked in at the repository root as `CODEX_TILED_MAP_EDITOR_BRIEF.md`. The Stage 0 implementation was originally completed from the user prompt while that file was unavailable, then verified against the attached brief during cleanup.
