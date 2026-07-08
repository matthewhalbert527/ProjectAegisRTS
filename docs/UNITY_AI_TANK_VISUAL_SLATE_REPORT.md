# Unity AI Tank Visual Slate Report

Date: 2026-07-07

## Goal

Create higher-quality, player-facing tank visuals for:

- `light_tank`
- `medium_tank`
- `heavy_tank`

The reference direction is olive military armor with dark tread mass, orange hardware accents, blue optics, dense panel breakup, turret motion, moving treads, and realistic muzzle fire.

## Generated Assets

Production prefabs:

- `Assets/Rts/Art/UnityAITankSlate/Prefabs/light_tank_unity_ai_tank.prefab`
- `Assets/Rts/Art/UnityAITankSlate/Prefabs/medium_tank_unity_ai_tank.prefab`
- `Assets/Rts/Art/UnityAITankSlate/Prefabs/heavy_tank_unity_ai_tank.prefab`

Review scene:

- `Assets/Rts/Scenes/UnityAI_TankVisualReview.unity`

Screenshot:

- `build/screenshots/unity_ai_tank_visual_review.png`

## Visual Improvements

- Three distinct tank silhouettes instead of one shared blockout shape.
- Textured olive armor materials with panel lines, scratches, grime, and derived normal maps.
- Dark scrolling tread material with individual road wheels, wheel hubs, and tread cleats.
- Orange safety hardware accents for top-down readability.
- Blue armored optic material for direction and detail contrast.
- Heavy tank has twin main guns and a visible rear missile battery.
- Muzzle tips include dark bores and a visual muzzle-flash assembly.

## Animation Hooks

- `TankVisualRigController` now supports:
  - track material scrolling
  - road wheel rotation
  - suspension bob
  - turret aim from desired world direction
  - barrel recoil
  - muzzle flash and flash light timing

- `ActorViewBehaviour` drives the tank rig from presentation-only snapshot data:
  - attack target cells update turret aim direction
  - weapon cooldown reset triggers recoil and muzzle flash

No deterministic gameplay logic was moved into Unity presentation code.

## Integration

The three `ActorVisualDefinition` assets now use the Unity AI tank prefabs as production prefabs:

- `light_tank_visual.asset`
- `medium_tank_visual.asset`
- `heavy_tank_visual.asset`

The Stage33 source-prefab tanks remain fallback prefabs, preserving socket and pivot safety.

## Validation

Focused generation/validation:

```powershell
.\tools\run-unity-ai-tank-visuals.ps1
```

The validator checks:

- all three prefabs exist
- production definitions point to the generated prefabs
- required Stage33/ActorPrefab sockets are present
- tank rig has track and muzzle hooks
- renderer count is above placeholder threshold
- textured material count is above placeholder threshold
- Stage29/Stage30 visual tags are complete
- review scene exists

## Known Limitations

- The current tanks are still procedural Unity assemblies, not imported artist-authored meshes.
- The reference quality target is higher than this first Unity AI tank slate; future passes should replace the modular primitives with actual modeled tank hulls while preserving the same sockets and runtime rig hooks.
- Muzzle flash is visual-only and driven from weapon cooldown edges, not a frame-perfect projectile event.
