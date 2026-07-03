# Unity Setup Notes

This folder is now the Unity desktop prototype root. Unity is presentation and input only: `Rts.Core` remains the deterministic authority, and Unity reads `WorldSnapshot` data while submitting command DTOs back to the core.

## Build the Core DLL

From `E:\OpenRA Mod\ProjectAegisRTS`:

```powershell
.\tools\build-rts-core-for-unity.ps1
```

The script builds `src/Rts.Core` and copies `Rts.Core.dll` into:

```text
unity\Assets\Rts\Plugins\RtsCore\Rts.Core.dll
```

## Open the Project

```powershell
.\tools\open-unity-project.ps1
```

The helper searches the custom local install path `E:\Unity\Hub\Editor\*\Editor\Unity.exe` as well as common Unity Hub locations. If Unity Editor is not found, open Unity Hub manually, add/open the `unity` folder, and let Unity import the project.

## Create the Stage 1 Scene

The Stage 1 check script can create this scene in batchmode. To recreate it manually in Unity, run:

```text
ProjectAegisRTS > Create Stage 1 Desktop Board Scene
```

This creates `Assets/Rts/Scenes/Stage1_DesktopBoard.unity` with `RtsGame`, `BoardRoot`, camera, light, and the Stage 1 scripts wired together.

## Create or Validate the Stage 2 Scene

Stage 2 adds the first PC RTS sidebar scene:

```text
Assets/Rts/Scenes/Stage2_PCSidebar.unity
```

To create or refresh it manually in Unity, run:

```text
ProjectAegisRTS > Create Stage 2 PC Sidebar Scene
```

To validate it from PowerShell:

```powershell
.\tools\run-unity-stage2-validation.ps1
.\tools\run-stage2-checks.ps1
```

The Stage 2 scene uses uGUI via `com.unity.ugui`, keeps the Stage 1 board/runtime bridge, and adds a screen-space canvas with the right sidebar, command bar, status log, production panels, selection panel, and minimap placeholder.

## Create or Validate the Stage 3 Scene

Stage 3 adds the Quest/OpenXR-ready board placement prototype:

```text
Assets/Rts/Scenes/Stage3_XRBoardPlacement.unity
```

To create or refresh it manually in Unity, run:

```text
ProjectAegisRTS > Create Stage 3 XR Board Placement Scene
```

To report XR package status manually in Unity, run:

```text
ProjectAegisRTS > Report Stage 3 XR Setup Status
```

To validate it from PowerShell:

```powershell
.\tools\run-unity-stage3-validation.ps1
.\tools\run-stage3-checks.ps1
```

Stage 3 installs or verifies Unity Registry packages for XR Plug-in Management, OpenXR, and Input System when batchmode can safely run. The runtime scene remains package-independent: if XR packages are absent or incomplete, the desktop fallback and placeholder XR rig still compile and run.

## Create or Validate the Stage 4 Scene

Stage 4 adds the Quest-style left-hand build and selection interface:

```text
Assets/Rts/Scenes/Stage4_LeftHandBuildSelection.unity
```

To create or refresh it manually in Unity, run:

```text
ProjectAegisRTS > Create Stage 4 Left-Hand Build Selection Scene
```

To report XR input package status manually in Unity, run:

```text
ProjectAegisRTS > Report Stage 4 XR Input Status
```

To validate it from PowerShell:

```powershell
.\tools\run-unity-stage4-validation.ps1
.\tools\run-stage4-checks.ps1
```

Stage 4 uses a simulated left-hand/controller rig, a wrist/radial uGUI build menu, placement and selection panels, ray selection, candidate cycling, and a simple board-space lasso. It compiles without XR Interaction Toolkit or Meta XR packages.

## Create or Validate the Stage 5 Scene

Stage 5 adds the dual-hand command scene:

```text
Assets/Rts/Scenes/Stage5_DualHandCommand.unity
```

To create or refresh it manually in Unity, run:

```text
ProjectAegisRTS > Create Stage 5 Dual-Hand Command Scene
```

To validate it from PowerShell:

```powershell
.\tools\run-unity-stage5-validation.ps1
.\tools\run-stage5-checks.ps1
```

Stage 5 preserves the Stage 4 left-hand build/selection interface and adds a simulated right-hand/controller rig, right-hand command HUD, command preview marker, move command routing, attack/force-attack placeholders, and board manipulation coexistence. It compiles without XR Interaction Toolkit or Meta XR packages.

