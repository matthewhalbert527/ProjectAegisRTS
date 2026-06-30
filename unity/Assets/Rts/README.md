# Rts Unity Assets

Stage 1 adds a desktop board prototype that renders `Rts.Core` snapshots and submits commands back into the deterministic simulation. Stage 2 keeps that board and adds the first PC RTS uGUI layer in `Assets/Rts/Scenes/Stage2_PCSidebar.unity`. Stage 3 adds `Assets/Rts/Scenes/Stage3_XRBoardPlacement.unity` for Quest/OpenXR-ready board placement with desktop fallback controls. Stage 4 adds `Assets/Rts/Scenes/Stage4_LeftHandBuildSelection.unity` for the Quest-style left-hand build and selection interface. Stage 5 adds `Assets/Rts/Scenes/Stage5_DualHandCommand.unity` for right-hand tactical commands alongside the left-hand build/selection interface. Stage 6 adds `Assets/Rts/Scenes/Stage6_MovementVisualization.unity` for visual-only movement profiles, controllers, path preview, and debug HUD. Stage 7 adds `Assets/Rts/Scenes/Stage7_BuildingPowerProduction.unity` for visual-only building animation, power, production, and damage-state presentation. Stage 8 adds `Assets/Rts/Scenes/Stage8_ArtPipelineShowcase.unity` for concept references, actor visual definitions, generated blockout prefabs, sockets, icons, validation, and resolver integration. Stage 9 adds `Assets/Rts/Scenes/Stage9_CombatWeaponsDamage.unity` for deterministic combat presentation, projectile visuals, combat events, and damage/death placeholders. Stage 10 adds `Assets/Rts/Scenes/Stage10_EconomyHarvesting.unity` for deterministic economy presentation, resource visuals, harvester cargo, refinery unloading, and economy events. Stage 11 adds `Assets/Rts/Scenes/Stage11_FogRadarMinimap.unity` for deterministic fog, radar, and minimap presentation. Stage 12 adds `Assets/Rts/Scenes/Stage12_AISkirmishFoundation.unity` for deterministic AI intent presentation. Stage 13 adds `Assets/Rts/Scenes/Stage13_MapTerrainPathing.unity` for deterministic terrain/pathing debug presentation.

## Folder Roles

- `Scripts/Bootstrap`: scene startup and reference wiring.
- `Scripts/Board`: Stage 3 board transform model and placement controller.
- `Scripts/CoreBridge`: Unity-to-core adapters, command helpers, and board coordinate mapping.
- `Scripts/Input`: desktop mouse/keyboard input plus XR-safe placement, left-hand, and right-hand adapters/placeholders.
- `Scripts/Rendering`: board, actor, selection, low-power, production, interpolation, movement profile, vehicle, infantry, aircraft, turret, path-preview, building animation, combat visuals, economy visuals, visibility/fog/minimap visuals, AI intent/timeline visuals, and map terrain/path debug visuals.
- `Scripts/Art`: Stage 8 actor visual definitions, concept references, prefab descriptors, sockets, resolver, and showcase components.
- `Scripts/UI`: Stage 1 IMGUI debug HUD, Stage 2 uGUI desktop sidebar controllers, Stage 3 board placement HUD, Stage 4 left-hand wrist/radial UI, Stage 5 right-hand command UI, Stage 6 movement debug HUD, Stage 7 building animation debug HUD, Stage 8 art pipeline debug HUD, Stage 9 combat debug HUD, Stage 10 economy debug HUD, Stage 11 fog debug HUD, Stage 12 AI debug HUD, and Stage 13 map validation debug HUD.
- `Scripts/Camera`: desktop camera controls.
- `Scripts/Utilities`: generated runtime materials.
- `Editor`: scene generator menu item and batchmode entry point.
- `Plugins/RtsCore`: copied `Rts.Core.dll` and optional PDB.

## Simulation Boundary

Unity does not own gameplay state. It smooths visual transforms between snapshots, but actor position, power state, production state, placement validation, move orders, attack orders, projectiles, damage, death/destruction state, resource amounts, harvester cargo, refinery unloading, fog, radar, and minimap data come from `Rts.Core`.

The actor view layer tracks previous and target snapshot positions, facing, normalized speed, visual motion profile id, and actor category. Stage 6 consumes those values through visual-only controllers for acceleration/braking presentation, turning arcs, tracks/wheels, suspension, turret lag, infantry locomotion placeholders, aircraft banking, and path previews without faking gameplay movement.

