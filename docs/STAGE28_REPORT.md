# Stage 28 Report

Stage 28 adds an integrated playtest stabilization and feature regression QA layer on top of Stage 27.1.

## Summary

- Preserved deterministic `Rts.Core` authority and Unity presentation/input boundaries.
- Added the hidden-by-default `FeatureRegressionHud` QA overlay.
- Added Stage 28 feature and play-mode validators.
- Added fast, medium, full, and player-facing validation scripts.
- Extended the medium-recursion audit to include Stage 28.
- Documented the feature regression matrix, playtest instructions, validation tiers, and known issues.

## Validation Evidence

Baseline before Stage 28 changes:

- `dotnet run --no-restore --project src/Rts.Core.Tests`: passed 109/109.
- `.\tools\build-rts-core-for-unity.ps1`: passed.
- `.\tools\audit-medium-validation-recursion.ps1`: passed for Stage 9-27.1 baseline.
- `.\tools\run-stage27-1-medium-checks.ps1`: passed.
- `.\tools\run-stage27-1-player-facing-checks.ps1 -SkipPlayerBuild`: passed.
- `.\tools\run-stage4-checks.ps1`: passed.
- `.\tools\run-stage5-checks.ps1`: passed.
- `.\tools\inspect-latest-player-log.ps1`: passed.
- `git diff --check`: passed with non-fatal Windows line-ending warnings.
- Rts.Core UnityEngine-free scan: passed.

Final Stage 28 validation results:

- `dotnet run --no-restore --project src/Rts.Core.Tests`: passed 109/109.
- `.\tools\build-rts-core-for-unity.ps1`: passed.
- `.\tools\audit-medium-validation-recursion.ps1`: passed for Stage 9-28.
- `.\tools\run-stage4-checks.ps1`: passed.
- `.\tools\run-stage5-checks.ps1`: passed.
- `.\tools\run-stage28-fast-checks.ps1`: passed.
- `.\tools\run-stage28-medium-checks.ps1`: passed and remained non-recursive.
- `.\tools\run-stage28-player-facing-checks.ps1 -SkipPlayerBuild`: passed.
- `.\tools\build-windows-player-stage16.ps1`: passed.
- Fresh Windows player launch smoke: passed.
- `.\tools\inspect-latest-player-log.ps1 -CopyToDebugLogs -RequireDisplayStartup`: passed with no red-error signatures.
- Rts.Core UnityEngine-free scan: passed.
- `git diff --check`: passed with non-fatal Windows line-ending warnings.
- `.\tools\run-stage28-checks.ps1`: attempted as the slow full acceptance gate; it timed out after 30 minutes while still progressing through recursive dependency validation/build work and did not report a validation failure before timeout.

## Stage27.1 Regression Coverage

Stage 28 keeps the Stage 27.1 PC placement split under test:

- PCDesktop starts with `BoardPlacementHud` hidden.
- Ready Power Plant enters `RtsSimulationDriver.HasPlacementMode`.
- Right-sidebar `PlacementModePanel` appears for building placement.
- Stage 3 board setup placement remains separate through `BoardPlacementController`.
- Fine-grid placement preview is required before placing.
- Escape/cancel clears building placement before pause.
- QuestXR board setup remains available when explicitly toggled.

## Player EXE Path

```powershell
.\build\windows-player-stage16\ProjectAegisRTS.exe
```

Open Boot first, choose `Start Vertical Slice`, and use the right sidebar for the PCDesktop validation path.
