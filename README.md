# ProjectAegisRTS

ProjectAegisRTS is a staged foundation for a modern RTS that can later run as a Meta Quest 3S VR/MR board game and as a PC RTS with a right-side production panel. Stage 0 created the deterministic, Unity-compatible C# simulation core. Stage 1 added a Unity desktop board prototype that consumes that core as a DLL. Stage 2 adds the first PC RTS sidebar, command bar, production queue, selection panel, minimap placeholder, and status log. Stage 3 adds the Quest/OpenXR-ready board placement prototype while preserving the PC scenes. Stage 4 adds a Quest-style left-hand build and selection interface with desktop fallback controls. Stage 5 adds the companion right-hand tactical command interface for movement, placeholder attack commands, command previews, and board manipulation. Stage 6 adds visual-only vehicle, infantry, aircraft, turret, and movement path presentation on top of deterministic snapshots. Stage 7 adds visual-only building animation, power-state, production, and damage-state presentation. Stage 8 adds the concept-art-to-production-prefab pipeline, actor visual definition catalog, generated blockout prefabs, icons, sockets, validation, and showcase scene. Stage 9 adds deterministic combat, weapons, projectiles, damage, death/destruction state, and Unity placeholder combat presentation. Stage 10 adds deterministic ore harvesting, harvester cargo, refinery unloading, economy snapshots/events, and Unity placeholder economy presentation. Stage 11 adds deterministic fog of war, radar status, minimap snapshots, and Unity placeholder fog/minimap presentation. Stage 12 adds deterministic skirmish AI planning, AI intent snapshots, and Unity placeholder AI debug presentation. Stage 13 adds deterministic terrain metadata, movement-class passability, path diagnostics, map validation, and Unity placeholder terrain/path debug presentation. Stage 14 adds snapshot-driven placeholder feedback presentation. Stage 15 adds performance/build-readiness scaffolding. Stage 16 adds an integrated playable vertical slice with match objectives, victory/defeat, and all prior gameplay/presentation systems in one scene. Stage 17 polishes the player-facing vertical slice with clearer boot/options/help flow, objective/prompt HUDs, win/loss screens, player-facing validation, log inspection, and fast/medium/full validation tiers. Stage 18 adds tester-guided playability: a build-order checklist, clearer prompts/sidebar states, EXE readability fixes, objective consistency checks, and Stage 18 validation tiers. Stage 18.5 adds an authoritative 2x fine placement grid so buildings keep the same physical size while placement footprints double in cell resolution. Stage 19 tunes that slice into a short mission flow with objective-gated tutorial beats, fine-grid guidance, readable build-order cues, non-debug victory validation, and Stage 19 validation tiers. Stage 19.5 reworks the Windows player UI into a CnC/OpenRA-style right sidebar and adds a normal Esc pause menu. Stage 20 adds MVP production proxy visuals and platform UI mode preservation. Stage 21 adds MVP visual QA, artist replacement readiness, optional artist model scanning, a review scene, and Stage 21 validation tiers. Stage 21.5 fixes Windows player resolution/UI scaling with display defaults, minimum clamping, options controls, Player.log diagnostics, and validation tiers. Stage 22 adds classic RTS command controls. Stage 23 adds base-management repair/sell/power/rally commands. Stage 24 adds tech prerequisites, advanced unlocks, support-power snapshots, Reveal Scan, Emergency Repair Pulse, and right-sidebar support buttons.

## Contents

