# Validation Tiers

Stage 8.1 adds validation tiers so normal development does not need to replay the slowest full acceptance chain after every small art, prefab, script, or documentation edit. Stage 9 follows the same model for combat iteration. Stage 10 follows it for economy iteration. Stage 11 follows it for fog/radar/minimap iteration. Stage 12 follows it for AI iteration. Stage 13 follows it for map/terrain/pathing iteration. Stage 14 follows it for feedback iteration. Stage 15 follows it for performance/build-readiness iteration. Stage 16 follows it for playable vertical-slice iteration. Stage 17 follows it for player-facing polish. Stage 18 follows it for tester-guided playability. Stage 18.5 follows it for fine placement grid iteration. Stage 19 follows it for mission flow and fine-grid playability tuning. Stage 19.5 follows it for the PC sidebar and pause menu rework. Stage 20 follows it for MVP proxy visuals. Stage 21 follows it for MVP visual QA. Stage 21.5 follows it for Windows player resolution/UI scaling. Stage 22 follows it for classic RTS command controls. Stage 23 follows it for base management commands. Stage 24 follows it for tech prerequisites and support powers. Stage 25 follows it for engineer capture/repair and transports. Stage 26 follows it for airfield, aircraft, and naval passability foundations. Stage 27 follows it for skirmish playability, AI pressure, and difficulty controls. Stage 27.1 follows it for the PC building placement overlay fix. Stage 28 follows it for integrated feature-regression QA and playtest stabilization. Stage 28.1 follows it for full-gate flattening, PCDesktop safe-area layout, and diagonal fixed-step movement. Stage 29 follows it for realistic battlefield visual-quality iteration. Stage 30 follows it for visual readability QA. Stage 31 follows it for artist handoff/package cleanup. Stage 32 follows it for terrain-piece library and set-dressing iteration. Stage 15.1 flattens the Stage 9-through-Stage 32 medium tiers so they validate direct dependencies instead of recursively calling prior medium checks. A follow-up hardening pass added `tools\audit-medium-validation-recursion.ps1` after runtime output showed recursive medium sections were still possible to miss. Stage 28.1 adds `tools\audit-full-validation-recursion.ps1` so current full gates avoid recursively replaying older full gates, and Stage 29/30/31/32 extend that audit for visual-quality, handoff, and terrain set-dressing gates. The full gate remains required for final acceptance; the faster tiers choose the right amount of evidence during iteration.

Stage 33 keeps the same validation discipline for tank source/proxy prefab generation. Its targeted generator validates the current tank source assets directly, while broad pre-commit confidence still comes from the highest available flat medium gate.

## Tier Summary