Stage 7 consumes building snapshot values through visual-only controllers for lights, machinery, production indicators, doors, damage placeholders, and type-specific loops. It does not write power, production, health, or animation presentation state back into `Rts.Core`.

Stage 8 consumes actor type IDs through Unity-only visual definition assets. It can replace generated primitives with blockout or production prefabs through `ActorVisualPrefabResolver`, but it never writes prefab, socket, icon, or concept state back into `Rts.Core`.

Stage 9 consumes combat snapshot and event data through Unity-only render systems for projectiles, muzzle flashes, impact markers, damage markers, death markers, and debug HUD readouts. It never writes health, projectile, cooldown, death, or target state back into `Rts.Core`.

Stage 10 consumes economy snapshot and event data through Unity-only render systems for resource cells, cargo markers, refinery dock/unload markers, economy event markers, and debug HUD readouts. It never writes resource amounts, cargo, refinery unload timing, or credit awards back into `Rts.Core`.

Stage 11 consumes fog/radar/minimap snapshot data through Unity-only render systems for fog overlays, visibility debug readouts, radar state, minimap actor dots, and debug HUD readouts. It never writes visibility, explored state, actor hiding, or radar activation back into `Rts.Core`.

Stage 12 consumes AI snapshot data through Unity-only render systems for intent counts, plan timeline readouts, and debug HUD readouts. It never chooses AI commands or writes plan state back into `Rts.Core`.

Stage 13 consumes map and path debug snapshot data through Unity-only render systems for terrain overlays, path lines, map validation readouts, and authoring placeholders. It never writes terrain, passability, costs, pathfinding, or actor positions back into `Rts.Core`.

## Validation Tiers

Stage 8.1 adds tiered validation commands from the repository root:

- `tools/run-stage8-fast-checks.ps1`: current Stage 8 art pipeline iteration only.
- `tools/run-stage8-medium-checks.ps1`: core tests, Unity DLL build, direct Stage 7 Unity validation, and Stage 8 validation before local commits.
- `tools/run-stage8-checks.ps1`: slow full acceptance gate from Stage 0 through Stage 8.
- `tools/run-stage9-fast-checks.ps1`: current Stage 9 combat iteration only.
- `tools/run-stage9-medium-checks.ps1`: core tests, Unity DLL build, Stage 8 immediate dependency validation, and Stage 9 validation before local commits.
- `tools/run-stage9-checks.ps1`: slow full acceptance gate from Stage 0 through Stage 9.
- `tools/run-stage10-fast-checks.ps1`: current Stage 10 economy iteration only.
- `tools/run-stage10-medium-checks.ps1`: core tests, Unity DLL build, Stage 9 immediate dependency validation, and Stage 10 validation before local commits.
- `tools/run-stage10-checks.ps1`: slow full acceptance gate from Stage 0 through Stage 10.
- `tools/run-stage11-fast-checks.ps1`: current Stage 11 fog/radar/minimap iteration only.
- `tools/run-stage11-medium-checks.ps1`: core tests, Unity DLL build, Stage 10 immediate dependency validation, and Stage 11 validation before local commits.
- `tools/run-stage11-checks.ps1`: slow full acceptance gate from Stage 0 through Stage 11.
- `tools/run-stage12-fast-checks.ps1`: current Stage 12 AI iteration only.
- `tools/run-stage12-medium-checks.ps1`: core tests, Unity DLL build, Stage 11 immediate dependency validation, and Stage 12 validation before local commits.
- `tools/run-stage12-checks.ps1`: slow full acceptance gate from Stage 0 through Stage 12.
- `tools/run-stage13-fast-checks.ps1`: current Stage 13 map/pathing iteration only.
- `tools/run-stage13-medium-checks.ps1`: core tests, Unity DLL build, Stage 12 immediate dependency validation, and Stage 13 validation before local commits.
- `tools/run-stage13-checks.ps1`: slow full acceptance gate from Stage 0 through Stage 13.

The fast and medium tiers do not weaken acceptance coverage; they make day-to-day Unity asset and tooling edits cheaper to validate.

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

## Stage 7 Building Animation And Power Visualization