## Create or Validate the Stage 6 Scene

Stage 6 adds the movement visualization scene:

```text
Assets/Rts/Scenes/Stage6_MovementVisualization.unity
```

To create or refresh it manually in Unity, run:

```text
ProjectAegisRTS > Create Stage 6 Movement Visualization Scene
```

To validate it from PowerShell:

```powershell
.\tools\run-unity-stage6-validation.ps1
.\tools\run-stage6-checks.ps1
```

Stage 6 preserves the Stage 5 dual-hand command scene and adds `VisualMotionProfileLibrary`, movement profile assets, actor visual motion controllers, vehicle/infantry/aircraft/turret placeholders, movement path preview, movement debug HUD, and a small visual-only showcase for categories not present in the default demo world.

## Create or Validate the Stage 7 Scene

Stage 7 adds the building power, production, and damage visualization scene:

```text
Assets/Rts/Scenes/Stage7_BuildingPowerProduction.unity
```

To create or refresh it manually in Unity, run:

```text
ProjectAegisRTS > Create Stage 7 Building Power Production Scene
```

To create or refresh the default building visual profiles manually in Unity, run:

```text
ProjectAegisRTS > Create Stage 7 Building Visual Profiles
```

To validate it from PowerShell:

```powershell
.\tools\run-unity-stage7-validation.ps1
.\tools\run-stage7-checks.ps1
```

Stage 7 preserves the Stage 6 movement scene and adds `BuildingVisualProfileLibrary`, building visual profile assets, generated placeholder building parts, light/machinery/production/door/damage/type-specific controllers, `BuildingPowerDemoController`, and `BuildingAnimationDebugHud`.

## Create or Validate the Stage 8 Scene

Stage 8 adds the art pipeline showcase scene:

```text
Assets/Rts/Scenes/Stage8_ArtPipelineShowcase.unity
```

To create or refresh it manually in Unity, run:

```text
ProjectAegisRTS > Stage 8 > Create Art Pipeline Showcase Scene
```

To validate it from PowerShell:

```powershell
.\tools\run-unity-stage8-validation.ps1
.\tools\run-stage8-fast-checks.ps1
.\tools\run-stage8-medium-checks.ps1
.\tools\run-stage8-checks.ps1
```

Stage 8 preserves the Stage 7 building scene and adds `ActorVisualDefinitionLibrary`, `ConceptArtReferenceLibrary`, `ActorVisualPrefabResolver`, generated blockout prefabs, icon generation, prefab socket validation, `ArtPipelineShowcaseController`, and `ArtPipelineDebugHud`.

Stage 8.1 adds validation tiers for faster iteration. Use the fast tier after small Stage 8 art/prefab/script changes, the medium tier before committing, and the full Stage 8 checks for final acceptance. The full gate remains the slow Stage 0-through-Stage 8 chain. See `..\docs\VALIDATION_TIERS.md`.

## Create or Validate the Stage 9 Scene

Stage 9 adds the combat, weapons, projectiles, damage, and death scene:

```text
Assets/Rts/Scenes/Stage9_CombatWeaponsDamage.unity
```

To create or refresh combat visual profiles manually in Unity, run:

```text
ProjectAegisRTS > Stage 9 > Create Combat Visual Profiles
```

To create or refresh the scene manually in Unity, run:

```text
ProjectAegisRTS > Stage 9 > Create Combat Weapons Damage Scene
```

To validate it from PowerShell:

```powershell
.\tools\run-unity-stage9-validation.ps1
.\tools\run-stage9-fast-checks.ps1
.\tools\run-stage9-medium-checks.ps1
.\tools\run-stage9-checks.ps1
```

Stage 9 preserves the Stage 8 art pipeline scene and adds `CombatVisualProfileLibrary`, `ProjectileRenderSystem`, `CombatEventRenderSystem`, placeholder combat VFX, deterministic attack routing, and `CombatDebugHud`.

## Create or Validate the Stage 10 Scene

Stage 10 adds the economy harvesting scene:

```text
Assets/Rts/Scenes/Stage10_EconomyHarvesting.unity
```

To create or refresh the scene manually in Unity, run:

```text
ProjectAegisRTS > Stage 10 > Create Economy Harvesting Scene
```

To validate it from PowerShell:

```powershell
.\tools\run-unity-stage10-validation.ps1
.\tools\run-stage10-fast-checks.ps1
.\tools\run-stage10-medium-checks.ps1
.\tools\run-stage10-checks.ps1
```