| Tier | Command | Use When | Scope |
| --- | --- | --- | --- |
| Fast | `.\tools\run-stage8-fast-checks.ps1` | You changed current Stage 8 art pipeline, generated prefabs, sockets, icons, scene wiring, or related scripts. | Builds/copies `Rts.Core` for Unity, runs Stage 8 generation and validation only, runs Stage 8 Play Mode smoke when batchmode can own the project lock, checks `Rts.Core` for `UnityEngine`, and runs `git diff --check`. |
| Medium | `.\tools\run-stage8-medium-checks.ps1` | You are preparing a local commit and want current stage plus immediate dependency confidence. | Runs `Rts.Core` tests, builds/copies the Unity DLL, runs direct Stage 7 Unity validation as the immediate dependency, then Stage 8 validation and Play Mode smoke/fallback, the `Rts.Core` UnityEngine-free scan, and `git diff --check`. |
| Full | `.\tools\run-stage8-checks.ps1` | You need final Stage 8 acceptance evidence. | Runs Stage 0 through Stage 8, including the existing full validation chain and Stage 8 Play Mode smoke/fallback. This is intentionally slow. |
| Fast | `.\tools\run-stage9-fast-checks.ps1` | You changed current Stage 9 combat presentation, scene wiring, profiles, or smoke tooling. | Builds/copies `Rts.Core` for Unity, runs Stage 9 generation/validation only, runs Stage 9 Play Mode smoke when batchmode can own the project lock, checks `Rts.Core` for `UnityEngine`, and runs `git diff --check`. |
| Medium | `.\tools\run-stage9-medium-checks.ps1` | You are preparing a local Stage 9 commit. | Runs `Rts.Core` tests, builds/copies the Unity DLL, runs Stage 8 immediate dependency validation, then Stage 9 validation and Play Mode smoke/fallback, the `Rts.Core` UnityEngine-free scan, and `git diff --check`. |
| Full | `.\tools\run-stage9-checks.ps1` | You need final Stage 9 acceptance evidence. | Runs Stage 0 through Stage 9 through the flattened full-chain runner, including each stage's Unity validation and Stage 9 Play Mode smoke/fallback. This is intentionally slow, but avoids recursive replay. |
| Fast | `.\tools\run-stage10-fast-checks.ps1` | You changed current Stage 10 economy presentation, scene wiring, harvest smoke tooling, or economy debug HUD. | Builds/copies `Rts.Core` for Unity, runs Stage 10 generation/validation only, runs Stage 10 Play Mode smoke when batchmode can own the project lock, checks `Rts.Core` for `UnityEngine`, and runs `git diff --check`. |
| Medium | `.\tools\run-stage10-medium-checks.ps1` | You are preparing a local Stage 10 commit. | Runs `Rts.Core` tests, builds/copies the Unity DLL, runs Stage 9 immediate dependency validation, then Stage 10 validation and Play Mode smoke/fallback, the `Rts.Core` UnityEngine-free scan, and `git diff --check`. |
| Full | `.\tools\run-stage10-checks.ps1` | You need final Stage 10 acceptance evidence. | Runs Stage 0 through Stage 10 through the flattened full-chain runner, including each stage's Unity validation and Stage 10 Play Mode smoke/fallback. This is intentionally slow, but avoids recursive replay. |
| Fast | `.\tools\run-stage11-fast-checks.ps1` | You changed current Stage 11 fog/radar/minimap presentation, scene wiring, or smoke tooling. | Builds/copies `Rts.Core` for Unity, runs Stage 11 generation/validation only, runs Stage 11 Play Mode smoke when batchmode can own the project lock, checks `Rts.Core` for `UnityEngine`, and runs `git diff --check`. |
| Medium | `.\tools\run-stage11-medium-checks.ps1` | You are preparing a local Stage 11 commit. | Runs `Rts.Core` tests, builds/copies the Unity DLL, runs Stage 10 immediate dependency validation, then Stage 11 validation and Play Mode smoke/fallback, the `Rts.Core` UnityEngine-free scan, and `git diff --check`. |
| Full | `.\tools\run-stage11-checks.ps1` | You need final Stage 11 acceptance evidence. | Runs Stage 0 through Stage 11 through the flattened full-chain runner, including each stage's Unity validation and Stage 11 Play Mode smoke/fallback. This is intentionally slow, but avoids recursive replay. |
| Fast | `.\tools\run-stage12-fast-checks.ps1` | You changed current Stage 12 AI core, scene wiring, debug HUD, or smoke tooling. | Builds/copies `Rts.Core` for Unity, runs Stage 12 generation/validation only, runs Stage 12 Play Mode smoke when batchmode can own the project lock, checks `Rts.Core` for `UnityEngine`, and runs `git diff --check`. |
| Medium | `.\tools\run-stage12-medium-checks.ps1` | You are preparing a local Stage 12 commit. | Runs `Rts.Core` tests, builds/copies the Unity DLL, runs Stage 11 immediate dependency validation, then Stage 12 validation and Play Mode smoke/fallback, the `Rts.Core` UnityEngine-free scan, and `git diff --check`. |
| Full | `.\tools\run-stage12-checks.ps1` | You need final Stage 12 acceptance evidence. | Runs Stage 0 through Stage 12 through the flattened full-chain runner, including each stage's Unity validation and Stage 12 Play Mode smoke/fallback. This is intentionally slow, but avoids recursive replay. |
| Fast | `.\tools\run-stage13-fast-checks.ps1` | You changed current Stage 13 map, terrain, pathing, scene wiring, debug HUD, or smoke tooling. | Builds/copies `Rts.Core` for Unity, runs Stage 13 generation/validation only, runs Stage 13 Play Mode smoke when batchmode can own the project lock, checks `Rts.Core` for `UnityEngine`, and runs `git diff --check`. |
| Medium | `.\tools\run-stage13-medium-checks.ps1` | You are preparing a local Stage 13 commit. | Runs `Rts.Core` tests, builds/copies the Unity DLL, runs Stage 12 immediate dependency validation, then Stage 13 validation and Play Mode smoke/fallback, the `Rts.Core` UnityEngine-free scan, and `git diff --check`. |
| Full | `.\tools\run-stage13-checks.ps1` | You need final Stage 13 acceptance evidence. | Runs Stage 0 through Stage 13 through the flattened full-chain runner, including each stage's Unity validation and Stage 13 Play Mode smoke/fallback. This is intentionally slow, but avoids recursive replay. |
| Fast | `.\tools\run-stage14-fast-checks.ps1` | You changed current Stage 14 feedback profiles, event bus, controllers, scene wiring, or smoke tooling. | Builds/copies `Rts.Core` for Unity, runs Stage 14 profile generation and validation only, runs Stage 14 Play Mode smoke when batchmode can own the project lock, checks `Rts.Core` for `UnityEngine`, and runs `git diff --check`. |
| Medium | `.\tools\run-stage14-medium-checks.ps1` | You are preparing a local Stage 14 commit. | Runs `Rts.Core` tests, builds/copies the Unity DLL, runs Stage 13 immediate dependency validation, then Stage 14 validation and Play Mode smoke/fallback, the `Rts.Core` UnityEngine-free scan, and `git diff --check`. |
| Full | `.\tools\run-stage14-checks.ps1` | You need final Stage 14 acceptance evidence. | Runs Stage 0 through Stage 14 through the flattened full-chain runner, including each stage's Unity validation and Stage 14 Play Mode smoke/fallback. This is intentionally slow, but avoids recursive replay. |
| Fast | `.\tools\run-stage15-fast-checks.ps1` | You changed current Stage 15 performance budgets, pooling, render stats, readiness reporters, scene wiring, or smoke tooling. | Builds/copies `Rts.Core` for Unity, runs Stage 15 profile generation and validation only, runs Stage 15 Play Mode smoke/build-readiness audit when batchmode can own the project lock, checks `Rts.Core` for `UnityEngine`, and runs `git diff --check`. |
| Medium | `.\tools\run-stage15-medium-checks.ps1` | You are preparing a local Stage 15 commit. | Runs `Rts.Core` tests, builds/copies the Unity DLL, runs Stage 14 immediate dependency validation, then Stage 15 validation and Play Mode smoke/fallback, the `Rts.Core` UnityEngine-free scan, and `git diff --check`. |
| Full | `.\tools\run-stage15-checks.ps1` | You need final Stage 15 acceptance evidence. | Runs Stage 0 through Stage 15 through the flattened full-chain runner, including each stage's Unity validation and Stage 15 Play Mode smoke/fallback. This is intentionally slow, but avoids recursive replay. |
| Fast | `.\tools\run-stage16-fast-checks.ps1` | You changed current Stage 16 match flow, scenario scene wiring, integrated HUDs, or smoke tooling. | Runs `Rts.Core` tests, builds/copies the Unity DLL, runs Stage 16 scene validation and Play Mode smoke/fallback, checks `Rts.Core` for `UnityEngine`, and runs `git diff --check`. |
| Medium | `.\tools\run-stage16-medium-checks.ps1` | You are preparing a local Stage 16 commit. | Runs `Rts.Core` tests, builds/copies the Unity DLL, runs direct Stage 15 Unity validation, then Stage 16 validation and Play Mode smoke/fallback, the medium recursion audit, the `Rts.Core` UnityEngine-free scan, and `git diff --check`. |
| Full | `.\tools\run-stage16-checks.ps1` | You need final Stage 16 acceptance evidence. | Runs Stage 0 through Stage 16 through the flattened full-chain runner, including each stage's Unity validation and Stage 16 Play Mode smoke/fallback. This is intentionally slow, but avoids recursive replay. |
| Fast | `.\tools\run-stage17-fast-checks.ps1` | You changed current Stage 17 boot/menu/HUD/result UI, player-facing validators, or docs. | Runs `Rts.Core` tests, builds/copies the Unity DLL, runs Stage 17 validation and Play Mode smoke, runs the medium recursion audit, checks `Rts.Core` for `UnityEngine`, and runs `git diff --check`. |
| Medium | `.\tools\run-stage17-medium-checks.ps1` | You are preparing a local Stage 17 commit. | Runs `Rts.Core` tests, builds/copies the Unity DLL, runs direct Stage 16.5 build-flow validation, runs Stage 17 validation and smoke, runs player-facing checks with the player build skipped, audits medium recursion, checks `Rts.Core` for `UnityEngine`, and runs `git diff --check`. |
| Full | `.\tools\run-stage17-checks.ps1` | You need final Stage 17 acceptance evidence. | Runs Stage 0 through Stage 17 through the flattened full-chain runner, then runs the Stage 17 player-facing build checks. This is intentionally slow. |
| Fast | `.\tools\run-stage18-fast-checks.ps1` | You changed current Stage 18 checklist, prompt, sidebar readability, EXE presentation defaults, validators, or docs. | Runs `Rts.Core` tests, builds/copies the Unity DLL, runs Stage 18 validation and Play Mode smoke, audits medium recursion, checks `Rts.Core` for `UnityEngine`, and runs `git diff --check`. |
| Medium | `.\tools\run-stage18-medium-checks.ps1` | You are preparing a local Stage 18 commit. | Runs `Rts.Core` tests, builds/copies the Unity DLL, runs direct Stage 17 validation/player-facing dependencies, runs Stage 18 validation and player-facing checks with the player build skipped, audits medium recursion, checks `Rts.Core` for `UnityEngine`, and runs `git diff --check`. |
| Full | `.\tools\run-stage18-checks.ps1` | You need final Stage 18 acceptance evidence. | Runs Stage 0 through Stage 18 through the flattened full-chain runner, then runs the Stage 18 player-facing build checks. This is intentionally slow. |
| Fast | `.\tools\run-stage18-5-fast-checks.ps1` | You changed current Stage 18.5 fine placement, grid rendering, placement preview, input mapping, validators, or docs. | Runs `Rts.Core` tests, builds/copies the Unity DLL, runs Stage 18.5 Unity validation and Play Mode smoke, audits medium recursion, checks `Rts.Core` for `UnityEngine`, and runs `git diff --check`. |
| Medium | `.\tools\run-stage18-5-medium-checks.ps1` | You are preparing a local Stage 18.5 commit. | Runs `Rts.Core` tests, builds/copies the Unity DLL, runs direct Stage 18 Unity/player-facing validation dependencies, runs Stage 18.5 validation and player-facing checks with the player build skipped, audits medium recursion, checks `Rts.Core` for `UnityEngine`, and runs `git diff --check`. |
| Full | `.\tools\run-stage18-5-checks.ps1` | You need final Stage 18.5 acceptance evidence. | Runs the slow full Stage 0-through-Stage 18.5 gate, including Stage 18 player-facing coverage, Stage 18.5 validation, medium recursion audit, the UnityEngine-free scan, and whitespace checks. |
| Fast | `.\tools\run-stage19-fast-checks.ps1` | You changed current Stage 19 mission flow, checklist/prompt/sidebar guidance, fine-grid readability, pacing, validators, or docs. | Runs `Rts.Core` tests, builds/copies the Unity DLL, runs Stage 19 Unity validation and Play Mode smoke, audits medium recursion, checks `Rts.Core` for `UnityEngine`, and runs `git diff --check`. |
| Medium | `.\tools\run-stage19-medium-checks.ps1` | You are preparing a local Stage 19 commit. | Runs `Rts.Core` tests, builds/copies the Unity DLL, runs direct Stage 18.5 Unity/player-facing validation dependencies, runs Stage 19 validation and player-facing checks with the player build skipped, audits medium recursion, checks `Rts.Core` for `UnityEngine`, and runs `git diff --check`. |
| Full | `.\tools\run-stage19-checks.ps1` | You need final Stage 19 acceptance evidence. | Runs the slow full Stage 0-through-Stage 19 gate through Stage 18.5 dependencies, Stage 19 player-facing validation, medium recursion audit, the UnityEngine-free scan, and whitespace checks. |
| Fast | `.\tools\run-stage19-5-fast-checks.ps1` | You changed current Stage 19.5 right sidebar, pause menu, PC/XR UI mode defaults, validators, or docs. | Runs `Rts.Core` tests, builds/copies the Unity DLL, runs Stage 19.5 Unity validation and Play Mode smoke, audits medium recursion, checks `Rts.Core` for `UnityEngine`, and runs `git diff --check`. |
| Medium | `.\tools\run-stage19-5-medium-checks.ps1` | You are preparing a local Stage 19.5 commit. | Runs `Rts.Core` tests, builds/copies the Unity DLL, runs direct Stage 19 Unity/player-facing validation dependencies, runs Stage 19.5 validation and player-facing checks with the player build skipped, audits medium recursion, checks `Rts.Core` for `UnityEngine`, and runs `git diff --check`. |
| Full | `.\tools\run-stage19-5-checks.ps1` | You need final Stage 19.5 acceptance evidence. | Runs the slow full Stage 0-through-Stage 19.5 gate through Stage 19 dependencies, Stage 19.5 player-facing validation, medium recursion audit, the UnityEngine-free scan, and whitespace checks. |
| Fast | `.\tools\run-stage20-fast-checks.ps1` | You changed Stage 20 MVP proxy visuals, standards, generated prefabs, scene wiring, or platform UI checks. | Runs `Rts.Core` tests/build, Stage 20 Unity validation, Stage 4/5 preservation checks, medium recursion audit, UnityEngine-free scan, and `git diff --check`. |
| Medium | `.\tools\run-stage20-medium-checks.ps1` | You are preparing a local Stage 20 commit. | Runs direct Stage 19.5 Unity/player-facing validation dependencies, Stage 20 validation and player-facing checks, Stage 4/5 preservation checks, medium recursion audit, UnityEngine-free scan, and `git diff --check`. |
| Full | `.\tools\run-stage20-checks.ps1` | You need final Stage 20 acceptance evidence. | Runs the slow full Stage 0-through-Stage 20 gate, player-facing validation, platform UI preservation checks, UnityEngine-free scan, and whitespace checks. |
| Fast | `.\tools\run-stage21-fast-checks.ps1` | You changed Stage 21 MVP visual QA, socket/pivot validation, replacement metadata, import scan behavior, or review scene wiring. | Runs `Rts.Core` tests/build, Stage 21 Unity validation, Stage 4/5 preservation checks, medium recursion audit, UnityEngine-free scan, and `git diff --check`. |
| Medium | `.\tools\run-stage21-medium-checks.ps1` | You are preparing a local Stage 21 commit. | Runs direct Stage 20 Unity/player-facing validation dependencies, Stage 21 validation and player-facing checks, Stage 4/5 preservation checks, medium recursion audit, UnityEngine-free scan, and `git diff --check`. |
| Full | `.\tools\run-stage21-checks.ps1` | You need final Stage 21 acceptance evidence. | Runs the slow full Stage 0-through-Stage 21 gate, player-facing validation, MVP visual QA, UnityEngine-free scan, and whitespace checks. |
| Fast | `.\tools\run-stage21-5-fast-checks.ps1` | You changed Windows display defaults, options UI, CanvasScaler enforcement, build script display settings, or Player.log diagnostics. | Runs `Rts.Core` tests/build, Stage 21.5 Unity validation and smoke, medium recursion audit, UnityEngine-free scan, and `git diff --check`. |
| Medium | `.\tools\run-stage21-5-medium-checks.ps1` | You are preparing a local Stage 21.5 commit. | Runs direct Stage 21 Unity/player-facing validation, Stage 4/5 preservation checks, Stage 21.5 validation/player-facing checks with player build skipped, medium recursion audit, UnityEngine-free scan, and `git diff --check`. |
| Full | `.\tools\run-stage21-5-checks.ps1` | You need final Stage 21.5 acceptance evidence. | Runs Stage 21 full acceptance, Stage 21.5 fast validation, Stage 21.5 player-facing build/log validation, UnityEngine-free scan, and whitespace checks. |
| Fast | `.\tools\run-stage22-fast-checks.ps1` | You changed classic RTS command controls, PC input selection, control groups, or command-bar layout. | Runs `Rts.Core` tests/build, Stage 22 Unity validation and smoke, medium recursion audit, UnityEngine-free scan, and `git diff --check`. |
| Medium | `.\tools\run-stage22-medium-checks.ps1` | You are preparing a local Stage 22 commit. | Runs direct Stage 21.5 Unity/player-facing validation, Stage 4/5 preservation checks, Stage 22 validation/player-facing checks with player build skipped, medium recursion audit, UnityEngine-free scan, and `git diff --check`. |
| Full | `.\tools\run-stage22-checks.ps1` | You need final Stage 22 acceptance evidence. | Runs Stage 21.5 full acceptance, Stage 22 fast validation, Stage 22 player-facing build/log validation, UnityEngine-free scan, and whitespace checks. |
| Fast | `.\tools\run-stage23-fast-checks.ps1` | You changed base management commands, PC command routing, repair/sell/power/rally snapshots, or command-bar layout. | Runs `Rts.Core` tests/build, Stage 23 Unity validation and smoke, medium recursion audit, UnityEngine-free scan, and `git diff --check`. |
| Medium | `.\tools\run-stage23-medium-checks.ps1` | You are preparing a local Stage 23 commit. | Runs direct Stage 22 Unity/player-facing validation, Stage 4/5 preservation checks, Stage 23 validation/player-facing checks with player build skipped, medium recursion audit, UnityEngine-free scan, and `git diff --check`. |
| Full | `.\tools\run-stage23-checks.ps1` | You need final Stage 23 acceptance evidence. | Runs Stage 22 full acceptance, Stage 23 fast validation, Stage 23 player-facing build/log validation, UnityEngine-free scan, and whitespace checks. |
| Fast | `.\tools\run-stage24-fast-checks.ps1` | You changed tech prerequisites, support-power definitions, support snapshots, or sidebar support buttons. | Runs `Rts.Core` tests/build, Stage 24 Unity validation and smoke, medium recursion audit, UnityEngine-free scan, and `git diff --check`. |
| Medium | `.\tools\run-stage24-medium-checks.ps1` | You are preparing a local Stage 24 commit. | Runs direct Stage 23 Unity/player-facing validation, Stage 4/5 preservation checks, Stage 24 validation/player-facing checks with player build skipped, medium recursion audit, UnityEngine-free scan, and `git diff --check`. |
| Full | `.\tools\run-stage24-checks.ps1` | You need final Stage 24 acceptance evidence. | Runs Stage 23 full acceptance, Stage 24 fast validation, Stage 24 player-facing build/log validation, UnityEngine-free scan, and whitespace checks. |
| Fast | `.\tools\run-stage25-fast-checks.ps1` | You changed engineer capture/repair, transport load/unload, passenger snapshots, or command-bar routing. | Runs `Rts.Core` tests/build, Stage 25 Unity validation and smoke, medium recursion audit, UnityEngine-free scan, and `git diff --check`. |
| Medium | `.\tools\run-stage25-medium-checks.ps1` | You are preparing a local Stage 25 commit. | Runs direct Stage 24 Unity/player-facing validation, Stage 4/5 preservation checks, Stage 25 validation/player-facing checks with player build skipped, medium recursion audit, UnityEngine-free scan, and `git diff --check`. |
| Full | `.\tools\run-stage25-checks.ps1` | You need final Stage 25 acceptance evidence. | Runs Stage 24 full acceptance, Stage 25 fast validation, Stage 25 player-facing build/log validation, UnityEngine-free scan, and whitespace checks. |
| Fast | `.\tools\run-stage26-fast-checks.ps1` | You changed helipad pads, aircraft metadata, aircraft docking/altitude, water/naval passability, or Stage 16 air assets. | Runs `Rts.Core` tests/build, Stage 26 Unity validation and smoke, medium recursion audit, UnityEngine-free scan, and `git diff --check`. |
| Medium | `.\tools\run-stage26-medium-checks.ps1` | You are preparing a local Stage 26 commit. | Runs direct Stage 25 Unity/player-facing validation, Stage 4/5 preservation checks, Stage 26 validation/player-facing checks with player build skipped, medium recursion audit, UnityEngine-free scan, and `git diff --check`. |
| Full | `.\tools\run-stage26-checks.ps1` | You need final Stage 26 acceptance evidence. | Runs Stage 25 full acceptance, Stage 26 fast validation, Stage 26 player-facing build/log validation, UnityEngine-free scan, and whitespace checks. |
| Fast | `.\tools\run-stage27-fast-checks.ps1` | You changed AI difficulty, attack-wave timing, AI production pressure, restart controls, or skirmish HUD status. | Runs `Rts.Core` tests/build, Stage 27 Unity validation and smoke, medium recursion audit, UnityEngine-free scan, and `git diff --check`. |
| Medium | `.\tools\run-stage27-medium-checks.ps1` | You are preparing a local Stage 27 commit. | Runs direct Stage 26 Unity/player-facing validation, Stage 4/5 preservation checks, Stage 27 validation/player-facing checks with player build skipped, medium recursion audit, UnityEngine-free scan, and `git diff --check`. |
| Full | `.\tools\run-stage27-checks.ps1` | You need final Stage 27 acceptance evidence. | Runs Stage 26 full acceptance, Stage 27 fast validation, Stage 27 player-facing build/log validation, UnityEngine-free scan, and whitespace checks. |
| Fast | `.\tools\run-stage27-1-fast-checks.ps1` | You changed PCDesktop building placement UI, BoardPlacementHud visibility, sidebar placement text, or Esc/cancel placement behavior. | Runs `Rts.Core` tests/build, Stage 27.1 Unity validation and smoke, medium recursion audit, UnityEngine-free scan, and `git diff --check`. |
| Medium | `.\tools\run-stage27-1-medium-checks.ps1` | You are preparing a local Stage 27.1 commit. | Runs direct Stage 27 player-facing validation, direct Stage 4/5 hand-control validation, Stage 27.1 validation/player-facing checks with player build skipped, medium recursion audit, UnityEngine-free scan, and `git diff --check`. |
| Full | `.\tools\run-stage27-1-checks.ps1` | You need final Stage 27.1 acceptance evidence. | Runs Stage 27 full acceptance, Stage 27.1 fast validation, Stage 27.1 player-facing build/log validation, Quest hand-control preservation, UnityEngine-free scan, and whitespace checks. |
| Fast | `.\tools\run-stage28-fast-checks.ps1` | You changed the Stage 28 feature matrix, hidden QA overlay, feature-route validation, Stage27.1 placement coverage, or docs/tooling. | Runs `Rts.Core` tests/build, Stage 28 Unity validation and smoke, medium recursion audit, UnityEngine-free scan, and `git diff --check`. |
| Medium | `.\tools\run-stage28-medium-checks.ps1` | You are preparing a local Stage 28 commit. | Runs direct Stage 27.1 Unity/player-facing validation, direct Stage 4/5 hand-control validation, Stage 28 validation/player-facing checks with player build skipped, medium recursion audit, UnityEngine-free scan, and `git diff --check`. |
| Full | `.\tools\run-stage28-checks.ps1` | You need final Stage 28 acceptance evidence. | Runs Stage 27.1 full acceptance, Stage 28 fast validation, Stage 28 player-facing build/log validation, Quest hand-control preservation, UnityEngine-free scan, and whitespace checks. |
| Fast | `.\tools\run-stage28-1-fast-checks.ps1` | You changed PCDesktop safe-area layout, placement smoke tooling, diagonal movement, or docs/tooling. | Runs `Rts.Core` tests/build, Stage 28.1 Unity validation and smoke, medium/full recursion audits, UnityEngine-free scan, and `git diff --check`. |
| Medium | `.\tools\run-stage28-1-medium-checks.ps1` | You are preparing a local Stage 28.1 commit. | Runs direct Stage 28, Stage 27.1, and Stage 4/5 validation dependencies, Stage 28.1 validation/player-facing checks with player build skipped, recursion audits, UnityEngine-free scan, and `git diff --check`. |
| Full | `.\tools\run-stage28-1-checks.ps1` | You need final Stage 28.1 acceptance evidence. | Runs the flattened Stage 28 full gate, then Stage 28.1 validation/player-facing build/log coverage, UnityEngine-free scan, and whitespace checks. |
| Fast | `.\tools\run-stage29-fast-checks.ps1` | You changed realistic battlefield materials, terrain profiles, lighting, MVP proxy detail, the Stage 29 review scene, or visual QA tooling. | Runs `Rts.Core` tests/build, Stage 29 Unity visual validation/smoke/screenshot, medium/full recursion audits, UnityEngine-free scan, and `git diff --check`. |
| Medium | `.\tools\run-stage29-medium-checks.ps1` | You are preparing a local Stage 29 visual-quality commit. | Runs direct Stage 28, Stage 28.1, Stage 4/5 validation dependencies, Stage 29 visual validation, Stage 29 player-facing checks with player build/log skipped, recursion audits, UnityEngine-free scan, and `git diff --check`. |
| Full | `.\tools\run-stage29-checks.ps1` | You need final Stage 29 acceptance evidence. | Runs the flattened Stage 28.1 full gate, Stage 29 fast visual validation, Stage 29 player-facing build/log validation, UnityEngine-free scan, and whitespace checks. |
| Fast | `.\tools\run-stage30-fast-checks.ps1` | You changed readability profile thresholds, proxy overlays, review-scene camera composition, contrast QA, or screenshot tooling. | Runs `Rts.Core` tests/build, Stage 30 Unity readability validation/smoke/screenshot, medium/full recursion audits, UnityEngine-free scan, and `git diff --check`. |
| Medium | `.\tools\run-stage30-medium-checks.ps1` | You are preparing a local Stage 30 visual-readability commit. | Runs direct Stage 29, Stage 28.1, Stage 4/5 validation dependencies, Stage 30 readability validation, Stage 30 player-facing checks with player build/log skipped, recursion audits, UnityEngine-free scan, and `git diff --check`. |
| Full | `.\tools\run-stage30-checks.ps1` | You need final Stage 30 acceptance evidence. | Runs the flattened Stage 29 full gate, Stage 30 readability validation, Stage 30 player-facing build/log validation, UnityEngine-free scan, and whitespace checks. |
| Fast | `.\tools\run-stage31-fast-checks.ps1` | You changed artist handoff docs, replacement guidance, budgets, screenshot/reference packaging, or Stage 31 tooling. | Runs `Rts.Core` tests/build, Stage 30 visual reference validation, Stage 31 handoff validation, medium/full recursion audits, UnityEngine-free scan, and `git diff --check`. |
| Medium | `.\tools\run-stage31-medium-checks.ps1` | You are preparing a local Stage 31 handoff/package commit. | Runs direct Stage 30 Unity validation, Stage 30 player-facing preservation with player build/log skipped, Stage 31 handoff validation, recursion audits, UnityEngine-free scan, and `git diff --check`. |
| Full | `.\tools\run-stage31-checks.ps1` | You need final Stage 31 acceptance evidence. | Runs the flattened Stage 30 full gate, Stage 31 handoff validation, Stage 31 player-facing build/log validation, UnityEngine-free scan, and whitespace checks. |

