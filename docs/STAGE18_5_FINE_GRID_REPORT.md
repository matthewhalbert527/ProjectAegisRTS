# Stage 18.5 Fine Placement Grid Report

## Summary

Stage 18.5 adds an authoritative 2x fine placement grid. Building visuals and board scale stay physically consistent, while placement and building occupancy now use finer cells in `Rts.Core`.

## Current Model Found

Before this stage, the same coarse grid drove map size, terrain, movement, pathing, building footprints, building placement, hover cells, and Unity placement previews. A 2 x 2 building occupied four coarse cells and could only be placed at whole coarse-cell offsets.

Unity rendered the board using coarse cell dimensions from `BoardCoordinateMapper`, and placement previews were generated from coarse footprint snapshots.

## Chosen Approach

The least disruptive approach is a layered grid:

- Keep the existing coarse map for terrain, movement, fog, resource cells, and command targeting.
- Add `PlacementGridMetrics.PlacementGridScale = 2`.
- Store building placement top-left and building occupancy in fine placement cells.
- Convert legacy coarse footprints to fine footprints automatically.
- Project fine building occupancy back onto coarse cells for pathing and coarse compatibility.

This gives real fine placement authority without rewriting movement, fog, minimap, economy, or AI in the same stage.

## Implementation Notes

- Added `PlacementGridMetrics` for deterministic coarse/fine conversion.
- Expanded building definitions, actor state, map snapshots, actor snapshots, and placement preview snapshots with fine-grid metadata.
- Updated `GridMap` to own fine building occupancy and rebuild coarse building occupancy from it.
- Updated placement validation to reject partial fine-cell overlaps and to accept valid half-coarse offsets.
- Updated demo/bootstrap code and validators to submit placement commands in fine coordinates.
- Updated Unity board mapping, hover, preview rendering, selection hit tests, desktop input, and XR input coordinators.
- Added Stage 18.5 Unity editor validators and fast/medium/full/player-facing validation scripts.

## Footprint Examples

| Actor | Legacy coarse footprint | Stage 18.5 fine footprint |
| --- | --- | --- |
| power_plant | 2 x 2 | 4 x 4 |
| barracks | 2 x 2 | 4 x 4 |
| war_factory | 3 x 2 | 6 x 4 |
| fabrication_hub | 3 x 3 | 6 x 6 |

## Known Limitations

- Movement/pathfinding still operates on coarse cells in Stage 18.5.
- Fine placement is currently most visible during placement preview and building occupancy.
- Existing building balance is intentionally unchanged.
- Future stages can introduce truly smaller fine-footprint structures without changing this grid model.

## Stage 19 Follow-Up

Stage 19 builds on this grid by tuning mission flow and placement guidance. Coarse grid boundaries are visually stronger, fine grid lines are quieter, placement prompts explain green/red footprints, and the vertical-slice checklist now includes explicit fine-grid placement beats for Power Plant and Refinery.

## Validation Plan

Run these acceptance commands from the repository root:

```powershell
dotnet run --no-restore --project src/Rts.Core.Tests
.\tools\build-rts-core-for-unity.ps1
.\tools\audit-medium-validation-recursion.ps1
.\tools\run-stage18-5-fast-checks.ps1
.\tools\run-stage18-5-medium-checks.ps1
.\tools\run-stage18-5-player-facing-checks.ps1 -SkipPlayerBuild
.\tools\build-windows-player-stage16.ps1
git diff --check
```

Then inspect the newest player log:

```powershell
.\tools\inspect-latest-player-log.ps1 -CopyToDebugLogs
```

The final commit response records the pass/fail evidence from the local run.

## Local Validation Results

Run date: July 1, 2026.

Passed locally:

- `dotnet run --no-restore --project src/Rts.Core.Tests`: 71/71 tests passed.
- `.\tools\build-rts-core-for-unity.ps1`: built and copied `Rts.Core.dll`.
- `.\tools\audit-medium-validation-recursion.ps1`: Stage 9-18.5 medium scripts use direct Unity validation dependencies only.
- `.\tools\run-stage18-5-fast-checks.ps1`: passed, including Stage 18.5 Unity validation and Play Mode smoke.
- `.\tools\run-stage18-5-medium-checks.ps1`: passed after fixing Stage 18.5 player-facing switch forwarding.
- `.\tools\run-stage18-5-player-facing-checks.ps1 -SkipPlayerBuild`: passed and inspected the latest Player.log.
- `.\tools\build-windows-player-stage16.ps1`: passed and refreshed `build\windows-player-stage16`.
- `.\tools\inspect-latest-player-log.ps1 -CopyToDebugLogs`: passed after launching the rebuilt EXE once.
- `git diff --check`: passed with non-fatal Windows line-ending warnings only.
- Rts.Core UnityEngine-free scan: passed with no matches.
