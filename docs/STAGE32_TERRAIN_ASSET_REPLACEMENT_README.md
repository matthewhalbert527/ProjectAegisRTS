# Stage 32 Terrain Asset Replacement Package

This package adds a Unity-side procedural terrain kit generator for ProjectAegisRTS and has been integrated into the repo as the Stage32 terrain asset replacement overlay.

It creates a first batch of realistic modular terrain proxy pieces using Unity primitives and shared materials. These are not final art; they are high-quality game-ready proxy terrain pieces meant to replace clean prototype grid visuals while final artist-authored terrain assets are produced.

## What it generates

- 47 modular terrain prefabs under `Assets/Rts/Art/Prefabs/Terrain/Stage32Generated/`
- shared terrain materials under `Assets/Rts/Art/Materials/Terrain/Stage32Generated/`
- a review scene: `Assets/Rts/Scenes/Stage32_TerrainAssetReplacementReview.unity`
- QA reports under `docs/`

## How to use

From the repository root, run:

```powershell
.\tools\run-stage32-terrain-kit-generator.ps1
```

Or open Unity and run:

`ProjectAegisRTS > Stage 32 > Generate High Quality Terrain Kit`

Then run:

`ProjectAegisRTS > Stage 32 > Validate Terrain Kit`

Batch methods:

- `ProjectAegisRTS.UnityClient.EditorTools.Stage32TerrainKitGenerator.GenerateTerrainKitBatch`
- `ProjectAegisRTS.UnityClient.EditorTools.Stage32TerrainKitValidator.ValidateTerrainKitBatch`

## Design goals

- realistic military tabletop style
- readable top-down strategy view
- modular terrain and battlefield props
- fine-grid compatible metadata
- Quest-safe primitive geometry and shared materials
- no protected C&C/Red Alert art, names, UI, or trade dress

## Integration note

This package intentionally does not modify Rts.Core. Terrain gameplay metadata remains in authoritative systems. The generated `Stage32TerrainPieceTag` component stores Unity-side art metadata that Codex can later map into existing terrain-definition systems.
