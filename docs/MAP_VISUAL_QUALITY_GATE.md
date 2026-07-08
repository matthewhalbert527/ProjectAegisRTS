# Map Visual Quality Gate

## Purpose

The visual quality gate verifies that the Unity map visual compiler behaves like a production-preview renderer rather than a debug topology visualization.

Run from Unity:

`Project Aegis > Map Editor > Validate Visual Quality Gate`

Run from batch mode:

```powershell
& "E:\Unity\Hub\Editor\6000.5.1f1\Editor\Unity.exe" -batchmode -quit -projectPath "E:\OpenRA Mod\ProjectAegisRTS\unity" -logFile "E:\OpenRA Mod\ProjectAegisRTS\unity-compile.log" -executeMethod ProjectAegisRTS.UnityClient.EditorTools.AegisMapVisualQualityGate.ValidateSampleForBatch
```

## Current Checks

- Default visual mode is `ProductionPreview`.
- Debug overlays are disabled by default.
- Temporary `sample_small_100.local-export.tiled.json` is absent.
- Art-pack manifest and `Materials/semantic_materials.json` exist.
- Core terrain, water, river, muddy-water, base-pad, resource-dust, road-dust, compacted-road, weathered bridge, battlefield decal, and layered terrain-detail roles have texture paths.
- Production terrain chunk size is below the old 16x16 dominant-role path.
- The showcase sample compiles a production preview root.
- Production output contains no debug-named overlay layer.
- Terrain transition masks are present and produce blend edges.
- Production terrain detail decals are present at the stricter layered-surface threshold.
- Terrain macro variation roles are texture-bound so broad grass/dirt areas do not fall back to flat repeated tiles.
- Water cells compile into merged water strips plus at least one production water ribbon mesh.
- Water cells compile muddy deep-pool, silt-flow, or midstream ripple details.
- Water cells compile at least one production shoreline bank mesh.
- Water cells compile water-edge depth/shallow/ripple details and eroded-bank decal coverage so the production preview does not fall back to a hard unadorned water cutout.
- Road-water conflicts are zero because water crossings become bridge prototype pieces.
- Synthetic bridge crossings produce contact prop dressing at the bank instead of floating above bare terrain.
- Road detail decals are present when roads are present.
- Road edge-grass and pebble breakup coverage is present above the minimum production-preview threshold.
- Rule-based scatter produces enough low-profile grass/pebble ground litter to break up open fields.
- Base pads produce layered panel, trim, crack, grime, and construction-wear decals.
- Resource glints stay capped relative to field count.
- The sample instantiates imported v2 art-pack prefabs and is not fallback-only.

## Limitations

This is a structural renderer gate, not a final art approval gate. It does not prove that every camera angle is beautiful, that bridge art is final, or that terrain shaders are final. It blocks regressions where production preview returns to debug overlays, per-cell water corridors, missing production water ribbon meshes, missing muddy water-surface details, missing shoreline bank meshes, missing eroded-bank or water-edge details, missing bridge texture bindings, missing bridge-bank contact dressing, missing macro terrain bindings, fallback-only visuals, missing layered terrain detail decals, missing low-profile ground litter, missing road/pad detail decals, missing road edge-breakup coverage, missing required art-pack texture paths, or unchecked road-water conflicts.

## Required Hygiene

Do not stage generated captures, `unity-compile.log`, or `*.local-export.tiled.json`. Preview images should remain in ignored temp output such as `%TEMP%\ProjectAegisRTS\VisualCompilerPreviews\`.