- `src/Rts.Core`: deterministic simulation library targeting `netstandard2.1`.
- `src/Rts.Core.Tests`: no-dependency console test runner targeting `net8.0`.
- `docs`: product, architecture, licensing, OpenRA audit, movement/animation targets, and stage planning.
- `external/openra`: copied OpenRA reference source for audit only.
- `external/redalert_reference`: copied historical reference source, read-only and not used as a code base.
- `art/concepts`: copied concept cards and generated registries.
- `unity`: Unity desktop board prototype, Stage 2 PC sidebar scene, Stage 3 XR board placement prototype, Stage 4 left-hand build/selection scene, Stage 5 dual-hand command scene, Stage 6 movement visualization scene, Stage 7 building power/production scene, Stage 8 art pipeline showcase scene, Stage 9 combat scene, Stage 10 economy scene, Stage 11 fog/radar/minimap scene, Stage 12 AI skirmish scene, Stage 13 map terrain pathing scene, Stage 14 feedback scene, Stage 15 performance/build-readiness scene, Stage 16/17/18/18.5/19 playable vertical slice flow, Stage 20 production proxy visuals, Stage 21 MVP visual QA, Stage 21.5 Windows display scaling, Stage 22 command controls, Stage 23 base management, Stage 24 tech/support UI, and setup notes.

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

Stage 15.1 keeps medium validation flat: Stage 9 and later medium scripts run core tests once, build/copy `Rts.Core` once, then call direct prior-stage and current-stage Unity validation. They must not call prior medium scripts. The guard command `.\tools\audit-medium-validation-recursion.ps1` now fails if Stage 9-24 medium scripts reintroduce recursive medium dependencies, and Stage 24 full is the current final Stage 0-through-Stage 24 acceptance gate. `git diff --check` remains the whitespace gate; Windows line-ending conversion warnings are non-fatal when that command passes. See `docs/VALIDATION_TIERS.md`.

Run Stage 16 checks:

```powershell
.\tools\run-unity-stage16-validation.ps1
.\tools\run-stage16-fast-checks.ps1
.\tools\run-stage16-medium-checks.ps1
.\tools\run-stage16-checks.ps1
```

Use `run-stage16-fast-checks.ps1` for current vertical-slice iteration, `run-stage16-medium-checks.ps1` before local commits, and `run-stage16-checks.ps1` as the full Stage 0-through-Stage 16 acceptance gate. The medium recursion audit now covers Stage 9 through Stage 16.

Stage 16.5 adds the player-facing boot/build flow and fixes the Stage 16 Play Mode HUD initialization error seen in exported Unity logs. Use:

```powershell
.\tools\run-stage16-player-build-checks.ps1 -SkipPlayerBuild
.\tools\build-windows-player-stage16.ps1
```

The Windows player is written to `build\windows-player-stage16\ProjectAegisRTS.exe`. The boot scene is `Assets/Rts/Scenes/Stage16_5_Boot.unity`; it is first in Build Settings, followed by `Assets/Rts/Scenes/Stage16_PlayableVerticalSlice.unity`.

Stage 17 adds player-facing polish and validation tiers:

```powershell
.\tools\run-unity-stage17-validation.ps1
.\tools\run-stage17-fast-checks.ps1
.\tools\run-stage17-medium-checks.ps1
.\tools\run-stage17-player-facing-checks.ps1 -SkipPlayerBuild
.\tools\run-stage17-checks.ps1
```

Use `run-stage17-fast-checks.ps1` for UI polish iteration, `run-stage17-medium-checks.ps1` before local commits, and `run-stage17-checks.ps1` as the full Stage 0-through-Stage 17 acceptance gate. The Windows player path remains `build\windows-player-stage16\ProjectAegisRTS.exe`.

Stage 18 adds tester-guided playability and validation tiers:

```powershell
.\tools\run-unity-stage18-validation.ps1
.\tools\run-stage18-fast-checks.ps1
.\tools\run-stage18-medium-checks.ps1
.\tools\run-stage18-player-facing-checks.ps1 -SkipPlayerBuild
.\tools\run-stage18-checks.ps1
```

Use `run-stage18-fast-checks.ps1` while iterating, `run-stage18-medium-checks.ps1` before local commits, and `run-stage18-checks.ps1` as the full Stage 0-through-Stage 18 acceptance gate. The Windows player path remains `build\windows-player-stage16\ProjectAegisRTS.exe`.

Stage 18.5 adds fine placement grid resolution and validation tiers:

```powershell
.\tools\run-unity-stage18-5-validation.ps1
.\tools\run-stage18-5-fast-checks.ps1
.\tools\run-stage18-5-medium-checks.ps1
.\tools\run-stage18-5-player-facing-checks.ps1 -SkipPlayerBuild
.\tools\run-stage18-5-checks.ps1
```

