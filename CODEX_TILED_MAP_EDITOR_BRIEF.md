# Codex implementation brief: Project Aegis RTS Tiled map-editor integration

## Objective

Implement a production-safe map authoring pipeline for ProjectAegisRTS using Tiled Map Editor as the external logical map editor, while keeping Project Aegis's own `.aegismap.json` / `AegisMapDocument` as the runtime source of truth.

Do not replace the game architecture. Do not port OpenRA or C&C/Red Alert code. Do not make Unity authoritative over gameplay state. `src/Rts.Core` remains deterministic and UnityEngine-free.

## Inputs to use

Preferred starting point:

1. `ProjectAegisRTS-map-editor-stage1.zip` — full repo with the Stage 1 map-editor package already integrated.

Alternative starting point:

1. Clone or unzip the current ProjectAegisRTS repo.
2. Also upload `ProjectAegis_MapEditorPackage.zip` and copy its contents into the repo using its `README_INSTALL.md`.

Optional reference only:

1. `OpenRA-bleed.zip` — read-only reference for RTS map-editor concepts. Do not copy code unless the GPL obligations are explicitly accepted and documented.

Do not use for implementation:

1. `CnC_Red_Alert-main.zip` — historical/protected-IP reference only. Do not copy code, names, art, UI, faction identifiers, or derived designs.

## Local downloads / prerequisites

For the developer machine, install:

1. Unity Hub and Unity 6.3 LTS or later.
2. .NET 8 SDK.
3. Tiled Map Editor 1.12.2 or later.
4. SuperTiled2Unity 2.4.0 or later, preferably through OpenUPM or as an embedded local Unity package.
5. Unity Terrain Tools package, optional but recommended.
6. Unity AI packages, optional, Unity 6+ only, with Unity Cloud project linked. The build must not fail if Unity AI is absent.

Do not commit desktop installers, Unity `Library`, `Temp`, `Logs`, `obj`, `bin`, or generated cache folders.

## Branch

Create a branch named:

```bash
git checkout -b codex/tiled-map-editor-integration
```

## Baseline commands

From the repository root:

```bash
dotnet run --project src/Rts.Core.Tests
```

If a Unity project exists under `unity`, compile it in batch mode. Use the installed Unity Editor path for the machine:

```bash
Unity.exe -batchmode -quit -projectPath unity -logFile unity-compile.log
```

## High-level implementation

### 1. Preserve and verify the Stage 1 Aegis map document

Keep or add these core files:

```text
src/Rts.Core/Maps/AegisMapDocument.cs
src/Rts.Core/Maps/AegisMapDocumentValidator.cs
src/Rts.Core/Maps/AegisMapDocumentWorldFactory.cs
```

These define the runtime map contract:

```text
schemaVersion = aegis.map.v1
min dimension = 100
max dimension = 400
small = 100 x 100
medium = 200 x 200
large = 400 x 400
```

The validator must reject dimensions below 100 or above 400. The world factory must create `GridMap(width, height)`, apply terrain flags/blockers/resources, add player slots, and instantiate actor placements through existing `RtsWorld.CreateActor`.

### 2. Add Tiled as an authoring bridge, not the runtime format

Add a new namespace:

```text
ProjectAegisRTS.Maps.Tiled
```

Suggested files:

```text
src/Rts.Core/Maps/Tiled/AegisTiledImportOptions.cs
src/Rts.Core/Maps/Tiled/AegisTiledImportResult.cs
src/Rts.Core/Maps/Tiled/AegisTiledMapImporter.cs
src/Rts.Core/Maps/Tiled/AegisTiledMapExporter.cs
src/Rts.Core/Maps/Tiled/TiledJsonDto.cs
```

Implement import from finite orthogonal Tiled JSON into `AegisMapDocument`.

Supported Tiled subset for v1:

```text
orientation: orthogonal only
infinite: false only
width: 100..400
height: 100..400
tilewidth == tileheight recommended, default 64
layer data: CSV/JSON array supported
object layers: supported
base64 compression: reject with a clear error in v1
infinite chunks: reject with a clear error in v1
```

