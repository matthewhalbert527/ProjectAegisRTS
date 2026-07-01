# Stage 18 Tester Playability Guide

## Launch

From the repository root:

```powershell
.\build\windows-player-stage16\ProjectAegisRTS.exe
```

If the EXE needs to be rebuilt:

```powershell
.\tools\build-windows-player-stage16.ps1
```

## Expected First Screen

The Windows player starts in the Boot scene. Choose `Start Vertical Slice` to load:

```text
Assets/Rts/Scenes/Stage16_PlayableVerticalSlice.unity
```

The board should be visible, brighter than the old dark EXE screenshot, and framed closer. The status log and debug panels should be hidden by default.

Stage 18.5 adds a denser fine placement grid. Coarse board boundaries should still be readable, but building placement now snaps at half the old coarse-cell spacing.

## In-Match Checklist

The visible checklist walks through:

1. Select Fabrication Hub.
2. Build Power Plant.
3. Build Refinery.
4. Harvest resources.
5. Build Barracks.
6. Train infantry.
7. Build War Factory.
8. Produce a light tank or harvester.
9. Scout the enemy base.
10. Attack the enemy base.
11. Destroy the enemy base.

Use the right sidebar cards to queue production. The recommended next item is marked with `NEXT:`.

## Controls

- Left click: select actor or place active building preview.
- Right click: move selected mobile units or attack an enemy under the cursor.
- Space: pause or resume.
- Period or N: single-step while paused.
- B: enter placement for a completed building.
- Escape: cancel placement or clear selection.
- O: toggle objective/match HUD.
- C: toggle build-order checklist.
- P: toggle next-step prompt.
- F1 or H: toggle controls overlay.

Developer debug hotkeys remain available, but those panels should start hidden.

## Fine Placement Check

When a completed building is ready to place, press `B` or use the sidebar placement action. The preview should snap to the fine grid. A building that previously covered 2 x 2 coarse cells now covers 4 x 4 fine cells while appearing the same physical size on the board.

The placement panel may show both coarse and fine footprint information while placement mode is active. That panel should hide again when placement is cancelled or completed.

## Pass Criteria

- Board, grid, resources, buildings, and placeholder actors are readable.
- Fine grid lines are visible without overwhelming coarse board boundaries.
- Building placement snaps to the fine grid, including half-coarse-cell offsets.
- Objective, prompt, checklist, sidebar, and match controls do not overlap.
- Tick count advances after starting.
- Pause/resume and single-step work.
- Left-click selection works.
- Right-click movement works.
- Production cards explain missing prerequisites and ready-to-place buildings.
- Low-power building visual state can still be observed.
- Victory appears only after the enemy base objective is completed.
- Defeat appears when the player base is destroyed.
- No repeating red console errors appear in Unity or Player logs.
