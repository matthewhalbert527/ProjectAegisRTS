# Stage Plan

## Stage 0

Complete. Deterministic core, concept extraction/registry, OpenRA audit, docs, and no-dependency console tests.

## Stage 1

Complete. Unity desktop board prototype with placeholder meshes driven by `WorldSnapshot` and commands submitted to `Rts.Core`. Completed with batchmode scene generation, validation, camera framing, runtime board rendering, placeholder actors, selection, move orders, pause, step, and low-power demo controls.

## Stage 2

Complete, pending visual polish. PC right-side OpenRA-style production panel using original UI implementation and Stage 0 command/snapshot APIs. Stage 2 now provides `Assets/Rts/Scenes/Stage2_PCSidebar.unity`, a uGUI right sidebar, production tabs/grid, queue cancellation, placement readout, selection details, command bar, minimap placeholder, status log, hidden-by-default debug overlay, batchmode validation scripts, and automated smoke validation.

## Stage 3

Quest/OpenXR board placement prototype with adjustable height, yaw, scale, recenter, reset, save/load, desktop fallback controls, XR-safe adapter placeholders, package setup reporting, and `Assets/Rts/Scenes/Stage3_XRBoardPlacement.unity`.

## Stage 4

Next recommended stage: left-hand build and selection interface for VR/MR, with right-hand tactical orders and board controls.

## Stage 5

High-quality movement visualization layered on deterministic simulation snapshots.

## Stage 6

Building animation and power states: powered idle, production active, low power, offline, damaged, and destroyed.

## Stage 7

Concept art to production 3D asset pipeline with licensing, naming, import, rigging, materials, and animation standards.

## Stage 8

Later stage: skirmish AI for economy, basebuilding, scouting, production, attacks, repairs, and defensive behavior.

## Stage 9

Later stage: multiplayer, replays, deterministic checksums, desync reporting, and command stream validation.
