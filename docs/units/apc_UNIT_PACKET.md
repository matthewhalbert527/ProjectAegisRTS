# ProjectAegisRTS Unit Packet: APC

## 1. Current Unit Architecture

### Source Of Truth

- Core gameplay definitions live in `src/Rts.Core/Demo/DemoRules.cs`.
- Core data contracts live in `src/Rts.Core/Data/Definitions.cs`.
- Runtime actor state lives in `src/Rts.Core/Actors/ActorTypes.cs`.
- Unity visual definitions live in `unity/Assets/Rts/ScriptableObjects/Art/ActorVisualDefinitions/`.
- Unity actor visual prefabs live under `unity/Assets/Rts/Art/Prefabs/Actors/`.
- Generated blockout prefabs are fallback/debug art, not final player-facing art.

### Format Used Today

- Gameplay: C# rules in `DemoRules.CreateDefaultRules()`, not JSON.
- Visuals: Unity `ActorVisualDefinition` ScriptableObjects plus Unity prefabs.
- Prefab validation: `ActorPrefabDescriptor` and child `ActorPrefabSocket` components.
- Motion presentation: Unity `VisualMotionProfile` ScriptableObjects.
- Combat presentation: Unity `CombatVisualProfile` ScriptableObjects.
- Runtime fallback: if a visual definition or prefab is missing, Unity creates primitive fallback visuals. This packet targets a production mesh prefab.

### Existing Examples To Copy

- Core unit definitions: `src/Rts.Core/Demo/DemoRules.cs`.
- Visual definition example: `unity/Assets/Rts/ScriptableObjects/Art/ActorVisualDefinitions/light_tank_visual.asset`.
- Vehicle prefab examples under `unity/Assets/Rts/Art/Prefabs/Actors/Production/`.
- Generic fallback examples under `unity/Assets/Rts/Art/Prefabs/Actors/GeneratedBlockouts/`.

### Required Core Unit Fields

```yaml
unit:
  type_id: apc
  display_name: APC
  actor_kind: Unit
  category: Vehicle
  role: armored personnel carrier with roof defensive machine gun and rear troop ramp
  max_health: 540

production:
  kind: Unit
  cost: 900
  build_time_ticks: 600
  factory_type_id: war_factory
  prerequisite_type_ids: []
  exempt_from_low_power_pause: false

movement:
  speed_per_tick: 10
  turn_rate_degrees_per_tick: 5
  visual_motion_profile_id: wheeled_apc
  movement_class: Wheeled

combat:
  has_weapon: true
  weapon: apc_127mm_heavy_machine_gun

sight:
  radius_cells: 5

special:
  capture: null
  transport: { capacity: 6, allowed_categories: [Infantry], load_socket: RearRamp, unload_socket: TransportExit }
  aircraft: null
  harvester_role: false
```

## 2. Visual Asset Requirements

### Runtime Visual Type

ProjectAegisRTS expects Unity-ready 3D prefabs for player-facing unit visuals. This packet supplies actual mesh files, texture maps, icon, effect sprites, review screenshots, and import scaffolding.

### Sprite Versus Mesh

- Required runtime format today: mesh prefab.
- Sprite facing count: not applicable.
- Review renders are included for top, front, side, three-quarter, socket-review, and team-color readability.

### Required Files

```yaml
visual_assets:
  source_model:
    path: unity/Assets/Rts/Art/Source/Units/apc/apc.obj
    format: obj
    real_world_units: meters
    forward_axis: +Z
    up_axis: +Y
    root_pivot: ground_center_of_footprint

  textures:
    albedo: unity/Assets/Rts/Art/Textures/Units/apc/apc_albedo.png
    normal: unity/Assets/Rts/Art/Textures/Units/apc/apc_normal.png
    orm_or_roughness: unity/Assets/Rts/Art/Textures/Units/apc/apc_orm.png
    emission: unity/Assets/Rts/Art/Textures/Units/apc/apc_emission.png
    team_mask: unity/Assets/Rts/Art/Textures/Units/apc/apc_team_mask.png
    projectile_albedo: unity/Assets/Rts/Art/Textures/Units/apc/apc_projectile_tracer.png
    muzzle_flash_sheet: unity/Assets/Rts/Art/Textures/Units/apc/apc_muzzle_flash_sheet.png
    icon: unity/Assets/Rts/Art/Icons/apc_icon.png

  reference_images:
    concept_front: build/screenshots/units/apc_front.png
    concept_side: build/screenshots/units/apc_side.png
    concept_top: build/screenshots/units/apc_top.png
    concept_three_quarter: build/screenshots/units/apc_three_quarter.png
```

