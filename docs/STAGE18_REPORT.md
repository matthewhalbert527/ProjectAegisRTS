# Stage 18 Report

Stage 18 is a tester-guided playability pass on the existing Boot plus Stage 16 vertical slice. It does not add a new gameplay stage or move authority out of `Rts.Core`.

## What Changed

- Added `VerticalSliceProgressTracker` for player-facing checklist state derived from snapshots and local selection.
- Added `VerticalSliceChecklistHud` and `PlayerPromptSystem` so first-time testers have an in-game build order and next-step prompt.
- Kept debug panels and the status log hidden by default in the Windows player flow.
- Improved objective and match-result consistency so hidden enemy bases are not reported as destroyed until the deterministic objective resolves.
- Improved sidebar production readability with clearer card states, missing-factory text, future placeholders, and next-step highlighting.
- Hardened production tab layout against duplicate layout components.
- Brightened the player-build view with closer camera defaults, stronger lighting, and lighter transparent fog.
- Added scaled, non-overlapping IMGUI HUD layout for high-resolution and ultrawide screenshots.

## Player-Facing Acceptance

The expected player route is:

1. Start from `Assets/Rts/Scenes/Stage16_5_Boot.unity`.
2. Start the vertical slice.
3. Select the Fabrication Hub.
4. Follow the checklist through power, refinery, harvest, barracks, infantry, war factory, vehicle, scouting, attacking, and enemy-base destruction.
5. Confirm victory and defeat screens are driven by deterministic match state.

The Windows player still exports to:

```text
build\windows-player-stage16\ProjectAegisRTS.exe
```

## Validation

Use the fast check while iterating:

```powershell
.\tools\run-stage18-fast-checks.ps1
```

Use the medium check before committing:

```powershell
.\tools\run-stage18-medium-checks.ps1
```

Use the full gate before accepting Stage 18:

```powershell
.\tools\run-stage18-checks.ps1
```

Use the focused player-facing script when checking the EXE path and logs:

```powershell
.\tools\run-stage18-player-facing-checks.ps1
```

The fast and medium scripts avoid replaying the full Stage 0-through-18 chain after every small UI fix. The full gate remains the final acceptance gate.