## Medium Flattening Rule

Medium checks must not call earlier `run-stage*-medium-checks.ps1` scripts. A medium check runs `Rts.Core` tests once, builds/copies `Rts.Core` once, calls the immediate prior stage's Unity validation script directly with `-SkipCoreBuild`, calls the current stage's Unity validation script with `-SkipCoreBuild`, then runs the UnityEngine-free scan and `git diff --check`.

For example, `run-stage16-medium-checks.ps1` calls `run-unity-stage15-validation.ps1 -SkipCoreBuild` and `run-unity-stage16-validation.ps1 -SkipCoreBuild`. It must not call `run-stage15-medium-checks.ps1`. Stage 17 is a special case because Stage 16.5 is a build-flow layer rather than a numbered Unity validation script: `run-stage17-medium-checks.ps1` calls the Stage 16.5 build-flow validator directly, then calls `run-unity-stage17-validation.ps1 -SkipCoreBuild`. Stage 18 calls direct Stage 17 validators and player-facing checks instead of calling `run-stage17-medium-checks.ps1`.

The machine-enforced guardrail is:

```powershell
.\tools\audit-medium-validation-recursion.ps1
```

This audit scans Stage 9 through Stage 31 medium scripts and fails if a prior medium dependency or old "medium validation as the immediate dependency" wording returns. Stage 13 and later medium scripts run the audit before tests, and the Stage 28/29/30/31 full gates run it before their acceptance chain.

