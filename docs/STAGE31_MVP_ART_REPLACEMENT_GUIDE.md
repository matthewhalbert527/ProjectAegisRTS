# Stage 31 MVP Art Replacement Guide

Stage 31 refines the MVP replacement contract for real artist-authored models. It does not replace proxies by itself.

## Replacement Sequence

1. Choose exactly one MVP actor.
2. Read that actor's checklist in `docs/STAGE31_PER_ACTOR_PRODUCTION_CHECKLIST.md`.
3. Put source files under `unity/Assets/Rts/Art/Models/Source/MVP` using the safe actor ID in the filename.
4. Run `.\tools\run-unity-stage21-validation.ps1 -SkipCoreBuild` to scan candidate models.
5. Create or update a candidate prefab under `unity/Assets/Rts/Art/Prefabs/Actors/Production/MVP`.
6. Preserve root pivot, footprint scale, `ActorPrefabDescriptor`, `ProductionVisualValidationTag`, required `ActorPrefabSocket` children, fallback references, and `LODGroup`.
7. Validate with Stage 21, Stage 29, Stage 30, and Stage 31 checks before making the candidate active.

## Replacement Rules

- Keep sockets stable. Animation, combat, production, power-state, VFX, selection, and UI hooks depend on those names and transforms.
- Keep the Stage 8 generated blockout as fallback until the real model has passed player-facing validation.
- Keep the Stage 29/30 visual intent: readable battlefield materials, grounded foundations, clear top-down identity, and player-readable role cues.
- Keep real model geometry within the authoritative footprint unless overhangs are purely decorative and do not confuse placement.
- Do not change `Rts.Core` actor definitions, production costs, placement footprints, pathing, combat, or AI while replacing art.

## Validation Commands

Use this order for one actor replacement:

```powershell
.\tools\run-unity-stage21-validation.ps1 -SkipCoreBuild
.\tools\run-unity-stage29-validation.ps1 -SkipCoreBuild
.\tools\run-unity-stage30-validation.ps1 -SkipCoreBuild
.\tools\run-stage31-medium-checks.ps1
```

Before accepting the replacement for playtest:

```powershell
.\tools\run-stage31-player-facing-checks.ps1 -SkipPlayerBuild
.\tools\build-windows-player-stage16.ps1
.\tools\inspect-latest-player-log.ps1 -CopyToDebugLogs -RequireDisplayStartup
```

## Per-Actor Safe IDs

- `fabrication_hub`
- `power_plant`
- `refinery`
- `barracks`
- `war_factory`
- `gun_tower`
- `rifle_infantry`
- `light_tank`
- `harvester`