Stage 10 preserves the Stage 9 combat scene and adds deterministic ore harvesting, harvester cargo/refinery unloading snapshots, resource markers, cargo and dock markers, economy event markers, and `EconomyDebugHud`.

## Create or Validate the Stage 11 Scene

Stage 11 adds the fog, radar, and minimap scene:

```text
Assets/Rts/Scenes/Stage11_FogRadarMinimap.unity
```

To create or refresh the scene manually in Unity, run:

```text
ProjectAegisRTS > Stage 11 > Create Fog Radar Minimap Scene
```

To validate it from PowerShell:

```powershell
.\tools\run-unity-stage11-validation.ps1
.\tools\run-stage11-fast-checks.ps1
.\tools\run-stage11-medium-checks.ps1
.\tools\run-stage11-checks.ps1
```

Stage 11 preserves the Stage 10 economy scene and adds player-perspective snapshots, fog overlay presentation, radar status, minimap dots, and `FogDebugHud`.

## Create or Validate the Stage 12 Scene

Stage 12 adds the AI skirmish foundation scene:

```text
Assets/Rts/Scenes/Stage12_AISkirmishFoundation.unity
```

To create or refresh the scene manually in Unity, run:

```text
ProjectAegisRTS > Stage 12 > Create AI Skirmish Foundation Scene
```

To validate it from PowerShell:

```powershell
.\tools\run-unity-stage12-validation.ps1
.\tools\run-stage12-fast-checks.ps1
.\tools\run-stage12-medium-checks.ps1
.\tools\run-stage12-checks.ps1
```

Stage 12 preserves the Stage 11 fog/radar/minimap scene and adds deterministic AI intent snapshots, AI intent/timeline placeholder presentation, and `AiDebugHud`.

## Create or Validate the Stage 13 Scene

Stage 13 adds the map, terrain, and pathing diagnostics scene:

```text
Assets/Rts/Scenes/Stage13_MapTerrainPathing.unity
```

To create or refresh the scene manually in Unity, run:

```text
ProjectAegisRTS > Stage 13 > Create Map Terrain Pathing Scene
```

To validate it from PowerShell:

```powershell
.\tools\run-unity-stage13-validation.ps1
.\tools\run-stage13-fast-checks.ps1
.\tools\run-stage13-medium-checks.ps1
.\tools\run-stage13-checks.ps1
```

Stage 13 preserves the Stage 12 AI scene and adds deterministic terrain/passability/path debug snapshots, terrain/path placeholder presentation, map validation readouts, and `MapValidationDebugHud`.

## Create or Validate the Stage 14 Scene

Stage 14 adds the feedback polish scene:

```text
Assets/Rts/Scenes/Stage14_FeedbackPolish.unity
```

To validate it from PowerShell:

```powershell
.\tools\run-unity-stage14-validation.ps1
.\tools\run-stage14-fast-checks.ps1
.\tools\run-stage14-medium-checks.ps1
.\tools\run-stage14-checks.ps1
```

Stage 14 preserves the Stage 13 map scene and adds snapshot-driven placeholder audio, VFX, UI, and haptic feedback.

## Create or Validate the Stage 15 Scene

Stage 15 adds the performance/build-readiness scene:

```text
Assets/Rts/Scenes/Stage15_PerformanceBuildReadiness.unity
```

To validate it from PowerShell:

```powershell
.\tools\run-unity-stage15-validation.ps1
.\tools\run-stage15-fast-checks.ps1
.\tools\run-stage15-medium-checks.ps1
.\tools\run-stage15-checks.ps1
```

Stage 15 preserves the Stage 14 feedback scene and adds object pooling, runtime stats, scene complexity, quality profiles, readiness reporters, and `RenderStatsHud`.

## Create or Validate the Stage 16 Scene

Stage 16 adds the integrated playable vertical slice:

```text
Assets/Rts/Scenes/Stage16_PlayableVerticalSlice.unity
```

To create or refresh it manually in Unity, run:

```text
ProjectAegisRTS > Stage 16 > Create Playable Vertical Slice Scene
```

To validate it from PowerShell:

```powershell
.\tools\run-unity-stage16-validation.ps1
.\tools\run-stage16-fast-checks.ps1
.\tools\run-stage16-medium-checks.ps1
.\tools\run-stage16-checks.ps1
```

