# ProjectAegisRTS

ProjectAegisRTS is a staged foundation for a modern RTS that can later run as a Meta Quest 3S VR/MR board game and as a PC RTS with a right-side production panel. Stage 0 created the deterministic, Unity-compatible C# simulation core. Stage 1 added a Unity desktop board prototype that consumes that core as a DLL. Stage 2 adds the first PC RTS sidebar, command bar, production queue, selection panel, minimap placeholder, and status log. Stage 3 adds the Quest/OpenXR-ready board placement prototype while preserving the PC scenes. Stage 4 adds a Quest-style left-hand build and selection interface with desktop fallback controls. Stage 5 adds the companion right-hand tactical command interface for movement, placeholder attack commands, command previews, and board manipulation. Stage 6 adds visual-only vehicle, infantry, aircraft, turret, and movement path presentation on top of deterministic snapshots. Stage 7 adds visual-only building animation, power-state, production, and damage-state presentation. Stage 8 adds the concept-art-to-production-prefab pipeline, actor visual definition catalog, generated blockout prefabs, icons, sockets, validation, and showcase scene. Stage 9 adds deterministic combat, weapons, projectiles, damage, death/destruction state, and Unity placeholder combat presentation. Stage 10 adds deterministic ore harvesting, harvester cargo, refinery unloading, economy snapshots/events, and Unity placeholder economy presentation. Stage 11 adds deterministic fog of war, radar status, minimap snapshots, and Unity placeholder fog/minimap presentation. Stage 12 adds deterministic skirmish AI planning, AI intent snapshots, and Unity placeholder AI debug presentation. Stage 13 adds deterministic terrain metadata, movement-class passability, path diagnostics, map validation, and Unity placeholder terrain/path debug presentation. Stage 14 adds snapshot-driven placeholder feedback presentation. Stage 15 adds performance/build-readiness scaffolding.

## Contents

