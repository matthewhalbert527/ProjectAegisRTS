# ProjectAegisRTS

ProjectAegisRTS is a staged foundation for a modern RTS that can later run as a Meta Quest 3S VR/MR board game and as a PC RTS with a right-side production panel. Stage 0 created the deterministic, Unity-compatible C# simulation core. Stage 1 added a Unity desktop board prototype that consumes that core as a DLL. Stage 2 adds the first PC RTS sidebar, command bar, production queue, selection panel, minimap placeholder, and status log. Stage 3 adds the Quest/OpenXR-ready board placement prototype while preserving the PC scenes. Stage 4 adds a Quest-style left-hand build and selection interface with desktop fallback controls. Stage 5 adds the companion right-hand tactical command interface for movement, placeholder attack commands, command previews, and board manipulation. Stage 6 adds visual-only vehicle, infantry, aircraft, turret, and movement path presentation on top of deterministic snapshots. Stage 7 adds visual-only building animation, power-state, production, and damage-state presentation. Stage 8 adds the concept-art-to-production-prefab pipeline, actor visual definition catalog, generated blockout prefabs, icons, sockets, validation, and showcase scene. Stage 9 adds deterministic combat, weapons, projectiles, damage, death/destruction state, and Unity placeholder combat presentation. Stage 10 adds deterministic ore harvesting, harvester cargo, refinery unloading, economy snapshots/events, and Unity placeholder economy presentation.

## Contents

- `src/Rts.Core`: deterministic simulation library targeting `netstandard2.1`.
- `src/Rts.Core.Tests`: no-dependency console test runner targeting `net8.0`.
- `docs`: product, architecture, licensing, OpenRA audit, movement/animation targets, and stage planning.
- `external/openra`: copied OpenRA reference source for audit only.
- `external/redalert_reference`: copied historical reference source, read-only and not used as a code base.
- `art/concepts`: copied concept cards and generated registries.
- `unity`: Unity desktop board prototype, Stage 2 PC sidebar scene, Stage 3 XR board placement prototype, Stage 4 left-hand build/selection scene, Stage 5 dual-hand command scene, Stage 6 movement visualization scene, Stage 7 building power/production scene, Stage 8 art pipeline showcase scene, Stage 9 combat scene, Stage 10 economy scene, and setup notes.

## Run Tests

From `ProjectAegisRTS`:

```powershell
dotnet run --no-restore --project src/Rts.Core.Tests
```

Stage 0 tests are now passing with the installed .NET SDK.

To publish the local test executable and refresh the desktop shortcut after changes:

```powershell
.\tools\build-stage0-test-runner.ps1
```

## Unity Stages

Build the core DLL for Unity:

```powershell
.\tools\build-rts-core-for-unity.ps1
```

Run Stage 1 checks:

```powershell
.\tools\run-stage1-checks.ps1
.\tools\run-unity-stage1-validation.ps1
```

Run Stage 2 checks:

```powershell
.\tools\run-unity-stage2-validation.ps1
.\tools\run-stage2-playmode-smoke.ps1
.\tools\run-stage2-checks.ps1
```

Run Stage 3 checks:

```powershell
.\tools\run-unity-stage3-validation.ps1
.\tools\run-stage3-checks.ps1
```

Run Stage 4 checks:

```powershell
.\tools\run-unity-stage4-validation.ps1
.\tools\run-stage4-checks.ps1
```

Run Stage 5 checks:

```powershell
.\tools\run-unity-stage5-validation.ps1
.\tools\run-stage5-checks.ps1
```

Run Stage 6 checks:

```powershell
.\tools\run-unity-stage6-validation.ps1
.\tools\run-stage6-checks.ps1
```

Run Stage 7 checks:

```powershell
.\tools\run-unity-stage7-validation.ps1
.\tools\run-stage7-checks.ps1
```

Run Stage 8 checks:

```powershell
.\tools\run-unity-stage8-validation.ps1
.\tools\run-stage8-fast-checks.ps1
.\tools\run-stage8-medium-checks.ps1
.\tools\run-stage8-checks.ps1
```

