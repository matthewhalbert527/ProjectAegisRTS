# Stage 19.5 UI Rework Report

## Why This Pass Exists

Stage 19 made the vertical slice completeable, but the Windows player still felt like a mixed desktop/XR prototype. The minimap was useful, yet production, selection, and command controls were not presented as a clear PC RTS sidebar, and Escape still behaved like a cancel key instead of opening a normal pause menu.

## Findings

- The PC production pieces already existed: `DesktopRtsHudRoot`, `ProductionCategoryTabs`, `ProductionGridController`, `ProductionQueuePanel`, `SelectionPanelController`, `CommandBarController`, and `MinimapPlaceholderController`.
- Those pieces were positioned as separate panels, and the command bar remained a bottom-wide strip.
- The old command bar exposed development actions such as single-step and low-power demo beside player commands.
- XR left-hand/right-hand fallback controllers could still own keyboard inputs in the player-facing PC build unless explicitly suppressed.
- The existing boot menu had Controls and Options screens, but the match scene did not have an Esc pause overlay.

## Implemented

- Added `CncStyleSidebarLayout` to own the right-side PC layout.
- Kept the minimap in the top-right, with resource/power/status, production tabs, cards, queue, placement, selection, and commands below it.
- Added `PlayerFacingUiModeController` so the Windows build defaults to PC mode and hides XR left-hand/right-hand build UI.
- Added `PauseMenuController` and `PauseMenuHud` with Resume, Restart Mission, Settings, Controls, Quit to Menu, and Quit Game.
- Reserved Escape for the pause overlay and blocked normal board/sidebar input while paused.
- Kept debug panels and placement UI hidden by default.
- Kept objective, prompt, and checklist HUDs compact on the left.
- Fixed the production queue duplicate `RectTransform`/`NullReferenceException` guard found during baseline log inspection.

## Validation

Stage 19.5 adds:

- `tools/run-unity-stage19-5-validation.ps1`
- `tools/run-stage19-5-fast-checks.ps1`
- `tools/run-stage19-5-medium-checks.ps1`
- `tools/run-stage19-5-player-facing-checks.ps1`
- `tools/run-stage19-5-checks.ps1`
- `Stage19_5SidebarPauseValidator`
- `Stage19_5PlayModeSmokeValidator`

The medium tier remains non-recursive and is covered by `tools/audit-medium-validation-recursion.ps1`.

## Known Limitations

The sidebar still uses generated uGUI text/buttons and placeholder icons. Settings and Controls in the in-match pause overlay are prototype panes; the full Options screen remains in the Boot scene. Final visual styling and model/icon replacement belong to Stage 20 and later art/UI passes.
