# Unity Setup Notes

This folder is now the Unity desktop prototype root. Unity is presentation and input only: `Rts.Core` remains the deterministic authority, and Unity reads `WorldSnapshot` data while submitting command DTOs back to the core.

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

The helper searches the custom local install path `E:\Unity\Hub\Editor\*\Editor\Unity.exe` as well as common Unity Hub locations. If Unity Editor is not found, open Unity Hub manually, add/open the `unity` folder, and let Unity import the project.

## Create the Stage 1 Scene

The Stage 1 check script can create this scene in batchmode. To recreate it manually in Unity, run:

```text
ProjectAegisRTS > Create Stage 1 Desktop Board Scene
```

This creates `Assets/Rts/Scenes/Stage1_DesktopBoard.unity` with `RtsGame`, `BoardRoot`, camera, light, and the Stage 1 scripts wired together.

## Create or Validate the Stage 2 Scene

Stage 2 adds the first PC RTS sidebar scene:

```text
Assets/Rts/Scenes/Stage2_PCSidebar.unity
```

To create or refresh it manually in Unity, run:

```text
ProjectAegisRTS > Create Stage 2 PC Sidebar Scene
```

To validate it from PowerShell:

```powershell
.\tools\run-unity-stage2-validation.ps1
.\tools\run-stage2-checks.ps1
```

The Stage 2 scene uses uGUI via `com.unity.ugui`, keeps the Stage 1 board/runtime bridge, and adds a screen-space canvas with the right sidebar, command bar, status log, production panels, selection panel, and minimap placeholder.

## Controls

- Left click: select actor or place active building preview.
- Right click: move selected mobile units.
- Space: pause or resume.
- Period or N: single-step one tick.
- Escape: cancel placement or clear selection.
- P/B/W/R/G: queue power plant, barracks, war factory, refinery, gun tower.
- I/T/H: queue rifle infantry, light tank, harvester.
- L: toggle forced low-power demo state.
- F1-F6: switch Stage 2 production tabs.
- S/M/A: stop, move mode, or attack placeholder command.
- Backquote: toggle the Stage 1 debug overlay if it is enabled in the scene.
- WASD: pan camera.
- Mouse wheel: zoom.
- Q/E: rotate camera.

Stage 2 also exposes these actions through buttons in the sidebar and bottom command bar: production, queue cancel, placement cancel, stop, move, attack placeholder, guard/patrol/deploy/repair/sell placeholders, power toggle, pause, step, and low-power demo.

## Known Limits

- No Quest, OpenXR, Meta XR, hand tracking, or MR board placement packages are included.
- Stage 2 is still placeholder-art PC UI, not final visual art.
- The attack, guard, patrol, deploy, repair, and sell buttons are logged placeholders until later gameplay systems exist.
- Placeholder primitives stand in for final art, animation, and vehicle motion.
- Unity 6000.5.1f1 batchmode script compilation and scene generation pass locally.
- Play mode interaction validation should still be checked interactively after opening the generated scene.

## Later Stages

Later Quest/MR stages can swap the board transform and input layer without moving authoritative simulation state out of `Rts.Core`.
