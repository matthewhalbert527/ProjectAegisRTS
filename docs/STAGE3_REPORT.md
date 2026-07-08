# Stage 3 Report

## Summary

Stage 3 adds the Quest/OpenXR Board Placement Prototype while preserving the Stage 1 desktop board and Stage 2 PC sidebar paths.

Branch: `codex/stage-3-quest-board-placement`
Base commit: `dc736c6 Fix Stage 2 validation fallback without ripgrep`

## Stage 2 Validation

Stage 2 received automated smoke tooling:

- `unity/Assets/Rts/Editor/Stage2PlayModeSmokeValidator.cs`
- `tools/run-stage2-playmode-smoke.ps1`
- `tools/run-stage2-checks.ps1` now calls the smoke script.

The smoke validator covers scene objects, camera framing, runtime board visuals, actor visuals, snapshots, actor count, tick advance, pause, single-step, production command routing, low-power command routing, selection panel routing, and red console errors.

Stage 2 functional fix made during Stage 3 work:

- `ActorViewBehaviour` now destroys generated colliders and actor visuals with `DestroyImmediate` in editor-driven smoke validation and `Destroy` in Play Mode. This avoids red editor errors without changing gameplay logic.

## Stage 3 Systems Created

Scene:

```text
unity/Assets/Rts/Scenes/Stage3_XRBoardPlacement.unity
```

Runtime systems:

- `BoardTransformModel`: owns board position, height, yaw, scale, meters per cell, width/height, reset, recenter, save, and load.
- `BoardPlacementController`: applies the model to `BoardRoot` and refreshes `BoardCoordinateMapper`.
- `DesktopBoardPlacementInput`: desktop fallback placement controls.
- `XrBoardPlacementInputAdapter`: package-independent future Quest/OpenXR adapter.
- `Stage3XrRigPlaceholder`: placeholder rig root, head, controller, and ray transforms.
- `BoardPlacementHud`: uGUI placement HUD with buttons and status readout.

Editor/validation systems:

- `Stage3SceneCreator`
- `Stage3SceneValidator`
- `Stage3PlayModeSmokeValidator`
- `Stage3OpenXrSetupReporter`
- `tools/run-unity-stage3-validation.ps1`
- `tools/run-stage3-checks.ps1`

## XR/OpenXR Package Status

See `docs/STAGE3_XR_SETUP_STATUS.md`.

Current status:

- XR Plug-in Management: present.
- OpenXR Plugin: present.
- Input System: present.
- XR Interaction Toolkit: not detected.
- Meta XR Core SDK: not detected.
- Meta XR Interaction SDK: not detected.

Meta XR packages were not imported automatically.

## Desktop Fallback Controls

- Tab: toggle board placement mode.
- Enter: confirm placement and save.
- Escape: cancel placement and restore the starting transform.
- R: reset defaults while placement mode is active.
- Arrow keys or WASD: move board horizontally.
- Q/E: yaw rotation.
- PageUp/PageDown or Z/X: height.
- Shift or Ctrl plus mouse wheel: scale.
- HUD buttons: toggle, confirm, cancel, reset, save, load, recenter.

## Save/Load Behavior

Stage 3 board placement saves through `PlayerPrefs` using the Stage 3 board transform key. Save/load changes Unity presentation state only. It does not mutate deterministic `Rts.Core` simulation state.

## Commands Run

Baseline and implementation validation included:

```powershell
dotnet run --project src/Rts.Core.Tests
.\tools\build-rts-core-for-unity.ps1
.\tools\run-stage1-checks.ps1
.\tools\run-unity-stage1-validation.ps1
.\tools\run-unity-stage2-validation.ps1
.\tools\run-stage2-checks.ps1
.\tools\run-unity-stage3-validation.ps1
```

Final acceptance commands run during this pass:

```powershell
dotnet run --project src/Rts.Core.Tests
.\tools\build-rts-core-for-unity.ps1
.\tools\run-stage1-checks.ps1
.\tools\run-unity-stage1-validation.ps1
.\tools\run-unity-stage2-validation.ps1
.\tools\run-stage2-checks.ps1
.\tools\run-stage2-playmode-smoke.ps1
.\tools\run-unity-stage3-validation.ps1
.\tools\run-stage3-checks.ps1
git diff --check
```

## Validation Results

Final acceptance results:

- .NET tests: passed 10/10.
- Unity Stage 1 validation: passed.
- Unity Stage 2 validation: passed.
- Unity Stage 2 Play Mode smoke: passed.
- Unity Stage 3 validation: passed.
- Unity Stage 3 Play Mode smoke: passed.
- `Rts.Core` UnityEngine-free check: passed.

## Manual Play Mode Checklist

Open:

```text
unity/Assets/Rts/Scenes/Stage3_XRBoardPlacement.unity
```

Press Play and verify:

- Board visible.
- Actors visible.
- Stage 3 board placement HUD visible.
- Tick advances.
- Placement mode toggles.
- Board moves in desktop fallback placement mode.
- Board height changes.
- Board yaw changes.
- Board scale changes.
- Reset works.
- Save/load works.
- Recenter works.
- Normal board/unit interaction resumes when placement mode is inactive.
- No repeating red console errors.
- `Stage2_PCSidebar.unity` still opens and validates.
- `Rts.Core` remains UnityEngine-free.

## Known Limitations

- Stage 3 uses a placeholder rig, not a final XR Origin.
- XR Interaction Toolkit is not installed.
- Meta XR packages are not installed.
- Passthrough, spatial anchors, hand tracking, and final Quest UI are not implemented.
- Board placement is presentation/input infrastructure only.

## Manual Quest Setup Still Needed

- Confirm XR Plug-in Management settings in Unity Project Settings.
- Enable OpenXR for the target build platform.
- Configure Quest/OpenXR feature groups and controller profiles.
- Install Meta XR packages manually later if project policy approves them.
- Replace the placeholder rig with a real XR Origin/controller setup.

## Stage 4 Recommendation

Build the Quest left-hand build/selection interface next. It should feed existing Stage 2 command routing and Stage 3 placement/controller adapters without moving gameplay authority out of `Rts.Core`.