Full checks are different: they remain the final Stage 0-through-current-stage acceptance gates and must not be weakened to match medium scope.

## Stage 8 Examples

After touching a Stage 8 blockout generator, socket validator, art definition, icon generator, or the Stage 8 showcase scene, use:

```powershell
.\tools\run-stage8-fast-checks.ps1
```

Before committing Stage 8 tooling or Unity presentation changes locally, use:

```powershell
.\tools\run-stage8-medium-checks.ps1
```

Before declaring Stage 8 accepted or before using Stage 8 as the base for a later stage, use:

```powershell
.\tools\run-stage8-checks.ps1
```

## Stage 9 Examples

After touching Stage 9 combat visuals, attack routing, profile assets, scene wiring, or smoke validation, use:

```powershell
.\tools\run-stage9-fast-checks.ps1
```

Before committing Stage 9 combat or Unity presentation changes locally, use:

```powershell
.\tools\run-stage9-medium-checks.ps1
```

Before declaring Stage 9 accepted or using Stage 9 as the base for a later stage, use:

```powershell
.\tools\run-stage9-checks.ps1
```

## Stage 10 Examples

After touching Stage 10 economy visuals, harvest routing, scene wiring, or smoke validation, use:

```powershell
.\tools\run-stage10-fast-checks.ps1
```