### Image Dimensions

- Albedo, normal, ORM, emission, and team mask: 2048x2048 PNG.
- Icon: 512x512 transparent PNG.
- Muzzle flash sheet: 512x512 transparent PNG.
- Projectile tracer: 256x64 transparent PNG.
- Review screenshots: 1600x1150 PNG.

### Transparency

- Icon, muzzle flash sheet, projectile tracer, glass material, and team mask use alpha where appropriate.
- Main albedo is opaque.
- Transparent textures have clean alpha edges.

### Scale, Pivot, And Orientation

```yaml
scale_and_pivots:
  footprint_width_cells: 1
  footprint_height_cells: 1
  visual_length_meters: 6.50
  visual_width_meters: 5.10
  visual_height_meters: 2.92
  root_origin: ground_center
  forward_axis: +Z
  turret_default_yaw_degrees: 0
  selection_radius: 2.50
  selection_height: 3.50
  prefab_height_offset: 0
```

### Required Sockets

Every socket listed below should become a child transform with `ActorPrefabSocket` set to the matching kind or nearest compatible project socket kind.

### Socket Coordinate Table

| Socket | Required | Local Position XYZ | Local Rotation XYZ | Notes |
| --- | --- | --- | --- | --- |
| Root | yes | 0.00,0.00,0.00 | 0,0,0 | Ground center |
| VisualRoot | yes | 0.00,0.00,0.00 | 0,0,0 | Parent for model art |
| BodyRoot | yes | 0.00,0.00,0.00 | 0,0,0 | Main armored body |
| TurretRoot | yes | 0.00,2.52,0.55 | 0,0,0 | Roof turret yaw pivot |
| BarrelRoot | yes | 0.00,2.80,0.87 | 0,0,0 | Barrel pitch/recoil root |
| MuzzlePrimary | yes | 0.00,2.80,1.69 | 0,0,0 | Weapon muzzle |
| WheelLeft | yes | -2.28,0.60,0.00 | 0,0,0 | Left wheel group |
| WheelRight | yes | 2.28,0.60,0.00 | 0,0,0 | Right wheel group |
| RearRamp | yes | 0.00,0.82,-2.91 | 0,180,0 | Rear infantry ramp |
| TransportExit | yes | 0.00,0.41,-3.35 | 0,180,0 | Infantry unload point |
| SelectionAnchor | yes | 0.00,0.11,0.17 | 0,0,0 | Selection ring center |
| HealthBarAnchor | yes | 0.00,3.26,-0.03 | 0,0,0 | Health bar anchor |
| UiAnchor | yes | 0.00,3.51,-0.03 | 0,0,0 | Floating UI anchor |
| VfxSmoke | yes | 0.96,2.26,-1.97 | 0,0,0 | Exhaust/damage smoke |
| VfxExplosion | yes | 0.00,1.26,0.17 | 0,0,0 | Death explosion center |
| LightRoot | yes | 0.00,1.00,2.89 | 0,0,0 | Headlight cluster |
| AimPivot | yes | 0.00,2.58,0.67 | 0,0,0 | Presentation aim helper |

### Import Settings

- Model scale factor: 1.
- Import normals and tangents when supplied.
- Generate lightmap UVs only if needed.
- Materials: Unity Lit/URP-compatible material, not flat unlit placeholders.
- Albedo/icon/emission textures: sRGB = true.
- Normal map: mark as Normal Map.
- ORM texture: sRGB = false.
- Mipmaps: enabled.

## 3. Movement Requirements

### Current Movement System

- Deterministic movement is in `Rts.Core`.
- Pathfinding uses 8-way movement with diagonal step cost and prevents diagonal corner-cutting.
- Unity visuals interpolate/smooth actor movement through `ActorVisualMotionController` and `VisualMotionProfile`.

### Required Movement Fields

