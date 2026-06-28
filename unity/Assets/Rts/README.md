# Rts Unity Assets

Stage 1 adds a desktop board prototype that renders `Rts.Core` snapshots and submits commands back into the deterministic simulation.

## Folder Roles

- `Scripts/Bootstrap`: scene startup and reference wiring.
- `Scripts/CoreBridge`: Unity-to-core adapters, command helpers, and board coordinate mapping.
- `Scripts/Input`: desktop mouse and keyboard input.
- `Scripts/Rendering`: board, actor, selection, low-power, production, and interpolation visuals.
- `Scripts/UI`: IMGUI debug HUD.
- `Scripts/Camera`: desktop camera controls.
- `Scripts/Utilities`: generated runtime materials.
- `Editor`: scene generator menu item and batchmode entry point.
- `Plugins/RtsCore`: copied `Rts.Core.dll` and optional PDB.

## Simulation Boundary

Unity does not own gameplay state. It smooths visual transforms between snapshots, but actor position, power state, production state, placement validation, and move orders come from `Rts.Core`.

The actor view layer tracks previous and target snapshot positions, facing, normalized speed, visual motion profile id, and actor category so later stages can add acceleration/braking visuals, turning arcs, tracks/wheels, suspension, turret lag, infantry locomotion, and aircraft banking without faking gameplay movement.