- `Scripts/Rendering/Buildings/BuildingVisualProfile.cs`: ScriptableObject tuning data for placeholder building lights, machinery, doors, production bays, type-specific loops, and damage thresholds.
- `Scripts/Rendering/Buildings/BuildingVisualProfileLibrary.cs`: building profile lookup by actor type id with generated safe defaults.
- `Scripts/Rendering/Buildings/BuildingVisualStateController.cs`: main snapshot-driven visual state controller for building power, animation, production progress, and health.
- `Scripts/Rendering/Buildings/BuildingLightVisualController.cs`: powered, low-power, offline, and warning-light placeholder behavior.
- `Scripts/Rendering/Buildings/BuildingMachineryVisualController.cs`: turbine, core, radar, crane, repair arm, dock pump, and generic machinery loops.
- `Scripts/Rendering/Buildings/BuildingProductionVisualController.cs`: production pulse/progress placeholder behavior and future event hook surface.
- `Scripts/Rendering/Buildings/BuildingDoorVisualController.cs`: bay/door open-close placeholder animation.
- `Scripts/Rendering/Buildings/BuildingDamageVisualController.cs`: damaged and destroyed placeholder markers.
- `Scripts/Rendering/Buildings/BuildingSpecificLoopController.cs`: type/category-specific loops for power, production, refinery, comms, repair, defense, airfield, and support buildings.
- `Scripts/Rendering/Buildings/BuildingPlaceholderPartFactory.cs`: generated primitive child parts for Stage 7 placeholder visuals.
- `Scripts/Rendering/Buildings/BuildingPowerDemoController.cs`: Stage 7 demo controls through existing simulation driver paths plus isolated visual-only override support.
- `Scripts/UI/Common/BuildingAnimationDebugHud.cs`: F10 debug HUD for selected/first building visual state.
- `Editor/Stage7BuildingProfileAssetCreator.cs`: creates default building profile assets.
- `Editor/Stage7SceneCreator.cs`: creates `Stage7_BuildingPowerProduction.unity`.
- `Editor/Stage7SceneValidator.cs`: validates the Stage 7 scene structure.
- `Editor/Stage7PlayModeSmokeValidator.cs`: validates runtime board/actor visuals, building controllers, profile lookup, low-power demo, production visual state, damage placeholder, HUD, and red console errors.

## Stage 8 Art Pipeline And Prefab Catalog

- `Scripts/Art/ActorVisualDefinition.cs`: ScriptableObject mapping an actor type to concept, icon, prefabs, profile IDs, socket requirements, and production status.
- `Scripts/Art/ConceptArtReference.cs`: concept metadata imported from the Stage 0 registry.
- `Scripts/Art/ActorVisualDefinitionLibrary.cs` and `ConceptArtReferenceLibrary.cs`: runtime-safe lookup libraries.
- `Scripts/Art/ActorPrefabDescriptor.cs` and `ActorPrefabSocket.cs`: prefab metadata and required attachment transforms.
- `Scripts/Art/ActorVisualPrefabResolver.cs`: optional prefab resolution service for `ActorRenderSystem`.
- `Scripts/Art/ArtPipelineShowcaseController.cs` and `ConceptArtCardView.cs`: Stage 8 review grid and concept cards.
- `Scripts/UI/Common/ArtPipelineDebugHud.cs`: F11 debug HUD for definitions, resolver stats, validation, and showcase controls.
- `Editor/Stage8ConceptArtImporter.cs`: copies concept PNGs into Unity and creates concept reference assets.
- `Editor/Stage8BlockoutPrefabGenerator.cs`: creates generated blockout prefabs for all 27 safe actor IDs.
- `Editor/Stage8ActorVisualDefinitionGenerator.cs`: creates actor visual definition assets.
- `Editor/Stage8IconGenerator.cs`: creates icon sprites.
- `Editor/Stage8PrefabSocketValidator.cs`: validates definitions, prefabs, sockets, icons, and IP flags.
- `Editor/Stage8SceneCreator.cs`: creates `Stage8_ArtPipelineShowcase.unity`.
- `Editor/Stage8SceneValidator.cs` and `Stage8PlayModeSmokeValidator.cs`: validate scene structure and runtime-equivalent art pipeline behavior.

## Stage 9 Combat Weapons Damage