```yaml
movement:
  movement_class: Wheeled
  speed_per_tick: 10
  turn_rate_degrees_per_tick: 5
  visual_motion_profile_id: wheeled_apc
  acceleration_smoothing: 0.16
  braking_smoothing: 0.14
  turn_smoothing: 0.18
  facing_lag: 0.06
  visual_arrival_distance: 0.08
  track_or_wheel_animation_scale: 1.00
  infantry_step_rate: null
  infantry_stride_length: null
  aircraft_altitude_offset: null
  aircraft_bank_amount: null
  formation_spacing_cells: 1.20
  pathing_footprint_cells: 1
  stopping_distance_cells: 0
  slope_limit_degrees: null
  collision_radius_meters: 1.54
```

### Movement Testing

- Unit accepts move order to an open destination.
- Unit rejects unreachable or impassable destinations.
- Unit can follow diagonal paths when both adjacent cardinal cells are passable.
- Unit does not cut diagonally through blocked corners.
- Unit visual faces travel direction without excessive sliding.
- Wheel motion profile visually matches the vehicle role.

## 4. Combat Requirements

### Current Combat System

- Weapon definitions are `WeaponDefinition` in `src/Rts.Core/Data/Definitions.cs`.
- Attack orders validate target ownership, destroyed state, target type, min/max range, and weapon targeting flags.
- Weapon is presented from `MuzzlePrimary`.

### Required Weapon Fields

```yaml
combat:
  has_weapon: true
  weapon_id: apc_127mm_heavy_machine_gun
  display_name: 12.7mm APC Machine Gun
  damage: 22
  damage_kind: Kinetic
  range_cells: 4
  min_range_cells: 0
  cooldown_ticks: 30
  fire_mode: Projectile
  projectile_kind: Bullet
  projectile_speed_subcells_per_tick: 90
  projectile_lifetime_ticks: 80
  projectile_homes_to_target: false
  can_target_ground: true
  can_target_air: false
  can_target_buildings: true
  can_target_units: true
  requires_line_of_sight: true
  burst_count: 1
  burst_delay_ticks: 0
  area_radius_cells: 0
  muzzle_socket_id: MuzzlePrimary
  projectile_visual_id: apc_127mm_heavy_machine_gun_projectile
  impact_visual_id: kinetic_impact_small
```

### Turret And Projectile Visual Contract

```yaml
turret:
  has_turret: true
  turret_root_socket: TurretRoot
  barrel_root_socket: BarrelRoot
  muzzle_socket: MuzzlePrimary
  yaw_axis: local_y
  default_forward: +Z
  recoil_distance_meters: 0.05

projectile_visual:
  prefab_required: true
  projectile_mesh_or_sprite: apc_projectile_tracer.png
  tracer_color: amber
  tracer_length: 0.55
  muzzle_flash_texture: apc_muzzle_flash_sheet.png
  impact_effect: kinetic_impact_small
```

### Damage And Armor Categories

```yaml
armor:
  armor_class: vehicle_medium
  intended_counters: [infantry_light, vehicle_light]
  vulnerable_to: [tank_cannon, rockets, mines]
```

### Combat Testing

- Valid attack order starts combat.
- Invalid target type is rejected.
- Cooldown prevents continuous firing.
- Projectile spawns from `MuzzlePrimary`.
- Turret and muzzle visually point toward target.

## 5. Unity Implementation Requirements

### Exact Folder Paths

```text
unity/Assets/Rts/Art/Source/Units/apc/
unity/Assets/Rts/Art/Textures/Units/apc/
unity/Assets/Rts/Art/Materials/Units/apc/
unity/Assets/Rts/Art/Icons/apc_icon.png
unity/Assets/Rts/Art/Prefabs/Actors/Production/Units/apc/apc.prefab
unity/Assets/Rts/ScriptableObjects/Art/ActorVisualDefinitions/apc_visual.asset
unity/Assets/Rts/Scenes/apc_UnitReview.unity
docs/units/apc_UNIT_PACKET.md
```

### Required Prefab Hierarchy

