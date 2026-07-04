# Stage 32.6 Terrain Runtime Asset Rules

Runtime terrain assets must follow these rules:

- Do not use full concept sheets as runtime textures.
- Do not use cropped concept-sheet images as runtime textures.
- Do not place sheet cards or image planes in player-facing scenes.
- Use Unity mesh assemblies with shared materials for terrain pieces.
- Keep prefabs visual-only: no colliders, no gameplay authority, no `Rts.Core` dependency.
- Keep pivots centered and grid-friendly so set dressing remains deterministic.
- Keep renderer counts Quest-safe.
- Use `Stage32_6RuntimeTerrainTag` on runtime terrain prefabs and mapped player-facing wrappers.
- Keep `TerrainArtSourceTag` out of player-facing runtime terrain.

Reference images belong in:

`unity/Assets/Rts/Art/References/Terrain/Stage32_6ArtDirection/`

Runtime prefabs belong in:

`unity/Assets/Rts/Art/Prefabs/Terrain/Stage32_6Runtime/`

Mapped Stage32 player-facing wrappers belong in:

`unity/Assets/Rts/Art/Prefabs/Terrain/Stage32_6Runtime/MappedDefinitions/`

Future artist-made terrain can be added by replacing the generated mesh assemblies with imported model/prefab assemblies that keep the same tags, pivots, material policy, and visual-only constraints.