Stage 8.1 adds validation tiers. Use `run-stage8-fast-checks.ps1` for current Stage 8 iteration, `run-stage8-medium-checks.ps1` before local commits, and `run-stage8-checks.ps1` as the slow full Stage 0-through-Stage 8 acceptance gate. See `docs/VALIDATION_TIERS.md`.

Run Stage 9 checks:

```powershell
.\tools\run-unity-stage9-validation.ps1
.\tools\run-stage9-fast-checks.ps1
.\tools\run-stage9-medium-checks.ps1
.\tools\run-stage9-checks.ps1
```

Use `run-stage9-fast-checks.ps1` for current combat iteration, `run-stage9-medium-checks.ps1` before local commits, and `run-stage9-checks.ps1` as the slow full Stage 0-through-Stage 9 acceptance gate.

Run Stage 10 checks:

```powershell
.\tools\run-unity-stage10-validation.ps1
.\tools\run-stage10-checks.ps1
```

Stage 10 uses the full Stage 0-through-Stage 10 acceptance gate for this pass. Faster Stage 10 iteration tiers can be added in the next validation-tooling pass without weakening the full gate.

Open the Unity project:

```powershell
.\tools\open-unity-project.ps1
```

Scene paths:

- `Assets/Rts/Scenes/Stage1_DesktopBoard.unity`: Stage 1 board-only desktop prototype.
- `Assets/Rts/Scenes/Stage2_PCSidebar.unity`: Stage 2 PC sidebar UI.
- `Assets/Rts/Scenes/Stage3_XRBoardPlacement.unity`: Stage 3 Quest/OpenXR-ready board placement prototype with desktop fallback controls.
- `Assets/Rts/Scenes/Stage4_LeftHandBuildSelection.unity`: Stage 4 Quest-style left-hand build and selection interface with desktop fallback controls.
- `Assets/Rts/Scenes/Stage5_DualHandCommand.unity`: Stage 5 dual-hand scene with left-hand build/selection plus right-hand tactical commands.
- `Assets/Rts/Scenes/Stage6_MovementVisualization.unity`: Stage 6 movement visualization scene with profile-driven visual motion, path preview, debug HUD, and showcase actors.
- `Assets/Rts/Scenes/Stage7_BuildingPowerProduction.unity`: Stage 7 building animation scene with power/production/damage placeholder parts, profile library, F10 debug HUD, and demo controls.
- `Assets/Rts/Scenes/Stage8_ArtPipelineShowcase.unity`: Stage 8 art pipeline scene with concept references, generated blockout prefabs, actor visual definitions, prefab sockets, validation, resolver integration, and F11 debug HUD.
- `Assets/Rts/Scenes/Stage9_CombatWeaponsDamage.unity`: Stage 9 combat scene with deterministic attack orders, projectiles, damage/death snapshots, placeholder combat VFX, combat profiles, and F12 debug HUD.
- `Assets/Rts/Scenes/Stage10_EconomyHarvesting.unity`: Stage 10 economy scene with deterministic ore harvesting, harvester cargo/refinery unloading snapshots, resource markers, economy events, and F8 debug HUD.

Stage 8 art assets:

- Concept copies: `unity/Assets/Rts/Art/Concepts/`
- Generated icons: `unity/Assets/Rts/Art/Icons/`
- Generated blockouts: `unity/Assets/Rts/Art/Prefabs/Actors/GeneratedBlockouts/`
- Production prefabs: `unity/Assets/Rts/Art/Prefabs/Actors/Production/`
- Actor visual definitions: `unity/Assets/Rts/ScriptableObjects/Art/ActorVisualDefinitions/`
- Validation report: `docs/STAGE8_PREFAB_VALIDATION.md`

Stage 9 combat assets:

- Combat visual profiles: `unity/Assets/Rts/ScriptableObjects/Combat/`
- Combat design notes: `docs/STAGE9_COMBAT_DESIGN.md`
- Stage report: `docs/STAGE9_REPORT.md`

Stage 10 economy assets:

- Economy render scripts: `unity/Assets/Rts/Scripts/Rendering/Economy/`
- Economy design notes: `docs/STAGE10_ECONOMY_DESIGN.md`
- Stage report: `docs/STAGE10_REPORT.md`