Stage 16 preserves Stage 15 systems, restores the PC desktop HUD/sidebar in the integrated scene, and adds deterministic match/objective state, vertical-slice world setup, scenario debug actions, `MatchObjectiveHud`, and `IntegratedSystemsStatusHud`.

## Stage 16.5 Player Build Flow

Stage 16.5 adds a player-facing boot scene and Windows player build path:

```text
Assets/Rts/Scenes/Stage16_5_Boot.unity
```

To configure it in Unity, run:

```text
ProjectAegisRTS > Stage 16.5 > Configure Player Build Flow
```

To validate or build from PowerShell:

```powershell
.\tools\run-stage16-player-build-checks.ps1 -SkipPlayerBuild
.\tools\build-windows-player-stage16.ps1
```

The player build starts from Boot, then loads `Stage16_PlayableVerticalSlice`. Debug panels and placement controls are hidden by default, while the objective HUD remains visible. The exported-log root issue was a repeated `ProductionCategoryTabs.BuildIfNeeded()` `NullReferenceException` caused by adding a duplicate `GridLayoutGroup` to prebuilt UI.

## Stage 17 Player-Facing Polish

Stage 17 keeps the same Boot and Stage 16 scenes, then adds clearer player-facing UI:

```text
Assets/Rts/Scenes/Stage16_5_Boot.unity
Assets/Rts/Scenes/Stage16_PlayableVerticalSlice.unity
```

To configure or refresh Stage 17 manually in Unity, run:

```text
ProjectAegisRTS > Stage 17 > Configure Player-Facing Polish
```

To validate from PowerShell:

```powershell
.\tools\run-unity-stage17-validation.ps1
.\tools\run-stage17-fast-checks.ps1
.\tools\run-stage17-medium-checks.ps1
.\tools\run-stage17-player-facing-checks.ps1 -SkipPlayerBuild
```

Stage 17 adds the Options screen, in-match objective/status HUD, prompt HUD, hidden-by-default controls overlay, win/loss result screen, player-facing smoke validation, and Unity/Player log inspection. The Windows player still exports to `build\windows-player-stage16\ProjectAegisRTS.exe`.

## Stage 18 Tester Playability

Stage 18 keeps the same Boot and Stage 16 scenes, then adds tester-guided playability:

```text
Assets/Rts/Scenes/Stage16_5_Boot.unity
Assets/Rts/Scenes/Stage16_PlayableVerticalSlice.unity
```

To configure or refresh Stage 18 manually in Unity, run:

```text
ProjectAegisRTS > Stage 18 > Configure Tester Playability Pass
```

To validate from PowerShell:

```powershell
.\tools\run-unity-stage18-validation.ps1
.\tools\run-stage18-fast-checks.ps1
.\tools\run-stage18-medium-checks.ps1
.\tools\run-stage18-player-facing-checks.ps1 -SkipPlayerBuild
```

Stage 18 adds the build-order checklist, snapshot-derived progress tracker, next-step prompt system, clearer sidebar production states, non-overlapping scaled HUD layout, brighter player-build camera/fog defaults, and stricter hidden-debug/status-log validation. The Windows player still exports to `build\windows-player-stage16\ProjectAegisRTS.exe`.

## Stage 18.5 Fine Placement Grid

Stage 18.5 keeps the same Boot and Stage 16 scenes, then doubles placement resolution without changing board scale:

```text
Assets/Rts/Scenes/Stage16_5_Boot.unity
Assets/Rts/Scenes/Stage16_PlayableVerticalSlice.unity
```

To validate from PowerShell:

```powershell
.\tools\run-unity-stage18-5-validation.ps1
.\tools\run-stage18-5-fast-checks.ps1
.\tools\run-stage18-5-medium-checks.ps1
.\tools\run-stage18-5-player-facing-checks.ps1 -SkipPlayerBuild
```

Stage 18.5 adds a 2x authoritative placement grid in `Rts.Core`, renders thinner fine-grid lines with stronger coarse boundaries, snaps building placement to fine cells, and keeps normal selection/move/attack commands on coarse cells. A legacy 2 x 2 building now uses a 4 x 4 fine footprint while keeping the same physical size. The Windows player still exports to `build\windows-player-stage16\ProjectAegisRTS.exe`.

## Stage 19 Mission Flow Tuning

Stage 19 keeps the same Boot and Stage 16 scenes, then tunes the player-facing vertical slice around the fine placement grid:

```text
Assets/Rts/Scenes/Stage16_5_Boot.unity
Assets/Rts/Scenes/Stage16_PlayableVerticalSlice.unity
```

