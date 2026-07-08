# Stage 33 Tank Source Prefabs

Generated first-pass tank source prefabs for Unity integration. These are production-source proxies, not final art.

## Prefabs
- `Assets/Rts/Art/Prefabs/Actors/Production/MVP/Tanks/light_tank_tank_source.prefab`
- `Assets/Rts/Art/Prefabs/Actors/Production/MVP/Tanks/medium_tank_tank_source.prefab`
- `Assets/Rts/Art/Prefabs/Actors/Production/MVP/Tanks/heavy_tank_tank_source.prefab`

## Review Scene

- `Assets/Rts/Scenes/Stage33_TankSourceReview.unity`

## Integration Rules

- Pivot at footprint/base center.
- Separate `BodyRoot`, `VisualRoot`, `TurretRoot`, `BarrelRoot`, `MuzzlePrimary`, `TrackLeft`, `TrackRight`, `SelectionAnchor`, `HealthBarAnchor`, `UiAnchor`, `VfxSmoke`, and `VfxExplosion`.
- Each required socket has both `Stage33TankSocket` and existing `ActorPrefabSocket` metadata.
- Each prefab has `Stage33TankSourceDescriptor`, `ActorPrefabDescriptor`, `ProductionVisualValidationTag`, `TankVisualRigController`, and `LODGroup` metadata/components.
- `TankVisualRigController` is visual-only and must never write to Rts.Core.
- ActorVisualDefinitions are updated when compatible fields are present; existing/generated blockouts remain fallback.
- Replace these proxies with artist-authored models by preserving socket names, pivot, scale, and descriptor metadata.

## Actor Visual Definitions

- `light_tank_visual.asset`, `medium_tank_visual.asset`, and `heavy_tank_visual.asset` are wired to their matching Stage33 production-source prefab.
- Generated blockout prefabs remain fallback references so runtime visual resolution stays safe if a production prefab is unavailable.
- Preferred prefab mode is production-prefab compatible where the current schema supports it.

## Preservation Notes

- No Rts.Core gameplay changes are required; Stage33 is Unity presentation/source-art integration.
- PCDesktop sidebar and safe-area behavior remain guarded by the Stage32 medium/player-facing checks.
- QuestXR left/right hand controls remain guarded by Stage4 and Stage5 checks.
- Stage27.1 placement HUD separation remains guarded by the Stage32 medium gate.
- Rts.Core must remain UnityEngine-free.

## Validation

Targeted Stage33 validation:

```powershell
.\tools\run-stage33-tank-source-generator.ps1
```

Broad pre-commit confidence:

```powershell
.\tools\run-stage32-medium-checks.ps1
```
