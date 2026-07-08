# Stage 32.6 Final Terrain Meshes - Batch 01

This batch proves the final terrain art ingestion path with two directly importable Unity mesh assets:

- `ground_grass_dirt_01`
- `resource_cluster_blue_01`

These are OBJ/MTL mesh assets with texture maps and manifest metadata. They are not concept sheets, preview images, cropped cards, or UI-like planes.

## Source Paths

```text
unity/Assets/Rts/Art/Source/Terrain/FinalMeshBatch01/
  terrain_final_mesh_batch01_manifest.json
  ground_grass_dirt_01/
    ground_grass_dirt_01.obj
    ground_grass_dirt_01.mtl
    textures/
  resource_cluster_blue_01/
    resource_cluster_blue_01.obj
    resource_cluster_blue_01.mtl
    textures/
```

## Generated Runtime Assets

The Unity importer creates:

```text
unity/Assets/Rts/Art/Materials/Terrain/FinalMeshBatch01/
unity/Assets/Rts/Art/Prefabs/Terrain/FinalMeshBatch01/ground_grass_dirt_01.prefab
unity/Assets/Rts/Art/Prefabs/Terrain/FinalMeshBatch01/resource_cluster_blue_01.prefab
unity/Assets/Rts/Art/Prefabs/Terrain/FinalMeshBatch01/MappedDefinitions/
unity/Assets/Rts/Scenes/Stage32_6_FinalTerrainMeshReview.unity
```

`ground_grass_dirt_01` is passable/buildable visual ground, 4m x 4m, 8x8 fine grid. `resource_cluster_blue_01` is a harvestable blue resource visual, non-passable/non-buildable, 3.3m x 3.3m, 7x7 fine grid.

## Player-Facing Mapping

The importer maps these final meshes into the existing Stage32 terrain definition layer where safe:

- `ground_grass_dirt_patch_01` -> `ground_grass_dirt_01`
- `resource_cluster_01` -> `resource_cluster_blue_01`

This is visual-only. `Rts.Core` remains authoritative for passability, buildability, harvesting, and simulation state.

## Validation

Run:

```powershell
.\tools\run-unity-stage32-6-final-terrain-mesh-validation.ps1
.\tools\run-stage32-6-medium-checks.ps1
```

The validator checks source OBJ/MTL files, texture maps, Unity materials, generated prefabs, manifest-aligned metadata, the focused review scene, and final-mesh mappings.
