# Stage 2 Report

## Summary

Stage 2 adds a PC RTS sidebar scene on top of the validated Stage 1 board:

```text
unity/Assets/Rts/Scenes/Stage2_PCSidebar.unity
```

The scene keeps `Rts.Core` as the deterministic authority and uses Unity only for presentation and input. The Stage 2 UI is built with uGUI and a new screen-space `Stage2 Canvas`.

## Implemented

- Right sidebar with credits, power, actor count, tick, and mode readout.
- Production category tabs for Buildings, Defenses, Infantry, Vehicles, Aircraft, and Support.
- Production grid with costs, build times, build progress, and future placeholder entries.
- Production queue with progress bars and queue cancellation.
- Placement panel with pending building type, footprint, valid/invalid hover state, and cancel action.
- Selection panel with selected actor details, stop, move, and power toggle actions.
- Bottom command bar with stop, move, attack placeholder, guard/patrol/deploy/repair/sell placeholders, power, pause, step, and low-power demo.
- Minimap placeholder with actor dots.
- Status log that deduplicates repeated command results.
- Hidden-by-default Stage 1 debug overlay with a runtime toggle field.
- Input routing that prevents uGUI click-through while preserving left-click selection and right-click movement on the board.

## New Validation

Stage 2 adds:

```powershell
.\tools\run-unity-stage2-validation.ps1
.\tools\run-stage2-checks.ps1
```

`run-unity-stage2-validation.ps1` builds `Rts.Core`, creates `Stage2_PCSidebar.unity` in Unity batchmode when Unity is closed, validates required scene objects/components, and verifies the safe orthographic camera settings. If the editor is already open for this project, it falls back to live scene-file and editor-log checks.

`run-stage2-checks.ps1` runs the core tests, rebuilds the Unity DLL, runs the Stage 1 checks, runs the Stage 1 validation, runs the Stage 2 validation, verifies `Rts.Core` remains `UnityEngine`-free, and runs `git diff --check`.

## Manual Play Checklist

Open `Assets/Rts/Scenes/Stage2_PCSidebar.unity`, press Play, then confirm:

- Board visible in Game view.
- Grid and board surface visible.
- Placeholder actors visible.
- Right sidebar, command bar, queue, selection panel, minimap placeholder, and status log visible.
- Tick count advances when unpaused.
- Pause/resume and single-step work.
- Left-click selection and right-click movement work.
- Production buttons queue MVP entries and completed buildings enter placement mode.
- Low-power demo changes building visual state.
- No repeating red console errors.

## Limits

Stage 2 does not add Quest/XR, final art, multiplayer, AI, pathfinding upgrades, combat systems, or final command implementations for attack/guard/patrol/deploy/repair/sell. Those actions are explicit placeholders in the status log.
