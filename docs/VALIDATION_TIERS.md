# Validation Tiers

Stage 8.1 adds validation tiers so normal development does not need to replay the slowest full acceptance chain after every small art, prefab, script, or documentation edit. Stage 9 follows the same model for combat iteration. Stage 10 follows it for economy iteration. Stage 11 follows it for fog/radar/minimap iteration. Stage 12 follows it for AI iteration. Stage 13 follows it for map/terrain/pathing iteration. The full gate remains required for final acceptance; the faster tiers choose the right amount of evidence during iteration.

## Tier Summary

| Tier | Command | Use When | Scope |
| --- | --- | --- | --- |
| Fast | `.\tools\run-stage8-fast-checks.ps1` | You changed current Stage 8 art pipeline, generated prefabs, sockets, icons, scene wiring, or related scripts. | Builds/copies `Rts.Core` for Unity, runs Stage 8 generation and validation only, runs Stage 8 Play Mode smoke when batchmode can own the project lock, checks `Rts.Core` for `UnityEngine`, and runs `git diff --check`. |
| Medium | `.\tools\run-stage8-medium-checks.ps1` | You are preparing a local commit and want current stage plus immediate dependency confidence. | Runs `Rts.Core` tests, builds/copies the Unity DLL, runs direct Stage 7 Unity validation as the immediate dependency, then Stage 8 validation and Play Mode smoke/fallback, the `Rts.Core` UnityEngine-free scan, and `git diff --check`. |
| Full | `.\tools\run-stage8-checks.ps1` | You need final Stage 8 acceptance evidence. | Runs Stage 0 through Stage 8, including the existing full validation chain and Stage 8 Play Mode smoke/fallback. This is intentionally slow. |
| Fast | `.\tools\run-stage9-fast-checks.ps1` | You changed current Stage 9 combat presentation, scene wiring, profiles, or smoke tooling. | Builds/copies `Rts.Core` for Unity, runs Stage 9 generation/validation only, runs Stage 9 Play Mode smoke when batchmode can own the project lock, checks `Rts.Core` for `UnityEngine`, and runs `git diff --check`. |
| Medium | `.\tools\run-stage9-medium-checks.ps1` | You are preparing a local Stage 9 commit. | Runs `Rts.Core` tests, builds/copies the Unity DLL, runs Stage 8 immediate dependency validation, then Stage 9 validation and Play Mode smoke/fallback, the `Rts.Core` UnityEngine-free scan, and `git diff --check`. |
| Full | `.\tools\run-stage9-checks.ps1` | You need final Stage 9 acceptance evidence. | Runs Stage 0 through Stage 9, including the existing full validation chain and Stage 9 Play Mode smoke/fallback. This is intentionally slow. |
| Fast | `.\tools\run-stage10-fast-checks.ps1` | You changed current Stage 10 economy presentation, scene wiring, harvest smoke tooling, or economy debug HUD. | Builds/copies `Rts.Core` for Unity, runs Stage 10 generation/validation only, runs Stage 10 Play Mode smoke when batchmode can own the project lock, checks `Rts.Core` for `UnityEngine`, and runs `git diff --check`. |
| Medium | `.\tools\run-stage10-medium-checks.ps1` | You are preparing a local Stage 10 commit. | Runs `Rts.Core` tests, builds/copies the Unity DLL, runs Stage 9 immediate dependency validation, then Stage 10 validation and Play Mode smoke/fallback, the `Rts.Core` UnityEngine-free scan, and `git diff --check`. |
| Full | `.\tools\run-stage10-checks.ps1` | You need final Stage 10 acceptance evidence. | Runs Stage 0 through Stage 10, including the existing full validation chain and Stage 10 Play Mode smoke/fallback. This is intentionally slow. |
| Fast | `.\tools\run-stage11-fast-checks.ps1` | You changed current Stage 11 fog/radar/minimap presentation, scene wiring, or smoke tooling. | Builds/copies `Rts.Core` for Unity, runs Stage 11 generation/validation only, runs Stage 11 Play Mode smoke when batchmode can own the project lock, checks `Rts.Core` for `UnityEngine`, and runs `git diff --check`. |
| Medium | `.\tools\run-stage11-medium-checks.ps1` | You are preparing a local Stage 11 commit. | Runs `Rts.Core` tests, builds/copies the Unity DLL, runs Stage 10 immediate dependency validation, then Stage 11 validation and Play Mode smoke/fallback, the `Rts.Core` UnityEngine-free scan, and `git diff --check`. |
| Full | `.\tools\run-stage11-checks.ps1` | You need final Stage 11 acceptance evidence. | Runs Stage 0 through Stage 11, including the existing full validation chain and Stage 11 Play Mode smoke/fallback. This is intentionally slow. |
| Fast | `.\tools\run-stage12-fast-checks.ps1` | You changed current Stage 12 AI core, scene wiring, debug HUD, or smoke tooling. | Builds/copies `Rts.Core` for Unity, runs Stage 12 generation/validation only, runs Stage 12 Play Mode smoke when batchmode can own the project lock, checks `Rts.Core` for `UnityEngine`, and runs `git diff --check`. |
| Medium | `.\tools\run-stage12-medium-checks.ps1` | You are preparing a local Stage 12 commit. | Runs `Rts.Core` tests, builds/copies the Unity DLL, runs Stage 11 immediate dependency validation, then Stage 12 validation and Play Mode smoke/fallback, the `Rts.Core` UnityEngine-free scan, and `git diff --check`. |
| Full | `.\tools\run-stage12-checks.ps1` | You need final Stage 12 acceptance evidence. | Runs Stage 0 through Stage 12, including the existing full validation chain and Stage 12 Play Mode smoke/fallback. This is intentionally slow. |
| Fast | `.\tools\run-stage13-fast-checks.ps1` | You changed current Stage 13 map, terrain, pathing, scene wiring, debug HUD, or smoke tooling. | Builds/copies `Rts.Core` for Unity, runs Stage 13 generation/validation only, runs Stage 13 Play Mode smoke when batchmode can own the project lock, checks `Rts.Core` for `UnityEngine`, and runs `git diff --check`. |
| Medium | `.\tools\run-stage13-medium-checks.ps1` | You are preparing a local Stage 13 commit. | Runs `Rts.Core` tests, builds/copies the Unity DLL, runs Stage 12 immediate dependency validation, then Stage 13 validation and Play Mode smoke/fallback, the `Rts.Core` UnityEngine-free scan, and `git diff --check`. |
| Full | `.\tools\run-stage13-checks.ps1` | You need final Stage 13 acceptance evidence. | Runs Stage 0 through Stage 13, including the existing full validation chain and Stage 13 Play Mode smoke/fallback. This is intentionally slow. |

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

## Expected Time

Fast checks should usually take minutes because they avoid earlier-stage validation. Medium checks are longer because they include core tests, Unity DLL build, immediate dependency validation, and current-stage validation, but they avoid the full replay where practical. Full checks are the slow acceptance gate and can take much longer because they replay every stage through the current stage.

## Why Full Validation Still Matters

The fast and medium tiers reduce iteration cost; they do not replace acceptance coverage. The full gate still proves that Stage 0 deterministic tests, earlier Unity scenes, smoke checks, dependencies, and current-stage validation continue to work together.

## NuGet And Network Restore

Validation helpers now check for each project's `obj\project.assets.json`. If assets exist, repeated runs use `--no-restore` for `dotnet run`, `dotnet build`, and `dotnet publish` paths that were updated in Stage 8.1. If assets are missing on a clean machine, the helper prints why and runs `dotnet restore` once for that project, then returns to no-restore execution.

This keeps first-time setup working without making every validation loop depend on network or NuGet access.

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

Stage 13 implementation hash: pending local commit.
