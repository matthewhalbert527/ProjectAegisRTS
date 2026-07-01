# Technical Architecture

## Simulation Core

`src/Rts.Core` is the authoritative gameplay model. It owns actor definitions, actor instances, grid occupancy, production queues, building placement, power state, pathfinding, deterministic movement, command handling, and snapshots. It targets `netstandard2.1` and does not reference UnityEngine.

## Unity Client

Unity will later render `WorldSnapshot` data and submit explicit command DTOs. Unity GameObjects, controller poses, physics, animation rigs, and floats are presentation/input concerns only. They must not become authoritative gameplay state.

## Stage 3 Board Placement Boundary

Stage 3 introduces a Unity-only board transform layer for Quest/OpenXR-style placement. `BoardTransformModel` owns position, height, yaw, scale, meters-per-cell, save/load, reset, and recenter state. `BoardPlacementController` applies those values to `BoardRoot` and refreshes `BoardCoordinateMapper` so ray-to-cell and cell-to-world math follow the visible board. This does not mutate `Rts.Core` actor state, command streams, pathfinding, occupancy, production, power, or deterministic ticks.

The XR adapter scripts are package-independent placeholders. OpenXR, XR Interaction Toolkit, or Meta SDK objects can later feed placement rays and confirm/cancel/adjust inputs into the controller without changing the simulation bridge.

## Stage 4 Left-Hand Interface Boundary

Stage 4 adds a Unity-only Quest-style left-hand build and selection layer. `DesktopLeftHandInputSource` and `XrLeftHandInputAdapter` feed rays and button-like actions into `Stage4ModeCoordinator`. The coordinator routes production, placement, selection, candidate cycling, and lasso actions through `LeftHandCommandRouter`, `RtsSimulationDriver`, and existing snapshot/command APIs.

The left-hand UI owns presentation state such as open/closed menu, active production category, selected build card, hovered cell, candidate list, and local selection indicators. It does not own gameplay rules. Production, placement validation, credits, power state, actor spawning, movement, and deterministic ticks remain in `Rts.Core`.

`XrLeftHandInputAdapter` intentionally avoids hard references to XR Interaction Toolkit and Meta XR packages. Future Quest bindings should connect controller rays, trigger/select, grip/modifier, thumbstick axes, and hand pinch behind compile-safe adapter boundaries.

## Stage 5 Right-Hand Command Boundary

Stage 5 adds a Unity-only right-hand tactical command layer. `DesktopRightHandInputSource` and `XrRightHandInputAdapter` feed command rays and button-like actions into `Stage5DualHandModeCoordinator`. Movement commands route through `RightHandCommandRouter` and `RtsSimulationDriver.TryIssueMoveSelectedToCell`, preserving `Rts.Core` as the authority.

Attack and force-attack are placeholder commands in Stage 5. They update status and command preview feedback without adding final combat, weapons, projectiles, or damage. The coordinator suppresses right-hand gameplay commands during Stage 4 building placement or Stage 3 board placement. Board manipulation uses the Stage 3 presentation transform and does not mutate deterministic gameplay state.

`XrRightHandInputAdapter` intentionally avoids hard references to XR Interaction Toolkit and Meta XR packages. Future Quest bindings should connect right controller rays, primary command, mode buttons, grip/board manipulation, and rotate/scale axes behind `IRightHandInputSource`.

## Stage 6 Visual Movement Boundary

Stage 6 adds Unity-only movement presentation. `VisualMotionProfileLibrary` resolves profile data from snapshot profile id, actor type, or actor category. `ActorVisualMotionController` smooths presentation transforms toward authoritative `ActorSnapshot` positions and facing, while vehicle, infantry, aircraft, turret, and path preview components add local visual state.

These controllers do not submit commands, update `Rts.Core`, use Unity physics authority, or write pathfinding/position data back to the simulation. The Stage 6 showcase exists only to exercise presentation controllers for actor categories not spawned by the current demo world.

## Stage 7 Building Visualization Boundary

