# Rts Unity Assets

Stage 1 adds a desktop board prototype that renders `Rts.Core` snapshots and submits commands back into the deterministic simulation. Stage 2 keeps that board and adds the first PC RTS uGUI layer in `Assets/Rts/Scenes/Stage2_PCSidebar.unity`. Stage 3 adds `Assets/Rts/Scenes/Stage3_XRBoardPlacement.unity` for Quest/OpenXR-ready board placement with desktop fallback controls. Stage 4 adds `Assets/Rts/Scenes/Stage4_LeftHandBuildSelection.unity` for the Quest-style left-hand build and selection interface. Stage 5 adds `Assets/Rts/Scenes/Stage5_DualHandCommand.unity` for right-hand tactical commands alongside the left-hand build/selection interface. Stage 6 adds `Assets/Rts/Scenes/Stage6_MovementVisualization.unity` for visual-only movement profiles, controllers, path preview, and debug HUD.

## Folder Roles

- `Scripts/Bootstrap`: scene startup and reference wiring.
- `Scripts/Board`: Stage 3 board transform model and placement controller.
- `Scripts/CoreBridge`: Unity-to-core adapters, command helpers, and board coordinate mapping.
- `Scripts/Input`: desktop mouse/keyboard input plus XR-safe placement, left-hand, and right-hand adapters/placeholders.
- `Scripts/Rendering`: board, actor, selection, low-power, production, interpolation, movement profile, vehicle, infantry, aircraft, turret, and path-preview visuals.
- `Scripts/UI`: Stage 1 IMGUI debug HUD, Stage 2 uGUI desktop sidebar controllers, Stage 3 board placement HUD, Stage 4 left-hand wrist/radial UI, Stage 5 right-hand command UI, and Stage 6 movement debug HUD.
- `Scripts/Camera`: desktop camera controls.
- `Scripts/Utilities`: generated runtime materials.
- `Editor`: scene generator menu item and batchmode entry point.
- `Plugins/RtsCore`: copied `Rts.Core.dll` and optional PDB.

## Simulation Boundary

Unity does not own gameplay state. It smooths visual transforms between snapshots, but actor position, power state, production state, placement validation, and move orders come from `Rts.Core`.

The actor view layer tracks previous and target snapshot positions, facing, normalized speed, visual motion profile id, and actor category. Stage 6 consumes those values through visual-only controllers for acceleration/braking presentation, turning arcs, tracks/wheels, suspension, turret lag, infantry locomotion placeholders, aircraft banking, and path previews without faking gameplay movement.

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

## Stage 4 Left-Hand Build And Selection

- `Scripts/UI/XR/LeftHand/LeftHandCommandMode.cs`: shared Stage 4 command modes.
- `Scripts/UI/XR/LeftHand/LeftHandBuildCategory.cs`: left-hand production categories.
- `Scripts/UI/XR/LeftHand/LeftHandBuildItemViewModel.cs`: Unity-side production card data.
- `Scripts/UI/XR/LeftHand/LeftHandBuildMenuController.cs`: build menu state and MVP/future item population.
- `Scripts/UI/XR/LeftHand/LeftHandRadialMenuView.cs`: generated wrist/radial uGUI menu.
- `Scripts/UI/XR/LeftHand/LeftHandCommandRouter.cs`: production, placement, selection, cancellation, and low-power routing through the simulation driver.
- `Scripts/UI/XR/LeftHand/Stage4ModeCoordinator.cs`: menu, placement, selection ray, lasso, ambiguous selection, desktop fallback, and XR adapter coordination.
- `Scripts/UI/XR/LeftHand/LeftHandPlacementPanel.cs`, `LeftHandSelectionPanel.cs`, and `LeftHandStatusHud.cs`: Stage 4 readouts.
- `Scripts/Input/Desktop/DesktopLeftHandInputSource.cs`: keyboard/mouse fallback controls.
- `Scripts/Input/XR/ILeftHandInputSource.cs`, `XrLeftHandInputAdapter.cs`, and `SimulatedLeftHandRig.cs`: compile-safe XR boundary and placeholder rig.
- `Scripts/Selection/SelectionResolver.cs`, `LeftHandSelectionController.cs`, `LeftHandLassoSelectionController.cs`, and `LeftHandSelectionCandidate.cs`: ray/cell candidate ranking, selection, cycling, and lasso.
- `Editor/Stage4SceneCreator.cs`: creates `Stage4_LeftHandBuildSelection.unity`.
- `Editor/Stage4SceneValidator.cs`: validates the Stage 4 scene structure.
- `Editor/Stage4PlayModeSmokeValidator.cs`: validates runtime board visuals, actors, ticking, menu, production, placement, selection, lasso, cancellation, and red console errors.
- `Editor/Stage4XrSetupReporter.cs`: reports package/input status and writes `docs/STAGE4_XR_INPUT_STATUS.md`.

