# ProjectAegisRTS

ProjectAegisRTS is a staged foundation for a modern RTS that can later run as a Meta Quest 3S VR/MR board game and as a PC RTS with a right-side production panel. Stage 0 created the deterministic, Unity-compatible C# simulation core. Stage 1 adds a Unity desktop board prototype that consumes that core as a DLL.

## Contents

- `src/Rts.Core`: deterministic simulation library targeting `netstandard2.1`.
- `src/Rts.Core.Tests`: no-dependency console test runner targeting `net8.0`.
- `docs`: product, architecture, licensing, OpenRA audit, movement/animation targets, and stage planning.
- `external/openra`: copied OpenRA reference source for audit only.
- `external/redalert_reference`: copied historical reference source, read-only and not used as a code base.
- `art/concepts`: copied concept cards and generated registries.
- `unity`: Stage 1 Unity desktop board prototype and setup notes.

## Run Tests

From `ProjectAegisRTS`:

```powershell
dotnet run --project src/Rts.Core.Tests
```

Stage 0 tests are now passing with the installed .NET SDK.

To publish the local test executable and refresh the desktop shortcut after changes:

```powershell
.\tools\build-stage0-test-runner.ps1
```

## Unity Stage 1

Build the core DLL for Unity:

```powershell
.\tools\build-rts-core-for-unity.ps1
```

Run Stage 1 checks:

```powershell
.\tools\run-stage1-checks.ps1
```

Open the Unity project:

```powershell
.\tools\open-unity-project.ps1
```