Stage 7 adds Unity-only building animation and state presentation. `BuildingVisualProfileLibrary` resolves profile data by actor type id, and `BuildingVisualStateController` derives visual power, animation, production, and damage states from `ActorSnapshot` values. Child controllers then drive generated or authored Unity presentation parts such as lights, turbines, radar dishes, doors, production pulses, repair arms, refinery pumps, warning markers, and damage placeholders.

The building visual layer does not own gameplay power, production, health, placement, repair, or destruction rules. It may call existing `RtsSimulationDriver` demo command paths from the debug controller, but it never mutates `Rts.Core` internals or writes animation state back to the simulation. Any forced production visual state is isolated as a Unity-only debug override for presentation validation.

## Stage 8 Art Pipeline Boundary

Stage 8 adds Unity-only art replacement data. `ActorVisualDefinitionLibrary` maps safe actor type IDs to concept references, icons, generated blockout prefabs, future production prefabs, motion/building profile IDs, and required sockets. `ActorVisualPrefabResolver` lets `ActorRenderSystem` prefer authored prefabs when available and fall back to generated primitives when definitions or prefabs are missing.

Prefab descriptors and sockets define presentation attachment points for turrets, barrels, weapons, doors, production exits, lights, VFX, aircraft rotors, wheels, tracks, and UI anchors. They do not change deterministic actor definitions, footprints, production rules, movement, power, combat, or health in `Rts.Core`.

## Stage 9 Combat Boundary

Stage 9 moves attack orders, weapon cooldowns, projectile movement, damage, death, destruction flags, and combat events into `Rts.Core`. The core exposes that state through expanded actor snapshots, projectile snapshots, and bounded combat event snapshots.

Unity submits attack commands and renders placeholder projectiles, muzzle flashes, impact markers, damage markers, death markers, and the combat debug HUD. Unity does not own target validation, cooldown timers, projectile position, hit application, health, death, or destruction state.

## Stage 10 Economy Boundary

Stage 10 moves ore resource cells, harvest orders, harvester cargo/work states, refinery dock/unload state, credit awards, and economy events into `Rts.Core`. The core exposes that state through `WorldSnapshot.Economy` and actor harvest-order flags.

Unity submits harvest/return commands and renders placeholder ore cells, cargo markers, dock markers, unload/event markers, and the economy debug HUD. Unity does not own resource amounts, cargo, refinery unload timing, credit awards, or harvester work states.

## Stage 11 Visibility Boundary

Stage 11 moves per-player fog of war, explored/visible/unexplored cells, sight radii, radar provider state, player-perspective actor filtering, and minimap data into `Rts.Core`. The core exposes that state through `FogSnapshot`, `RadarSnapshot`, and `MinimapSnapshot`.

Unity opts into player-perspective snapshots for the Stage 11 scene, renders placeholder fog overlays and minimap dots, and reads radar status for HUD/debug presentation. Unity does not own visibility state, actor hiding, explored cell persistence, or radar activation.

## Stage 12 AI Boundary

Stage 12 adds deterministic skirmish AI to `Rts.Core`. `AiSystem` owns registered AI players, difficulty profile, plan state, decision sequence, recent intents, and command generation. AI planners inspect deterministic world state and submit normal commands through `RtsWorld.IssueCommand`.

Unity reads `AiSnapshot` for debug HUDs, intent counts, and plan timeline presentation. Unity does not choose AI actions, mutate AI state, bypass command validation, or write plan data back into the simulation.

## Stage 13 Map Terrain Pathing Boundary

Stage 13 moves terrain kind, movement class, passability, movement cost, map validation, and structured path diagnostics into `Rts.Core`. Movement, harvesting, spawn, docking, and placement checks use the deterministic terrain-aware path query while preserving existing clear-map behavior.

Unity reads `MapSnapshot` and `PathDebugSnapshot` for terrain overlays, path debug lines, map validation HUDs, and authoring placeholders. Unity does not mutate authoritative terrain, pathfinding, actor positions, occupancy, resource amounts, or movement costs.

## Stage 14 Feedback Boundary

