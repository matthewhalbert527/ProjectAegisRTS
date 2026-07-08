# Stage 9 Combat Design

## Summary

Stage 9 adds the first deterministic combat loop to `Rts.Core`: attack orders, weapon cooldowns, projectile state, damage, death, destruction flags, combat events, and snapshot fields for Unity presentation.

Unity remains presentation and input only. It can render projectiles, muzzle flashes, impact markers, damage markers, death markers, and a debug HUD, but it does not own combat rules.

## Core Rules

- Weapons live in actor definitions and use deterministic integer fields for damage, range, cooldown, projectile speed, target filters, and visual ids.
- `IssueAttackOrderCommand` targets an enemy actor and starts an attack state when the attacker is armed, alive, and in range.
- Projectile weapons create deterministic projectile records that move in fixed sub-cell coordinates and apply damage on impact.
- Instant weapons apply damage on the firing tick and still emit combat events for Unity.
- Health is reduced in `Rts.Core`; actors at zero health enter destroyed/death state and stop accepting move or attack orders.
- Destroyed actors stay in snapshots for presentation, selection filtering, and later wreck/destruction art work.

## Snapshot Contract

Stage 9 extends snapshots with:

- actor max health, alive/dying/destroyed flags, active weapon id, cooldown, target, and death metadata,
- projectile snapshots with source, target, position, damage, and visual ids,
- bounded combat event snapshots for muzzle, impact, damage, death, and projectile presentation.

Unity consumes those fields through the bridge and render systems. It does not infer hidden gameplay state from GameObjects.

## Unity Presentation

The Stage 9 scene is:

```text
Assets/Rts/Scenes/Stage9_CombatWeaponsDamage.unity
```

It adds:

- `CombatVisualProfileLibrary`
- `ProjectileRenderSystem`
- `CombatEventRenderSystem`
- `CombatDebugHud`
- combat profile assets under `Assets/Rts/ScriptableObjects/Combat/`
- a combat demo world with friendly and enemy actors in range

## Validation

Use the tier that matches the risk:

```powershell
.\tools\run-stage9-fast-checks.ps1
.\tools\run-stage9-medium-checks.ps1
.\tools\run-stage9-checks.ps1
```

The full Stage 9 check remains the acceptance gate and can take a long time because it replays earlier stage coverage before validating Stage 9.

## Known Limits

- Weapons are MVP definitions, not final balance.
- Line-of-sight, armor tables, splash damage, burst sequencing, sounds, final VFX, corpse/wreck rules, and advanced targeting are future work.
- Force-attack is still a safe placeholder route.
- Unity visuals are placeholders layered on deterministic snapshots.
