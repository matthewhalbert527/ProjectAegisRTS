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

## Stage 21.5

Complete after local validation. Windows player resolution and UI scaling fix. Adds runtime display defaults, minimum 1280x720 clamping, 1600x900 windowed defaults, Boot Options display controls, responsive CanvasScaler enforcement, Player.log display diagnostics, 1080p launch helper, build-script display configuration, and Stage 21.5 fast/medium/player-facing/full validation tiers while preserving Stage 0-21 behavior.

## Stage 22

Complete after local validation. Classic RTS command matrix pass. Adds deterministic attack-move, guard, patrol, scatter, and deploy-placeholder commands in `Rts.Core`; adds PCDesktop command buttons, double-click same-type selection, box selection, and client-local control groups; preserves QuestXR Stage 4/5 controls; and adds Stage 22 fast/medium/player-facing/full validation tiers while preserving Stage 0-21.5 behavior.

## Stage 23

Complete after local validation. Base management command pass. Adds deterministic building repair, sell/refund/removal, manual power toggle production pausing, rally point validation and spawned-unit routing; exposes repair/rally/power/sell snapshot state; adds PCDesktop command buttons and Quest left-hand compatible routing; and adds Stage 23 fast/medium/player-facing/full validation tiers while preserving Stage 0-22 behavior.

## Stage 24

Complete after local validation. Tech tree / prerequisites / support powers foundation. Adds deterministic production prerequisites, advanced unlocks, support-power definitions/state/commands/snapshots, real Reveal Scan and Emergency Repair Pulse powers, placeholder gated support powers, PCDesktop sidebar support buttons, production-card availability reasons, Quest left-hand support routing, and Stage 24 fast/medium/player-facing/full validation tiers while preserving Stage 0-23 behavior.

## Stage 25

Complete after local validation. Engineers / capture / repairs / transports foundation. Adds deterministic engineer building capture, engineer field repair, infantry transport load/unload, passenger snapshots, transport death passenger resolution, PCDesktop command buttons, Quest left-hand compatible routes, and Stage 25 fast/medium/player-facing/full validation tiers while preserving Stage 0-24 behavior.

## Stage 26

Complete after local validation. Airfield / aircraft / helipad / naval foundation. Adds deterministic aircraft metadata, dual-helipad pad state, aircraft docking/rearm/fuel placeholder snapshots, aircraft altitude state, water/naval passability, player vertical-slice air assets, Unity altitude visual integration, and Stage 26 fast/medium/player-facing/full validation tiers while preserving Stage 0-25 behavior.

## Stage 27

Complete after local validation. Skirmish playability / AI pressure / completion polish. Adds Easy/Normal/Hard deterministic AI profiles, timed attack-wave state, production target tuning, Hard building repair, reachable attack-wave staging, saved Boot Options difficulty selection, Stage16 difficulty restart support, a player-facing enemy-pressure HUD readout, non-debug victory validation, and Stage 27 fast/medium/player-facing/full validation tiers while preserving Stage 0-26 behavior.

## Stage 27.1

Complete after local validation. PC building placement UX fix. Separates board setup placement from production building placement in player-facing UI visibility, keeps `BoardPlacementHud` hidden during PCDesktop right-sidebar building placement, preserves explicit QuestXR/DebugHybrid board setup placement, makes Escape cancel active placement before opening pause, and adds Stage 27.1 fast/medium/player-facing/full validation tiers while preserving Stage 0-27 behavior.

## Stage 28

Complete after local validation. Integrated playtest stabilization and feature regression QA. Adds a hidden F10 feature-regression HUD, Stage 28 feature/play-mode validators, a documented feature matrix, known-issues notes, player-facing validation, and flat Stage 28 fast/medium/player-facing/full validation tiers while preserving Stage 0-27.1 behavior.

## Stage 28.1