Use `run-stage18-5-fast-checks.ps1` while iterating on placement/grid changes, `run-stage18-5-medium-checks.ps1` before local commits, `run-stage18-5-player-facing-checks.ps1 -SkipPlayerBuild` for a focused EXE-scene readiness pass, and `run-stage18-5-checks.ps1` as the slow full Stage 0-through-Stage 18.5 acceptance gate. The Windows player path remains `build\windows-player-stage16\ProjectAegisRTS.exe`.

Stage 19 adds mission-flow tuning and validation tiers:

```powershell
.\tools\run-unity-stage19-validation.ps1
.\tools\run-stage19-fast-checks.ps1
.\tools\run-stage19-medium-checks.ps1
.\tools\run-stage19-player-facing-checks.ps1 -SkipPlayerBuild
.\tools\run-stage19-checks.ps1
```

Use `run-stage19-fast-checks.ps1` while iterating on mission flow, UI guidance, placement readability, or pacing. Use `run-stage19-medium-checks.ps1` before local commits, `run-stage19-player-facing-checks.ps1 -SkipPlayerBuild` for a focused player-facing pass, and `run-stage19-checks.ps1` as the slow full Stage 0-through-Stage 19 acceptance gate. The Windows player path remains `build\windows-player-stage16\ProjectAegisRTS.exe`.

Stage 19.5 adds the CnC-style PC sidebar and Esc pause menu:

```powershell
.\tools\run-unity-stage19-5-validation.ps1
.\tools\run-stage19-5-fast-checks.ps1
.\tools\run-stage19-5-medium-checks.ps1
.\tools\run-stage19-5-player-facing-checks.ps1 -SkipPlayerBuild
.\tools\run-stage19-5-checks.ps1
```

Use `run-stage19-5-fast-checks.ps1` while iterating on PC sidebar, pause menu, or player-facing UI layout. Use `run-stage19-5-medium-checks.ps1` before local commits, `run-stage19-5-player-facing-checks.ps1 -SkipPlayerBuild` for a focused player-facing pass, and `run-stage19-5-checks.ps1` as the slow full Stage 0-through-Stage 19.5 acceptance gate. The Windows player path remains `build\windows-player-stage16\ProjectAegisRTS.exe`.

Stage 20 adds MVP production proxy visuals and validation tiers:

```powershell
.\tools\run-unity-stage20-validation.ps1
.\tools\run-stage20-fast-checks.ps1
.\tools\run-stage20-medium-checks.ps1
.\tools\run-stage20-player-facing-checks.ps1 -SkipPlayerBuild
.\tools\run-stage20-checks.ps1
```

Stage 21 adds MVP visual QA, artist replacement readiness, optional artist model scanning, and validation tiers:

```powershell
.\tools\run-unity-stage21-validation.ps1
.\tools\run-stage21-fast-checks.ps1
.\tools\run-stage21-medium-checks.ps1
.\tools\run-stage21-player-facing-checks.ps1 -SkipPlayerBuild
.\tools\run-stage21-checks.ps1
```

Use `run-stage21-fast-checks.ps1` while iterating on proxy readability, sockets, pivots, replacement metadata, import scan behavior, or the Stage 21 review scene. Use `run-stage21-medium-checks.ps1` before local commits, `run-stage21-player-facing-checks.ps1 -SkipPlayerBuild` for a focused player-facing pass, and `run-stage21-checks.ps1` as the slow full acceptance gate. The Windows player path remains `build\windows-player-stage16\ProjectAegisRTS.exe`.

Stage 21.5 fixes Windows player resolution and UI scaling:

```powershell
.\tools\run-unity-stage21-5-validation.ps1
.\tools\run-stage21-5-fast-checks.ps1
.\tools\run-stage21-5-medium-checks.ps1
.\tools\run-stage21-5-player-facing-checks.ps1 -SkipPlayerBuild
.\tools\run-stage21-5-checks.ps1
```

