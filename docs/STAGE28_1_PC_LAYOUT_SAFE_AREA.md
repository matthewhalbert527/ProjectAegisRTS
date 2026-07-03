# Stage 28.1 PC Layout Safe Area

Stage 28.1 reserves a player-facing camera safe area for PCDesktop so the board no longer renders underneath the right-side sidebar.

## Layout Contract

- PCDesktop and DebugHybrid reserve the right CnC/OpenRA sidebar.
- PCDesktop and DebugHybrid reserve the left objective/checklist/prompt column.
- The board camera renders only into the remaining center gameplay rect.
- QuestXR keeps a full-screen camera rect so hand-control and board-placement behavior remains unchanged.
- Debug and QA panels remain hidden by default.

## Runtime Components

- `PcGameplaySafeAreaController` computes the screen-space safe area from the active canvas scale, sidebar width, objective stack width, and margins.
- `PlayerFacingCameraFramer` applies the camera rect and orthographic framing so the board bounds fit inside the safe area.
- `PlayerBuildSceneInitializer` adds these components at runtime if an older serialized Stage 16 scene does not already contain them.
- `Stage16SceneCreator` wires the same components when the player-facing scene is regenerated.

## Manual PC Check

```powershell
cd "E:\OpenRA Mod\ProjectAegisRTS"
.\tools\build-windows-player-stage16.ps1
.\build\windows-player-stage16\ProjectAegisRTS.exe
```

In the EXE:

- Boot menu is readable.
- Start Vertical Slice opens the board.
- Right sidebar is visible and docked to the right edge.
- Minimap sits at the top of the sidebar.
- Board stays in the center gameplay area and does not appear under the sidebar.
- Objective/checklist/prompt text remains on the left and does not cover the main board.
- Select a unit and right-click a diagonal destination; the unit should advance diagonally along the board.
- Build a Power Plant, click the ready card, hover a valid fine-grid location, and left-click to place it.
- `Player.log` has no repeating red errors.

## Automated Check

```powershell
.\tools\run-unity-stage28-1-validation.ps1
.\tools\run-stage28-1-player-facing-checks.ps1 -SkipPlayerBuild
```

The Unity validator checks safe-area behavior at 1280x720, 1600x900, 1920x1080, and 2560x1440.
