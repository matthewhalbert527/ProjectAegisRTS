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

## Stage 13

Complete. Map, terrain, pathing tools, and polish foundation. Adds deterministic terrain definitions, movement classes, passability masks, movement costs, terrain/resource map snapshots, structured path query diagnostics, map validation, `Assets/Rts/Scenes/Stage13_MapTerrainPathing.unity`, placeholder terrain/path debug presentation, F5 map validation HUD, authoring overlay placeholder, and fast/medium/full Stage 13 validation tiers.

## Stage 14

Complete. Audio, VFX, UI, and haptic feedback foundation. Adds a Unity-side feedback event bus, feedback profile assets, silent audio cues, primitive VFX markers, UI messages, haptic placeholders, `Assets/Rts/Scenes/Stage14_FeedbackPolish.unity`, F4 feedback debug HUD, and fast/medium/full Stage 14 validation tiers.

## Stage 15

Complete. Quest performance and build-readiness foundation. Adds placeholder Quest/PC performance budget profiles, object pooling for short-lived projectile and feedback marker visuals, runtime performance stats, scene complexity reporting, quality profile application, build-readiness reporters, `Assets/Rts/Scenes/Stage15_PerformanceBuildReadiness.unity`, F3 render stats HUD, and fast/medium/full Stage 15 validation tiers.

## Stage 16

Complete. Integrated playable vertical slice. Adds deterministic match/scenario/victory state, vertical-slice demo world, match and scenario snapshots, player/enemy bases, resources, fog/minimap, AI, terrain/pathing, economy, combat, feedback, performance HUD, PC sidebar, simulated dual-hand controls, `Assets/Rts/Scenes/Stage16_PlayableVerticalSlice.unity`, match/objective HUD, integrated systems status HUD, scenario debug actions, and fast/medium/full Stage 16 validation tiers with non-recursive medium validation guarded by the audit script.

## Stage 16.5

Complete. Player-facing boot/build flow and runtime hardening. Adds `Assets/Rts/Scenes/Stage16_5_Boot.unity`, boot/menu HUDs, player build settings, debug-panel visibility control, Stage 16 camera/placement/default HUD initialization, Windows player build scripts, and build-flow validation without adding Stage 17 gameplay scope.

## Stage 17

Complete. Player-facing vertical-slice polish. Adds clearer boot menu labels, controls/help, prototype options, in-match objective/status HUD, input prompts, hidden-by-default controls overlay, win/loss result screen, player-facing default validation hooks, Player.log/Unity log inspection, and fast/medium/full Stage 17 validation tiers while preserving deterministic `Rts.Core` authority.

## Stage 18

Complete. Tester-guided playability pass. Adds snapshot-derived checklist/progress tracking, next-step prompts, clearer sidebar production states, objective/match consistency tests, high-resolution player HUD layout, brighter EXE camera/lighting/fog defaults, stricter hidden-debug/status-log validation, Player.log inspection, and fast/medium/full Stage 18 validation tiers while preserving Stage 0-17 behavior.

## Stage 18.5

Complete. Fine placement grid / footprint resolution pass. Adds an authoritative 2x placement grid in `Rts.Core`, converts legacy building footprints to fine footprints, keeps physical building and board scale stable, projects fine building occupancy back to coarse pathing cells, updates Unity grid/preview/input/selection to use fine placement cells, and adds Stage 18.5 validation tiers while preserving Stage 0-18 behavior.

## Stage 19

Complete. Mission flow / tutorial beats / fine-grid playability tuning. Adds a Unity-only mission flow controller, 15 objective-gated player guidance beats, compact/expanded checklist presentation, sidebar/production/placement readability improvements, conservative resource/enemy/base pacing changes, a normal-command victory path test, Player.log inspection hardening, and Stage 19 validation tiers while preserving Stage 0-18.5 behavior.

## Stage 19.5

Complete. CnC-style PC sidebar and pause menu rework. Consolidates the Windows player UI into a right-side sidebar with minimap, credits/power/status, production tabs/cards/queue, placement, selection, and command buttons; hides XR left-hand/right-hand build UI in PC mode; reserves the left side for objective/checklist/prompts; adds a centered Esc pause menu; and adds Stage 19.5 validation tiers while preserving Stage 0-19 behavior.

## Stage 20

Complete after local validation. 360-degree MVP production visual replacement pass. Adds first-pass primitive proxy prefabs for the MVP actor set, a production visual standard library, validation tags, socket/LOD/view-coverage validation, a Stage20 showcase scene, explicit PCDesktop/QuestXR/DebugHybrid UI mode preservation, and Stage 20 fast/medium/player-facing/full validation tiers while preserving Stage 0-19.5 behavior.

## Stage 21

Complete after local validation. MVP visual QA and artist replacement readiness pass. Adds structured visual QA status, socket/pivot/scale validation, optional artist model import scanning, a Stage21 review scene, replacement metadata on MVP proxies, improved 360-degree readability details, Player.log coverage, and Stage 21 fast/medium/player-facing/full validation tiers while preserving Stage 0-20 behavior.

## Later Stages

Next later stages include applying artist-authored source model replacements, multiplayer, replays, deterministic checksums, desync reporting, command stream validation, deeper Quest profiling, accessibility, and release packaging.