Stage 14 adds a Unity-only feedback event layer. `FeedbackEventBus` converts existing deterministic snapshots and already-computed command results into presentation events for selection, move, invalid command, production, building placement, low power, harvest, unload, attack, impact, damage, death/destruction, and radar changes.

Feedback profiles, silent audio cues, primitive VFX markers, UI messages, and haptic placeholders are presentation-only. They do not mutate `Rts.Core`, do not introduce final audio/VFX assets, and do not become authoritative gameplay state.

## Stage 15 Performance Boundary

Stage 15 adds Unity-only performance and build-readiness tooling. `ObjectPoolService` reuses short-lived presentation objects for projectile views and feedback markers, while `RuntimePerformanceStats`, `SceneComplexityReporter`, `QualityProfileApplier`, and the readiness reporters expose budget and configuration data.

The performance layer does not change deterministic gameplay, does not make Unity physics authoritative, does not produce final Quest optimization, and does not require Android/Quest build modules. It is an audit and guardrail layer for future profiling and packaging work.

## Stage 16 Match And Scenario Boundary

Stage 16 adds deterministic match flow to `Rts.Core`. `MatchState` owns start/reset, phase, local outcome, elapsed ticks, victory/defeat detection, and objective state. `ScenarioDefinition` describes the vertical slice players, objectives, victory condition, and defeat condition. `WorldSnapshot` exposes `MatchSnapshot` and `ScenarioSnapshot`.

Unity reads those snapshots through `VerticalSliceScenarioController`, `MatchObjectiveHud`, and `IntegratedSystemsStatusHud`. Unity debug actions call safe scenario APIs for damage, credit grants, map reveal, production, harvest, and attack smoke paths. Unity does not mutate actor health, credits, visibility, objective state, or match state directly.

## Stage 17 Player-Facing UI Boundary

Stage 17 adds player-facing Unity UI on top of the Stage 16 match snapshots. `PlayerObjectiveHud`, `PlayerPromptHud`, `PlayerControlsOverlay`, and `MatchResultHud` read `WorldSnapshot`, local selection, and deterministic match state. They do not own gameplay rules, win/loss detection, production, placement, combat, economy, or AI.

The Options menu stores local prototype preferences with `PlayerPrefs`. It may apply presentation settings such as fullscreen and audio volume, but it does not alter authoritative simulation state. The match result screen restarts through `VerticalSliceScenarioController` and returns to the boot scene through Unity scene loading.

## Stage 18 Tester Guidance Boundary

Stage 18 keeps the same simulation boundary and adds player guidance on top. `VerticalSliceProgressTracker`, `VerticalSliceChecklistHud`, `PlayerPromptSystem`, and the sidebar read snapshots, local selection, placement state, and scenario objective state. They may recommend build-order actions and improve presentation, but they do not complete objectives, grant resources, alter production, or resolve win/loss state.

The Stage 18 validators assert that Boot is still first, debug/status panels are hidden by default, placement UI starts hidden, objective and match state agree after victory/defeat, and `Rts.Core` remains free of UnityEngine references.

## Stage 18.5 Fine Placement Grid Boundary

Stage 18.5 moves building placement and building occupancy to a 2x fine placement grid inside `Rts.Core`. Coarse map cells still drive terrain, resources, fog, pathing, movement commands, and high-level compatibility. Fine placement cells drive `PlaceBuildingCommand` validation, building top-left placement, footprint occupancy, placement previews, and building snapshot metadata.

Unity reads the fine placement metadata through snapshots and `BoardCoordinateMapper`. It can render the denser grid, snap placement rays to fine cells, and draw fine footprint previews, but it does not own placement validity or occupancy. Normal selection, move, attack, and harvest command targets remain coarse command cells unless a building placement mode is active.

Fine building occupancy is projected back to coarse blocked cells so existing pathing and movement continue to respect placed buildings without rewriting the movement model in this stage.

## Stage 19 Mission Flow Boundary

Stage 19 adds Unity-only mission guidance on top of the Stage 16 match and Stage 18.5 fine placement systems. `VerticalSliceMissionFlowController` and the expanded `VerticalSliceProgressTracker` read snapshots, local selection, production queue state, economy events, combat events, scenario objectives, placement mode, and match outcome.

