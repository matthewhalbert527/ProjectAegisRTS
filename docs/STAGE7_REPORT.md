# Stage 7 Report

## Summary

Stage 7 adds a Unity-only building animation, power, production, and damage visualization layer on top of the Stage 6 movement foundation. Buildings now resolve profile data, generate placeholder visual parts, and react visually to powered, low-power, offline, producing, damaged, and destroyed-placeholder states without changing deterministic gameplay authority.

## Branch And Base

- Branch: `codex/stage-7-building-animation-power`
- Base commit: `6aa0f7f703e30cfb2cf4d278aac31722ecff9c60`
- Implementation commit: pending final local commit.

## Files Changed

- Scene: `unity/Assets/Rts/Scenes/Stage7_BuildingPowerProduction.unity`
- Runtime building visuals: `unity/Assets/Rts/Scripts/Rendering/Buildings`
- Runtime HUD: `unity/Assets/Rts/Scripts/UI/Common/BuildingAnimationDebugHud.cs`
- Actor render integration: `unity/Assets/Rts/Scripts/Rendering/ActorViewBehaviour.cs` and `unity/Assets/Rts/Scripts/Rendering/ActorRenderSystem.cs`
- Building visual profiles: `unity/Assets/Rts/ScriptableObjects/BuildingProfiles`
- Editor tooling: `unity/Assets/Rts/Editor/Stage7*`
- Validation tooling: `tools/run-unity-stage7-validation.ps1` and `tools/run-stage7-checks.ps1`
- Documentation: this report, `docs/STAGE7_BUILDING_ANIMATION_PLAN.md`, README/setup/stage/architecture updates.

## Systems Created

Stage 7 adds the following visual-only systems:

- `BuildingVisualProfile` and `BuildingVisualProfileLibrary` for safe Unity-side tuning by actor type.
- `BuildingVisualStateController` as the main snapshot-driven building visual state owner.
- Child controllers for lights, machinery loops, production indicators, doors, damage placeholders, and building-specific loops.
- `BuildingPlaceholderPartFactory` and `Stage7BuildingMaterialLibrary` for generated primitive placeholder parts and materials.
- `BuildingPowerDemoController` for Stage 7 debug/demo actions through `RtsSimulationDriver` where possible, with isolated visual-only overrides for presentation checks.
- `BuildingAnimationDebugHud` as the F10 debug panel.
- `Stage7SceneCreator`, `Stage7SceneValidator`, and `Stage7PlayModeSmokeValidator`.

## Scene

Stage 7 scene path:

```text
unity/Assets/Rts/Scenes/Stage7_BuildingPowerProduction.unity
```

The scene includes `RtsGame`, `BoardRoot`, `Main Camera`, `Directional Light`, `EventSystem`, `Canvas`, `RtsGameBootstrapper`, `BoardRenderer`, `ActorRenderSystem`, `VisualMotionProfileLibrary`, `BuildingVisualProfileLibrary`, `BuildingAnimationDebugHud`, `BuildingPowerDemoController`, `RtsStatusLog`, and a generated Stage 7 placeholder building showcase.

Camera framing:

- Position: `16, 38, -26`
- Rotation: `60, 0, 0`
- Orthographic: true
- Size: `28`
- Clip planes: `0.1 / 1000`

## Architecture Boundary

`Rts.Core` remains the deterministic authority for actors, production, placement, power, movement, command handling, and snapshots. Stage 7 consumes `ActorSnapshot` values and applies Unity-only presentation state to child transforms, primitive meshes, colors, and debug readouts. Building controllers do not issue authoritative commands except through existing driver demo methods, do not use Unity physics for gameplay, and do not write visual animation state back into `Rts.Core`.

## Power-State Visualization

Building visuals derive:

- `BuildingPowerVisualState.Normal` when powered.
- `BuildingPowerVisualState.LowPower` when the snapshot reports low power.
- `BuildingPowerVisualState.Offline` when the snapshot reports unpowered/offline.

Light intensity, blinking, machinery speed, and type-specific loops respond to these states. Low power dims lights and slows machinery; offline turns them off where supported.

## Production Animation Foundation

Production visualization uses snapshot production state and progress where available. Production profiles can pulse indicators, drive placeholder bay doors, and expose future event hook methods for production start, progress, and completion visuals. If core production data is not observable during a demo smoke test, the Stage 7 demo controller can apply an isolated visual-only production override to prove the presentation path without mutating simulation state.

## Building-Specific Loops

Default profiles cover:

- `fabrication_hub`
- `power_plant`
- `advanced_power_plant`
- `barracks`
- `war_factory`
- `refinery`
- `gun_tower`
- `cannon_turret`
- `advanced_gun_tower`
- `comm_center`
- `repair_bay`
- `tech_center`
- `field_hospital`
- `dual_helipad`
- `default_building`
- `default_defense`