To validate from PowerShell:

```powershell
.\tools\run-unity-stage19-validation.ps1
.\tools\run-stage19-fast-checks.ps1
.\tools\run-stage19-medium-checks.ps1
.\tools\run-stage19-player-facing-checks.ps1 -SkipPlayerBuild
```

Stage 19 adds a Unity-only mission flow controller, 15 tutorial beats, compact/expanded checklist guidance, fine-grid placement copy, better sidebar recommendations, tuned resource/enemy spacing, and a non-debug victory smoke path. The Windows player still exports to `build\windows-player-stage16\ProjectAegisRTS.exe`.

## Stage 19.5 PC Sidebar And Pause Menu

Stage 19.5 keeps the same Boot and Stage 16 scenes, then reorganizes the Windows player UI:

```text
Assets/Rts/Scenes/Stage16_5_Boot.unity
Assets/Rts/Scenes/Stage16_PlayableVerticalSlice.unity
```

```powershell
.\tools\run-unity-stage19-5-validation.ps1
.\tools\run-stage19-5-fast-checks.ps1
.\tools\run-stage19-5-medium-checks.ps1
.\tools\run-stage19-5-player-facing-checks.ps1 -SkipPlayerBuild
```

The PC player-facing build now defaults to a right-side CnC/OpenRA-style sidebar: minimap top-right, credits/power/status under it, production tabs/cards/queue, placement readout, selection details, and command buttons. The left side is reserved for compact objective/checklist/prompt HUDs. Quest/MR left-hand and right-hand UI remains available for XR scenes, but it is hidden and input-suppressed in the default Windows build. Escape opens a centered pause menu with Resume, Restart Mission, Settings, Controls, Quit to Menu, and Quit Game.

## Stage 20 MVP Production Visuals

Stage 20 adds the MVP production proxy visual showcase:

```text
Assets/Rts/Scenes/Stage20_MvpProductionVisuals.unity
```

```powershell
.\tools\run-unity-stage20-validation.ps1
.\tools\run-stage20-fast-checks.ps1
.\tools\run-stage20-medium-checks.ps1
.\tools\run-stage20-player-facing-checks.ps1 -SkipPlayerBuild
```

Stage 20 keeps the Windows player on Boot and Stage 16 while swapping MVP actor visual definitions to generated production proxies with Stage 8 blockouts as fallbacks. It also validates the `PCDesktop`, `QuestXR`, and `DebugHybrid` UI split.

## Stage 21 MVP Visual QA

Stage 21 adds the MVP visual QA review scene and artist replacement readiness tooling:

```text
Assets/Rts/Scenes/Stage21_MvpVisualQaReview.unity
```

```powershell
.\tools\run-unity-stage21-validation.ps1
.\tools\run-stage21-fast-checks.ps1
.\tools\run-stage21-medium-checks.ps1
.\tools\run-stage21-player-facing-checks.ps1 -SkipPlayerBuild
```

Use `.\tools\run-stage21-fast-checks.ps1` while iterating on proxy readability, sockets, pivots, replacement metadata, or optional artist import scan behavior. Use `.\tools\run-stage21-medium-checks.ps1` before commits; it stays flat and does not call older medium scripts. Use `.\tools\run-stage21-checks.ps1` for full acceptance.

Optional artist-authored MVP source models can be staged under:

```text
Assets/Rts/Art/Models/Source/MVP
```

The scanner writes `docs/STAGE21_ARTIST_MODEL_IMPORT_STATUS.md`, and MVP QA writes `docs/STAGE21_MVP_VISUAL_QA.md`. Generated proxies remain active until a candidate passes Stage 21 QA, player-facing checks, and Player.log inspection.

## Stage 21.5 Windows Player Resolution

Stage 21.5 keeps the Windows player on the same Boot and Stage 16 scenes, but adds robust display startup behavior:

```powershell
.\tools\run-unity-stage21-5-validation.ps1
.\tools\run-stage21-5-fast-checks.ps1
.\tools\run-stage21-5-medium-checks.ps1
.\tools\run-stage21-5-player-facing-checks.ps1 -SkipPlayerBuild
```

Build and test the EXE:

```powershell
.\tools\build-windows-player-stage16.ps1
.\build\windows-player-stage16\ProjectAegisRTS.exe
.\tools\run-player-windowed-1080p.ps1
```

