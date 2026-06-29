# Stage 4 Report

## Summary

Stage 4 adds the Quest Left-Hand Build/Selection Interface while preserving the Stage 1 desktop board, Stage 2 PC sidebar, and Stage 3 board placement scenes.

Branch: `codex/stage-4-left-hand-build-selection`
Base commit: `37975c9 Implement Stage 3 Quest board placement prototype`

## Systems Created

Scene:

```text
unity/Assets/Rts/Scenes/Stage4_LeftHandBuildSelection.unity
```

Runtime systems:

- `LeftHandBuildMenuController`: owns menu state, active category, selected build item, MVP/future production view models, and queue actions.
- `LeftHandRadialMenuView`: generated wrist/radial uGUI build surface attached to the simulated left-hand anchor.
- `LeftHandCommandRouter`: routes production, placement, selection, cancellation, and low-power demo requests through the existing simulation driver.
- `Stage4ModeCoordinator`: coordinates menu, placement, selection ray, lasso, ambiguous selection, desktop fallback input, and XR adapter input.
- `DesktopLeftHandInputSource`: headset-free controls for Stage 4.
- `XrLeftHandInputAdapter`: compile-safe future XR adapter with no hard dependency on XR Interaction Toolkit or Meta packages.
- `SimulatedLeftHandRig`: visible placeholder controller, wrist anchor, and ray line.
- `SelectionResolver`, `LeftHandSelectionController`, and `LeftHandLassoSelectionController`: ray/cell candidate search, ranking, candidate cycling, additive selection, and board-space lasso start/cancel/complete.
- `LeftHandPlacementPanel`, `LeftHandSelectionPanel`, and `LeftHandStatusHud`: Stage 4 placement, selection, and debug/status readouts.

Editor/validation systems:

- `Stage4SceneCreator`
- `Stage4SceneValidator`
- `Stage4PlayModeSmokeValidator`
- `Stage4XrSetupReporter`
- `tools/run-unity-stage4-validation.ps1`
- `tools/run-stage4-checks.ps1`

## Files Changed Summary

- `unity/Assets/Rts/Scripts/UI/XR/LeftHand/`: left-hand menu, radial/wrist view, command router, placement panel, selection panel, status HUD, and mode coordinator.
- `unity/Assets/Rts/Scripts/Input/Desktop/` and `unity/Assets/Rts/Scripts/Input/XR/`: desktop fallback input, compile-safe XR input interface/adapter, and simulated left-hand rig.
- `unity/Assets/Rts/Scripts/Selection/`: selection candidate model, resolver, ray selection controller, and lasso controller.
- `unity/Assets/Rts/Scripts/CoreBridge/RtsSimulationDriver.cs`: client-local selection and placement-preview helper APIs.
- `unity/Assets/Rts/Scripts/Rendering/BoardRenderer.cs`: Stage 4 hover/placement preview APIs.
- `unity/Assets/Rts/Editor/`: Stage 4 scene creator, scene validator, smoke validator, and XR input status reporter.
- `unity/Assets/Rts/Scenes/Stage4_LeftHandBuildSelection.unity`: generated Stage 4 scene.
- `tools/`: Stage 4 validation/check scripts and Unity batch wrapper robustness updates.
- `docs/`, `README.md`, and Unity setup readmes: Stage 4 report, XR input status, controls, architecture, and stage plan updates.

## Desktop Fallback Controls

- `C`: toggle the left-hand build interface.
- `F1`-`F6`: switch production categories.
- `1`-`8`: queue the matching build card in the active category.
- Mouse ray: simulated left-hand ray.
- Left mouse or Enter: select, confirm placement, or activate current action.
- Ctrl + left mouse: additive selection.
- Escape: cancel placement/menu/active mode or clear selection.
- Tab: cycle ambiguous selection candidates.
- Shift + Tab: cycle backward through candidates.
- Backquote: toggle the Stage 4 status HUD.
- `L` plus mouse drag: board-space lasso selection.