- `src/Rts.Core`: deterministic simulation library targeting `netstandard2.1`.
- `src/Rts.Core.Tests`: no-dependency console test runner targeting `net8.0`.
- `docs`: product, architecture, licensing, OpenRA audit, movement/animation targets, and stage planning.
- `external/openra`: copied OpenRA reference source for audit only.
- `external/redalert_reference`: copied historical reference source, read-only and not used as a code base.
- `art/concepts`: copied concept cards and generated registries.
- `unity`: Unity desktop board prototype, Stage 2 PC sidebar scene, Stage 3 XR board placement prototype, Stage 4 left-hand build/selection scene, Stage 5 dual-hand command scene, Stage 6 movement visualization scene, Stage 7 building power/production scene, Stage 8 art pipeline showcase scene, Stage 9 combat scene, Stage 10 economy scene, Stage 11 fog/radar/minimap scene, Stage 12 AI skirmish scene, Stage 13 map terrain pathing scene, Stage 14 feedback scene, Stage 15 performance/build-readiness scene, and setup notes.

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
.\tools\run-stage10-fast-checks.ps1
.\tools\run-stage10-medium-checks.ps1
.\tools\run-stage10-checks.ps1
```

Use `run-stage10-fast-checks.ps1` for current economy iteration, `run-stage10-medium-checks.ps1` before local commits, and `run-stage10-checks.ps1` as the slow full Stage 0-through-Stage 10 acceptance gate.

Run Stage 11 checks:

```powershell
.\tools\run-unity-stage11-validation.ps1
.\tools\run-stage11-fast-checks.ps1
.\tools\run-stage11-medium-checks.ps1
.\tools\run-stage11-checks.ps1
```

Use `run-stage11-fast-checks.ps1` for current fog/radar/minimap iteration, `run-stage11-medium-checks.ps1` before local commits, and `run-stage11-checks.ps1` as the slow full Stage 0-through-Stage 11 acceptance gate.

Run Stage 12 checks:

```powershell
.\tools\run-unity-stage12-validation.ps1
.\tools\run-stage12-fast-checks.ps1
.\tools\run-stage12-medium-checks.ps1
.\tools\run-stage12-checks.ps1
```

Use `run-stage12-fast-checks.ps1` for current AI iteration, `run-stage12-medium-checks.ps1` before local commits, and `run-stage12-checks.ps1` as the slow full Stage 0-through-Stage 12 acceptance gate.

Run Stage 13 checks:

```powershell
.\tools\run-unity-stage13-validation.ps1
.\tools\run-stage13-fast-checks.ps1
.\tools\run-stage13-medium-checks.ps1
.\tools\run-stage13-checks.ps1
```

Use `run-stage13-fast-checks.ps1` for current map/pathing iteration, `run-stage13-medium-checks.ps1` before local commits, and `run-stage13-checks.ps1` as the slow full Stage 0-through-Stage 13 acceptance gate.

Run Stage 14 checks:

```powershell
.\tools\run-unity-stage14-validation.ps1
.\tools\run-stage14-fast-checks.ps1
.\tools\run-stage14-medium-checks.ps1
.\tools\run-stage14-checks.ps1
```

Use `run-stage14-fast-checks.ps1` for current feedback iteration, `run-stage14-medium-checks.ps1` before local commits, and `run-stage14-checks.ps1` as the full Stage 0-through-Stage 14 acceptance gate.

Run Stage 15 checks:

```powershell
.\tools\run-unity-stage15-validation.ps1
.\tools\run-stage15-fast-checks.ps1
.\tools\run-stage15-medium-checks.ps1
.\tools\run-stage15-checks.ps1
```

Use `run-stage15-fast-checks.ps1` for current performance/build-readiness iteration, `run-stage15-medium-checks.ps1` before local commits, and `run-stage15-checks.ps1` as the full Stage 0-through-Stage 15 acceptance gate.

Stage 15.1 keeps medium validation flat: Stage 9 and later medium scripts run core tests once, build/copy `Rts.Core` once, then call direct prior-stage and current-stage Unity validation. They must not call prior medium scripts. `git diff --check` remains the whitespace gate; Windows line-ending conversion warnings are non-fatal when that command passes. See `docs/VALIDATION_TIERS.md`.

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
- `Assets/Rts/Scenes/Stage11_FogRadarMinimap.unity`: Stage 11 fog/radar/minimap scene with player-perspective snapshots, fog overlay, radar status, minimap dots, and F7 debug HUD.
- `Assets/Rts/Scenes/Stage12_AISkirmishFoundation.unity`: Stage 12 AI skirmish scene with deterministic AI intents, command generation, plan timeline, and F6 debug HUD.
- `Assets/Rts/Scenes/Stage13_MapTerrainPathing.unity`: Stage 13 map terrain pathing scene with deterministic terrain metadata, path diagnostics, map validation, and F5 debug HUD.
- `Assets/Rts/Scenes/Stage14_FeedbackPolish.unity`: Stage 14 feedback scene with snapshot-driven placeholder audio/VFX/UI/haptic events and F4 debug HUD.
- `Assets/Rts/Scenes/Stage15_PerformanceBuildReadiness.unity`: Stage 15 performance/build-readiness scene with pooling, runtime stats, scene complexity, quality profiles, build-readiness reporters, and F3 render stats HUD.

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

Stage 11 fog/radar/minimap assets:

- Visibility render scripts: `unity/Assets/Rts/Scripts/Rendering/Visibility/`
- Fog/radar design notes: `docs/STAGE11_FOG_RADAR_DESIGN.md`
- Stage report: `docs/STAGE11_REPORT.md`

Stage 12 AI assets:

- AI render scripts: `unity/Assets/Rts/Scripts/Rendering/Ai/`
- AI design notes: `docs/STAGE12_AI_DESIGN.md`
- Stage report: `docs/STAGE12_REPORT.md`

Stage 13 map/pathing assets:

- Map render scripts: `unity/Assets/Rts/Scripts/Rendering/Map/`
- Map terrain/pathing design notes: `docs/STAGE13_MAP_TERRAIN_PATHING_DESIGN.md`
- Stage report: `docs/STAGE13_REPORT.md`

Stage 14 feedback assets:

- Feedback scripts: `unity/Assets/Rts/Scripts/Feedback/`
- Feedback profiles: `unity/Assets/Rts/ScriptableObjects/Feedback/`
- Feedback design notes: `docs/STAGE14_FEEDBACK_DESIGN.md`
- Stage report: `docs/STAGE14_REPORT.md`

Stage 15 performance/build-readiness assets:

- Performance scripts: `unity/Assets/Rts/Scripts/Performance/`
- Performance budget profiles: `unity/Assets/Rts/ScriptableObjects/Performance/`
- Quest performance budget: `docs/STAGE15_QUEST_PERFORMANCE_BUDGET.md`
- Build readiness notes: `docs/STAGE15_BUILD_READINESS.md`
- Stage report: `docs/STAGE15_REPORT.md`