Layer-name contract:

```text
terrain_base        -> AegisMapCellOverride.terrainTypeId and terrainFlags
terrain_overlay     -> optional road/scorch/rubble/visual flags
blockers            -> blocked cells / BlocksMovement / BlocksBuilding
resources           -> resource cell overrides or AegisMapResourceNode objects
player_starts       -> AegisMapPlayerSlot records
actor_placements    -> AegisMapActorPlacement records
regions             -> future metadata only; preserve as comments/docs if not implemented
nav_overrides       -> future metadata only; preserve as comments/docs if not implemented
```

Tile custom-property names to support:

```text
aegisTerrainTypeId: string
terrainFlags: int
blocked: bool
blocksMovement: bool
blocksBuilding: bool
resourceTypeId: string
resourceAmount: int
```

Object custom-property names to support:

```text
aegisTypeId: string
ownerPlayerId: int
playerId: int
startingCredits: int
footprintX: int
footprintY: int
rotationDegrees: int
resourceTypeId: string
radiusCells: int
amountPerCell: int
```

Object coordinate conversion:

```text
cellX = floor(object.x / map.tilewidth)
cellY = floor(object.y / map.tileheight)
```

Document edge cases in comments and tests, especially Tiled tile-object origin behavior. For v1, use rectangle objects snapped to the grid for player starts, resource nodes, and actor placements.

### 3. Add command-line tooling

Add a small CLI or script wrapper so maps can be converted without opening Unity.

Suggested files:

```text
tools/export-tiled-map.ps1
tools/export-tiled-map.sh
tools/README_TILED_MAP_PIPELINE.md
```

Example Tiled CLI command to document:

```bash
tiled --export-map json --embed-tilesets --resolve-types-and-properties "unity/Assets/Rts/Maps/Tiled/sample_small_100.tmx" "unity/Assets/Rts/Maps/Generated/sample_small_100.tiled.json"
```

Then run the Project Aegis importer:

```bash
dotnet run --project tools/Aegis.MapTools -- import-tiled "unity/Assets/Rts/Maps/Generated/sample_small_100.tiled.json" "unity/Assets/Rts/MapEditor/Samples/sample_small_100.aegismap.json"
```

If adding a new C# CLI project is too large for this pass, implement the importer tests first and create the scripts as documented stubs with clear TODOs.

### 4. Unity integration

If `unity` is still only a placeholder, create the minimum Unity project scaffolding safely:

```text
unity/Assets/
unity/Packages/manifest.json
unity/ProjectSettings/ProjectVersion.txt
```

Do not commit `unity/Library`, `unity/Temp`, `unity/Logs`, or platform build outputs.

Keep or add these Unity map-editor files:

```text
unity/Assets/Rts/Scripts/MapEditor/Runtime/AegisMapAsset.cs
unity/Assets/Rts/Scripts/MapEditor/Runtime/AegisMapTilePalette.cs
unity/Assets/Rts/Scripts/MapEditor/Runtime/AegisMapPiecePalette.cs
unity/Assets/Rts/Scripts/MapEditor/Runtime/AegisMapSceneBuilder.cs
unity/Assets/Rts/Scripts/MapEditor/Editor/AegisMapEditorWindow.cs
unity/Assets/Rts/Scripts/MapEditor/Editor/AegisMapAiPromptExporter.cs
unity/Assets/Rts/Scripts/MapEditor/Editor/AegisMapProxyAssetBuilder.cs
```

Add Tiled-facing Unity menu items:

```text
Project Aegis > Map Editor > Import Tiled JSON as Aegis Map
Project Aegis > Map Editor > Export Selected Aegis Map to Tiled JSON
Project Aegis > Map Editor > Create Tiled Starter Tileset
```

SuperTiled2Unity is optional for visual authoring convenience. The deterministic import must not depend on SuperTiled2Unity-generated prefabs. Use SuperTiled2Unity only to make Tiled maps appear in Unity as scene/prefab previews.

### 5. Unity package setup

