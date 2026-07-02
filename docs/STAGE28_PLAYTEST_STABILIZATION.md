# Stage 28 Playtest Stabilization

Stage 28 focuses on proving the current vertical slice is coherent from the player side after the Stage 27.1 placement UX fix and the Stage 28 diagonal movement update already present in `Rts.Core`.

## What Changed

- Added a hidden `FeatureRegressionHud` QA overlay toggled with `F10`.
- Added Stage 28 Unity validators for integrated feature regression and play-mode smoke.
- Added Stage 28 fast, medium, full, and player-facing validation scripts.
- Expanded the medium-recursion audit so Stage 28 cannot reintroduce recursive medium validation.
- Documented the feature regression matrix and known limitations.

## Player-Facing Checks

Run the current player build:

```powershell
.\build\windows-player-stage16\ProjectAegisRTS.exe
```

Manual checklist:

- Boot menu appears first.
- Start Vertical Slice loads the board.
- PC right sidebar is visible and docked to the right edge.
- Minimap appears above production.
- Debug panels and status log are hidden by default.
- Build Power Plant from the sidebar.
- Click the ready Power Plant card.
- The Stage 3 center `BoardPlacementHud` does not appear.
- The right-sidebar `PlacementModePanel` appears.
- A fine-grid footprint preview appears on board hover.
- Left-click places the building.
- Escape cancels placement before it opens pause.
- Escape outside placement opens pause.
- Move, stop, attack-move, repair, power toggle, rally, support power, and transport/engineer routes produce feedback.
- AI pressure appears but does not instantly end the match.
- A normal victory path exists by scouting, producing combat units, and destroying the enemy base.
- `Player.log` has no repeating red errors.

## Validation Commands

Fast iteration:

```powershell
.\tools\run-stage28-fast-checks.ps1
```

Before commit:

```powershell
.\tools\run-stage28-medium-checks.ps1
```

Player-facing confidence:

```powershell
.\tools\run-stage28-player-facing-checks.ps1 -SkipPlayerBuild
```

Full acceptance:

```powershell
.\tools\run-stage28-checks.ps1
```

Full validation remains intentionally slow because it includes Stage 0-through-Stage 27.1 acceptance, Stage 28 validation, Quest hand-control preservation, Windows build/log inspection, the UnityEngine-free scan, and whitespace checks.

## QA Overlay

`FeatureRegressionHud` is created by `RtsGameBootstrapper` at runtime and is hidden by default. Press `F10` during development to show the current route audit. It reports the major command groups, whether expected routes are present, and key PCDesktop/QuestXR/placement status. It is deliberately a QA/debug overlay, not a player UI surface.