They may choose current player-facing text, recommended sidebar items, checklist rows, and validation expectations. They do not complete objectives, grant resources, place buildings, damage actors, alter AI, resolve victory, or write state back into `Rts.Core`.

Stage 19 also adds a normal-command victory validation path. Unity selects local combat units and issues normal move/attack commands through `RtsSimulationDriver`; `Rts.Core` still validates range, target legality, damage, objective completion, and match outcome.

## Stage 19.5 PC UI Boundary

Stage 19.5 reorganizes Unity-only player UI for the Windows build. `CncStyleSidebarLayout` arranges existing desktop HUD components into a right-side PC sidebar, and `PlayerFacingUiModeController` hides XR left-hand/right-hand fallback menus in PC player-facing mode. These systems do not submit gameplay commands directly; production, placement, selection, and orders still route through the existing command routers and `RtsSimulationDriver`.

`PauseMenuController` opens a Unity UI overlay on Escape, pauses/resumes through `RtsSimulationDriver`, restarts through `VerticalSliceScenarioController`, and uses Unity scene/application APIs for menu/quit actions. It blocks local Unity input while open but does not mutate `Rts.Core` state outside the existing pause/reset command paths.

## Stage 20 Production Visual Boundary

Stage 20 adds Unity-only first-pass production proxy prefabs and validation markers. `ActorVisualDefinition` remains the data boundary: MVP definitions prefer `ProductionPrefab`, while generated Stage 8 blockouts remain fallback prefabs. `ActorRenderSystem` and `ActorViewBehaviour` continue resolving visuals from snapshots and do not write visual state back into `Rts.Core`.

`ProductionVisualValidationTag`, `ProductionVisualStandardLibrary`, and the Stage20 showcase are presentation/validation tools only. They validate all-around detail, socket coverage, LOD presence, and footprint readability for Quest-style 360-degree viewing. Gameplay placement, occupancy, production, power, movement, combat, and victory still come from deterministic core snapshots and command results.

`PlayerFacingUiModeController` now exposes explicit `PCDesktop`, `QuestXR`, and `DebugHybrid` modes. Windows player builds default to `PCDesktop` with the right-side sidebar; `QuestXR` keeps left-hand build/selection and right-hand tactical controls while hiding the PC sidebar.

## Command and Snapshot Bridge

The bridge is intentionally simple:

- Client submits commands such as `BeginProductionCommand`, `PlaceBuildingCommand`, `IssueMoveOrderCommand`, `IssueAttackOrderCommand`, and `IssueHarvestOrderCommand`.
- Core validates commands and returns `CommandResult`.
- Core advances in fixed ticks.
- Client reads `WorldSnapshot`, `ActorSnapshot`, `ProductionSnapshot`, `PowerSnapshot`, `PlacementPreviewSnapshot`, `ProjectileSnapshot`, `CombatEventSnapshot`, `EconomySnapshot`, `FogSnapshot`, `RadarSnapshot`, `MinimapSnapshot`, `AiSnapshot`, `MapSnapshot`, `MatchSnapshot`, and `ScenarioSnapshot`.

## Deterministic Tick Loop

The current loop updates power, lets deterministic AI planners submit validated commands, advances production, advances terrain-aware movement, advances harvesting/refinery unloading, advances combat/projectiles, updates visibility, updates match/objective state, and refreshes actor flags. State uses integers and fixed cell-scaled positions. The smoke test compares deterministic summaries after replaying the same command sequence twice.

## OpenRA Reference Boundary

OpenRA is used as an architecture reference for concepts such as actors, traits, orders, production queues, placement previews, power state, and right-side production palettes. Stage 0 does not port the OpenRA renderer, SDL input, OpenGL platform layer, or YAML chrome UI.

## Future OpenRA-Derived Systems

If future stages copy or derive OpenRA code, the project must preserve GPL headers, document obligations, and treat the codebase as GPL-compatible. Stage 0 avoids that by implementing a clean prototype from scratch.
