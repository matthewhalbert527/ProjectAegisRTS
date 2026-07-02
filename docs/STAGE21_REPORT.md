# Stage 21 Report

Stage 21 performs an MVP visual QA and artist-asset replacement readiness pass on the Stage 20 production proxy layer. It keeps the playable vertical slice, PC desktop sidebar, QuestXR hand controls, fine placement grid, and deterministic `Rts.Core` behavior intact.

## Scope

- Added structured MVP visual QA data and runtime reporting for the nine MVP actors.
- Added a Stage 21 visual QA review scene at `Assets/Rts/Scenes/Stage21_MvpVisualQaReview.unity`.
- Added socket, pivot, scale, LOD, material, fallback, and replacement-metadata validation.
- Added optional artist model import scanning under `Assets/Rts/Art/Models/Source/MVP`.
- Improved Stage 20 proxy readability with footprint corner markers, identity striping, tank/infantry details, collector details, and corrected primitive base alignment.
- Preserved Stage 20 proxy fallback behavior so real artist models can be staged without replacing active gameplay visuals until validated.

## Player-Facing Guarantees

- `Rts.Core` remains deterministic and UnityEngine-free.
- Debug panels remain hidden by default in the player-facing vertical slice.
- PCDesktop keeps the right-side production sidebar, minimap, placement readout, selection panel, and command buttons.
- QuestXR keeps the left-hand build/selection and right-hand tactical command surfaces while hiding PC-only sidebar UI.
- Stage 16.5 Boot remains first in Build Settings, followed by the Stage 16 vertical slice.
- The Windows player continues to build to `build\windows-player-stage16\ProjectAegisRTS.exe`.

## Reports

- MVP QA report: `docs/STAGE21_MVP_VISUAL_QA.md`
- Artist import status: `docs/STAGE21_ARTIST_MODEL_IMPORT_STATUS.md`
- Replacement checklist: `docs/STAGE21_ARTIST_ASSET_REPLACEMENT_CHECKLIST.md`

## Validation

Use the fast tier while iterating on proxy visuals, replacement metadata, import scan behavior, or the Stage 21 review scene:

```powershell
.\tools\run-stage21-fast-checks.ps1
```

Use the medium tier before commits. It runs Stage 20 direct validation dependencies and Stage 21 checks without calling previous medium scripts:

```powershell
.\tools\run-stage21-medium-checks.ps1
```

Use the full tier before accepting the stage:

```powershell
.\tools\run-stage21-checks.ps1
```

For player-facing build confidence without rebuilding the Windows player:

```powershell
.\tools\run-stage21-player-facing-checks.ps1 -SkipPlayerBuild
```

The final manual player test remains:

```powershell
.\tools\build-windows-player-stage16.ps1
& "build\windows-player-stage16\ProjectAegisRTS.exe"
```