Use `run-stage21-5-fast-checks.ps1` while iterating on display defaults, options, CanvasScaler enforcement, Player.log diagnostics, or player build tooling. Use `run-stage21-5-medium-checks.ps1` before local commits; it stays flat and does not call prior medium scripts. Build the player with `.\tools\build-windows-player-stage16.ps1`, test the normal EXE, and use `.\tools\run-player-windowed-1080p.ps1` for a 1920x1080 windowed launch.

Stage 22 adds classic RTS command controls:

```powershell
.\tools\run-unity-stage22-validation.ps1
.\tools\run-stage22-fast-checks.ps1
.\tools\run-stage22-medium-checks.ps1
.\tools\run-stage22-player-facing-checks.ps1 -SkipPlayerBuild
.\tools\run-stage22-checks.ps1
```

Use `run-stage22-fast-checks.ps1` while iterating on command controls, PC input, or command-bar layout. Use `run-stage22-medium-checks.ps1` before local commits; it stays flat and calls direct Stage 21.5, Stage 4, Stage 5, and Stage 22 validation dependencies. Stage 22 adds attack-move, guard, patrol, scatter, deploy placeholder, double-click same-type selection, box selection, and client-local control groups while keeping gameplay authority in `Rts.Core`.

Stage 23 adds base-management commands:

```powershell
.\tools\run-unity-stage23-validation.ps1
.\tools\run-stage23-fast-checks.ps1
.\tools\run-stage23-medium-checks.ps1
.\tools\run-stage23-player-facing-checks.ps1 -SkipPlayerBuild
.\tools\run-stage23-checks.ps1
```

Use `run-stage23-fast-checks.ps1` while iterating on repair, sell, power toggle, rally points, command routing, or base-management snapshots. Use `run-stage23-medium-checks.ps1` before local commits; it stays flat and calls direct Stage 22, Stage 4, Stage 5, and Stage 23 validation dependencies. Stage 23 adds deterministic building repair, sell/refund/removal, powered-off production pausing, rally points for production buildings, PCDesktop command routing, and Quest left-hand compatible command routes while keeping gameplay authority in `Rts.Core`.

Stage 24 adds tech prerequisites and support powers:

```powershell
.\tools\run-unity-stage24-validation.ps1
.\tools\run-stage24-fast-checks.ps1
.\tools\run-stage24-medium-checks.ps1
.\tools\run-stage24-player-facing-checks.ps1 -SkipPlayerBuild
.\tools\run-stage24-checks.ps1
```

Use `run-stage24-fast-checks.ps1` while iterating on prerequisites, advanced unlocks, support-power definitions, support snapshots, or sidebar support buttons. Use `run-stage24-medium-checks.ps1` before local commits; it stays flat and calls direct Stage 23, Stage 4, Stage 5, and Stage 24 validation dependencies. Stage 24 adds deterministic prerequisite gating, Reveal Scan, Emergency Repair Pulse, placeholder support powers with cooldowns, PCDesktop sidebar support buttons, and Quest left-hand compatible support-power routing while keeping gameplay authority in `Rts.Core`.

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
- `Assets/Rts/Scenes/Stage16_5_Boot.unity`: Stage 16.5 player-facing boot/menu scene for Windows player builds.
- `Assets/Rts/Scenes/Stage16_PlayableVerticalSlice.unity`: Stage 16/17/18/18.5/19 integrated playable vertical slice with match/objective HUD, mission flow, checklist, player prompts, result screen, fine placement grid, PC sidebar, dual-hand controls, economy, combat, fog/minimap, AI, terrain/pathing, feedback, and performance presentation.
- `Assets/Rts/Scenes/Stage20_MvpProductionVisuals.unity`: Stage 20 production proxy visual showcase with MVP actor replacements, validation tags, sockets, LODs, and platform UI mode checks.
- `Assets/Rts/Scenes/Stage21_MvpVisualQaReview.unity`: Stage 21 MVP visual QA review scene with replacement-readiness status, socket/pivot/scale checks, and import scan reporting.
- `Scripts/Boot/PlayerDisplaySettings.cs`: Stage 21.5 Windows player display defaults, minimum resolution clamp, saved display preference reset, and Player.log startup diagnostics.
- `Scripts/UI/Common/ResponsiveCanvasScalerEnforcer.cs`: Stage 21.5 CanvasScaler guard for player-facing canvases.

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

