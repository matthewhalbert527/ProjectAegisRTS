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

## Command and Snapshot Bridge

The bridge is intentionally simple:

- Client submits commands such as `BeginProductionCommand`, `PlaceBuildingCommand`, and `IssueMoveOrderCommand`.
- Core validates commands and returns `CommandResult`.
- Core advances in fixed ticks.
- Client reads `WorldSnapshot`, `ActorSnapshot`, `ProductionSnapshot`, `PowerSnapshot`, and `PlacementPreviewSnapshot`.

## Deterministic Tick Loop

The current loop updates power, advances production, advances movement, and refreshes actor flags. State uses integers and fixed cell-scaled positions. The smoke test compares deterministic summaries after replaying the same command sequence twice.

## OpenRA Reference Boundary

OpenRA is used as an architecture reference for concepts such as actors, traits, orders, production queues, placement previews, power state, and right-side production palettes. Stage 0 does not port the OpenRA renderer, SDL input, OpenGL platform layer, or YAML chrome UI.

## Future OpenRA-Derived Systems

If future stages copy or derive OpenRA code, the project must preserve GPL headers, document obligations, and treat the codebase as GPL-compatible. Stage 0 avoids that by implementing a clean prototype from scratch.
