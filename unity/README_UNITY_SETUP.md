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
.\tools\run-stage10-checks.ps1
```

Stage 10 preserves the Stage 9 combat scene and adds deterministic ore harvesting, harvester cargo/refinery unloading snapshots, resource markers, cargo and dock markers, economy event markers, and `EconomyDebugHud`.

## Controls

- Left click: select actor or place active building preview.
- Right click: move selected mobile units.
- Space: pause or resume.
- Period or N: single-step one tick.
- Escape: cancel placement or clear selection.
- P/B/W/R/G: queue power plant, barracks, war factory, refinery, gun tower.
- I/T/H: queue rifle infantry, light tank, harvester.
- L: toggle forced low-power demo state.
- F8: toggle the Stage 10 economy debug HUD.
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
- Force-attack, guard, patrol, deploy, repair, and sell buttons are logged placeholders until later gameplay systems exist.
- Placeholder primitives stand in for final art, animation, and vehicle motion.
- Unity 6000.5.1f1 batchmode script compilation and scene generation pass locally.
- Play mode interaction validation should still be checked interactively after opening the generated scene.

## Later Stages

Later Quest/MR stages can swap the board transform and input layer without moving authoritative simulation state out of `Rts.Core`.