The default player window is 1600x900, invalid tiny windows are clamped to at least 1280x720, and Boot Options exposes Windowed, Fullscreen Window, 1280x720, 1600x900, 1920x1080, Apply Display, and Reset Display Settings. `Player.log` records Stage 21.5 display startup diagnostics.

## Stage 27.1 PC Placement UX

Stage 27.1 keeps the Windows player on Boot and Stage 16, but separates board setup placement from production building placement:

```powershell
.\tools\run-unity-stage27-1-validation.ps1
.\tools\run-stage27-1-fast-checks.ps1
.\tools\run-stage27-1-medium-checks.ps1
.\tools\run-stage27-1-player-facing-checks.ps1 -SkipPlayerBuild
```

PCDesktop building placement uses the right sidebar `PlacementModePanel` plus the fine-grid footprint preview. The Stage 3 `BoardPlacementHud` remains hidden unless board setup placement is explicitly active. QuestXR board setup and Stage 4/5 hand controls remain available.

## Stage 28 Integrated Feature Regression QA

Stage 28 keeps the Windows player on Boot and Stage 16, but adds hidden QA coverage around the whole current feature surface:

```powershell
.\tools\run-unity-stage28-validation.ps1
.\tools\run-stage28-fast-checks.ps1
.\tools\run-stage28-medium-checks.ps1
.\tools\run-stage28-player-facing-checks.ps1 -SkipPlayerBuild
```

## Create or Validate the Stage 32 Terrain Set Dressing Scene

Stage 32 adds the generated terrain-piece catalog and review scene:

```text
Assets/Rts/Scenes/Stage32_TerrainSetDressingReview.unity
```

To generate or refresh the terrain-piece prefabs, catalogs, player-facing set dressing profile, and review scene manually in Unity, run:

```text
ProjectAegisRTS > Stage 32 > Generate Terrain Pieces Batch
ProjectAegisRTS > Stage 32 > Create Terrain Set Dressing Review Scene
```

To validate it from PowerShell:

```powershell
.\tools\run-stage32-fast-checks.ps1
.\tools\run-stage32-medium-checks.ps1
.\tools\run-stage32-checks.ps1
```

Stage 32 terrain pieces are visual-only. They must not be used as gameplay terrain, passability, placement, or resource authority; `Rts.Core` remains the deterministic source of truth.

The hidden `FeatureRegressionHud` is created at runtime and toggles with `F10` in development builds/editor play. It audits major command routes, PCDesktop sidebar state, QuestXR control presence, fine-grid placement status, economy, AI, visibility, air/naval, support, engineer, and transport surfaces. It is hidden by default and is not intended as normal player UI.

## Stage 28.1 PC Safe Area And Full Gate Flattening

Stage 28.1 keeps the Windows player on Boot and Stage 16, but reserves a PCDesktop gameplay camera safe area so the board does not render under the right sidebar or left objective stack:

```powershell
.\tools\run-unity-stage28-1-validation.ps1
.\tools\run-stage28-1-fast-checks.ps1
.\tools\run-stage28-1-medium-checks.ps1
.\tools\run-stage28-1-player-facing-checks.ps1 -SkipPlayerBuild
```

`PcGameplaySafeAreaController` computes the usable screen rect, and `PlayerFacingCameraFramer` applies camera rect/framing. QuestXR keeps full-screen camera framing. Stage 28.1 also flattens the Stage 28 full acceptance gate and adds `tools\audit-full-validation-recursion.ps1`.

## Controls

- Left click: select actor or place active building preview.
- Right click: move selected mobile units.
- Escape: cancel active building or board setup placement; otherwise open the pause menu.
- Space: quick pause or resume.
- Period or N: developer single-step one tick.
- P/B/W/R/G: queue power plant, barracks, war factory, refinery, gun tower.
- I/T/H: queue rifle infantry, light tank, harvester.
- L: toggle forced low-power demo state.
- F8: toggle the Stage 10 economy debug HUD.
- F7: toggle the Stage 11 fog/radar debug HUD.
- F6: toggle the Stage 12 AI debug HUD.
- F5: toggle the Stage 13 map validation debug HUD.
- F4: toggle the Stage 14 feedback debug HUD.
- F3: toggle the Stage 15 render stats HUD.
- F10: toggle the Stage 28 feature regression QA overlay.
- O: toggle the Stage 16 match/objective HUD.
- C: toggle the Stage 19 mission-flow checklist.
- Tab: expand or compact the Stage 19 checklist.
- P: toggle the Stage 19 next-step prompt.
- Y: toggle the Stage 16 integrated systems debug HUD.
- F1 or H: toggle the Stage 17 player controls overlay.
- F1-F6: switch Stage 2 production tabs.
- S/M/A: stop, move mode, or attack placeholder command.
- Backquote: toggle the Stage 1 debug overlay if it is enabled in the scene.
- WASD: pan camera.
- Mouse wheel: zoom.
- Q/E: rotate camera.