Stage 16 vertical slice assets:

- Scenario scripts: `unity/Assets/Rts/Scripts/Scenario/`
- Player build flow scripts: `unity/Assets/Rts/Scripts/Boot/`
- Match/objective UI: `unity/Assets/Rts/Scripts/UI/Common/MatchObjectiveHud.cs`
- Player-facing HUDs: `unity/Assets/Rts/Scripts/UI/Common/PlayerObjectiveHud.cs`, `PlayerPromptHud.cs`, `PlayerControlsOverlay.cs`, and `MatchResultHud.cs`
- Integrated systems HUD: `unity/Assets/Rts/Scripts/UI/Common/IntegratedSystemsStatusHud.cs`
- Design notes: `docs/STAGE16_VERTICAL_SLICE_DESIGN.md`
- Stage report: `docs/STAGE16_REPORT.md`
- Stage 16.5 build flow report: `docs/STAGE16_5_BUILD_FLOW_REPORT.md`
- Stage 16.5 player build guide: `docs/STAGE16_5_PLAYER_BUILD_GUIDE.md`
- Stage 17 player-facing polish: `docs/STAGE17_PLAYER_FACING_POLISH.md`
- Stage 17 report: `docs/STAGE17_REPORT.md`
- Stage 18 tester playability guide: `docs/STAGE18_TESTER_PLAYABILITY_GUIDE.md`
- Stage 18 report: `docs/STAGE18_REPORT.md`
- Stage 18.5 fine grid design: `docs/STAGE18_5_FINE_GRID_DESIGN.md`
- Stage 18.5 fine grid report: `docs/STAGE18_5_FINE_GRID_REPORT.md`
- Stage 19 mission flow tuning: `docs/STAGE19_MISSION_FLOW_TUNING.md`
- Stage 19 report: `docs/STAGE19_REPORT.md`
- Stage 19.5 PC sidebar/pause menu: `docs/STAGE19_5_PC_SIDEBAR_PAUSE_MENU.md`
- Stage 19.5 UI rework report: `docs/STAGE19_5_UI_REWORK_REPORT.md`
- Stage 20 360-degree visual standards: `docs/STAGE20_360_VISUAL_STANDARDS.md`
- Stage 20 MVP visual replacement guide: `docs/STAGE20_MVP_VISUAL_REPLACEMENT_GUIDE.md`
- Stage 20 production visual validation: `docs/STAGE20_PRODUCTION_VISUAL_VALIDATION.md`
- Stage 20 report: `docs/STAGE20_REPORT.md`
- Stage 21 MVP visual QA: `docs/STAGE21_MVP_VISUAL_QA.md`
- Stage 21 artist import status: `docs/STAGE21_ARTIST_MODEL_IMPORT_STATUS.md`
- Stage 21 artist replacement checklist: `docs/STAGE21_ARTIST_ASSET_REPLACEMENT_CHECKLIST.md`
- Stage 21 report: `docs/STAGE21_REPORT.md`
- Stage 21.5 display scaling report: `docs/STAGE21_5_DISPLAY_SCALING_REPORT.md`
- Stage 21.5 player window guide: `docs/STAGE21_5_PLAYER_WINDOW_GUIDE.md`
- Stage 22 command interaction design: `docs/STAGE22_COMMAND_INTERACTION_DESIGN.md`
- Stage 22 report: `docs/STAGE22_REPORT.md`
- Stage 23 base management design: `docs/STAGE23_BASE_MANAGEMENT_DESIGN.md`
- Stage 23 report: `docs/STAGE23_REPORT.md`
- Stage 24 tech/support design: `docs/STAGE24_TECH_SUPPORT_DESIGN.md`
- Stage 24 report: `docs/STAGE24_REPORT.md`