## Production And Placement Flow

The left-hand menu is Unity-side view state only. It reads definitions, production snapshots, and queue state from `RtsSimulationDriver`. Production requests go through `LeftHandCommandRouter.QueueProduction`, which calls the same simulation bridge used by the PC sidebar. Completed building production enters placement mode through the driver, and placement confirmation calls `TryPlacePendingBuildingAtCell`.

Placement preview uses `BoardCoordinateMapper` for ray-to-cell conversion and `BoardRenderer` for hovered-cell and footprint preview rendering. Invalid placement returns a structured command failure and stays in placement mode.

## Selection Flow

Selection stays client-local in Unity, matching the current Stage 1/2 structure. `SelectionResolver` searches actor snapshots by board cell and ray distance, ranks candidates by actor type and existing selection state, and returns sorted `LeftHandSelectionCandidate` values. The controller supports single selection, additive selection, candidate cycling, and rectangular board-space lasso selection.

## XR Input Status

See `docs/STAGE4_XR_INPUT_STATUS.md`.

Current status:

- XR Plug-in Management: present.
- OpenXR Plugin: present.
- Input System: present.
- XR Interaction Toolkit: not detected.
- Meta XR Core SDK: not detected.
- Meta XR Interaction SDK: not detected.

Meta XR packages were not imported automatically. Stage 4 runtime scripts compile without those packages.

## Validation

Local automated validation result:

- `.NET/Rts.Core`: passed 10/10 Stage 0 tests.
- Stage 1 validation: passed.
- Stage 2 validation: passed.
- Stage 2 Play Mode smoke: passed.
- Stage 3 validation: passed.
- Stage 3 Play Mode smoke: passed through `run-unity-stage3-validation.ps1`.
- Stage 4 validation: passed.
- Stage 4 Play Mode smoke: passed through `run-unity-stage4-validation.ps1`.
- `Rts.Core` UnityEngine-free check: passed; no references found.
- `git diff --check`: passed after normalizing Unity-generated whitespace.

Automated Stage 4 validation covers:

- core DLL build/copy
- XR package/input status report generation
- Stage 4 scene creation
- Stage 4 scene structure validation
- runtime smoke validation for board visuals, actor visuals, tick advance, menu toggle, category switching, MVP build items, queuing `power_plant`, placement preview, invalid placement failure, valid placement path, candidate search, candidate cycling, selection state, lasso start/cancel, cancel behavior, and red console errors

Acceptance commands:

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
.\tools\run-unity-stage4-validation.ps1
.\tools\run-stage4-checks.ps1
git diff --check
```

## Manual Play Mode Checklist

Open:

```text
unity/Assets/Rts/Scenes/Stage4_LeftHandBuildSelection.unity
```

Press Play and verify:

- board visible
- actors visible
- simulated left hand/controller visible
- left-hand ray visible
- left-hand build menu visible when toggled
- category switching works
- MVP production cards appear
- queueing `power_plant` works
- placement mode starts
- placement footprint appears on board
- valid placement succeeds
- invalid placement gives feedback
- ray selection finds actors
- ambiguous selection cycling works if candidates overlap
- selected actor visual indicator updates
- lasso/box selection starts and cancels
- cancel exits active modes
- Stage 3 board placement scene still opens/validates
- Stage 2 PC sidebar scene still opens/validates
- no repeating red console errors

## Known Limitations

- The wrist/radial UI is generated placeholder uGUI, not final Quest visual design.
- The XR adapter is intentionally no-op until package-backed controller/hand bindings are added.
- Meta hand pinch, controller trigger/grip mapping, and physical Quest testing remain manual future work.
- Lasso is a simple board-space rectangle, not a final freeform gesture.
- Right-hand tactical orders remain out of scope for Stage 4.

## Stage 5 Recommendation

Build the Quest right-hand tactical command interface next: movement/attack confirmation, command ray, board manipulation coexistence, and clear division between left-hand build/selection and right-hand tactical orders.
