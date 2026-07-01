# Stage 19.5 PC Sidebar And Pause Menu

## Player-Facing Layout

The Windows player now defaults to a PC RTS layout:

- right side: minimap, credits/power/status, production categories, build cards, queue, placement status, selection details, and command buttons,
- center: playable board,
- left/top-left: objective, checklist, and next-step prompts only.

The Quest/MR left-hand and right-hand UI remains in the project for XR work, but it is hidden and input-suppressed by default in PC player-facing mode.

## Pause Menu

Escape opens a centered pause overlay with a dim background:

- Resume
- Restart Mission
- Settings
- Controls
- Quit to Menu
- Quit Game

Opening the menu pauses the deterministic simulation through `RtsSimulationDriver`. Resume restores simulation if the menu caused the pause. Restart resets the Stage16 vertical slice through `VerticalSliceScenarioController`.

## Manual Checklist

Run:

```powershell
.\build\windows-player-stage16\ProjectAegisRTS.exe
```

Verify:

- Boot menu appears.
- Start Vertical Slice loads the board.
- Minimap is top-right above the production bar.
- Right-side production categories and cards are visible.
- Left side shows only objective/checklist/prompt UI.
- Power Plant, Refinery, Barracks, War Factory, Gun Tower, Rifle Infantry, Light Tank, and Harvester are reachable through the sidebar categories.
- Building placement uses the fine-grid preview.
- Escape opens the pause menu.
- Resume, Restart Mission, Controls, Settings, Quit to Menu, and Quit Game are present.
- Debug panels start hidden.
- Placement UI is hidden until placement mode is active.
- Player.log has no repeating red-error signatures.

## Validation Commands

```powershell
.\tools\run-stage19-5-fast-checks.ps1
.\tools\run-stage19-5-medium-checks.ps1
.\tools\run-stage19-5-player-facing-checks.ps1 -SkipPlayerBuild
.\tools\run-stage19-5-checks.ps1
```

Use fast checks during iteration, medium before commit, player-facing checks for EXE readiness, and full checks for final acceptance.