Before committing Stage 10 economy or Unity presentation changes locally, use:

```powershell
.\tools\run-stage10-medium-checks.ps1
```

Before declaring Stage 10 accepted or using Stage 10 as the base for a later stage, use:

```powershell
.\tools\run-stage10-checks.ps1
```

## Stage 11 Examples

After touching Stage 11 fog, radar, minimap, scene wiring, or smoke validation, use:

```powershell
.\tools\run-stage11-fast-checks.ps1
```

Before committing Stage 11 visibility or Unity presentation changes locally, use:

```powershell
.\tools\run-stage11-medium-checks.ps1
```

Before declaring Stage 11 accepted or using Stage 11 as the base for a later stage, use:

```powershell
.\tools\run-stage11-checks.ps1
```

## Stage 12 Examples

After touching Stage 12 AI core, Unity AI debug presentation, scene wiring, or smoke validation, use:

```powershell
.\tools\run-stage12-fast-checks.ps1
```

Before committing Stage 12 AI or Unity presentation changes locally, use:

```powershell
.\tools\run-stage12-medium-checks.ps1
```

Before declaring Stage 12 accepted or using Stage 12 as the base for a later stage, use:

```powershell
.\tools\run-stage12-checks.ps1
```

## Stage 13 Examples

After touching Stage 13 map/terrain/pathing core, Unity terrain/path debug presentation, scene wiring, or smoke validation, use:

```powershell
.\tools\run-stage13-fast-checks.ps1
```

Before committing Stage 13 map/pathing or Unity presentation changes locally, use:

```powershell
.\tools\run-stage13-medium-checks.ps1
```

Before declaring Stage 13 accepted or using Stage 13 as the base for a later stage, use:

```powershell
.\tools\run-stage13-checks.ps1
```

## Stage 14 Examples

After touching Stage 14 feedback profiles, event bus hooks, placeholder audio/VFX/UI/haptic controllers, scene wiring, or smoke validation, use:

```powershell
.\tools\run-stage14-fast-checks.ps1
```

Before committing Stage 14 feedback or Unity presentation changes locally, use:

```powershell
.\tools\run-stage14-medium-checks.ps1
```

Before declaring Stage 14 accepted or using Stage 14 as the base for a later stage, use:

```powershell
.\tools\run-stage14-checks.ps1
```

## Stage 15 Examples

After touching Stage 15 performance budgets, pooling, render stats, readiness reporters, scene wiring, or smoke validation, use:

```powershell
.\tools\run-stage15-fast-checks.ps1
```

Before committing Stage 15 performance/build-readiness changes locally, use:

```powershell
.\tools\run-stage15-medium-checks.ps1
```

Before declaring Stage 15 accepted or using Stage 15 as the base for a later stage, use:

```powershell
.\tools\run-stage15-checks.ps1
```

## Stage 16 Examples

After touching Stage 16 match flow, vertical-slice world setup, scene wiring, HUDs, or smoke validation, use:

```powershell
.\tools\run-stage16-fast-checks.ps1
```

Before committing Stage 16 vertical-slice changes locally, use:

```powershell
.\tools\run-stage16-medium-checks.ps1
```

Before declaring Stage 16 accepted or using Stage 16 as the base for a later stage, use:

```powershell
.\tools\run-stage16-checks.ps1
```

## Stage 16.5 Player Build Flow

After touching the player-facing boot scene, Stage 16 default presentation, debug-panel visibility, or Windows player build scripts, use:

```powershell
.\tools\run-stage16-player-build-checks.ps1 -SkipPlayerBuild
```

Before handing someone an executable, use:

```powershell
.\tools\build-windows-player-stage16.ps1
```

The player-build check keeps `run-stage16-checks.ps1` as the full Stage 16 acceptance gate. It layers on the Stage 16.5 boot/build-flow validator and smoke check, then optionally runs the Windows player build. The build script configures Boot first and Stage 16 second before exporting `build\windows-player-stage16\ProjectAegisRTS.exe`.

## Stage 17 Examples

After touching Stage 17 boot/options/HUD/result UI, player-facing validators, or docs, use:

```powershell
.\tools\run-stage17-fast-checks.ps1
```

Before committing Stage 17 player-facing polish locally, use:

```powershell
.\tools\run-stage17-medium-checks.ps1
```

Before declaring Stage 17 accepted or handing off a player build, use:

```powershell
.\tools\run-stage17-checks.ps1
```

For a focused player-facing smoke without rebuilding the EXE:

```powershell
.\tools\run-stage17-player-facing-checks.ps1 -SkipPlayerBuild
```

## Stage 18 Examples

After touching Stage 18 checklist/progress, prompts, sidebar readability, player-build camera/fog/HUD defaults, validators, or docs, use:

```powershell
.\tools\run-stage18-fast-checks.ps1
```

Before committing Stage 18 tester playability locally, use:

```powershell
.\tools\run-stage18-medium-checks.ps1
```

Before declaring Stage 18 accepted or handing off a player build, use:

```powershell
.\tools\run-stage18-checks.ps1
```

For a focused player-facing smoke without rebuilding the EXE:

```powershell
.\tools\run-stage18-player-facing-checks.ps1 -SkipPlayerBuild
```

## Stage 18.5 Examples

After touching fine placement core logic, board grid rendering, placement preview, mouse/hand ray placement, player-facing placement text, validators, or docs, use:

```powershell
.\tools\run-stage18-5-fast-checks.ps1
```

Before committing Stage 18.5 fine placement changes locally, use:

```powershell
.\tools\run-stage18-5-medium-checks.ps1
```

Before declaring Stage 18.5 accepted or handing off a player build, use:

```powershell
.\tools\run-stage18-5-checks.ps1
```

For a focused player-facing smoke without rebuilding the EXE:

```powershell
.\tools\run-stage18-5-player-facing-checks.ps1 -SkipPlayerBuild
```

## Stage 19 Examples

After touching mission flow, prompts, checklist/sidebar guidance, fine-grid readability, pacing, or Stage 19 validators, use:

```powershell
.\tools\run-stage19-fast-checks.ps1
```

Before committing Stage 19 mission tuning locally, use:

```powershell
.\tools\run-stage19-medium-checks.ps1
```

Before declaring Stage 19 accepted or handing off a player build, use:

```powershell
.\tools\run-stage19-checks.ps1
```

For a focused player-facing smoke without rebuilding the EXE:

```powershell
.\tools\run-stage19-player-facing-checks.ps1 -SkipPlayerBuild
```

## Stage 19.5 Examples

After touching the right-side PC sidebar, pause menu, PC/XR UI mode defaults, player-facing layout, or Stage 19.5 validators, use:

```powershell
.\tools\run-stage19-5-fast-checks.ps1
```

Before committing Stage 19.5 UI changes locally, use:

```powershell
.\tools\run-stage19-5-medium-checks.ps1
```

Before declaring Stage 19.5 accepted or handing off a player build, use:

```powershell
.\tools\run-stage19-5-checks.ps1
```

For a focused player-facing smoke without rebuilding the EXE:

```powershell
.\tools\run-stage19-5-player-facing-checks.ps1 -SkipPlayerBuild
```

## Expected Time

Fast checks should usually take minutes because they avoid earlier-stage validation. Medium checks are longer because they include core tests, Unity DLL build, immediate dependency validation, and current-stage validation, but they avoid the full replay where practical. Full checks are the slow acceptance gate and can take much longer because they validate every stage through the current stage. Stage 9 and later full checks avoid recursively nesting lower full gates, so they should scale roughly with the number of stages instead of repeating prior stages many times.

Stage 15.1 keeps medium checks to one core test run and one Unity DLL build per medium command. Direct Unity validation calls use `-SkipCoreBuild` after that build, which keeps failure output clear without introducing fragile cache state. Stage 17 keeps the same principle and treats Stage 16.5 build-flow validation as the direct immediate dependency. Stage 18 keeps the same principle with direct Stage 17 dependencies and Stage 18-specific player-facing checks. Stage 18.5 keeps Stage 18 as the direct dependency and then runs fine-grid validation without calling any prior medium script. Stage 19 keeps Stage 18.5 as the direct dependency and then runs mission-flow validation without calling any prior medium script. Stage 19.5 keeps Stage 19 as the direct dependency and then runs PC-sidebar/pause validation without calling any prior medium script. Stage 20, Stage 21, Stage 21.5, Stage 22, Stage 23, Stage 24, Stage 25, Stage 26, Stage 27, Stage 27.1, Stage 28, Stage 28.1, Stage 29, Stage 30, and Stage 31 keep the same flat shape with direct player-facing and Unity validation dependencies only.

