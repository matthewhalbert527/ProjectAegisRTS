# Stage 32 Terrain Piece Library

The Stage32 terrain-piece library is a Unity-side visual catalog. It helps the board look like a practical tabletop battlefield while keeping gameplay authority in `Rts.Core`.

## Categories

| Category | Count | Examples |
| --- | ---: | --- |
| Ground/base terrain | 18 | grass/dirt patches, compact soil, mud, scorch marks, concrete pads, roads, resource ground, shore placeholder, rocky blocked ground |
| Transitions | 16 | grass-dirt edges, dirt-road blends, concrete seams, resource edges, water edges, rock edges, buildable edge cues |
| Base construction | 16 | foundation pads, ramps, production aprons, road strips, seams, footprint decals, rally/exit markings |
| Obstacles | 18 | rocks, ridges, cliff chunks, craters, foliage clusters, wreckage, debris |
| Resources | 12 | standard/rich/depleted resource clusters, resource decals, harvest markers |
| Props | 16 | sandbags, barriers, tank traps, tire tracks, shell marks, crates, antenna beacon, destroyed-vehicle proxy |

Total generated pieces: 96.

## Asset Layout

- Prefabs: `unity/Assets/Rts/Art/Prefabs/TerrainPieces/<Category>/`
- Materials: `unity/Assets/Rts/Art/Materials/TerrainPieces/`
- Definitions: `unity/Assets/Rts/ScriptableObjects/Art/TerrainPieces/Definitions/<Category>/`
- Catalog: `unity/Assets/Rts/ScriptableObjects/Art/TerrainPieces/stage32_terrain_piece_library.asset`
- Material catalog: `unity/Assets/Rts/ScriptableObjects/Art/TerrainPieces/stage32_terrain_piece_material_library.asset`

## Definition Fields

Every `TerrainPieceDefinition` includes:

- `pieceId`
- `displayName`
- `category`
- `sizeClass`
- `footprintFineWidth`
- `footprintFineHeight`
- `prefab`
- `materialProfileId`
- `passabilityVisualHint`
- `buildableVisualHint`
- `supportsRotation`
- `supportsTint`
- `isGameplayBlockingVisualOnly`
- `notes`
- `questBudgetTag`

The matching prefab must include `TerrainPieceValidationTag` with the same core metadata.

## Quest-Safe Budget Rules

- Prefer primitive geometry and shared materials.
- Keep renderer counts low; Stage32 validation fails pieces above the renderer budget.
- Do not add colliders to terrain-piece prefabs.
- Keep ground plates below fine-grid line height.
- Use obstacles mostly at map edges or non-critical visual spaces.
- Do not place tall props where they hide units, buildings, resources, or placement previews.

## Visual-Only Boundary

`isGameplayBlockingVisualOnly` is a communication hint, not gameplay state. It can say a rock looks like a blocker, but `Rts.Core` terrain/passability remains the only authority for movement, building placement, harvesting, and targeting.
