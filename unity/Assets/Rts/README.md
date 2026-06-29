# Rts Unity Assets

Stage 1 adds a desktop board prototype that renders `Rts.Core` snapshots and submits commands back into the deterministic simulation. Stage 2 keeps that board and adds the first PC RTS uGUI layer in `Assets/Rts/Scenes/Stage2_PCSidebar.unity`. Stage 3 adds `Assets/Rts/Scenes/Stage3_XRBoardPlacement.unity` for Quest/OpenXR-ready board placement with desktop fallback controls.

## Folder Roles

- `Scripts/Bootstrap`: scene startup and reference wiring.
- `Scripts/Board`: Stage 3 board transform model and placement controller.
- `Scripts/CoreBridge`: Unity-to-core adapters, command helpers, and board coordinate mapping.
- `Scripts/Input`: desktop mouse/keyboard input plus XR-safe placement adapters and placeholders.
- `Scripts/Rendering`: board, actor, selection, low-power, production, and interpolation visuals.
- `Scripts/UI`: Stage 1 IMGUI debug HUD plus Stage 2 uGUI common and desktop sidebar controllers.
- `Scripts/Camera`: desktop camera controls.
- `Scripts/Utilities`: generated runtime materials.
- `Editor`: scene generator menu item and batchmode entry point.
- `Plugins/RtsCore`: copied `Rts.Core.dll` and optional PDB.

## Simulation Boundary

Unity does not own gameplay state. It smooths visual transforms between snapshots, but actor position, power state, production state, placement validation, and move orders come from `Rts.Core`.

The actor view layer tracks previous and target snapshot positions, facing, normalized speed, visual motion profile id, and actor category so later stages can add acceleration/braking visuals, turning arcs, tracks/wheels, suspension, turret lag, infantry locomotion, and aircraft banking without faking gameplay movement.

## Stage 2 UI

- `Scripts/UI/Desktop/DesktopRtsHudRoot.cs`: root initializer for the Stage 2 canvas and command router.
- `Scripts/UI/Desktop/DesktopSidebarController.cs`: credits, power, actor count, tick, and command mode readout.
- `Scripts/UI/Desktop/ProductionCategoryTabs.cs`: F1-F6 and button-driven production categories.
- `Scripts/UI/Desktop/ProductionGridController.cs`: build cards, costs, progress, and MVP/future production state.
- `Scripts/UI/Desktop/ProductionQueuePanel.cs`: active queue rows, progress bars, and cancellation.
- `Scripts/UI/Desktop/PlacementModePanel.cs`: pending building placement status and footprint readout.
- `Scripts/UI/Desktop/SelectionPanelController.cs`: selected actor details and basic commands.
- `Scripts/UI/Desktop/CommandBarController.cs`: bottom PC command bar for stop, move, pause, step, low power, and placeholders.
- `Scripts/UI/Desktop/MinimapPlaceholderController.cs`: compact actor-dot minimap placeholder.
- `Scripts/UI/Common/RtsStatusLog.cs`: deduplicated command/result log.

`Editor/Stage2SceneCreator.cs` creates the scene, and `Editor/Stage2SceneValidator.cs` validates that the scene contains the board, camera, UI systems, event system, and safe orthographic camera framing. `Editor/Stage2PlayModeSmokeValidator.cs` adds runtime smoke coverage for bootstrap, board visuals, actor visuals, ticks, pause, step, production, low power, and UI command routing.

## Stage 3 Board Placement

- `Scripts/Board/BoardTransformModel.cs`: serializable presentation transform data with reset, save/load, recenter, height, yaw, scale, and meters-per-cell controls.
- `Scripts/Board/BoardPlacementController.cs`: applies the transform to `BoardRoot` and refreshes `BoardCoordinateMapper` without mutating deterministic simulation state.
- `Scripts/Input/Desktop/DesktopBoardPlacementInput.cs`: desktop fallback controls for moving, rotating, scaling, saving, and cancelling placement.
- `Scripts/Input/XR/XrBoardPlacementInputAdapter.cs`: package-independent Quest/OpenXR adapter placeholder.
- `Scripts/Input/XR/Stage3XrRigPlaceholder.cs`: fallback rig root, head/camera, controller, and ray placeholders.
- `Scripts/UI/XR/BoardPlacementHud.cs`: Stage 3 placement HUD with status readout and buttons.
- `Editor/Stage3SceneCreator.cs`: creates `Stage3_XRBoardPlacement.unity`.
- `Editor/Stage3SceneValidator.cs`: validates the Stage 3 scene structure.
- `Editor/Stage3PlayModeSmokeValidator.cs`: validates runtime board visuals, actors, ticking, transform controls, save/load, and coordinate mapping.
- `Editor/Stage3OpenXrSetupReporter.cs`: reports package status and writes `docs/STAGE3_XR_SETUP_STATUS.md`.