## Why Full Validation Still Matters

The fast and medium tiers reduce iteration cost; they do not replace acceptance coverage. The full gate still proves that Stage 0 deterministic tests, earlier Unity scenes, smoke checks, dependencies, and current-stage validation continue to work together.

## NuGet And Network Restore

Validation helpers now check for each project's `obj\project.assets.json`. If assets exist, repeated runs use `--no-restore` for `dotnet run`, `dotnet build`, and `dotnet publish` paths that were updated in Stage 8.1. If assets are missing on a clean machine, the helper prints why and runs `dotnet restore` once for that project, then returns to no-restore execution.

This keeps first-time setup working without making every validation loop depend on network or NuGet access.

## Line Endings And Whitespace

The repository uses `.gitattributes` to keep Unity YAML-style files (`.unity`, `.prefab`, `.asset`, `.meta`, `.json`, `.yml`, `.yaml`) on LF line endings and mark generated binaries such as `.dll` and `.pdb` as binary. C# and PowerShell files use CRLF on checkout. Validation whitespace normalization preserves each file's existing newline style while trimming trailing whitespace.

Git may still print local line-ending conversion warnings after an edit, especially on Windows. Treat those warnings as non-fatal when `git diff --check` passes. The whitespace gate remains `git diff --check`; do not mass-rewrite Unity assets just to silence warnings.

## Stage 15.1 Branching

The pushed Stage 15 checkpoint is `codex/overnight-stage10-stage15` at `04c6c768bd6cdda74c6593a7d046de62ac27a39b`. Stage 15.1 validation-tier flattening work belongs on `codex/stage-15-1-validation-flattening`.

## Unity Already Open

Unity batchmode cannot safely open a project that is already open in the editor. Stage validation detects that project lock and uses the live/file/log fallback instead:

- validates the existing current-stage scene file,
- scans the editor log for red-error signatures,
- normalizes generated current-stage YAML whitespace,
- reports that Play Mode smoke used fallback instead of full batchmode automation.

If the project lock blocks batchmode Play Mode smoke, that is not automatically a failure when the fallback validation passes. Close Unity and rerun the same command when you need full batchmode Play Mode evidence.

## Future Stages

Every stage after Stage 8 should include fast, medium, and full validation tiers where practical:

- fast: current-stage iteration only,
- medium: current stage plus immediate dependency and core checks,
- full: Stage 0 through current stage acceptance coverage.

Do not weaken or remove the full chain when adding faster tiers.

## Stage 8.1 Commit

Stage 8.1 implementation hash: c6cfde1111538f204abbb3e6583df3bf8f363858.

Stage 9 implementation hash: pending local commit.

Stage 10 implementation hash: 718ab2a3157a1753b074dc10b43d296800d739b5.

Stage 11 implementation hash: ae0f5ee0fd6574e71a885423c63686a0736a1570.

Stage 12 implementation hash: 5aca8fb0cc3b7b952adbdcedd5496f88719587f1.

Stage 13 implementation hash: 17527ff5848ba3a0a333cb8e0bd8332ca9f2f860.

Stage 14 implementation hash: b54ea7d.

Stage 15 implementation hash: 04c6c768bd6cdda74c6593a7d046de62ac27a39b.

## Stage 20 Validation

Stage 20 adds:

- `.\tools\run-stage20-fast-checks.ps1` for current-stage visual iteration.
- `.\tools\run-stage20-medium-checks.ps1` for pre-commit confidence without calling prior medium scripts.
- `.\tools\run-stage20-player-facing-checks.ps1` for Stage16 player-facing UI and MVP proxy resolution confidence.
- `.\tools\run-stage20-checks.ps1` for slow full final acceptance.

The medium recursion audit includes Stage 20 and fails if `run-stage20-medium-checks.ps1` calls any prior `run-stage*-medium-checks.ps1`.

## Stage 21 Validation

Stage 21 adds:

- `.\tools\run-unity-stage21-validation.ps1` for Stage 21 Unity-only generation, import scanning, QA, scene, and play-mode smoke validation.
- `.\tools\run-stage21-fast-checks.ps1` for current-stage MVP visual QA iteration.
- `.\tools\run-stage21-medium-checks.ps1` for pre-commit confidence without calling prior medium scripts.
- `.\tools\run-stage21-player-facing-checks.ps1` for Stage16 player-facing UI, Player.log, and MVP proxy resolution confidence.
- `.\tools\run-stage21-checks.ps1` for slow full final acceptance.

Fast checks are intended for small proxy readability, socket, pivot, replacement metadata, import scan, or Stage21 scene changes. Medium checks include Rts.Core tests, direct Stage 20 validation dependencies, Stage 21 validation, Stage 4/5 UI preservation, the UnityEngine-free scan, and `git diff --check`.

The medium recursion audit includes Stage 21 and fails if `run-stage21-medium-checks.ps1` calls any prior `run-stage*-medium-checks.ps1`. Full validation remains the final acceptance gate because it exercises the whole Stage 0-through-current chain.

## Stage 21.5 Validation

Stage 21.5 adds:

- `.\tools\run-unity-stage21-5-validation.ps1` for display settings configuration, validation, and Play Mode smoke.
- `.\tools\run-stage21-5-fast-checks.ps1` for display-scaling iteration.
- `.\tools\run-stage21-5-medium-checks.ps1` for pre-commit confidence without calling prior medium scripts.
- `.\tools\run-stage21-5-player-facing-checks.ps1` for player build/log confidence, normal launch smoke, and 1920x1080 windowed launch smoke.
- `.\tools\run-stage21-5-checks.ps1` for slow full final acceptance.
- `.\tools\run-player-windowed-1080p.ps1` for manual 1920x1080 windowed testing.

Fast checks are intended for Boot Options display controls, `PlayerDisplaySettings`, CanvasScaler enforcement, player build defaults, or Player.log diagnostic edits. Medium checks include Rts.Core tests, direct Stage 21 validation dependencies, Stage 4/5 UI preservation, Stage 21.5 validation, the UnityEngine-free scan, and `git diff --check`.

The medium recursion audit includes Stage 21.5 and fails if `run-stage21-5-medium-checks.ps1` calls any prior `run-stage*-medium-checks.ps1`.

## Stage 22 Validation

Stage 22 adds:

- `.\tools\run-unity-stage22-validation.ps1` for command matrix validation and Play Mode smoke.
- `.\tools\run-stage22-fast-checks.ps1` for command-control iteration.
- `.\tools\run-stage22-medium-checks.ps1` for pre-commit confidence without calling prior medium scripts.
- `.\tools\run-stage22-player-facing-checks.ps1` for player-facing command bar, player build/log confidence, and launch smoke.
- `.\tools\run-stage22-checks.ps1` for slow full final acceptance.

Fast checks are intended for attack-move, guard, patrol, scatter, deploy placeholder, PC input selection, control groups, or command-bar layout edits. Medium checks include Rts.Core tests, direct Stage 21.5 validation dependencies, Stage 4/5 UI preservation, Stage 22 validation, the UnityEngine-free scan, and `git diff --check`.

The medium recursion audit includes Stage 22 and fails if `run-stage22-medium-checks.ps1` calls any prior `run-stage*-medium-checks.ps1`.

## Stage 23 Validation

Stage 23 adds:

- `.\tools\run-unity-stage23-validation.ps1` for base-management command bar validation and Play Mode smoke.
- `.\tools\run-stage23-fast-checks.ps1` for repair/sell/power/rally iteration.
- `.\tools\run-stage23-medium-checks.ps1` for pre-commit confidence without calling prior medium scripts.
- `.\tools\run-stage23-player-facing-checks.ps1` for player-facing command routing, Player.log, and launch smoke.
- `.\tools\run-stage23-checks.ps1` for slow full final acceptance.

Fast checks are intended for repair, sell, power toggle, rally point, command-bar routing, or snapshot field edits. Medium checks include Rts.Core tests, direct Stage 22 validation dependencies, Stage 4/5 UI preservation, Stage 23 validation, the UnityEngine-free scan, and `git diff --check`.

