# Stage 21 Artist Asset Replacement Checklist

Stage 21 keeps generated MVP proxies active by default, but it adds the folders, metadata, and validation needed to stage real artist-authored MVP models safely.

## Source Drop

Place candidate model files here:

```text
unity/Assets/Rts/Art/Models/Source/MVP
```

Accepted candidate extensions are `.fbx`, `.glb`, `.gltf`, and `.obj`. Name files with the safe actor ID so the scanner can associate them with the correct MVP actor, for example `light_tank.fbx` or `war_factory.glb`.

## Required Actor IDs

- `fabrication_hub`
- `power_plant`
- `refinery`
- `barracks`
- `war_factory`
- `gun_tower`
- `rifle_infantry`
- `light_tank`
- `harvester`

## Prefab Contract

Every replacement candidate must preserve:

- `ActorPrefabDescriptor.actorTypeId`
- `ProductionVisualValidationTag`
- Required `ActorPrefabSocket` children
- Stage 8 generated blockout fallback reference
- Gameplay footprint scale and base alignment
- `LODGroup`
- Material counts within the current MVP budget

## Socket Expectations

All MVP replacements need stable root, selection, health, ground, and fallback sockets. Actor-specific hooks must also remain intact:

- Buildings: exit/rally, production, power, damage, and VFX hooks where applicable.
- Gun tower and light tank: turret, barrel, muzzle, aim, smoke, and explosion hooks.
- Rifle infantry: aim, muzzle, and selection hooks.
- Harvester/refinery: dock, unload, cargo, collector, smoke, and production hooks.

## Pivot And Scale

- Root pivot should sit at the center of the gameplay footprint on the ground plane.
- Rendered bounds should not dip below the root plane except for tiny tolerances.
- Foundation/base geometry should visually match the authoritative fine-grid footprint.
- Upper geometry may be inset, stepped, or raised, but it must not imply a larger blocked area.

## Import Scan

Run the Stage 21 import scanner and validation after adding candidates:

```powershell
.\tools\run-unity-stage21-validation.ps1 -SkipCoreBuild
```

The scanner writes `docs/STAGE21_ARTIST_MODEL_IMPORT_STATUS.md`. Candidate prefabs are staged under:

```text
unity/Assets/Rts/Art/Prefabs/Actors/Production/MVP
```

Do not assign a candidate as an active `productionPrefab` until the Stage 21 QA, socket/pivot/scale validation, Stage 21 player-facing checks, and Windows Player.log inspection are clean.
