# Validation Tiers

Stage 8.1 adds validation tiers so normal development does not need to replay the slowest full acceptance chain after every small art, prefab, script, or documentation edit. Stage 9 follows the same model for combat iteration. Stage 10 follows it for economy iteration. Stage 11 follows it for fog/radar/minimap iteration. Stage 12 follows it for AI iteration. Stage 13 follows it for map/terrain/pathing iteration. Stage 14 follows it for feedback iteration. Stage 15 follows it for performance/build-readiness iteration. Stage 16 follows it for playable vertical-slice iteration. Stage 17 follows it for player-facing polish. Stage 18 follows it for tester-guided playability. Stage 18.5 follows it for fine placement grid iteration. Stage 19 follows it for mission flow and fine-grid playability tuning. Stage 19.5 follows it for the PC sidebar and pause menu rework. Stage 20 follows it for MVP proxy visuals. Stage 21 follows it for MVP visual QA. Stage 21.5 follows it for Windows player resolution/UI scaling. Stage 15.1 flattens the Stage 9-through-Stage 21.5 medium tiers so they validate direct dependencies instead of recursively calling prior medium checks. A follow-up hardening pass added `tools\audit-medium-validation-recursion.ps1` after runtime output showed recursive medium sections were still possible to miss. The full gate remains required for final acceptance; the faster tiers choose the right amount of evidence during iteration. Stage 9 and later full gates use `tools\run-stage-full-chain-checks.ps1` to walk Stage 0 through the current stage once instead of recursively replaying lower full gates.

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

## Medium Flattening Rule

Medium checks must not call earlier `run-stage*-medium-checks.ps1` scripts. A medium check runs `Rts.Core` tests once, builds/copies `Rts.Core` once, calls the immediate prior stage's Unity validation script directly with `-SkipCoreBuild`, calls the current stage's Unity validation script with `-SkipCoreBuild`, then runs the UnityEngine-free scan and `git diff --check`.

For example, `run-stage16-medium-checks.ps1` calls `run-unity-stage15-validation.ps1 -SkipCoreBuild` and `run-unity-stage16-validation.ps1 -SkipCoreBuild`. It must not call `run-stage15-medium-checks.ps1`. Stage 17 is a special case because Stage 16.5 is a build-flow layer rather than a numbered Unity validation script: `run-stage17-medium-checks.ps1` calls the Stage 16.5 build-flow validator directly, then calls `run-unity-stage17-validation.ps1 -SkipCoreBuild`. Stage 18 calls direct Stage 17 validators and player-facing checks instead of calling `run-stage17-medium-checks.ps1`.

The machine-enforced guardrail is:

```powershell
.\tools\audit-medium-validation-recursion.ps1
```

This audit scans Stage 9 through Stage 21.5 medium scripts and fails if a prior medium dependency or old "medium validation as the immediate dependency" wording returns. Stage 13 and later medium scripts run the audit before tests, and the Stage 21.5 full gate runs it before the full acceptance chain.

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

Stage 15.1 keeps medium checks to one core test run and one Unity DLL build per medium command. Direct Unity validation calls use `-SkipCoreBuild` after that build, which keeps failure output clear without introducing fragile cache state. Stage 17 keeps the same principle and treats Stage 16.5 build-flow validation as the direct immediate dependency. Stage 18 keeps the same principle with direct Stage 17 dependencies and Stage 18-specific player-facing checks. Stage 18.5 keeps Stage 18 as the direct dependency and then runs fine-grid validation without calling any prior medium script. Stage 19 keeps Stage 18.5 as the direct dependency and then runs mission-flow validation without calling any prior medium script. Stage 19.5 keeps Stage 19 as the direct dependency and then runs PC-sidebar/pause validation without calling any prior medium script. Stage 20, Stage 21, and Stage 21.5 keep the same flat shape with direct player-facing and Unity validation dependencies only.

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
