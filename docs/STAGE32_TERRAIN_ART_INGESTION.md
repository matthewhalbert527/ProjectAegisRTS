# Stage 32 Terrain Art Ingestion

Stage32.6 supersedes the original Batch01 texture-card runtime output. Batch01 sheet images are now reference-only and must not generate cropped runtime cards. Use `docs/STAGE32_6_TERRAIN_ART_INTEGRATION_CORRECTION.md` and `tools/run-unity-stage32-6-validation.ps1` for the corrected runtime terrain path.

Stage 32 now has a source-art ingestion path for externally supplied terrain art. The old generated terrain proxies remain available for debug/review scenes and fallback, but the player-facing Stage16/Stage32 set dressing must use imported source-art replacements whenever the Batch01 source assets exist.

## Source Folder

Place external terrain art under:

```text
unity/Assets/Rts/Art/Source/Terrain/Batch01/
```

Supported source files:

- `.png`
- `.jpg`
- `.jpeg`
- `.fbx`
- `.obj`
- `.prefab`

The current Batch01 source set uses four named external terrain sheets:

```text
batch01_sheet_a_ground_foundations.jpg
batch01_sheet_b_roads_edges.jpg
batch01_sheet_c_resources_obstacles.jpg
batch01_sheet_d_props_vegetation.jpg
```

## Generated Assets

Run:

```powershell
.\tools\run-stage32-terrain-art-ingestion.ps1
```

The ingestion pass generates:

- Manifest: `unity/Assets/Rts/ScriptableObjects/Art/TerrainPieces/stage32_terrain_art_manifest.asset`
- Cropped textures: `unity/Assets/Rts/Art/Textures/Terrain/Batch01Imported/`
- Materials: `unity/Assets/Rts/Art/Materials/Terrain/Batch01Imported/`
- Meshes: `unity/Assets/Rts/Art/Meshes/Terrain/Batch01Imported/`
- Prefabs: `unity/Assets/Rts/Art/Prefabs/Terrain/Batch01Imported/`

Texture-sheet inputs are cropped into one generated PNG per player-facing terrain piece. Ground, road, and pad crops stay opaque. Resource, obstacle, and prop crops use a conservative dark-background alpha cutout so the player-facing scene is not forced to render the source sheet background rectangles.

Each player-facing replacement prefab has:

- `TerrainPieceValidationTag`
- `TerrainArtSourceTag`
- source asset path metadata
- Batch01 manifest entry
- visual-only/no-collider terrain-piece contract

## Player-Facing Rule

When Batch01 source art is present, `Stage32TerrainPieceGenerator` updates the Stage32 terrain definitions so the player-facing set-dressing profile references imported source-art prefabs instead of primitive-only generated proxies.

Validation fails if the player-facing profile or rendered Stage16 scene still uses proxy-only terrain while Batch01 source-art replacements are available. The proxy prefabs remain valid only for fallback/debug review workflows.

## Validation

Run:

```powershell
.\tools\run-unity-stage32-validation.ps1
.\tools\run-stage32-medium-checks.ps1
```

The Stage32 Unity validation now runs terrain-piece generation, explicit Batch01 ingestion, scene creation, terrain-piece validation, Stage16 scene validation, smoke validation, and screenshot capture. `docs/STAGE32_VISUAL_QA_REPORT.md` records the source-art replacement count and player-facing source-art placement count.
