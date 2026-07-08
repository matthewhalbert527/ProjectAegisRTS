# Art-Directed Benchmark QA

## Manual Target

The benchmark screenshot should read as a curated RTS battlefield, not a debug output. Roads should lead from the two base pads to one deliberate crossing. The bridge/fording location should be visually obvious, and no road body should pass through open water without that crossing.

## Automated Gate

Unity menu:

`Project Aegis > Map Editor > Validate Art-Directed Benchmark`

Batch method:

```powershell
& "E:\Unity\Hub\Editor\6000.5.1f1\Editor\Unity.exe" -batchmode -quit -projectPath "E:\OpenRA Mod\ProjectAegisRTS-art-directed-map-preview\unity" -logFile "E:\OpenRA Mod\ProjectAegisRTS-art-directed-map-preview\unity-compile.log" -executeMethod ProjectAegisRTS.UnityClient.EditorTools.AegisArtDirectedBenchmarkGate.ValidateForBatch
```

The gate checks:

- sample map and visual metadata exist
- sample is 100x100 with two starts
- exactly one authored road-water crossing exists
- production preview uses authored roads, not generated full-map fallback roads
- exactly one bridge/fording visual compiles
- no road-water conflict is reported
- river compiles as one continuous ribbon
- base pads have modular detail decals
- resource fields have ore dust/soil and capped glints
- no temporary local Tiled export remains in the generated maps folder

## Screenshot Capture

Unity menu:

`Project Aegis > Map Editor > Capture Art-Directed Preview`

Batch method:

```powershell
& "E:\Unity\Hub\Editor\6000.5.1f1\Editor\Unity.exe" -batchmode -quit -projectPath "E:\OpenRA Mod\ProjectAegisRTS-art-directed-map-preview\unity" -logFile "E:\OpenRA Mod\ProjectAegisRTS-art-directed-map-preview\unity-compile.log" -executeMethod ProjectAegisRTS.UnityClient.EditorTools.AegisArtDirectedBenchmarkGate.CaptureForBatch
```

Captured screenshots are written to:

`%TEMP%\ProjectAegisRTS\ArtDirectedPreviews\`

Generated screenshots are local QA artifacts and must not be staged.

## Human Approval

The automated gate blocks structural regressions. It does not prove the terrain is final-quality art. Screenshot review remains the human visual approval step for terrain mood, readability, value grouping, and composition.