The placeholder loop layer supports cranes, turbines, radar dishes, doors, factory bays, refinery pumps, repair arms, turret/barrel sweeps, pad lights, warning lights, and damage markers.

## Damage-State Placeholder

The damage controller derives normalized health from snapshot health and max health. It can show warning/damage markers below the profile damage threshold and a destroyed placeholder below the destroyed threshold. This is visual-only and does not implement final destruction, debris, VFX, or combat logic.

## Debug HUD Controls

Open the Stage 7 scene and press Play.

- F10: toggle the building animation debug HUD.
- Trigger Low Power: toggles the existing low-power demo path.
- Clear Low Power: clears the demo low-power state.
- Power Plant, Barracks, War Factory, Refinery, Gun Tower: route demo production requests through the simulation driver.
- Force Visual Production: applies an isolated visual-only production state to the current building visual for smoke testing.
- Clear Overrides: clears visual demo overrides.
- Toggle Visual Debug: toggles debug state on the selected/first building visual.

## Commands Run

Baseline before Stage 7 implementation:

```powershell
dotnet run --project src/Rts.Core.Tests
.\tools\build-rts-core-for-unity.ps1
.\tools\run-stage1-checks.ps1
.\tools\run-stage2-checks.ps1
.\tools\run-stage2-playmode-smoke.ps1
.\tools\run-stage3-checks.ps1
.\tools\run-stage4-checks.ps1
.\tools\run-stage5-checks.ps1
.\tools\run-stage6-checks.ps1
git diff --check
```

Stage 7 focused validation:

```powershell
.\tools\run-unity-stage7-validation.ps1
```

Final acceptance validation:

```powershell
dotnet run --project src/Rts.Core.Tests
.\tools\build-rts-core-for-unity.ps1
.\tools\run-stage1-checks.ps1
.\tools\run-stage2-checks.ps1
.\tools\run-stage2-playmode-smoke.ps1
.\tools\run-stage3-checks.ps1
.\tools\run-stage4-checks.ps1
.\tools\run-stage5-checks.ps1
.\tools\run-stage6-checks.ps1
.\tools\run-unity-stage7-validation.ps1
.\tools\run-stage7-checks.ps1
git diff --check
```

## Validation Results

Validated locally with Unity `6000.5.1f1` at `E:\Unity\Hub\Editor\6000.5.1f1\Editor\Unity.exe`.

- Stage 0 core tests: 10/10 passing.
- Stage 1 validation: passing.
- Stage 2 validation: passing.
- Stage 2 Play Mode smoke: passing.
- Stage 3 validation: passing.
- Stage 4 validation: passing.
- Stage 5 validation: passing.
- Stage 6 validation: passing.
- Stage 7 scene creation/static validation/Play Mode smoke: passing in batchmode.
- `Rts.Core` UnityEngine scan: passing; no UnityEngine references found.
- `git diff --check`: passing.

## Manual Play Mode Checklist

Open:

```text
unity/Assets/Rts/Scenes/Stage7_BuildingPowerProduction.unity
```

Press Play and verify:

- Board visible.
- Actors visible.
- Buildings have visible placeholder parts.
- Building lights appear when powered.
- Low-power demo dims/stops building lights and machinery.
- Offline state turns lights/machinery off if available.
- Power plant turbine/core moves while powered.
- Barracks/war factory production indicator activates during production.
- War factory/barracks door placeholder moves during production if available.
- Refinery dock/pump placeholder moves if profile exists.
- Comm center radar placeholder moves if profile exists.
- Repair bay arms move if profile exists.
- Turret/gun tower barrel placeholder exists.
- Damage placeholder can activate; final damage/destruction is intentionally deferred.
- `BuildingAnimationDebugHud` appears or toggles with F10.
- `Stage6_MovementVisualization` still opens and validates.
- No repeating red console errors.

## Known Limitations

- Stage 7 uses generated primitives, simple materials, and local transform loops rather than final 3D art, VFX, sounds, Animator Controllers, or combat destruction.
- Production visualization depends on currently exposed snapshot data. Isolated visual-only overrides exist only for demo and smoke validation.
- Offline/destroyed visuals are placeholder state presentations, not final gameplay systems.
- The debug HUD is IMGUI and intended for validation and tuning, not final UI.

## Recommendation For Stage 8

Stage 8 should build the production art pipeline: asset naming/import rules, licensed model sources, model-to-profile mapping, pivot and socket conventions, material standards, animation clip naming, future Animator Controller hooks, and validation for replacing generated placeholder parts with final building meshes while keeping the Stage 7 snapshot-driven controller boundary intact.
