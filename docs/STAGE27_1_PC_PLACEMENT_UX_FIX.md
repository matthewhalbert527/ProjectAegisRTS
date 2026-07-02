# Stage 27.1 PC Placement UX Fix

Stage 27.1 fixes a Windows player-facing placement regression without changing `Rts.Core` gameplay rules.

## Root Cause

The center overlay with `Toggle Placement Mode`, `Confirm Placement`, `Cancel`, `Reset`, `Save`, `Load`, and `Recenter` is owned by `BoardPlacementHud`. That HUD belongs to Stage 3 board setup placement, where the player moves, rotates, scales, saves, or recenters the whole game board.

The regression came from `DebugHudVisibilityController` treating all placement UI as one generic placement mode. Its placement-panel sync checked `RtsSimulationDriver.HasPlacementMode`, which means a completed building is being placed on the grid. When the player clicked a ready Power Plant card in the PC right sidebar, `RtsSimulationDriver.TryQueueProduction("power_plant")` entered building placement mode, and the generic visibility check showed every placement panel, including `BoardPlacementHud`.

That confused two separate concepts:

- Board setup placement: Unity presentation mode for adjusting `BoardRoot`, owned by `BoardPlacementController.IsPlacementModeActive` and shown through `BoardPlacementHud`.
- Building placement: deterministic gameplay placement for a completed production building, owned by `RtsSimulationDriver.HasPlacementMode` and shown through the PC right-sidebar `PlacementModePanel` or Quest left-hand placement panel.

## Behavior After The Fix

`DebugHudVisibilityController` now syncs placement panels by mode:

- `BoardPlacementHud` is visible only when debug panels are explicitly shown or board setup placement is active.
- `PlacementModePanel` is visible for PCDesktop/DebugHybrid building placement.
- `LeftHandPlacementPanel` is visible for QuestXR/DebugHybrid building placement.

`PauseMenuController` now handles Escape in priority order:

1. cancel active building placement;
2. cancel active board setup placement;
3. otherwise toggle the pause menu.

The PCDesktop flow is therefore:

1. build Power Plant from the right sidebar;
2. click the ready Power Plant production card;
3. see the fine-grid footprint preview and right-sidebar placement panel;
4. left-click to place, or press Esc / sidebar Cancel to cancel;
5. never show the Stage 3 board setup overlay during building placement.

QuestXR keeps the Stage 4/5 hand-control split, and explicit board setup placement remains available through `BoardPlacementController` and `BoardPlacementHud`.

## Validation

New Stage 27.1 validators cover the regression directly:

- `Stage27_1PlacementUxValidator`: Boot scene first, Stage16 PCDesktop defaults, right sidebar/minimap, hidden BoardPlacementHud, ready Power Plant building placement, fine-grid preview, sidebar Cancel, explicit board setup availability, and medium-recursion audit wiring.
- `Stage27_1PlayModeSmokeValidator`: runtime-style Power Plant queue/ready/placement path, BoardPlacementHud hidden during building placement, right-sidebar `PlacementModePanel` active, Esc cancel before pause, placement through command router, normal pause after placement, and red-console error capture.

Use:

```powershell
.\tools\run-stage27-1-fast-checks.ps1
.\tools\run-stage27-1-medium-checks.ps1
.\tools\run-stage27-1-player-facing-checks.ps1 -SkipPlayerBuild
```

Full final acceptance remains:

```powershell
.\tools\run-stage27-1-checks.ps1
```

Manual player check:

```powershell
.\build\windows-player-stage16\ProjectAegisRTS.exe
```

Start the vertical slice, build a Power Plant from the right sidebar, click the ready card, verify the center board setup overlay stays hidden, place on the fine grid, and inspect `Player.log`.
