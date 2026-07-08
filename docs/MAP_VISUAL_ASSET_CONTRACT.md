# Map Visual Asset Contract

The visual compiler maps `.aegismap.json` semantics to Unity-only visual assets. Runtime gameplay remains in `.aegismap.json` and `src/Rts.Core`.

## Required Fields

Every visual asset entry should provide:

- `visualId`: stable Project Aegis visual identifier.
- `semanticRole`: compiler role such as `terrain.grass` or `cliff.edge.straight`.
- `biome`: theme or biome name.
- `path`: Unity asset path.
- `material`: material or material descriptor path.
- `scale`: default Unity scale.
- `pivot`: expected pivot, usually `center` or `edge_center`.
- `rotationRule`: deterministic placement rule, such as `face_lower_neighbor`, `align_segment`, or `random_yaw_from_seed`.
- `placementRule`: where the asset may be placed.
- `densityRule`: how many instances may be placed per cell/field/chunk.
- `depletionStateBehavior`: resource behavior such as `hide_when_depleted`, `scale_with_amount`, or `swap_to_depleted_mesh`.
- `importAssumptions`: Unity importer requirements.
- `qualityStatus`: `debug`, `prototype`, or `final`.

## Semantic Roles

- `terrain.grass`
- `terrain.dark_grass`
- `terrain.dirt`
- `terrain.gravel`
- `terrain.mud`
- `terrain.shallow_water`
- `terrain.deep_water`
- `terrain.cliff_ground`
- `terrain.ore_stained_soil`
- `terrain.concrete_base_pad`
- `road.dirt`
- `road.gravel`
- `river.water`
- `river.shoreline`
- `cliff.edge.straight`
- `cliff.edge.corner_inner`
- `cliff.edge.corner_outer`
- `cliff.edge.endcap`
- `blocker.rock`
- `resource.ore`
- `resource.crystal`
- `resource.salvage`
- `resource.energy`
- `vegetation.tree`
- `vegetation.bush`
- `vegetation.grass`
- `decal.crater`
- `decal.scorch`
- `decal.rubble`
- `basepad.panel`
- `basepad.trim`
- `basepad.corner`
- `basepad.grime`
- `bridge.prototype.deck`
- `bridge.prototype.rail`
- `bridge.prototype.shadow`

## Example Entry

```json
{
  "visualId": "aegis_forest_cliff_straight_proto_01",
  "semanticRole": "cliff.edge.straight",
  "biome": "forest",
  "path": "Assets/Rts/MapEditor/ArtPack/ProjectAegis_MapVisualArtPack_v1/Meshes/Cliffs/cliff_straight_01.glb",
  "material": "Assets/Rts/MapEditor/VisualAssets/aegis_visual_compiler_cliff_edge_straight.mat",
  "scale": [1.0, 0.75, 1.0],
  "pivot": "edge_center",
  "rotationRule": "face_lower_neighbor",
  "placementRule": "exposed_cliff_edge_not_in_start_protected_zone",
  "densityRule": "one_per_exposed_edge_with_compiler_cap",
  "depletionStateBehavior": "not_applicable",
  "importAssumptions": "Unity glTFast imports GLB; compiler has generated fallback geometry",
  "qualityStatus": "prototype"
}
```

## Resource State Contract

Resource visuals must be field-based:

- Use stable field IDs from `.aegismap.json`.
- Derive center/radius from field cells.
- Scale visual density from `currentAmount / maxAmount`.
- Hide or reduce chunks for depleted fields.
- Reserve glints for high-fill/high-value fields.
- Keep resources out of start cleanup zones and base pads.
- Cap sparkle/glint decals so fields read as ore/resource deposits rather than visual noise.

## Road Crossing Contract

Road visuals must not paint directly through water cells. If a deterministic road segment crosses water, the visual compiler must either emit a bridge/ford representation or report a warning/error. The current bridge representation is a neutral Project Aegis prototype named with `bridge_prototype_*` scene objects; it is not final art.

## Current Prototype Pack

`ProjectAegis_MapVisualArtPack_v1` is imported and used for production-proxy v2 materials, decals, and GLB meshes. The folder name remains `v1` for code compatibility, but `Materials/semantic_materials.json` identifies the current semantic terrain/material entries as `production_proxy_v2`. Treat these assets as production-proxy content until final art review promotes them to shipping art. Its license/origin notes state it is original Project Aegis art and not copied from protected RTS projects.
