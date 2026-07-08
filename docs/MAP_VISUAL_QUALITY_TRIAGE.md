# Map Visual Quality Triage

## Screenshot Problems

- Visible chunk blocks: `AegisTerrainLayerCompiler` used 16x16 dominant-role quads, so mixed terrain areas became large square patches. This pass changes production terrain to 4-cell chunks, uses one naturalized base role for mixed production chunks, suppresses literal rough/cliff base fills in production preview, uses world-continuous UVs on production terrain chunks, and uses soft transition blend overlays instead of opaque transition strips.
- Debug outlines in production preview: the visual compiler had UI overlay toggles, but they were not real compile settings and defaulted on in the window. This pass adds `ProductionPreview`, `DebugOverlay`, and `Hybrid` render modes with production as the default and debug overlays off by default.
- Cell-stepped river borders: `AegisWaterAndShorelineCompiler` emitted one water quad and edge quad per water cell. This pass hides production water cells from the base terrain layer, converts water rows into smoothed ribbon meshes, keeps water-strip metrics for validation, and uses continuous bank/cap shoreline meshes instead of horizontal/vertical shoreline rectangles. Shorelines now have a narrower wet core plus a lighter feather, and both use transparent river decal textures instead of opaque muddy terrain strips.
- Road segments crossing water without bridges/fords: `AegisRoadVisualCompiler` rendered each segment as a single road body over every terrain type. This pass splits road segments by sampled water crossings and emits named `bridge_prototype_*` deck and rail pieces over water without the earlier scorch-shadow quad that produced black block artifacts. Bridge previews now include deterministic deck seams, side posts, and a soft under-shadow.
- Road slabs at close zoom: production terrain now naturalizes road-adjacent dirt base cells back into grass so the road compiler owns the visible path. The road compiler uses a narrower, lighter organic road body with stronger edge jitter, dimension-scaled road/dust UVs, and irregular worn-edge, rut, and mud-track meshes instead of opaque rectangular strips.
- Flat gray base pads: `AegisBasePadVisualCompiler` could quietly fall back to flat quads. This pass keeps the v2 `base_pad_14x14.glb` path, uses textured concrete panel/trim roles, and reports a warning if the pad mesh or concrete texture path is missing.
- Noisy ore sparkle: `AegisResourceFieldVisualCompiler` allowed many small chunks and glints per field. This pass reduces chunk count, increases chunk scale, always emits field dust, caps glints to four per field, and uses depletion-aware density.
- Cliffs/rocks reading as gray blobs: `AegisCliffTopologyCompiler` could emit raw blocker core cubes inside cliff regions. This pass hides blocker fill in production and reserves `debug_cliff_blocker_core_*` for debug or hybrid overlays.
- Terrain texture repetition: production terrain now uses smaller semantic patches and material tiling rather than stretching one large debug-like dominant role over 16 cells. Production preview also naturalizes cliff/rough terrain to grass/dirt base surfaces so imported cliff pieces, rubble speckles, wet-bank meshes, and blend overlays carry the edge detail instead of flat gray masks.
- Sparse zoom-in detail: the first production preview still read too clean at close range. The current pass adds denser deterministic production terrain detail decals, road dust/rut/wear decals, ore-dust/glint decals, and layered base-pad crack/grime/construction-wear decals. Terrain transition and terrain detail decals now use irregular mesh silhouettes in production so close-up blends have softer, less rectangular edges.

## Implemented Now

- Production render mode is the default.
- Debug helper geometry is opt-in through `DebugOverlay` or `Hybrid`.
- Production terrain chunk size is below the old 16x16 behavior.
- Production terrain chunks use world-continuous UVs so tileable textures do not restart on every patch.
- Mixed terrain chunks no longer use only a single dominant role.
- Production transition masks use transparent grass/dirt/gravel/mud blend roles with deterministic width, offset variation, and organic feather meshes.
- Rough/cliff terrain is visually naturalized to softer grass/dirt base surfaces in production preview while debug mode keeps literal terrain roles.
- Road-adjacent dirt terrain is visually naturalized in production preview so deterministic road meshes provide the path surface.
- Water bodies compile to smoothed production ribbon meshes, with strip counts retained for validation.
- Shoreline wetness compiles to deterministic bank and end-cap meshes that follow the smoothed river span.
- Shoreline wetness uses a two-stage core/feather mesh so the river edge is softer in production preview.
- Shoreline core and feather meshes use transparent art-pack river decals rather than tiled terrain albedo.
- Road-water crossings are represented by bridge prototype deck, rail, seam, post, and soft-shadow pieces.
- Resource glints are capped and field density scales with amount.
- Raw blocker fill is hidden in production.
- Production terrain detail decals add denser grass mottling, road-adjacent dust, gravel speckles, wet bank marks, and water highlights.
- Production terrain detail decals use deterministic organic mesh silhouettes instead of plain rectangular quads.
- Roads use narrower organic body meshes with dimension-scaled road/dust UVs plus worn edge, tire-rut, and mud-track decal roles.
- Base pads use transparent panel/trim decals, grime, cracks, and construction-wear decals.
- Resource fields use transparent ore-dust decals and capped glint decals.
- The visual compiler reads art-pack textures without rewriting texture importer metadata during validation.
- `Project Aegis > Map Editor > Validate Visual Quality Gate` validates key production-preview invariants.

## Future Art Polish

- Replace quad terrain patches with a shader-driven terrain mesh or terrain-layer system.
- Replace soft transition decals with real shader splat/blend weights once a terrain-layer renderer exists.
- Replace prototype bridge deck/rail geometry with original authored bridge/fording assets and tuned shadow/occlusion materials.
- Replace deterministic close-up decals with authored terrain blend masks once the final terrain renderer exists.
- Add authored road/river spline metadata to `.aegismap.json` when the core map format needs it.
- Add final sculpted cliffs, vegetation, ore, and base-pad prefabs with LODs and tuned materials.
- Add automated screenshot comparison once visual targets stabilize.
