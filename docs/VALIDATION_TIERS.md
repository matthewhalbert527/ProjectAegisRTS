# Validation Tiers

Stage 8.1 adds validation tiers so normal development does not need to replay the slowest full acceptance chain after every small art, prefab, script, or documentation edit. Stage 9 follows the same model for combat iteration. The full gate remains required for final acceptance; the faster tiers choose the right amount of evidence during iteration.

## Tier Summary

| Tier | Command | Use When | Scope |
| --- | --- | --- | --- |
| Fast | `.\tools\run-stage8-fast-checks.ps1` | You changed current Stage 8 art pipeline, generated prefabs, sockets, icons, scene wiring, or related scripts. | Builds/copies `Rts.Core` for Unity, runs Stage 8 generation and validation only, runs Stage 8 Play Mode smoke when batchmode can own the project lock, checks `Rts.Core` for `UnityEngine`, and runs `git diff --check`. |
| Medium | `.\tools\run-stage8-medium-checks.ps1` | You are preparing a local commit and want current stage plus immediate dependency confidence. | Runs `Rts.Core` tests, builds/copies the Unity DLL, runs direct Stage 7 Unity validation as the immediate dependency, then Stage 8 validation and Play Mode smoke/fallback, the `Rts.Core` UnityEngine-free scan, and `git diff --check`. |
| Full | `.\tools\run-stage8-checks.ps1` | You need final Stage 8 acceptance evidence. | Runs Stage 0 through Stage 8, including the existing full validation chain and Stage 8 Play Mode smoke/fallback. This is intentionally slow. |
| Fast | `.\tools\run-stage9-fast-checks.ps1` | You changed current Stage 9 combat presentation, scene wiring, profiles, or smoke tooling. | Builds/copies `Rts.Core` for Unity, runs Stage 9 generation/validation only, runs Stage 9 Play Mode smoke when batchmode can own the project lock, checks `Rts.Core` for `UnityEngine`, and runs `git diff --check`. |
| Medium | `.\tools\run-stage9-medium-checks.ps1` | You are preparing a local Stage 9 commit. | Runs `Rts.Core` tests, builds/copies the Unity DLL, runs Stage 8 immediate dependency validation, then Stage 9 validation and Play Mode smoke/fallback, the `Rts.Core` UnityEngine-free scan, and `git diff --check`. |
| Full | `.\tools\run-stage9-checks.ps1` | You need final Stage 9 acceptance evidence. | Runs Stage 0 through Stage 9, including the existing full validation chain and Stage 9 Play Mode smoke/fallback. This is intentionally slow. |

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