Stage 2 also exposes these actions through buttons in the sidebar and bottom command bar: production, queue cancel, placement cancel, stop, move, attack placeholder, guard/patrol/deploy/repair/sell placeholders, power toggle, pause, step, and low-power demo.

## Stage 4 Left-Hand Desktop Fallback Controls

- C: toggle the left-hand build interface.
- F1-F6: switch left-hand production categories.
- 1-8: queue the matching build card in the active category.
- Mouse ray: simulated left-hand ray.
- Left mouse or Enter: select, confirm placement, or activate the current action.
- Ctrl + left mouse: additive selection.
- Escape: cancel placement/menu/active mode or clear selection.
- Tab / Shift+Tab: cycle ambiguous selection candidates.
- Backquote: toggle the Stage 4 status HUD.
- L plus mouse drag: board-space lasso selection.

## Stage 5 Right-Hand Desktop Fallback Controls

- V: toggle the right-hand command HUD.
- M: enter move mode.
- A: enter attack placeholder mode.
- F: enter force-attack placeholder mode.
- Right mouse or Enter: confirm the current right-hand command.
- Space or middle mouse: board manipulation mode.
- Q/E: rotate the board while manipulating.
- Mouse wheel: scale/zoom the board while manipulating.
- Escape: cancel the active right-hand command mode.

## Stage 6 Movement Visualization Controls

- F9: toggle the movement debug HUD.
- Right-hand/desktop move commands continue to show the Stage 5 target marker and now may also draw a Stage 6 path preview.

## Stage 7 Building Animation Controls

- F10: toggle the building animation debug HUD.
- Trigger Low Power: force the existing low-power demo path.
- Clear Low Power: clear the demo low-power state.
- Power Plant, Barracks, War Factory, Refinery, Gun Tower: send demo production requests through `RtsSimulationDriver`.
- Force Visual Production: apply an isolated visual-only production override for presentation validation.
- Clear Overrides: clear visual demo overrides.
- Toggle Visual Debug: toggle debug state on the selected or first building visual.

## Stage 8 Art Pipeline Controls

- F11: toggle the art pipeline debug HUD.
- Prev/Next: cycle actor visual definitions.
- Spawn: spawn the selected blockout preview.
- Concepts: toggle concept reference cards.
- Sockets: toggle socket labels.
- Refresh: rebuild the showcase grid.
- Validate All: run runtime definition validation.

## Stage 9 Combat Controls

- F12: toggle the combat debug HUD.
- Select Attacker: select the first friendly combat demo attacker.
- Attack Target: issue an attack order against the first enemy target.
- Stop: stop selected combat orders.
- Reset Combat: recreate the combat demo world.
- A: desktop/right-hand attack mode now routes to real attack orders when targeting an enemy actor.
- F: force-attack remains a safe placeholder route.

## Stage 3 Board Placement Controls

- Tab: toggle board placement mode.
- Enter: confirm placement and save settings.
- Escape: cancel placement mode and restore the starting transform.
- R: reset placement to defaults while placement mode is active.
- Arrow keys or WASD: move the board horizontally while placement mode is active.
- Q/E: adjust board yaw.
- PageUp/PageDown or Z/X: adjust height.
- Shift or Ctrl plus mouse wheel: adjust board scale.
- HUD buttons: toggle placement, confirm, cancel, reset, save, load, and recenter.

Placement settings save through `PlayerPrefs` under the Stage 3 board placement key. The transform affects only Unity presentation and coordinate mapping; `Rts.Core` simulation state is not mutated.

## Known Limits

