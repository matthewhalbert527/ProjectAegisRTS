# Stage 5 Report

## Summary

Stage 5 adds the Quest Right-Hand Tactical Command Interface while preserving the Stage 1 desktop board, Stage 2 PC sidebar, Stage 3 board placement, and Stage 4 left-hand build/selection scene.

Branch: `codex/overnight-stage4-stage5`

Scene:

```text
unity/Assets/Rts/Scenes/Stage5_DualHandCommand.unity
```

## Systems Created

Runtime systems:

- `RightHandCommandMode`: right-hand command mode enum for hidden, idle, move, attack, force-attack, context command, board manipulation, and disabled states.
- `DesktopRightHandInputSource`: mouse/keyboard fallback for command ray, right-click confirmation, HUD toggle, move/attack modes, board manipulation, rotation, and zoom.
- `XrRightHandInputAdapter`: compile-safe XR adapter boundary with no hard dependency on XR Interaction Toolkit or Meta packages.
- `SimulatedRightHandRig`: visible placeholder right-hand/controller rig, wrist anchor, and command ray.
- `RightHandCommandRouter`: routes move commands through `RtsSimulationDriver`, blocks tactical commands during placement, and provides attack/force-attack placeholder feedback.
- `Stage5DualHandModeCoordinator`: coordinates desktop/XR right-hand input with Stage 3 board placement and Stage 4 placement modes.
- `RightHandCommandHud`, `RightHandCommandReticle`, and `RightHandStatusPanel`: generated placeholder UI and command feedback.
- `CommandPreviewRenderer`: target-cell marker for move, attack placeholder, and invalid command previews.

Editor/validation systems:

- `Stage5SceneCreator`
- `Stage5SceneValidator`
- `Stage5PlayModeSmokeValidator`
- `tools/run-unity-stage5-validation.ps1`
- `tools/run-stage5-checks.ps1`

## Desktop Fallback Controls

- `V`: toggle the right-hand command HUD.
- Right mouse or Enter: confirm the current right-hand command.
- `M`: enter move mode.
- `A`: enter attack placeholder mode.
- `F`: enter force-attack placeholder mode.
- Space or middle mouse: board manipulation mode.
- `Q` / `E`: rotate the board while in board manipulation mode.
- Mouse wheel: scale/zoom the board while in board manipulation mode.
- Escape: cancel the active right-hand command mode.
- Shift: reserved alternate-command modifier.

## Command Boundary

Right-hand movement uses `RtsSimulationDriver.TryIssueMoveSelectedToCell`, which submits into the existing simulation bridge. Attack and force-attack are placeholders because final combat is still out of scope. They show command feedback and preview markers without adding weapons, projectiles, damage, or new authoritative combat rules.

The right hand is disabled while Stage 4 building placement or Stage 3 board placement is active. Board manipulation uses the Stage 3 `BoardPlacementController` presentation layer and does not mutate `Rts.Core` gameplay state.

## Validation

Automated Stage 5 validation covers:

- scene creation for `Stage5_DualHandCommand.unity`
- scene structure validation for Stage 3, Stage 4, and Stage 5 components
- runtime smoke validation for bootstrap, board visuals, actor visuals, left-hand menu preservation, mobile actor selection, move command routing, command preview, attack placeholder, force-attack placeholder, board manipulation mode, placement suppression, cancellation, and red console errors
- `Rts.Core` UnityEngine-free checks with optional `rg` and PowerShell fallback

Acceptance commands:

```powershell
dotnet run --project src/Rts.Core.Tests
.\tools\build-rts-core-for-unity.ps1
.\tools\run-stage1-checks.ps1
.\tools\run-stage2-checks.ps1
.\tools\run-stage2-playmode-smoke.ps1
.\tools\run-stage3-checks.ps1
.\tools\run-stage4-checks.ps1
.\tools\run-unity-stage5-validation.ps1
.\tools\run-stage5-checks.ps1
git diff --check
```

## Manual Play Mode Checklist

Open:

```text
unity/Assets/Rts/Scenes/Stage5_DualHandCommand.unity
```

Press Play and verify:

- board visible
- actors visible
- simulated left hand visible
- simulated right hand visible
- left-hand build/selection still works
- right-hand command HUD toggles
- right-hand ray visible
- move mode works
- right-click or right-hand confirm moves selected units
- command preview marker appears
- attack placeholder mode does not throw
- context command feedback appears
- board manipulation mode does not conflict with placement/build modes
- no repeating red console errors

## Known Limitations

- Right-hand XR input is a compile-safe no-op adapter until package-backed Quest bindings are added.
- Attack and force-attack are intentionally feedback-only placeholders.
- Command previews use simple generated geometry, not final VFX.
- Board manipulation reuses the Stage 3 presentation transform layer.
- Physical Quest hand/controller testing remains a future manual pass.

## Stage 6 Recommendation

Build the high-quality visual movement layer next: acceleration/braking visuals, turning arcs, track/wheel animation, suspension, turret lag, infantry locomotion blend states, and aircraft banking layered over deterministic snapshots.