Prefer OpenUPM scoped registry for SuperTiled2Unity:

```json
{
  "scopedRegistries": [
    {
      "name": "OpenUPM",
      "url": "https://package.openupm.com",
      "scopes": [
        "com.seanba"
      ]
    }
  ],
  "dependencies": {
    "com.seanba.super-tiled2unity": "2.4.0",
    "com.unity.terrain-tools": "5.3.2"
  }
}
```

If OpenUPM is not available, document the fallback: download SuperTiled2Unity, unzip it, and place the unzipped package folder under:

```text
unity/Packages/com.seanba.super-tiled2unity
```

### 6. Samples

Add sample authoring files:

```text
unity/Assets/Rts/Maps/Tiled/sample_small_100.tmx
unity/Assets/Rts/Maps/Tiled/sample_small_100.tsx
unity/Assets/Rts/Maps/Generated/sample_small_100.tiled.json
unity/Assets/Rts/MapEditor/Samples/sample_small_100.aegismap.json
unity/Assets/Rts/MapEditor/Samples/sample_large_400_shell.aegismap.json
```

Keep sample art original and generic. No C&C, Red Alert, GDI, Nod, Soviet, Allied, EA, Westwood, or similar protected names or visual identifiers.

### 7. Tests

Update `src/Rts.Core.Tests/Program.cs` or add a small test structure without introducing a test framework dependency unless necessary.

Minimum tests:

```text
AegisMapValidatorAccepts100x100
AegisMapValidatorAccepts400x400
AegisMapValidatorRejects99x100
AegisMapValidatorRejects401x400
AegisMapWorldFactoryCreates400x400World
AegisTiledImporterRejectsInfiniteMap
AegisTiledImporterRejectsUnsupportedOrientation
AegisTiledImporterImportsTerrainLayer
AegisTiledImporterImportsBlockersLayer
AegisTiledImporterImportsPlayerStarts
AegisTiledImporterImportsActorPlacements
AegisTiledImporterRoundTripsSampleSmall100
```

Run:

```bash
dotnet run --project src/Rts.Core.Tests
```

### 8. Documentation

Update or add:

```text
docs/MAP_EDITOR_PLAN.md
docs/TILED_MAP_PIPELINE.md
docs/UNITY_AI_ASSET_PIPELINE.md
docs/LICENSE_AND_IP_NOTES.md
unity/Assets/Rts/Scripts/MapEditor/README.md
```

Document:

1. Tiled layer naming rules.
2. Required Tiled map settings.
3. Tiled custom properties.
4. Export command.
5. Import command.
6. Unity menu workflow.
7. 100x100 to 400x400 validation.
8. Legal/IP boundary around OpenRA and C&C/Red Alert references.
9. Unity AI generated-asset review gate.

## Guardrails

1. Do not put `UnityEngine` references in `src/Rts.Core`.
2. Do not port OpenRA code into Project Aegis unless GPL obligations are explicitly accepted and documented.
3. Do not use C&C/Red Alert code, names, art, faction terms, UI designs, or file formats as implementation sources.
4. Do not make one GameObject per terrain cell for 400x400 maps.
5. Do not make Tiled JSON the runtime contract; convert to `.aegismap.json`.
6. Do not hardcode the old 32x32 demo map into map-editor paths.
7. Do not fail compilation when Unity AI packages are absent.
8. Keep generated/AI source assets separated from reviewed production assets.

## Done criteria

The task is complete when:

1. Core tests pass with `dotnet run --project src/Rts.Core.Tests`.
2. The importer converts a Tiled 100x100 sample to `.aegismap.json`.
3. The validator accepts 100x100 and 400x400 maps and rejects out-of-range maps.
4. `AegisMapDocumentWorldFactory` creates an `RtsWorld` from the imported map.
5. Unity map-editor scripts compile in Unity 6.3 LTS or later.
6. Unity menu items exist for Aegis map editing and Tiled import/export.
7. Documentation explains exactly how to author a map in Tiled and import it into Project Aegis.
8. No protected IP names/assets/code were copied into the implementation.
