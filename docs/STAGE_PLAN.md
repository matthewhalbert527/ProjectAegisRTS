# Stage Plan

## Stage 0

Complete. Deterministic core, concept extraction/registry, OpenRA audit, docs, and no-dependency console tests.

## Stage 1

Complete. Unity desktop board prototype with placeholder meshes driven by `WorldSnapshot` and commands submitted to `Rts.Core`. Completed with batchmode scene generation, validation, camera framing, runtime board rendering, placeholder actors, selection, move orders, pause, step, and low-power demo controls.

## Stage 2

Complete, pending visual polish. PC right-side OpenRA-style production panel using original UI implementation and Stage 0 command/snapshot APIs. Stage 2 now provides `Assets/Rts/Scenes/Stage2_PCSidebar.unity`, a uGUI right sidebar, production tabs/grid, queue cancellation, placement readout, selection details, command bar, minimap placeholder, status log, hidden-by-default debug overlay, batchmode validation scripts, and automated smoke validation.

## Stage 3

Complete. Quest/OpenXR board placement prototype with adjustable height, yaw, scale, recenter, reset, save/load, desktop fallback controls, XR-safe adapter placeholders, package setup reporting, automated smoke validation, and `Assets/Rts/Scenes/Stage3_XRBoardPlacement.unity`.

## Stage 4

Complete. Quest left-hand build and selection interface. Adds `Assets/Rts/Scenes/Stage4_LeftHandBuildSelection.unity`, simulated left-hand/controller rig, wrist/radial production categories, build item cards, queue routing through the simulation bridge, placement footprint preview, ray selection, ambiguous candidate cycling, board-space lasso selection, desktop fallback controls, XR-safe adapter boundaries, package/input status reporting, and automated smoke validation.

## Stage 5

Complete. Quest right-hand tactical command interface. Adds `Assets/Rts/Scenes/Stage5_DualHandCommand.unity`, simulated right-hand/controller rig, right-hand command HUD, move command routing, attack and force-attack placeholders, command preview markers, board manipulation coexistence, desktop fallback controls, XR-safe adapter boundaries, and automated smoke validation while preserving Stage 2 and Stage 4 controls.

## Stage 6

High-quality movement visualization layered on deterministic simulation snapshots.

## Stage 7

Building animation and power states: powered idle, production active, low power, offline, damaged, and destroyed.

## Stage 8

Concept art to production 3D asset pipeline with licensing, naming, import, rigging, materials, and animation standards.

## Stage 9

Later stage: combat/economy/fog expansion, skirmish AI, multiplayer, replays, deterministic checksums, desync reporting, and command stream validation.