Complete after local validation. Full acceptance gate flattening and PC safe-area layout fix. Removes recursive lower full-gate replay from Stage 28 final validation, adds a full-recursion audit, adds Stage 28.1 validation tiers, reserves the PCDesktop camera safe area around the right CnC/OpenRA sidebar and left objective stack, preserves QuestXR full-screen hand-control behavior, and makes deterministic fixed-step unit movement advance along diagonal path steps while preserving Stage 0-28 behavior.

## Stage 29

Complete after local validation. Realistic battlefield visual-quality pass. Adds terrain/environment material profiles, lighting/atmosphere, a Stage 29 battlefield visual review scene, screenshot capture, visual QA, and an additive detail/material pass for the nine MVP production proxies while preserving Stage 0-28.1 gameplay, PCDesktop sidebar/safe-area behavior, QuestXR hand controls, hidden debug defaults, and UnityEngine-free `Rts.Core`.

## Stage 30

Complete after local validation. Visual readability QA pass. Adds readability profile thresholds, additive proxy ground/identity/role overlays, a Stage 30 visual readability review scene, screenshot capture, and fast/medium/player-facing/full validation tiers while preserving Stage 0-29 gameplay, PCDesktop sidebar/safe-area behavior, QuestXR hand controls, hidden debug defaults, and UnityEngine-free `Rts.Core`.

## Stage 31

Complete after local validation. Artist handoff and package cleanup pass. Adds concise export/modeling briefs, material naming rules, trim-sheet guidance, LOD targets, Quest budgets, screenshot/reference package notes, updated MVP art replacement docs, per-actor production checklists, and Stage 31 validation tiers while preserving Stage 0-30 behavior and without importing final artist models.

## Stage 32

Complete after local validation. Terrain piece library / battlefield set dressing expansion. Adds 96 terrain/base/resource/obstacle/prop definitions, Unity-side terrain piece catalogs, Stage32 material profiles, Batch01 external terrain source-art ingestion, visual-only Stage16 source-art set dressing when real source assets exist, a Stage32 terrain review scene, screenshot capture, fast/medium/player-facing/full validation tiers, Player.log coverage, and docs while preserving Stage 0-31 behavior, PCDesktop sidebar/safe-area layout, QuestXR hand controls, Stage27.1 placement HUD separation, hidden debug defaults, and UnityEngine-free `Rts.Core`. The Stage32 overlay pass also keeps a separate generated terrain asset replacement kit, shared terrain materials, a `Stage32_TerrainAssetReplacementReview.unity` scene, and generator/validator tooling as debug/fallback support for future artist-authored terrain replacement.

## Stage 32.6

Complete after local validation. Terrain art integration correction pass. Rejects the prior Batch01 cropped texture/card output for player-facing runtime use, moves Batch01 sheets to reference-only storage, removes `Batch01Imported` runtime folders, generates Stage32.6 mesh/material terrain prefab assemblies and mapped player-facing wrappers, adds a Stage32.6 terrain review scene and screenshots, and adds fast/medium/player-facing/full validation tiers while preserving Stage 0-32 behavior, PCDesktop sidebar/safe-area layout, QuestXR hand controls, Stage27.1 placement HUD separation, hidden debug defaults, and UnityEngine-free `Rts.Core`.

## Stage 33

Complete after local validation. Tank source / proxy prefab integration pass. Installs the Stage33 tank source generator, creates structured light/medium/heavy tank production-source prefabs, adds project-compatible sockets, pivots, LODs, visual-only turret/barrel/track hooks, smoke/explosion/UI/selection anchors, a Stage33 tank review scene, ActorVisualDefinition production-prefab wiring with blockout fallback preservation, and documentation while preserving Stage 0-32 behavior, PCDesktop sidebar/safe-area layout, QuestXR hand controls, Stage27.1 placement HUD separation, hidden debug defaults, medium/full validation flattening, and UnityEngine-free `Rts.Core`.

## Later Stages

Next later stages include replacing Stage33 source proxies with artist-authored tank models, replacing generated terrain pieces with authored environment modules, multiplayer, replays, deterministic checksums, desync reporting, command stream validation, deeper Quest profiling, accessibility, and release packaging.