- Meta XR Core/Interaction SDK packages are not imported automatically.
- Stage 2 is still placeholder-art PC UI, not final visual art.
- Stage 3 uses a placeholder XR rig until a proper XR Origin/controller rig is added in a later stage.
- Stage 4 uses generated placeholder wrist/radial UI and a compile-safe no-op XR adapter until physical Quest input is connected.
- Stage 5 uses generated placeholder right-hand command UI and a compile-safe no-op XR adapter until physical Quest input is connected.
- Stage 6 uses placeholder primitives and profile-driven visual motion controllers; it is not final art, physics, combat animation, or skeletal animation.
- Stage 7 uses placeholder building parts, simple materials, and transform loops; it is not final building art, VFX, sound, combat damage, or destruction.
- Stage 8 uses generated blockout prefabs and concept copies; it is not final production modeling, rigging, VFX, audio, combat, or destruction.
- Stage 9 uses MVP combat balance and placeholder VFX; it is not final combat balance, armor, line-of-sight, splash damage, audio, or destruction art.
- Stage 10 uses MVP economy balance and placeholder ore/cargo/refinery visuals; it is not final resource balance or art.
- Stage 11 uses deterministic fog and placeholder minimap/fog visuals; it is not final scouting UX.
- Stage 12 uses deterministic skirmish AI intents; it is not final opponent strategy.
- Stage 13 uses placeholder terrain/path debug visuals; it is not final map authoring.
- Stage 14 uses placeholder/silent feedback assets; it is not final audio, VFX, haptics, or UI polish.
- Stage 15 uses placeholder budgets/readiness checks; it is not final Quest profiling or release packaging.
- Stage 16 is a vertical slice with simple base-destroy objectives; it is not campaign scripting, multiplayer, replay, save/load, final balance, or final art.
- Stage 17 is player-facing polish for the vertical slice; it is not final tutorial design, final options UI, final campaign flow, final balance, or final art.
- Stage 18 is tester-guided playability for the vertical slice; it is not a final tutorial, final mission script, final UX, final balance, or final art pass.
- Stage 18.5 is a placement-resolution pass; movement/pathing still use coarse command cells while building placement and building occupancy use fine cells.
- Stage 19 is mission-flow tuning for the prototype slice; it is not a full campaign system, final tutorial, advanced AI, final balance, or final art pass.
- Stage 19.5 is PC UI layout and pause-menu work; it is not final chrome art, final icons, or final settings UX.
- Stage 20 is a first-pass MVP production proxy visual layer; it is not final artist-authored FBX/GLB art, final VFX, final audio, or final Quest device profiling.
- Stage 21 is an MVP visual QA and replacement-readiness pass; it is not final art direction, final source model replacement, final VFX, final audio, or Quest device profiling.
- Stage 21.5 is Windows player resolution/UI-scaling hardening; it is not final settings UI, final platform packaging, final accessibility, or Stage 22 artist intake.
- Stage 27.1 is a targeted PC placement UX fix; it is not a new gameplay stage or final production art/UI pass.
- Stage 28 is an integrated QA/stabilization pass; it is not final content, final art, replay/multiplayer, campaign scripting, or release packaging.
- Stage 28.1 is a targeted validation/layout follow-up; it is not Stage 29, final UI art, final camera design, or release packaging.
- Stage 29 is a realistic battlefield visual-quality pass over terrain materials, MVP proxy detail, lighting, and review tooling; it is not final imported art, final VFX, or a gameplay/balance pass.
- Stage 30 is a visual readability QA pass over Stage 29 contrast, camera readability, proxy distinguishability, resource readability, and screenshot tooling; it is not final imported art, final VFX, or a gameplay/balance pass.
- Stage 31 is an artist handoff/package cleanup pass over export briefs, naming rules, trim-sheet guidance, LOD targets, Quest budgets, screenshots, and replacement checklists; it is not final imported art, final VFX, Quest device profiling, or a gameplay/balance pass.
- Force-attack, guard, patrol, deploy, repair, and sell buttons are logged placeholders until later gameplay systems exist.
- Placeholder primitives stand in for final art, animation, and vehicle motion.
- Unity 6000.5.1f1 batchmode script compilation and scene generation pass locally.
- Play mode interaction validation should still be checked interactively after opening the generated scene.

## Later Stages

Later Quest/MR stages can swap the board transform and input layer without moving authoritative simulation state out of `Rts.Core`.

Stage 20, Stage 21, Stage 21.5, Stage 27.1, Stage 28, Stage 28.1, Stage 29, Stage 30, and Stage 31 preserve the platform UI split: Windows player builds default to `PCDesktop` with the right-side sidebar and safe-area board framing, while `QuestXR` keeps left-hand build/selection, explicit board setup placement, full-screen camera framing, and right-hand tactical controls with the PC sidebar hidden.