The medium recursion audit includes Stage 23 and fails if `run-stage23-medium-checks.ps1` calls any prior `run-stage*-medium-checks.ps1`.

## Stage 24 Validation

Stage 24 adds:

- `.\tools\run-unity-stage24-validation.ps1` for tech/support validation and Play Mode smoke.
- `.\tools\run-stage24-fast-checks.ps1` for prerequisite, tech unlock, support-power, or support-sidebar iteration.
- `.\tools\run-stage24-medium-checks.ps1` for pre-commit confidence without calling prior medium scripts.
- `.\tools\run-stage24-player-facing-checks.ps1` for player-facing support UI, Player.log, and launch smoke.
- `.\tools\run-stage24-checks.ps1` for slow full final acceptance.

Fast checks are intended for production prerequisite gates, support-power definitions, cooldowns, Reveal Scan, Emergency Repair Pulse, support snapshots, production-card availability text, or right-sidebar support buttons. Medium checks include Rts.Core tests, direct Stage 23 validation dependencies, Stage 4/5 UI preservation, Stage 24 validation, the UnityEngine-free scan, and `git diff --check`.

The medium recursion audit includes Stage 24 and fails if `run-stage24-medium-checks.ps1` calls any prior `run-stage*-medium-checks.ps1`.

## Stage 25 Validation

Stage 25 adds:

- `.\tools\run-unity-stage25-validation.ps1` for engineer/transport validation and Play Mode smoke.
- `.\tools\run-stage25-fast-checks.ps1` for capture, engineer repair, load/unload, passenger snapshot, or command-bar iteration.
- `.\tools\run-stage25-medium-checks.ps1` for pre-commit confidence without calling prior medium scripts.
- `.\tools\run-stage25-player-facing-checks.ps1` for player-facing command routing, Player.log, and launch smoke.
- `.\tools\run-stage25-checks.ps1` for slow full final acceptance.

Fast checks are intended for engineer capture/repair definitions, captureable metadata, APC transport capacity, passenger state, transport snapshots, command-bar routing, or Quest pass-through routing. Medium checks include Rts.Core tests, direct Stage 24 validation dependencies, Stage 4/5 UI preservation, Stage 25 validation, the UnityEngine-free scan, and `git diff --check`.

The medium recursion audit includes Stage 25 and fails if `run-stage25-medium-checks.ps1` calls any prior `run-stage*-medium-checks.ps1`.

## Stage 26 Validation

Stage 26 adds:

- `.\tools\run-unity-stage26-validation.ps1` for airfield/aircraft/naval validation and Play Mode smoke.
- `.\tools\run-stage26-fast-checks.ps1` for air/naval foundation iteration.
- `.\tools\run-stage26-medium-checks.ps1` for pre-commit confidence without calling prior medium scripts.
- `.\tools\run-stage26-player-facing-checks.ps1` for player-facing air/naval smoke, Player.log, and launch smoke.
- `.\tools\run-stage26-checks.ps1` for slow full final acceptance.

Fast checks are intended for aircraft metadata, dual-helipad pad state, aircraft docking/rearm/fuel placeholders, aircraft altitude snapshots, water/naval passability, vertical-slice air assets, or visual altitude integration. Medium checks include Rts.Core tests, direct Stage 25 validation dependencies, Stage 4/5 UI preservation, Stage 26 validation, the UnityEngine-free scan, and `git diff --check`.

The medium recursion audit includes Stage 26 and fails if `run-stage26-medium-checks.ps1` calls any prior `run-stage*-medium-checks.ps1`.

## Stage 27 Validation

Stage 27 adds:

- `.\tools\run-unity-stage27-validation.ps1` for skirmish AI pressure, difficulty controls, restart, and Play Mode smoke.
- `.\tools\run-stage27-fast-checks.ps1` for skirmish playability iteration.
- `.\tools\run-stage27-medium-checks.ps1` for pre-commit confidence without calling prior medium scripts.
- `.\tools\run-stage27-player-facing-checks.ps1` for player-facing skirmish smoke, Player.log, and launch smoke.
- `.\tools\run-stage27-checks.ps1` for slow full final acceptance.

Fast checks are intended for AI difficulty profiles, attack-wave timing, AI production pressure, Hard repair behavior, Boot Options difficulty selection, restart controls, or the objective HUD enemy-pressure readout. Medium checks include Rts.Core tests, direct Stage 26 validation dependencies, Stage 4/5 UI preservation, Stage 27 validation, the UnityEngine-free scan, and `git diff --check`.

The medium recursion audit includes Stage 27 and fails if `run-stage27-medium-checks.ps1` calls any prior `run-stage*-medium-checks.ps1`.

## Stage 27.1 Validation

Stage 27.1 adds:

- `.\tools\run-unity-stage27-1-validation.ps1` for PC building placement HUD separation and Play Mode smoke.
- `.\tools\run-stage27-1-fast-checks.ps1` for PCDesktop placement UX iteration.
- `.\tools\run-stage27-1-medium-checks.ps1` for pre-commit confidence without calling prior medium scripts.
- `.\tools\run-stage27-1-player-facing-checks.ps1` for player-facing placement smoke, Player.log, Quest hand-control preservation, and launch smoke.
- `.\tools\run-stage27-1-checks.ps1` for slow full final acceptance.

Fast checks are intended for BoardPlacementHud visibility, `PlacementModePanel`, ready production-card placement, fine-grid preview guidance, Esc/cancel priority, or docs/tooling changes. Medium checks include Rts.Core tests, direct Stage 27 player-facing validation, direct Stage 4/5 hand-control validation, Stage 27.1 validation, the UnityEngine-free scan, and `git diff --check`.

The medium recursion audit includes Stage 27.1 and fails if `run-stage27-1-medium-checks.ps1` calls any prior `run-stage*-medium-checks.ps1`.

## Stage 28 Validation

Stage 28 adds:

- `.\tools\run-unity-stage28-validation.ps1` for integrated feature regression and Play Mode smoke.
- `.\tools\run-stage28-fast-checks.ps1` for current Stage 28 QA/tooling iteration.
- `.\tools\run-stage28-medium-checks.ps1` for pre-commit confidence without calling prior medium scripts.
- `.\tools\run-stage28-player-facing-checks.ps1` for player-facing feature regression, Player.log, Quest hand-control preservation, and launch smoke.
- `.\tools\run-stage28-checks.ps1` for slow full final acceptance.

Fast checks are intended for `FeatureRegressionHud`, Stage 28 validators, feature-matrix documentation, Stage27.1 placement regression coverage, or player-facing QA tooling changes. Medium checks include Rts.Core tests, direct Stage 27.1 Unity/player-facing validation, direct Stage 4/5 hand-control validation, Stage 28 validation, the UnityEngine-free scan, and `git diff --check`.

The medium recursion audit includes Stage 28 and fails if `run-stage28-medium-checks.ps1` calls any prior `run-stage*-medium-checks.ps1`.

## Stage 28.1 Validation

Stage 28.1 adds:

- `.\tools\run-unity-stage28-1-validation.ps1` for PC safe-area layout and placement smoke validation.
- `.\tools\run-stage28-1-fast-checks.ps1` for PC layout, validation tooling, or diagonal movement iteration.
- `.\tools\run-stage28-1-medium-checks.ps1` for pre-commit confidence without calling prior medium scripts.
- `.\tools\run-stage28-1-player-facing-checks.ps1` for player-facing layout, Player.log, and launch smoke confidence.
- `.\tools\run-stage28-1-checks.ps1` for slow full final acceptance through the flattened Stage 28 full gate plus Stage 28.1 coverage.
- `.\tools\audit-full-validation-recursion.ps1` to fail if Stage 28 or Stage 28.1 reintroduces lower full-gate recursion.

Fast checks are intended for `PcGameplaySafeAreaController`, `PlayerFacingCameraFramer`, Stage 28.1 validators, diagonal movement core tests, or docs/tooling changes. Medium checks include Rts.Core tests, direct Stage 28, Stage 27.1, Stage 4/5, Stage 28.1 validation, the UnityEngine-free scan, and `git diff --check`.

The medium recursion audit includes Stage 28.1 and fails if `run-stage28-1-medium-checks.ps1` calls any prior `run-stage*-medium-checks.ps1`. The full recursion audit allows the flattened Stage 28.1 full gate to call the flattened Stage 28 full gate, but it fails if older full gates are nested again.

## Stage 29 Validation

Stage 29 adds:

