# Map Visual Quality Triage

## Screenshot Problems

- Visible chunk blocks: `AegisTerrainLayerCompiler` used 16x16 dominant-role quads, so mixed terrain areas became large square patches. This pass changes production terrain to 4-cell chunks and breaks mixed chunks down to per-cell patches with transition masks.
- Debug outlines in production preview: the visual compiler had UI overlay toggles, but they were not real compile settings and defaulted on in the window. This pass adds `ProductionPreview`, `DebugOverlay`, and `Hybrid` render modes with production as the default and debug overlays off by default.
- Cell-stepped river borders: `AegisWaterAndShorelineCompiler` emitted one water quad and edge quad per water cell. This pass merges water into horizontal strips and merges shorelines into longer softened strips.
- Road segments crossing water without bridges/fords: `AegisRoadVisualCompiler` rendered each segment as a single road body over every terrain type. This pass splits road segments by sampled water crossings and emits named `bridge_prototype_*` pieces over water.
- Flat gray base pads: `AegisBasePadVisualCompiler` could quietly fall back to flat quads. This pass keeps the v2 `base_pad_14x14.glb` path, uses textured concrete panel/trim roles, and reports a warning if the pad mesh or concrete texture path is missing.
- Noisy ore sparkle: `AegisResourceFieldVisualCompiler` allowed many small chunks and glints per field. This pass reduces chunk count, increases chunk scale, always emits field dust, caps glints to four per field, and uses depletion-aware density.
- Cliffs/rocks reading as gray blobs: `AegisCliffTopologyCompiler` could emit raw blocker core cubes inside cliff regions. This pass hides blocker fill in production and reserves `debug_cliff_blocker_core_*` for debug or hybrid overlays.
- Terrain texture repetition: production terrain now uses smaller semantic patches and material tiling rather than stretching one large debug-like dominant role over 16 cells. Final shader-layer terrain remains future art/renderer work.

## Implemented Now

- Production render mode is the default.
- Debug helper geometry is opt-in through `DebugOverlay` or `Hybrid`.
- Production terrain chunk size is below the old 16x16 behavior.
- Mixed terrain chunks no longer use only a single dominant role.
- Water bodies and shorelines are strip-merged.
- Road-water crossings are represented by bridge prototype deck, rail, and shadow pieces.
- Resource glints are capped and field density scales with amount.
- Raw blocker fill is hidden in production.
- `Project Aegis > Map Editor > Validate Visual Quality Gate` validates key production-preview invariants.

## Future Art Polish

- Replace quad terrain patches with a shader-driven terrain mesh or terrain-layer system.
- Replace prototype bridge deck/rail geometry with original authored bridge/fording assets.
- Add authored road/river spline metadata to `.aegismap.json` when the core map format needs it.
- Add final sculpted cliffs, vegetation, ore, and base-pad prefabs with LODs and tuned materials.
- Add automated screenshot comparison once visual targets stabilize.