```text
apc
  ActorPrefabDescriptor
  LODGroup
  Root [ActorPrefabSocket: Root]
    VisualRoot [ActorPrefabSocket: VisualRoot]
      BodyRoot [ActorPrefabSocket: BodyRoot]
        LOD0_Meshes
        LOD1_Meshes
        LOD2_Meshes
      TurretRoot [ActorPrefabSocket: TurretRoot]
        BarrelRoot [ActorPrefabSocket: BarrelRoot]
          MuzzlePrimary [ActorPrefabSocket: MuzzlePrimary]
      WheelLeft [ActorPrefabSocket]
      WheelRight [ActorPrefabSocket]
      VfxSmoke [ActorPrefabSocket: VfxSmoke]
      VfxExplosion [ActorPrefabSocket: VfxExplosion]
    SelectionAnchor [ActorPrefabSocket: SelectionAnchor]
    HealthBarAnchor [ActorPrefabSocket: HealthBarAnchor]
    UiAnchor [ActorPrefabSocket: UiAnchor]
```

### Required Components

- ActorPrefabDescriptor
- ActorPrefabSocket on every required socket child
- LODGroup
- MeshFilter/MeshRenderer children for renderable parts
- Wheeled visual rig or equivalent vehicle motion controller
- Turret visual controller if available
- No UnityEngine references in `src/Rts.Core`

### ActorVisualDefinition Fields

```yaml
actor_visual_definition:
  actorTypeId: apc
  displayName: APC
  safeDisplayName: APC
  category: Vehicle
  productionStatus: production_candidate
  icon: apc_icon.png
  generatedBlockoutPrefab: existing_safe_fallback
  productionPrefab: unity/Assets/Rts/Art/Prefabs/Actors/Production/Units/apc/apc.prefab
  fallbackPrefab: existing_safe_fallback
  preferredPrefabMode: production
  motionProfileId: wheeled_apc
  selectionRadius: 2.05
  selectionHeight: 3.50
  footprintWidth: 1
  footprintHeight: 1
  visualScale: 1
  prefabHeightOffset: 0
  useVehicleMotionController: true
  useInfantryMotionController: false
  useAircraftMotionController: false
  useTurretVisualController: true
  requiredSockets: [Root, VisualRoot, BodyRoot, TurretRoot, BarrelRoot, MuzzlePrimary, WheelLeft, WheelRight, SelectionAnchor, HealthBarAnchor, UiAnchor, VfxSmoke, VfxExplosion]
```

### Naming Conventions

- Actor id: `apc`.
- Prefab root: `apc`.
- Materials: `mat_apc_body`, `mat_apc_team`, `mat_apc_metal`, `mat_apc_glass`, `mat_apc_emission`.
- Textures: `apc_albedo.png`, `apc_normal.png`, `apc_orm.png`, `apc_emission.png`, `apc_team_mask.png`.

### Editor Tools Or Importers To Check

- Actor visual generation/import tools under `unity/Assets/Rts/Editor/`.
- Existing validation tools under `tools/`.

## 6. Validation Requirements

### Compile And Core Tests

```powershell
dotnet run --no-restore --project src/Rts.Core.Tests
.\tools\build-rts-core-for-unity.ps1
```

### Unity And Regression Checks

```powershell
.\tools\audit-medium-validation-recursion.ps1
if (Test-Path .\tools\audit-full-validation-recursion.ps1) { .\tools\audit-full-validation-recursion.ps1 }
.\tools\run-stage4-checks.ps1
.\tools\run-stage5-checks.ps1
.\tools\run-stage32-8-medium-checks.ps1
git diff --check
```

### Required Screenshots Or Captures

```yaml
screenshots:
  unit_review_scene: build/screenshots/units/apc_socket_review.png
  player_facing_scene: build/screenshots/units/apc_three_quarter.png
  optional_turntable_gif: null
```

### Required Validation Scene

```text
unity/Assets/Rts/Scenes/apc_UnitReview.unity
```

### Done Means

- Core rules compile and deterministic tests pass.
- Unit has an ActorVisualDefinition.
- Unit resolves to a production prefab, not a generated primitive fallback.
- Prefab has required sockets and metadata.
- Textures/materials are assigned and visible.
- Movement and combat behavior match the packet.
- Rts.Core remains UnityEngine-free.
- Validation scene and screenshots exist.

## 7. Completed Unit Packet