- `.\tools\run-unity-stage29-validation.ps1` for battlefield material/profile generation, Stage 29 review scene creation, visual QA, Play Mode smoke, and screenshot capture.
- `.\tools\run-stage29-fast-checks.ps1` for realistic battlefield visual iteration.
- `.\tools\run-stage29-medium-checks.ps1` for pre-commit confidence without calling prior medium scripts.
- `.\tools\run-stage29-player-facing-checks.ps1` for player-facing PC layout/log confidence, Stage 28.1 preservation, and optional Windows player build/launch smoke.
- `.\tools\run-stage29-checks.ps1` for slow full final acceptance through the flattened Stage 28.1 full gate plus Stage 29 coverage.

Fast checks are intended for terrain materials, material profile libraries, lighting/atmosphere, MVP proxy detail, review-scene composition, screenshot capture, or visual QA docs/tooling. Medium checks include Rts.Core tests, direct Stage 28, Stage 28.1, Stage 4, and Stage 5 validation dependencies, Stage 29 Unity visual validation, Stage 29 player-facing checks with player build/log skipped, recursion audits, the UnityEngine-free scan, and `git diff --check`.

The medium recursion audit includes Stage 29 and fails if `run-stage29-medium-checks.ps1` calls any prior `run-stage*-medium-checks.ps1`. The full recursion audit allows the Stage 29 full gate to call the flattened Stage 28.1 full gate, but fails if Stage 29 starts recursively replaying older full gates.

## Stage 30 Validation

Stage 30 adds:

- `.\tools\run-unity-stage30-validation.ps1` for readability profile generation, proxy overlay generation, Stage 30 review scene creation, visual readability QA, Play Mode smoke, and screenshot capture.
- `.\tools\run-stage30-fast-checks.ps1` for visual readability iteration.
- `.\tools\run-stage30-medium-checks.ps1` for pre-commit confidence without calling prior medium scripts.
- `.\tools\run-stage30-player-facing-checks.ps1` for player-facing PC layout/log confidence, Stage 29 preservation, and optional Windows player build/launch smoke.
- `.\tools\run-stage30-checks.ps1` for slow full final acceptance through the flattened Stage 29 full gate plus Stage 30 coverage.

Fast checks are intended for actor/terrain contrast, fine-grid dominance, camera readability, resource readability, proxy distinguishability, screenshot capture, or readability docs/tooling. Medium checks include Rts.Core tests, direct Stage 29, Stage 28.1, Stage 4, and Stage 5 validation dependencies, Stage 30 Unity readability validation, Stage 30 player-facing checks with player build/log skipped, recursion audits, the UnityEngine-free scan, and `git diff --check`.

The medium recursion audit includes Stage 30 and fails if `run-stage30-medium-checks.ps1` calls any prior `run-stage*-medium-checks.ps1`. The full recursion audit allows the Stage 30 full gate to call the flattened Stage 29 full gate, but fails if Stage 30 starts recursively replaying older full gates.

## Stage 31 Validation

Stage 31 adds:

- `.\tools\run-stage31-handoff-validation.ps1` for artist handoff package, replacement guide, per-actor checklist, Quest LOD budget, screenshot/reference package, and overnight report validation.
- `.\tools\run-stage31-fast-checks.ps1` for handoff-doc and package iteration.
- `.\tools\run-stage31-medium-checks.ps1` for pre-commit confidence without calling prior medium scripts.
- `.\tools\run-stage31-player-facing-checks.ps1` for player-facing preservation, Player.log, and optional Windows player build confidence.
- `.\tools\run-stage31-checks.ps1` for slow full final acceptance through the flattened Stage 30 full gate plus Stage 31 coverage.

Fast checks are intended for export/modeling briefs, material naming rules, trim-sheet guidance, LOD targets, Quest budgets, screenshot/reference package text, MVP art replacement docs, and per-actor production checklists. Medium checks include Rts.Core tests, direct Stage 30 Unity validation, Stage 30 player-facing preservation with player build/log skipped, Stage 31 handoff validation, recursion audits, the UnityEngine-free scan, and `git diff --check`.

The medium recursion audit includes Stage 31 and fails if `run-stage31-medium-checks.ps1` calls any prior `run-stage*-medium-checks.ps1`. The full recursion audit allows the Stage 31 full gate to call the flattened Stage 30 full gate, but fails if Stage 31 starts recursively replaying older full gates.

## Stage 32 Validation

Stage 32 adds:

- `.\tools\run-unity-stage32-validation.ps1` for terrain-piece generation, explicit Batch01 source-art ingestion, material/catalog validation, review scene creation, player-facing integration validation, play-mode smoke, and screenshot capture.
- `.\tools\run-stage32-terrain-art-ingestion.ps1` for direct external source-art ingestion from `unity\Assets\Rts\Art\Source\Terrain\Batch01`.
- `.\tools\run-stage32-terrain-kit-generator.ps1` for the fallback/debug terrain asset replacement generator, review kit, QA report, and review scene.
- `.\tools\run-stage32-fast-checks.ps1` for terrain-piece and set-dressing iteration without replaying Stage1-31 validation chains.
- `.\tools\run-stage32-medium-checks.ps1` for pre-commit confidence without calling prior medium scripts.
- `.\tools\run-stage32-player-facing-checks.ps1` for PCDesktop sidebar/safe-area preservation, Stage27.1 placement HUD separation, Player.log, and optional Windows player launch smoke.
- `.\tools\run-stage32-checks.ps1` for slow full final acceptance through the Stage31 final gate plus Stage32 coverage.

Fast checks are intended for terrain-piece geometry, material profiles, catalog definitions, set-dressing placements, Batch01 source-art ingestion, terrain replacement-kit generation, review-scene composition, screenshots, or Stage32 tooling/docs. Medium checks include Rts.Core tests, direct Stage31 handoff/player-facing preservation, direct Stage28.1 safe-area validation, direct Stage27.1 placement validation, direct Stage4/5 hand-control validation, Stage32 validation, the terrain-kit generator/validator, Stage32 player-facing checks with player build/log skipped, recursion audits, the UnityEngine-free scan, and `git diff --check`.

When Batch01 source art exists, Stage32 validation must fail if the player-facing profile or rendered Stage16 terrain root still uses primitive-only generated proxies for the core terrain batch. Generated proxy terrain is retained for fallback/debug review scenes only.

The medium recursion audit includes Stage32 and fails if `run-stage32-medium-checks.ps1` calls any prior `run-stage*-medium-checks.ps1`. The full recursion audit allows the Stage32 full gate to call the Stage31 final gate, but fails if Stage32 starts recursively replaying older full gates. The overlay terrain-kit script is a direct Stage32 generator/validator dependency, not a medium-tier dependency.

## Stage 32.6 Validation

Stage 32.6 adds:

- `.\tools\run-unity-stage32-6-validation.ps1` for corrected terrain asset generation, reference-only sheet enforcement, runtime prefab validation, review-scene creation, and screenshot capture.
- `.\tools\run-stage32-6-fast-checks.ps1` for terrain-art iteration after small generator/material/prefab changes.
- `.\tools\run-stage32-6-medium-checks.ps1` for pre-commit confidence without calling prior medium scripts.
- `.\tools\run-stage32-6-player-facing-checks.ps1` for player-facing terrain and optional Windows player/log coverage.
- `.\tools\run-stage32-6-checks.ps1` for full Stage32.6 acceptance.

Stage32.6 validation fails if runtime terrain uses Batch01 reference textures, flat image cards, missing runtime prefabs/materials, missing grid-friendly metadata, or player-facing Stage16 set dressing still points at `Batch01Imported` terrain.

The medium recursion audit includes Stage32.6 and fails if `run-stage32-6-medium-checks.ps1` calls any prior `run-stage*-medium-checks.ps1`.

## Stage 33 Validation

Stage 33 adds:

- `.\tools\run-stage33-tank-source-generator.ps1` for tank source/proxy prefab generation, ActorVisualDefinition integration, review-scene generation, and direct prefab validation.
- The current highest flat medium gate, `.\tools\run-stage32-medium-checks.ps1`, for pre-commit confidence across existing gameplay, PCDesktop sidebar/safe-area behavior, Stage27.1 placement HUD separation, QuestXR Stage4/Stage5 controls, Player.log checks, recursion audits, the UnityEngine-free scan, and whitespace validation.

Run the Stage33 generator after editing tank source geometry, sockets, descriptors, LODs, materials, ActorVisualDefinition wiring, or the Stage33 review scene. Run the highest medium gate before committing Stage33 changes. Final acceptance should still use the latest full gate when a stage is being accepted; Stage33 does not make medium/full validation recursive.
