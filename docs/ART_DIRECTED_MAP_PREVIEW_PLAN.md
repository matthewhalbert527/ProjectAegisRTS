# Art-Directed Map Preview Plan

## Why The Previous Preview Still Failed

The previous production visual pass improved materials and detail density, but the map still read as a procedural renderer test. The largest issues were compositional, not asset-count problems:

- Roads were synthesized from starts to the center and between starts, which created uncontrolled road spaghetti.
- Bridge visuals appeared wherever generated roads crossed water, so crossings were accidental rather than authored.
- River shape, base-pad placement, resources, and scatter were all inferred from broad map data rather than composed as a tactical scene.
- Terrain detail was distributed broadly, which helped zoom texture but did not create a clear RTS battlefield hierarchy.

## Implemented Now

- Added `sample_art_directed_forest_river_2p.aegismap.json` as a 100x100 runtime map benchmark.
- Added `sample_art_directed_forest_river_2p.visual.json` as Unity-side visual metadata for authored preview composition.
- Changed production visual compilation to prefer authored road metadata when present.
- Changed production fallback roads to short base-access roads instead of full-map start-to-center routes.
- Added one authored river crossing for the benchmark.
- Added `Project Aegis > Map Editor > Validate Art-Directed Benchmark`.
- Added `Project Aegis > Map Editor > Capture Art-Directed Preview`.

## Current Benchmark Target

The benchmark is a forest/grassland 2-player RTS battlefield with:

- one main river
- one intentional bridge/fording crossing
- two readable base pads
- compact ore fields near, but outside, the base pads
- limited cliffs and rocks
- restrained scatter
- no generated full-map road network

## Future Work

- Generalize authored route planning to generated maps.
- Add final bridge meshes if the production art direction moves beyond the current textured procedural bridge assembly.
- Add stronger terrain shader blending once the benchmark screenshot is approved.
- Add human screenshot approval as a required visual gate before expanding the style to all maps.

## Guardrails

The `.aegismap.json` document remains the runtime source of truth. The `.visual.json` sidecar is Unity editor preview metadata only. No Stage 1, C&C / Red Alert implementation files, or copied OpenRA code are used.
