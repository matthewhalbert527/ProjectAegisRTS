# Stage 8 Art Pipeline Plan

## Production Flow

1. Concept art remains under `art/concepts/source/`.
2. Safe actor IDs come from `art/concepts/registry/assets.json`.
3. Stage 8 imports copies into `Assets/Rts/Art/Concepts/`.
4. `ConceptArtReference` assets preserve labels, notes, roles, and IP flags.
5. `ActorVisualDefinition` assets map actor IDs to concept refs, icons, profile IDs, sockets, and prefabs.
6. Generated blockout prefabs establish scale, silhouette, sockets, and controller hooks.
7. High-poly source models stay in `Assets/Rts/Art/Models/Source/`.
8. Game-ready imported models go to `Assets/Rts/Art/Models/Imported/`.
9. Production prefabs go to `Assets/Rts/Art/Prefabs/Actors/Production/`.
10. Validation checks descriptors, sockets, icons, concept refs, missing scripts, and IP flags before commit.

## Naming

- Actor IDs use lower snake case, for example `light_tank`.
- Blockout prefabs use `<actor_id>_blockout.prefab`.
- Production prefabs should use `<actor_id>_production.prefab`.
- Materials should use purpose names, not source-tool export names.
- Sockets are named `Socket_<ActorPrefabSocketKind>`.

## Socket Requirements

- Buildings: body, visual root, selection, health, UI, light, smoke, explosion.
- Production buildings: door, production exit, rally exit, production VFX.
- Power buildings: turbine and light sockets.
- Defenses: turret, barrel, muzzle, selection, health.
- Vehicles: body, visual root, selection, health, wheel/track pairs, smoke, explosion, and turret/barrel/muzzle where armed.
- Infantry: body, visual root, head, weapon, aim pivot, selection, health.
- Aircraft: body, visual root, rotor/engine, selection, health, landing pad, smoke, explosion, and muzzle where armed.

## Materials And Textures

Use production materials with clear names and reusable shader choices. Keep blockout materials separate under `Assets/Rts/Art/Materials/`. Final texture sets should use consistent albedo/normal/metallic/roughness naming and be sized for Quest budgets.

## Rigging And Animation

Rig only the transforms that need runtime animation. Preserve sockets for turrets, barrels, doors, turbines, radar dishes, cranes, repair arms, wheels, tracks, weapons, heads, rotors, and VFX/audio anchors. Animation clips should be named by state: `powered_idle`, `low_power_idle`, `production_active`, `damaged_idle`, `destroyed`, `move`, `turn`, `fire`, and `reload` where relevant.

## VFX And Audio Hooks

Use `VfxSmoke`, `VfxExplosion`, `VfxProduction`, `MuzzlePrimary`, `MuzzleSecondary`, and `AudioLoop` sockets as attachment points. Do not embed final combat or damage rules in VFX prefabs; gameplay remains in `Rts.Core`.

## Collision And LOD

Author simple non-authoritative presentation colliders only when useful for picking or editor review. Quest production assets should include LODs and use shared materials aggressively. Any gameplay footprint or occupancy still comes from deterministic data, not Unity colliders.

## DCC Export Checklist

- Apply transforms and freeze scale.
- Set pivot at ground/root center.
- Preserve socket empty names.
- Export in meters.
- Keep forward direction consistent with Unity +Z.
- Keep source files in `Models/Source/`, imported assets in `Models/Imported/`.
- Re-run Stage 8 prefab validation after every replacement.

## Unity Import Checklist

- Assign production prefab on the actor visual definition.
- Keep generated blockout as fallback.
- Preserve `ActorPrefabDescriptor`.
- Preserve all required `ActorPrefabSocket` children.
- Assign icon and concept reference.
- Run `.\tools\run-stage8-fast-checks.ps1` after small Stage 8 art, prefab, socket, icon, or showcase changes.
- Run `.\tools\run-stage8-medium-checks.ps1` before committing local Stage 8 fixes.
- Run `.\tools\run-stage8-checks.ps1` for the slow full Stage 0-through-Stage 8 acceptance gate.
- If Unity is already open and batchmode cannot take the project lock, use the reported live/file/log fallback for iteration, then close Unity and rerun when full batch Play Mode evidence is needed.
