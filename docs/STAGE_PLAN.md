# Stage Plan

## Stage 0

Complete. Deterministic core, concept extraction/registry, OpenRA audit, docs, and no-dependency console tests.

## Stage 1

Complete. Unity desktop board prototype with placeholder meshes driven by `WorldSnapshot` and commands submitted to `Rts.Core`. Completed with batchmode scene generation, validation, camera framing, runtime board rendering, placeholder actors, selection, move orders, pause, step, and low-power demo controls.

## Stage 2

Complete, pending visual polish. PC right-side OpenRA-style production panel using original UI implementation and Stage 0 command/snapshot APIs. Stage 2 now provides `Assets/Rts/Scenes/Stage2_PCSidebar.unity`, a uGUI right sidebar, production tabs/grid, queue cancellation, placement readout, selection details, command bar, minimap placeholder, status log, hidden-by-default debug overlay, batchmode validation scripts, and automated smoke validation.

## Stage 3

Complete. Quest/OpenXR board placement prototype with adjustable height, yaw, scale, recenter, reset, save/load, desktop fallback controls, XR-safe adapter placeholders, package setup reporting, automated smoke validation, and `Assets/Rts/Scenes/Stage3_XRBoardPlacement.unity`.

## Stage 4

Complete. Quest left-hand build and selection interface. Adds `Assets/Rts/Scenes/Stage4_LeftHandBuildSelection.unity`, simulated left-hand/controller rig, wrist/radial production categories, build item cards, queue routing through the simulation bridge, placement footprint preview, ray selection, ambiguous candidate cycling, board-space lasso selection, desktop fallback controls, XR-safe adapter boundaries, package/input status reporting, and automated smoke validation.

## Stage 5

Complete. Quest right-hand tactical command interface. Adds `Assets/Rts/Scenes/Stage5_DualHandCommand.unity`, simulated right-hand/controller rig, right-hand command HUD, move command routing, attack and force-attack placeholders, command preview markers, board manipulation coexistence, desktop fallback controls, XR-safe adapter boundaries, and automated smoke validation while preserving Stage 2 and Stage 4 controls.

## Stage 6

Complete. High-quality movement visualization layered on deterministic simulation snapshots. Adds `Assets/Rts/Scenes/Stage6_MovementVisualization.unity`, profile-driven visual motion ScriptableObjects, visual-only actor smoothing, vehicle track/suspension placeholders, infantry locomotion placeholders, aircraft bank/hover placeholders, turret lag/recoil placeholders, movement path preview, movement debug HUD, Stage 6 showcase actors, and automated scene/play-mode validation.

## Stage 7

Complete. Building animation, power, production, and damage-state visualization layered on deterministic snapshots. Adds `Assets/Rts/Scenes/Stage7_BuildingPowerProduction.unity`, profile-driven building visual ScriptableObjects, generated placeholder building parts, light/machinery/production/door/damage/type-specific controllers, F10 building debug HUD, low-power and production demo controls, and automated scene/play-mode validation.

## Stage 8

Complete. Art pipeline / concept art to production prefabs. Adds `Assets/Rts/Scenes/Stage8_ArtPipelineShowcase.unity`, concept import/copy tooling, 27 concept reference assets, 27 actor visual definition assets, 27 generated blockout prefabs, generated icons, prefab socket/descriptors, runtime prefab resolver integration, F11 art pipeline debug HUD, generated validation reports, and automated scene/play-mode smoke validation.

## Stage 8.1

Complete. Validation tier tooling and hardening. Adds fast, medium, and full Stage 8 validation entrypoints, shared restore-if-needed/no-restore validation helpers, broader generated Unity YAML whitespace normalization, explicit live-editor fallback guidance, and documentation requiring future stages to include fast, medium, and full validation tiers where practical.

## Stage 9

Complete. Combat, weapons, projectiles, damage, death, and destruction presentation. Adds deterministic attack orders, weapon cooldowns, projectile simulation, damage/death state, combat snapshots/events, `Assets/Rts/Scenes/Stage9_CombatWeaponsDamage.unity`, placeholder projectile/muzzle/impact/damage/death presentation, F12 combat debug HUD, and fast/medium/full Stage 9 validation tiers.

## Stage 10

Complete. Economy, resource harvesting, and refinery loop. Adds deterministic ore resource cells, harvest orders, harvester cargo/work states, refinery dock/unload state, credit awards, economy snapshots/events, `Assets/Rts/Scenes/Stage10_EconomyHarvesting.unity`, resource/cargo/dock/event placeholder presentation, F8 economy debug HUD, scene validation, Play Mode smoke validation, and a full Stage 0-through-Stage 10 acceptance gate.

## Stage 11

Complete. Fog of war, radar, and minimap foundation. Adds deterministic per-player visibility state, explored/visible/unexplored cells, sight/radar definitions, player-perspective snapshots that hide unseen enemies, fog/radar/minimap snapshots, `Assets/Rts/Scenes/Stage11_FogRadarMinimap.unity`, placeholder fog overlay, minimap dots, F7 fog debug HUD, and fast/medium/full Stage 11 validation tiers.

## Stage 12

Complete. Skirmish AI foundation. Adds deterministic AI player definitions, difficulty profile, plan state, economy/production/attack/scouting/defense intents, AI command generation through the existing core command pipeline, AI snapshots, `Assets/Rts/Scenes/Stage12_AISkirmishFoundation.unity`, placeholder AI intent/timeline presentation, F6 AI debug HUD, and fast/medium/full Stage 12 validation tiers.

## Later Stages

Map/terrain/pathing tools, feedback foundation, performance/build readiness, multiplayer, replays, deterministic checksums, desync reporting, command stream validation, optimization, accessibility, and release packaging.
