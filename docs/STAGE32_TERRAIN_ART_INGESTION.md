# Stage 32 Terrain Art Ingestion

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

The current Batch01 seed uses the four Stage31 terrain source sheets as imported source art:

```text
terrain_reference_sheet_01_full_kit.jpg
terrain_reference_sheet_02_board_layout.jpg
terrain_reference_sheet_03_road_base_edges.jpg
terrain_reference_sheet_04_cliffs_resources_props.jpg
```

## Generated Assets

Run:

```powershell
.\tools\run-stage32-terrain-art-ingestion.ps1
```

The ingestion pass generates:

- Manifest: `unity/Assets/Rts/ScriptableObjects/Art/TerrainPieces/stage32_terrain_art_manifest.asset`
- Materials: `unity/Assets/Rts/Art/Materials/Terrain/Batch01Imported/`
- Meshes: `unity/Assets/Rts/Art/Meshes/Terrain/Batch01Imported/`
- Prefabs: `unity/Assets/Rts/Art/Prefabs/Terrain/Batch01Imported/`

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
