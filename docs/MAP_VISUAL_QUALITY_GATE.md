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
- Core terrain, water, river, and base-pad roles have texture paths.
- Production terrain chunk size is below the old 16x16 dominant-role path.
- The showcase sample compiles a production preview root.
- Production output contains no debug-named overlay layer.
- Water cells compile into merged water strips.
- Road-water conflicts are zero because water crossings become bridge prototype pieces.
- Resource glints stay capped relative to field count.
- The sample instantiates imported v2 art-pack prefabs and is not fallback-only.

## Limitations

This is a structural renderer gate, not a final art approval gate. It does not prove that every camera angle is beautiful, that bridge art is final, or that terrain shaders are final. It blocks regressions where production preview returns to debug overlays, per-cell water corridors, fallback-only visuals, or unchecked road-water conflicts.

## Required Hygiene

Do not stage generated captures, `unity-compile.log`, or `*.local-export.tiled.json`. Preview images should remain in ignored temp output such as `%TEMP%\ProjectAegisRTS\VisualCompilerPreviews\`.
