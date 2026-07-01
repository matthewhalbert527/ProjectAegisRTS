# Stage 20 Production Visual Validation

Stage 20 validates the first-pass MVP production proxy layer. These prefabs are still proxy art, but they must be true 3D tabletop miniatures with all-around detail, grid-accurate bases, sockets, LOD groups, and safe fallback paths.

- Expected MVP proxies: 9
- ActorVisualDefinitions checked: 9
- Production proxy prefabs: 9
- Required socket sets valid: 9
- View coverage markers valid: 9
- LODGroups present: 9
- Errors: 0
- Warnings: 0

## MVP Actors
- light_tank
- rifle_infantry
- fabrication_hub
- barracks
- war_factory
- refinery
- harvester
- power_plant
- gun_tower

## Errors
- None

## Warnings
- None

## Validation Rules
- MVP ActorVisualDefinitions must prefer `ProductionPrefab`.
- MVP definitions must keep Stage 8 blockouts or fallback prefabs assigned.
- MVP prefabs must include `ActorPrefabDescriptor`, `ProductionVisualValidationTag`, required sockets, and an `LODGroup`.
- Buildings must mark top, front, back, left, right, and roof detail.
- Light tank and gun tower must keep turret/barrel/muzzle hooks.
- Harvester must keep track and dock hooks.
- Rifle infantry must keep head, weapon, and aim sockets.