## Stage 5 Right-Hand Tactical Commands

- `Scripts/UI/XR/RightHand/RightHandCommandMode.cs`: Stage 5 right-hand command modes.
- `Scripts/UI/XR/RightHand/RightHandCommandRouter.cs`: move, context, attack placeholder, force-attack placeholder, cancellation, and status routing through the simulation driver.
- `Scripts/UI/XR/RightHand/Stage5DualHandModeCoordinator.cs`: desktop/XR right-hand input coordination, placement suppression, and board manipulation.
- `Scripts/UI/XR/RightHand/RightHandCommandHud.cs`, `RightHandCommandReticle.cs`, and `RightHandStatusPanel.cs`: generated command readouts and feedback.
- `Scripts/Input/Desktop/DesktopRightHandInputSource.cs`: keyboard/mouse fallback controls.
- `Scripts/Input/XR/IRightHandInputSource.cs`, `XrRightHandInputAdapter.cs`, and `SimulatedRightHandRig.cs`: compile-safe XR boundary and placeholder rig.
- `Scripts/Rendering/CommandPreviewRenderer.cs`: generated target marker for move, attack placeholder, and invalid command feedback.
- `Editor/Stage5SceneCreator.cs`: creates `Stage5_DualHandCommand.unity`.
- `Editor/Stage5SceneValidator.cs`: validates the Stage 5 scene structure.
- `Editor/Stage5PlayModeSmokeValidator.cs`: validates runtime board visuals, actors, left-hand preservation, move commands, attack placeholders, board manipulation, placement suppression, cancellation, and red console errors.

## Stage 6 Movement Visualization

- `Scripts/Rendering/Motion/VisualMotionProfile.cs`: ScriptableObject tuning data for visual speed, smoothing, tracks, infantry step phase, aircraft bank/hover, turret lag, and recoil placeholders.
- `Scripts/Rendering/Motion/VisualMotionProfileLibrary.cs`: profile lookup by snapshot profile id, actor type id, or category.
- `Scripts/Rendering/Motion/ActorVisualMotionController.cs`: visual-only transform smoothing toward authoritative snapshots.
- `Scripts/Rendering/Motion/VehicleVisualMotionController.cs`: track/wheel phase, suspension placeholder, braking, and turning readouts.
- `Scripts/Rendering/Motion/InfantryVisualMotionController.cs`: idle/walk/run and aim/fire placeholders.
- `Scripts/Rendering/Motion/AircraftVisualMotionController.cs`: bank, altitude offset, and hover placeholders.
- `Scripts/Rendering/Motion/TurretVisualAimController.cs`: turret lag and recoil placeholder.
- `Scripts/Rendering/Motion/MovementPathPreview.cs`: visual-only movement path line and endpoint markers.
- `Scripts/Rendering/Motion/Stage6MotionShowcase.cs`: scene-only visual showcase for vehicle, infantry, aircraft, and turret controllers.
- `Scripts/UI/Common/MovementDebugHud.cs`: F9 debug HUD for visual controller counts and selected actor motion state.
- `Editor/Stage6MotionProfileAssetCreator.cs`: creates the Stage 6 profile assets.
- `Editor/Stage6SceneCreator.cs`: creates `Stage6_MovementVisualization.unity`.
- `Editor/Stage6SceneValidator.cs`: validates the Stage 6 scene structure.
- `Editor/Stage6PlayModeSmokeValidator.cs`: validates runtime actor motion, path preview, showcase controllers, pause/resume, single-step, low-power state, and red console errors.
