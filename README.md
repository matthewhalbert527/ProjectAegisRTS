# ProjectAegisRTS

ProjectAegisRTS is a Stage 0 foundation for a modern RTS that can later run as a Meta Quest 3S VR/MR board game and as a PC RTS with a right-side production panel. This stage is intentionally not a Unity project and not a renderer port. It creates a deterministic, Unity-compatible C# simulation core that a Unity or PC client can drive through commands and read through snapshots.

## Stage 0 Contents

- `src/Rts.Core`: deterministic simulation library targeting `netstandard2.1`.
- `src/Rts.Core.Tests`: no-dependency console test runner targeting `net8.0`.
- `docs`: product, architecture, licensing, OpenRA audit, movement/animation targets, and stage planning.
- `external/openra`: copied OpenRA reference source for audit only.
- `external/redalert_reference`: copied historical reference source, read-only and not used as a code base.
- `art/concepts`: copied concept cards and generated registries.
- `unity`: placeholder folder and setup notes for a later Unity client.

## Run Tests

From `ProjectAegisRTS`:

```powershell
dotnet run --project src/Rts.Core.Tests
```

The current machine did not have `dotnet` on PATH during Stage 0 creation, so this command still needs to be run after installing or exposing a .NET SDK.

To publish the local test executable and refresh the desktop shortcut after changes:

```powershell
.\tools\build-stage0-test-runner.ps1
```

## Next Milestone

Stage 1 should create a Unity desktop board prototype that references `Rts.Core`, renders simple placeholder meshes from `WorldSnapshot`, and sends commands back to the simulation without putting Unity objects, physics, or floats in charge of gameplay state.
