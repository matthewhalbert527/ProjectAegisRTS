# Unity Setup Notes

This folder is now the Stage 1 Unity desktop prototype root. Unity is presentation and input only: `Rts.Core` remains the deterministic authority, and Unity reads `WorldSnapshot` data while submitting command DTOs back to the core.

## Build the Core DLL

From `E:\OpenRA Mod\ProjectAegisRTS`:

```powershell
.\tools\build-rts-core-for-unity.ps1
```

The script builds `src/Rts.Core` and copies `Rts.Core.dll` into:

```text
unity\Assets\Rts\Plugins\RtsCore\Rts.Core.dll
```

## Open the Project

```powershell
.\tools\open-unity-project.ps1
```

If Unity Editor is not installed, open Unity Hub manually, add/open the `unity` folder, and let Unity import the project.

## Create the Stage 1 Scene

In Unity, run:

```text
ProjectAegisRTS > Create Stage 1 Desktop Board Scene
```

This creates `Assets/Rts/Scenes/Stage1_DesktopBoard.unity` with `RtsGame`, `BoardRoot`, camera, light, and the Stage 1 scripts wired together.

## Controls

- Left click: select actor or place active building preview.
- Right click: move selected mobile units.
- Space: pause or resume.
- Period or N: single-step one tick.
- Escape: cancel placement or clear selection.
- P/B/W/R/G: queue power plant, barracks, war factory, refinery, gun tower.
- I/T/H: queue rifle infantry, light tank, harvester.
- L: toggle forced low-power demo state.
- WASD: pan camera.
- Mouse wheel: zoom.
- Q/E: rotate camera.

## Known Stage 1 Limits

- No Quest, OpenXR, Meta XR, hand tracking, or MR board placement packages are included.
- The HUD is an IMGUI debug surface, not the final PC sidebar.
- Placeholder primitives stand in for final art, animation, and vehicle motion.
- Unity Editor was not available during local implementation, so Unity compile/play validation must be run after opening the project.

## Later Stages

Stage 2 can replace the debug HUD with a desktop RTS sidebar and stronger production UX. Later Quest/MR stages can swap the board transform and input layer without moving authoritative simulation state out of `Rts.Core`.
