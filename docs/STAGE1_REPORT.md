# Stage 1 Report

## Summary

Stage 1 adds a Unity desktop board prototype under `unity/`. The prototype keeps `Rts.Core` authoritative, copies the core DLL into Unity, renders Stage 0 snapshots with generated placeholder board and actor visuals, supports desktop selection/move/production/placement input, shows a debug HUD, and includes low-power and production visual indicators.

Unity Editor 6000.5.1f1 was found at `E:\Unity\Hub\Editor\6000.5.1f1\Editor\Unity.exe`. Batchmode script compilation completed and `Assets/Rts/Scenes/Stage1_DesktopBoard.unity` was generated.

## Files Created Or Changed

- `.gitignore`
- `README.md`
- `docs/STAGE1_REPORT.md`
- `tools/build-rts-core-for-unity.ps1`
- `tools/open-unity-project.ps1`
- `tools/run-stage1-checks.ps1`
- `unity/README_UNITY_SETUP.md`
- `unity/Packages/manifest.json`
- `unity/Packages/packages-lock.json`
- `unity/ProjectSettings/ProjectVersion.txt`
- Unity-generated `unity/ProjectSettings/*.asset`
- `unity/Assets/Rts/README.md`
- `unity/Assets/Rts/Plugins/RtsCore/Rts.Core.dll`
- `unity/Assets/Rts/Plugins/RtsCore/Rts.Core.pdb`
- `unity/Assets/Rts/Scenes/Stage1_DesktopBoard.unity`
- Unity `.meta` files for Stage 1 assets
- `unity/Assets/Rts/Editor/Stage1SceneCreator.cs`
- `unity/Assets/Rts/Scripts/Bootstrap/RtsGameBootstrapper.cs`
- `unity/Assets/Rts/Scripts/CoreBridge/BoardCoordinateMapper.cs`
- `unity/Assets/Rts/Scripts/CoreBridge/RtsCommandAdapter.cs`
- `unity/Assets/Rts/Scripts/CoreBridge/RtsSimulationDriver.cs`
- `unity/Assets/Rts/Scripts/Input/RtsDesktopInputController.cs`
- `unity/Assets/Rts/Scripts/Rendering/ActorRenderSystem.cs`
- `unity/Assets/Rts/Scripts/Rendering/ActorViewBehaviour.cs`
- `unity/Assets/Rts/Scripts/Rendering/BoardRenderer.cs`
- `unity/Assets/Rts/Scripts/UI/RtsDebugHud.cs`
- `unity/Assets/Rts/Scripts/Camera/RtsCameraController.cs`
- `unity/Assets/Rts/Scripts/Utilities/Stage1MaterialLibrary.cs`

## Rts.Core Linkage

`tools/build-rts-core-for-unity.ps1` builds `src/Rts.Core` for `netstandard2.1` and copies the resulting DLL to:

```text
unity/Assets/Rts/Plugins/RtsCore/Rts.Core.dll
```

Unity scripts reference the copied DLL and interact with the core through:

- `DemoWorldFactory.CreateMvpWorld()`
- `RtsWorld.CreateSnapshot()`
- `RtsWorld.IssueCommand(...)`
- `RtsWorld.PreviewPlacement(...)`
- `RtsWorld.ForcePlayerPowerState(...)`

No UnityEngine references were added to `src/Rts.Core`.

## Controls

- Left click: select actor or place the active building preview.
- Right click: issue move order for selected mobile units.
- Space: pause or resume.
- Period or N: single-step one tick.
- Escape: cancel placement or clear selection.
- P/B/W/R/G: queue power plant, barracks, war factory, refinery, gun tower.
- I/T/H: queue rifle infantry, light tank, harvester.
- L: toggle forced low-power demo state.
- WASD: pan camera.
- Mouse wheel: zoom.
- Q/E: rotate camera.

## Known Limitations

- Unity batchmode compile and scene generation pass locally.
- Play mode visual/interaction validation should still be checked interactively in the Editor.
- Placeholder primitives are used for units, buildings, lights, machinery, and production state.
- Drag select, full RTS sidebar UX, final animation controllers, final art, OpenXR, Meta XR, Quest support, multiplayer, and AI are intentionally out of scope.

## Commands Run

```powershell
Get-CimInstance Win32_OperatingSystem | Select-Object Caption, Version, BuildNumber, OSArchitecture | Format-List
Get-Location
git status --short --branch
dotnet --version
& 'C:\Program Files\dotnet\dotnet.exe' --version
git switch -c codex/stage-1-unity-desktop-board
& 'C:\Program Files\dotnet\dotnet.exe' run --project src/Rts.Core.Tests
.\tools\build-rts-core-for-unity.ps1
.\tools\run-stage1-checks.ps1
E:\Unity\Hub\Editor\6000.5.1f1\Editor\Unity.exe -batchmode -quit -projectPath "E:\OpenRA Mod\ProjectAegisRTS\unity" -executeMethod ProjectAegisRTS.UnityClient.EditorTools.Stage1SceneCreator.CreateStage1SceneBatch -logFile "E:\OpenRA Mod\ProjectAegisRTS\build\stage1-unity-batchmode.log"
rg -n "UnityEngine" src/Rts.Core
git diff --check
```

## Test Results

- `& 'C:\Program Files\dotnet\dotnet.exe' run --project src/Rts.Core.Tests`: passed 10/10.
- `.\tools\build-rts-core-for-unity.ps1`: succeeded, copied `Rts.Core.dll` and `Rts.Core.pdb`.
- `.\tools\run-stage1-checks.ps1`: passed core tests, DLL copy, Unity 6000.5.1f1 batchmode compilation, and Stage 1 scene generation.
- Unity batchmode log: initialized Unity `6000.5.1f1 (0d9463e84828)` and logged `Created Stage 1 scene at Assets/Rts/Scenes/Stage1_DesktopBoard.unity`.
- `rg -n "UnityEngine" src/Rts.Core`: no matches.
- `git diff --check`: no whitespace errors; Git reported CRLF conversion warnings for text files.

## Manual Play Validation Steps

1. Run `.\tools\open-unity-project.ps1`.
2. Open `Assets/Rts/Scenes/Stage1_DesktopBoard.unity`.
3. Press Play and verify board render, selection, movement, production buttons/hotkeys, placement preview, low-power toggle, production indicators, and camera controls.

## Next Recommended Stage

Stage 2 should build the desktop RTS command surface: a proper right-side production panel, selection details, command feedback, production queue management, clearer placement UX, and a deeper Unity smoke-test path now that the Editor is installed.