- `Scripts/Rendering/Combat/CombatVisualProfile.cs`: ScriptableObject tuning data for placeholder projectile, muzzle, impact, damage, and death visuals.
- `Scripts/Rendering/Combat/CombatVisualProfileLibrary.cs`: combat profile lookup by weapon/projectile/impact visual id.
- `Scripts/Rendering/Combat/ProjectileRenderSystem.cs`: renders deterministic `ProjectileSnapshot` data as Unity-only placeholder visuals.
- `Scripts/Rendering/Combat/CombatEventRenderSystem.cs`: consumes bounded combat events for muzzle, impact, damage, and death presentation.
- `Scripts/Rendering/Combat/MuzzleFlashVisualController.cs`, `ImpactVisualController.cs`, `DamageVisualController.cs`, and `DeathVisualController.cs`: short-lived placeholder VFX controllers.
- `Scripts/UI/Common/CombatDebugHud.cs`: F12 debug HUD and combat demo controls.
- `Editor/Stage9CombatProfileAssetCreator.cs`: creates default combat visual profile assets.
- `Editor/Stage9SceneCreator.cs`: creates `Stage9_CombatWeaponsDamage.unity`.
- `Editor/Stage9SceneValidator.cs` and `Stage9PlayModeSmokeValidator.cs`: validate scene structure and runtime combat behavior.

## Stage 10 Economy Harvesting

- `Scripts/Rendering/Economy/ResourceFieldRenderSystem.cs`: renders deterministic resource cell snapshots.
- `Scripts/Rendering/Economy/HarvesterCargoVisualController.cs`: renders cargo state for harvester snapshots.
- `Scripts/Rendering/Economy/RefineryDockVisualController.cs`: renders dock and unloading state for refinery snapshots.
- `Scripts/Rendering/Economy/EconomyEventRenderSystem.cs`: consumes bounded economy events for harvest/unload/depletion markers.
- `Scripts/UI/Common/EconomyDebugHud.cs`: F8 debug HUD and economy demo controls.
- `Editor/Stage10SceneCreator.cs`: creates `Stage10_EconomyHarvesting.unity`.
- `Editor/Stage10SceneValidator.cs` and `Stage10PlayModeSmokeValidator.cs`: validate scene structure and runtime harvesting behavior.

## Stage 11 Fog Radar Minimap

- `Scripts/Rendering/Visibility/FogOverlayRenderer.cs`: renders unexplored and explored fog cells from `FogSnapshot`.
- `Scripts/Rendering/Visibility/VisibilityDebugRenderer.cs`: tracks visible cell counts for validation/debugging.
- `Scripts/Rendering/Visibility/RadarSnapshotAdapter.cs`: exposes radar status from `RadarSnapshot`.
- `Scripts/Rendering/Visibility/MinimapRenderSystem.cs` and `MinimapActorDotView.cs`: render minimap actor dots from `MinimapSnapshot`.
- `Scripts/UI/Common/FogDebugHud.cs`: F7 debug HUD and fog demo reset control.
- `Editor/Stage11SceneCreator.cs`: creates `Stage11_FogRadarMinimap.unity`.
- `Editor/Stage11SceneValidator.cs` and `Stage11PlayModeSmokeValidator.cs`: validate scene structure and runtime fog/radar/minimap behavior.

## Stage 12 AI Skirmish Foundation

- `Scripts/Rendering/Ai/AiIntentRenderSystem.cs`: reads `AiSnapshot` intent data for validation/debug counts.
- `Scripts/Rendering/Ai/AiPlanTimelineView.cs`: reads AI plan sequence and intent timeline data.
- `Scripts/UI/Common/AiDebugHud.cs`: F6 debug HUD and AI demo reset control.
- `Editor/Stage12SceneCreator.cs`: creates `Stage12_AISkirmishFoundation.unity`.
- `Editor/Stage12SceneValidator.cs` and `Stage12PlayModeSmokeValidator.cs`: validate scene structure and runtime AI intent behavior.

## Stage 13 Map Terrain Pathing

- `Scripts/Rendering/Map/TerrainDebugRenderer.cs`: renders non-clear terrain cells from `MapSnapshot`.
- `Scripts/Rendering/Map/PathDebugRenderer.cs`: renders the latest successful path from `PathDebugSnapshot`.
- `Scripts/Rendering/Map/MapAuthoringOverlay.cs`: placeholder authoring overlay surface for future map tools.
- `Scripts/UI/Common/MapValidationDebugHud.cs`: F5 debug HUD and map demo/path controls.
- `Editor/Stage13SceneCreator.cs`: creates `Stage13_MapTerrainPathing.unity`.
- `Editor/Stage13SceneValidator.cs` and `Stage13PlayModeSmokeValidator.cs`: validate scene structure and runtime map/pathing diagnostics.
