# Stage 9 Report

## Summary

Stage 9 adds deterministic combat, weapons, projectile simulation, damage, death/destruction state, combat snapshots, and Unity placeholder combat presentation. The playable scene is `Assets/Rts/Scenes/Stage9_CombatWeaponsDamage.unity`.

## Branch And Commits

- Branch: `codex/stage-9-combat-weapons-damage`
- Base Stage 8.1 validation-tier commit: `c6cfde1111538f204abbb3e6583df3bf8f363858`
- Stage 9 implementation commit: this commit

## Systems Created

- Core combat definitions: damage, projectile, death, target filters, cooldowns, visual ids, and weapon metadata.
- Core combat runtime: attack orders, cooldown ticking, projectile movement, damage application, destroyed actor state, and bounded combat events.
- Combat snapshots: projectile snapshots and combat event snapshots plus expanded actor combat/death fields.
- Unity bridge commands: attack selected actor, attack selected cell, force-attack placeholder route, stop combat.
- Unity presentation: projectile renderer, combat event renderer, muzzle/impact/damage/death markers, combat profile assets, and F12 combat debug HUD.
- Stage 9 scene generation, scene validation, Play Mode smoke validation, and fast/medium/full validation tiers.

## Architecture Boundary

`Rts.Core` remains deterministic and UnityEngine-free. Unity submits attack commands and renders snapshot/event data; it does not own targeting, cooldowns, projectile movement, damage, death, or destruction rules.

## Validation Tiers

- `tools/run-stage9-fast-checks.ps1`: current Stage 9 Unity validation, Play Mode smoke or live fallback, UnityEngine-free scan, and `git diff --check`.
- `tools/run-stage9-medium-checks.ps1`: `Rts.Core` tests, Unity DLL build, Stage 8 immediate dependency validation, Stage 9 validation, UnityEngine-free scan, and `git diff --check`.
- `tools/run-stage9-checks.ps1`: slow full Stage 0-through-Stage 9 acceptance gate.

## Manual Play Mode Checklist

Open `Assets/Rts/Scenes/Stage9_CombatWeaponsDamage.unity`, press Play, and verify:

- Board, grid, and actors are visible.
- F12 toggles the combat debug HUD.
- Attack mode routes to real attack orders against enemy actors.
- Projectiles or instant-impact markers appear.
- Damage and death markers appear.
- Target health decreases and destroyed actors stop accepting new move/attack orders.
- Tick count advances.
- Pause/resume and single-step still work.
- Right-hand attack route still works with the Stage 5 command surface.
- No repeating red console errors.

## Known Limits

- Force-attack remains a placeholder command.
- Combat uses MVP balance and placeholder visuals.
- Armor classes, splash damage, line-of-sight, audio, final VFX, wrecks, veterancy, AI, multiplayer, and replays remain future stages.

## Commands

Acceptance commands:

```powershell
dotnet run --no-restore --project src/Rts.Core.Tests
.\tools\build-rts-core-for-unity.ps1
.\tools\run-stage9-fast-checks.ps1
.\tools\run-stage9-medium-checks.ps1
.\tools\run-stage9-checks.ps1
git diff --check
```