```yaml
packet_version: 1
project: ProjectAegisRTS
unit:
  type_id: apc
  display_name: APC
  category: Vehicle
  role: armored personnel carrier with roof defensive machine gun and rear troop ramp
  design_summary: reference-match wheeled combat vehicle with stronger silhouette, attached details, separate turret/barrel, visible wheels, hull-mounted team panels, glass, lights, and no primitive fallback geometry
  visual_style_reference: provided Ironclad Federation vehicle card reference plus ProjectAegisRTS olive industrial style
  comparable_existing_unit: apc

core_definition:
  max_health: 540
  production:
    kind: Unit
    cost: 900
    build_time_ticks: 600
    factory_type_id: war_factory
    prerequisite_type_ids: []
    exempt_from_low_power_pause: false
  sight:
    radius_cells: 5
  movement:
    movement_class: Wheeled
    speed_per_tick: 10
    turn_rate_degrees_per_tick: 5
    visual_motion_profile_id: wheeled_apc
  weapon:
    has_weapon: true
    weapon_id: apc_127mm_heavy_machine_gun
    display_name: 12.7mm APC Machine Gun
    damage: 22
    damage_kind: Kinetic
    range_cells: 4
    min_range_cells: 0
    cooldown_ticks: 30
    fire_mode: Projectile
    projectile_kind: Bullet
    projectile_speed_subcells_per_tick: 90
    projectile_lifetime_ticks: 80
    projectile_homes_to_target: false
    can_target_ground: true
    can_target_air: false
    can_target_buildings: true
    can_target_units: true
    requires_line_of_sight: true
    burst_count: 1
    burst_delay_ticks: 0
    area_radius_cells: 0
    muzzle_socket_id: MuzzlePrimary
    projectile_visual_id: apc_127mm_heavy_machine_gun_projectile
    impact_visual_id: kinetic_impact_small
  special:
    capture: null
    transport: { capacity: 6, allowed_categories: [Infantry], load_socket: RearRamp, unload_socket: TransportExit }
    aircraft: null

visuals:
  runtime_format: mesh_prefab
  model_files:
    - unity/Assets/Rts/Art/Source/Units/apc/apc_hull.obj
    - unity/Assets/Rts/Art/Source/Units/apc/apc_wheel_left.obj
    - unity/Assets/Rts/Art/Source/Units/apc/apc_wheel_right.obj
    - unity/Assets/Rts/Art/Source/Units/apc/apc_turret.obj
    - unity/Assets/Rts/Art/Source/Units/apc/apc_barrel.obj
    - unity/Assets/Rts/Art/Source/Units/apc/apc_rear_ramp.obj
    - unity/Assets/Rts/Art/Source/Units/apc/apc_team_body.obj
    - unity/Assets/Rts/Art/Source/Units/apc/apc_team_turret.obj
    - unity/Assets/Rts/Art/Source/Units/apc/apc_glass.obj
    - unity/Assets/Rts/Art/Source/Units/apc/apc_emissive.obj
    - unity/Assets/Rts/Art/Source/Units/apc/apc_dark_panel_details.obj
    - unity/Assets/Rts/Art/Source/Units/apc/apc_bolts_details.obj
    - unity/Assets/Rts/Art/Source/Units/apc/apc_projectile_bullet.obj
    - unity/Assets/Rts/Art/Source/Units/apc/apc_lod1.obj
    - unity/Assets/Rts/Art/Source/Units/apc/apc_lod2.obj
  texture_files:
    - unity/Assets/Rts/Art/Textures/Units/apc/apc_albedo.png
    - unity/Assets/Rts/Art/Textures/Units/apc/apc_normal.png
    - unity/Assets/Rts/Art/Textures/Units/apc/apc_orm.png
    - unity/Assets/Rts/Art/Textures/Units/apc/apc_emission.png
    - unity/Assets/Rts/Art/Textures/Units/apc/apc_team_mask.png
    - unity/Assets/Rts/Art/Textures/Units/apc/apc_muzzle_flash_sheet.png
    - unity/Assets/Rts/Art/Textures/Units/apc/apc_projectile_tracer.png
  icon_file: unity/Assets/Rts/Art/Icons/apc_icon.png
  material_plan: body, metal, rubber/wheels, glass, emissive, team-color panels, projectile
  dimensions:
    footprint_width_cells: 1
    footprint_height_cells: 1
    visual_length_meters: 6.50
    visual_width_meters: 5.10
    visual_height_meters: 2.92
    selection_radius: 2.50
    selection_height: 3.50
  sockets:
    - socket: Root
      local_position: [0.00, 0.06, 0.17]
      local_rotation: [0, 0, 0]
    - socket: VisualRoot
      local_position: [0.00, 0.06, 0.17]
      local_rotation: [0, 0, 0]
    - socket: BodyRoot
      local_position: [0.00, 0.06, 0.17]
      local_rotation: [0, 0, 0]
    - socket: TurretRoot
      local_position: [0.00, 2.52, 0.55]
      local_rotation: [0, 0, 0]
    - socket: BarrelRoot
      local_position: [0.00, 2.80, 0.87]
      local_rotation: [0, 0, 0]
    - socket: MuzzlePrimary
      local_position: [0.00, 2.80, 1.69]
      local_rotation: [0, 0, 0]
    - socket: WheelLeft
      local_position: [-1.68, 0.68, 0.22]
      local_rotation: [0, 0, 0]
    - socket: WheelRight
      local_position: [1.68, 0.68, 0.22]
      local_rotation: [0, 0, 0]
    - socket: RearRamp
      local_position: [0.00, 0.82, -2.91]
      local_rotation: [0, 0, 0]
    - socket: TransportExit
      local_position: [0.00, 0.41, -3.35]
      local_rotation: [0, 0, 0]
    - socket: SelectionAnchor
      local_position: [0.00, 0.11, 0.17]
      local_rotation: [0, 0, 0]
    - socket: HealthBarAnchor
      local_position: [0.00, 3.26, -0.03]
      local_rotation: [0, 0, 0]
    - socket: UiAnchor
      local_position: [0.00, 3.51, -0.03]
      local_rotation: [0, 0, 0]
    - socket: VfxSmoke
      local_position: [0.96, 2.26, -1.97]
      local_rotation: [0, 0, 0]
    - socket: VfxExplosion
      local_position: [0.00, 1.26, 0.17]
      local_rotation: [0, 0, 0]
    - socket: LightRoot
      local_position: [0.00, 1.00, 2.89]
      local_rotation: [0, 0, 0]
    - socket: AimPivot
      local_position: [0.00, 2.58, 0.67]
      local_rotation: [0, 0, 0]
  required_animation_hooks:
    turret_yaw: TurretRoot
    barrel_recoil: BarrelRoot
    tracks_or_wheels: WheelLeft/WheelRight
    infantry_walk: null
    aircraft_rotor_or_hover: null
    death_vfx: VfxExplosion

unity_paths:
  source_folder: unity/Assets/Rts/Art/Source/Units/apc/
  textures_folder: unity/Assets/Rts/Art/Textures/Units/apc/
  materials_folder: unity/Assets/Rts/Art/Materials/Units/apc/
  prefab_path: unity/Assets/Rts/Art/Prefabs/Actors/Production/Units/apc/apc.prefab
  visual_definition_path: unity/Assets/Rts/ScriptableObjects/Art/ActorVisualDefinitions/apc_visual.asset
  review_scene_path: unity/Assets/Rts/Scenes/apc_UnitReview.unity

acceptance:
  must_be_buildable: true
  must_appear_in_player_facing_scene: true
  required_core_tests:
    - dotnet run --no-restore --project src/Rts.Core.Tests
    - tools/build-rts-core-for-unity.ps1
  required_unity_validations:
    - production prefab resolves instead of generated fallback
    - required sockets match packet coordinates
    - wheels, turret, barrel, muzzle flash, and team color are wired
    - team-color material changes only team panels
  required_screenshots:
    - build/screenshots/units/apc_three_quarter.png
    - build/screenshots/units/apc_top.png
    - build/screenshots/units/apc_front.png
    - build/screenshots/units/apc_side.png
    - build/screenshots/units/apc_socket_review.png
    - build/screenshots/units/apc_team_color_readability.png
```

Triangle budget:

- LOD0: 3,372 triangles.
- LOD1: 428 triangles.
- LOD2: 44 triangles.

Unity Editor validation was not run in this environment.


### Wheel Clearance Revision

The wheel meshes were moved outboard and the old separate hub/tread armor-detail components inside the tire volumes were removed. The final wheel assemblies are self-contained meshes with tire, hub, and tread geometry.
