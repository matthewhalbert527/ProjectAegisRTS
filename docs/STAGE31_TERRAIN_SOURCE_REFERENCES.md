# Stage 31 Terrain Source References

The attached terrain sheets are integrated as source/reference images for the Stage 29-32 terrain pipeline.

## Unity Reference Assets

- `unity/Assets/Rts/Art/References/Terrain/Stage31TerrainSource/terrain_reference_sheet_01_full_kit.jpg`
- `unity/Assets/Rts/Art/References/Terrain/Stage31TerrainSource/terrain_reference_sheet_02_board_layout.jpg`
- `unity/Assets/Rts/Art/References/Terrain/Stage31TerrainSource/terrain_reference_sheet_03_road_base_edges.jpg`
- `unity/Assets/Rts/Art/References/Terrain/Stage31TerrainSource/terrain_reference_sheet_04_cliffs_resources_props.jpg`

## Integration

- Stage 29 material direction now references worn asphalt, concrete hardstand, compacted dirt, mossy rock, mineral glow, rubble, barriers, and restrained foliage from the sheets.
- Stage 30 readability remains the guardrail: terrain detail must not overpower actors, selection, placement previews, or the fine placement grid.
- Stage 31 handoff uses these sheets as modular terrain art direction for future authored meshes.
- Stage 32 generators convert the source direction into Quest-safe visual proxies for roads, base pads, cliffs, resources, craters, wreckage, barriers, fences, and vegetation.

## Boundary

These are visual source references only. `Rts.Core` terrain, passability, buildability, resources, AI, fog, and pathing remain authoritative and UnityEngine-free.
