# Art Deliverables Description - medium_tank

Preferred deliverable status: **OPTION A supplied**. This packet includes Unity-ready mesh source files and texture/VFX deliverables for a ProjectAegisRTS Medium Tank. The runtime design is a production RTS tracked vehicle with real 3D volume, separate hull/turret/barrel/tracks, strong top-down silhouette, visible armor plates, vents, bolts, hatches, exhaust, panel seams, glass optics, orange/cyan emissive accents, and large readable hull-attached team-color plates.

Delivered files:

- `unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank.obj`
- `unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank_articulated.glb`
- `unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank_hull.obj`
- `unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank_turret.obj`
- `unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank_barrel.obj`
- `unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank_track_left.obj`
- `unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank_track_right.obj`
- `unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank_team_body.obj`
- `unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank_team_turret.obj`
- `unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank_emissive_body.obj`
- `unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank_emissive_turret.obj`
- `unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank_glass_body.obj`
- `unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank_bolts.obj`
- `unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank_lod1.obj`
- `unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank_lod2.obj`
- `unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank_shell_projectile.obj`
- `unity/Assets/Rts/Art/Textures/Units/medium_tank/medium_tank_albedo.png`
- `unity/Assets/Rts/Art/Textures/Units/medium_tank/medium_tank_normal.png`
- `unity/Assets/Rts/Art/Textures/Units/medium_tank/medium_tank_orm.png`
- `unity/Assets/Rts/Art/Textures/Units/medium_tank/medium_tank_emission.png`
- `unity/Assets/Rts/Art/Textures/Units/medium_tank/medium_tank_team_mask.png`
- `unity/Assets/Rts/Art/Textures/Units/medium_tank/medium_tank_muzzle_flash_sheet.png`
- `unity/Assets/Rts/Art/Textures/Units/medium_tank/medium_tank_shell_projectile.png`
- `unity/Assets/Rts/Art/Icons/medium_tank_icon.png`
- `unity/Assets/Rts/Editor/ProjectAegisMediumTankPrefabBuilder.cs`
- `unity/Assets/Rts/Scripts/Art/Production/ProjectAegisMediumTankVisualRig.cs`
- `build/screenshots/units/medium_tank_top.png`
- `build/screenshots/units/medium_tank_front.png`
- `build/screenshots/units/medium_tank_side.png`
- `build/screenshots/units/medium_tank_three_quarter.png`
- `build/screenshots/units/medium_tank_socket_review.png`
- `build/screenshots/units/medium_tank_team_color_readability.png`

Exact dimensions in meters:

- Total visual length including barrel: **7.35 m**.
- Main hull/track footprint length: **5.40 m**.
- Visual width across tracks: **3.58 m**.
- Visual height including hatches/antennas: **2.70 m**.
- Root pivot: **0,0,0 ground center**.
- Forward axis: **+Z**.
- Up axis: **+Y**.
- Intended prefab scale: **1,1,1**.

Polygon / triangle budget:

- LOD0: approximately **7,820 triangles** including hull, turret, barrel, separate tracks, bolts/details, team panels, emissive lenses, and glass.
- LOD1: approximately **3,752 triangles** static simplified tank silhouette.
- LOD2: approximately **68 triangles** static extreme-distance silhouette.
- Target range for this unit class: **7,500-10,500 triangles**.

Team-color implementation:

- Team color is represented by separate white geometry in `medium_tank_team_body.obj` and `medium_tank_team_turret.obj` plus `medium_tank_team_mask.png`.
- Body/hull team areas include left hull side, right hull side, upper deck, front glacis, lower front strip, and rear hull panel.
- Turret team areas include turret left side, turret right side, and turret top support panels.
- Team color is intentionally not applied to the whole vehicle.

Socket summary:

| Socket | Parent | Local Position XYZ | Local Rotation XYZ | Purpose |
| --- | --- | --- | --- | --- |
| Root | yes | 0.00,0.00,0.00 | 0,0,0 | Ground center / actor root |
| VisualRoot | yes | 0.00,0.00,0.00 | 0,0,0 | Parent for model art |
| BodyRoot | yes | 0.00,0.00,0.00 | 0,0,0 | Main hull/body root |
| TurretRoot | yes | 0.00,1.52,0.12 | 0,0,0 | Yaw pivot; child of VisualRoot |
| BarrelRoot | yes | 0.00,0.12,1.02 | 0,0,0 | Pitch/recoil child; child of TurretRoot |
| MuzzlePrimary | yes | 0.00,0.00,3.88 | 0,0,0 | Barrel exit; child of BarrelRoot; default world position approximately 0.00,1.64,5.02 |
| SelectionAnchor | yes | 0.00,0.05,0.00 | 0,0,0 | Center of selection ring |
| HealthBarAnchor | yes | 0.00,3.00,0.00 | 0,0,0 | Above tallest visible point |
| UiAnchor | yes | 0.00,2.78,-0.15 | 0,0,0 | Floating UI anchor |
| VfxSmoke | yes | 1.05,1.62,-2.20 | 0,0,0 | Rear deck damage smoke/exhaust anchor |
| VfxExplosion | yes | 0.00,1.18,0.00 | 0,0,0 | Death explosion center |
| TrackLeft | yes | -1.34,0.58,0.00 | 0,0,0 | Left track animation root |
| TrackRight | yes | 1.34,0.58,0.00 | 0,0,0 | Right track animation root |

Texture-to-material assignment table:

| Material | Renderers / Parts | Albedo | Normal | ORM | Emission | Team Mask |
| --- | --- | --- | --- | --- | --- | --- |
| `mat_medium_tank_body` | HullArmor, TurretArmor, LODs | `medium_tank_albedo.png` | `medium_tank_normal.png` | `medium_tank_orm.png` | none | none |
| `mat_medium_tank_tracks` | TrackLeftMesh, TrackRightMesh | `medium_tank_albedo.png` | `medium_tank_normal.png` | `medium_tank_orm.png` | none | none |
| `mat_medium_tank_weapon` | BarrelMesh, projectile mesh | `medium_tank_albedo.png` | `medium_tank_normal.png` | `medium_tank_orm.png` | none | none |
| `mat_medium_tank_team_color` | TeamColorPanels_Body, TeamColorPanels_Turret | `medium_tank_albedo.png` | `medium_tank_normal.png` | `medium_tank_orm.png` | none | `medium_tank_team_mask.png` |
| `mat_medium_tank_emissive` | Emissive_Body, Emissive_Turret | `medium_tank_albedo.png` | `medium_tank_normal.png` | `medium_tank_orm.png` | `medium_tank_emission.png` | none |
| `mat_medium_tank_glass` | Glass_Body optics | `medium_tank_albedo.png` | `medium_tank_normal.png` | `medium_tank_orm.png` | `medium_tank_emission.png` | none |
| `mat_medium_tank_bolts` | BoltsAndFasteners | `medium_tank_albedo.png` | `medium_tank_normal.png` | `medium_tank_orm.png` | none | none |

---

# ProjectAegisRTS Unit Packet: Medium Tank

## 1. Current Unit Architecture

### Source Of Truth

- Core gameplay definitions live in `src/Rts.Core/Demo/DemoRules.cs`.
- Core data contracts live in `src/Rts.Core/Data/Definitions.cs`.
- Runtime actor state lives in `src/Rts.Core/Actors/ActorTypes.cs`.
- Unity visual definitions live in `unity/Assets/Rts/ScriptableObjects/Art/ActorVisualDefinitions/`.
- Unity actor visual prefabs live under `unity/Assets/Rts/Art/Prefabs/Actors/`.
- Generated blockout prefabs are fallback/debug art, not final player-facing art.
- Medium Tank implementation packet path: `docs/units/medium_tank_UNIT_PACKET.md`.

### Format Used Today

- Gameplay: C# rules in `DemoRules.CreateDefaultRules()`, not JSON.
- Visuals: Unity `ActorVisualDefinition` ScriptableObjects plus Unity prefabs.
- Prefab validation: `ActorPrefabDescriptor` and child `ActorPrefabSocket` components.
- Motion presentation: Unity `VisualMotionProfile` ScriptableObjects.
- Combat presentation: Unity `CombatVisualProfile` ScriptableObjects.
- Runtime fallback: if a visual definition or prefab is missing, Unity creates primitive fallback visuals. This finished Medium Tank must resolve to the production prefab and avoid the fallback path.

### Existing Examples To Copy

- Core unit definitions: `src/Rts.Core/Demo/DemoRules.cs`
  - `light_tank`
  - `medium_tank`
  - `heavy_tank`
  - `scout_rover`
  - `apc`
- Visual definition example: `unity/Assets/Rts/ScriptableObjects/Art/ActorVisualDefinitions/light_tank_visual.asset`
- Tank prefab examples:
  - `unity/Assets/Rts/Art/UnityAITankSlate/Prefabs/light_tank_unity_ai_tank.prefab`
  - `unity/Assets/Rts/Art/Prefabs/Actors/Production/MVP/Tanks/light_tank_tank_source.prefab`
- Generic fallback examples: `unity/Assets/Rts/Art/Prefabs/Actors/GeneratedBlockouts/`

### Required Core Unit Fields

```yaml
unit:
  type_id: medium_tank
  display_name: Medium Tank
  actor_kind: Unit
  category: Vehicle
  role: mid-tier main battle tank with heavier armor and one primary cannon
  max_health: 520

production:
  kind: Unit
  cost: 700
  build_time_ticks: 540
  factory_type_id: war_factory
  prerequisite_type_ids: []
  exempt_from_low_power_pause: false

movement:
  speed_per_tick: 14
  turn_rate_degrees_per_tick: 4
  visual_motion_profile_id: tracked_medium
  movement_class: Tracked

combat:
  has_weapon: true
  weapon: medium_tank_90mm_cannon

sight:
  radius_cells: 6

special:
  capture: null
  transport: null
  aircraft: null
  harvester_role: false
```

## 2. Visual Asset Requirements

### Runtime Visual Type

ProjectAegisRTS expects Unity-ready 3D prefabs for player-facing unit visuals.

- Preferred deliverable supplied: `.obj` model files plus texture maps, icon, muzzle flash sprite sheet, projectile texture/model, screenshots, prefab auto-builder, and completed unit packet.
- Acceptable reference-only deliverable: not used for this packet because mesh assets are supplied.
- Not sufficient: a single concept image, sprite sheet without pivots, or unlabelled art.

### Sprite Versus Mesh

- Required runtime format today: mesh prefab.
- Sprite facing count: not applicable for current runtime units.
- Optional review renders: included as top, front, side, three-quarter, socket-review, and team-color readability PNG files.

### Required Files

```yaml
visual_assets:
  source_model:
    path: unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank.obj
    format: obj
    real_world_units: meters
    forward_axis: +Z
    up_axis: +Y
    root_pivot: ground_center_of_footprint

  textures:
    albedo: unity/Assets/Rts/Art/Textures/Units/medium_tank/medium_tank_albedo.png
    normal: unity/Assets/Rts/Art/Textures/Units/medium_tank/medium_tank_normal.png
    orm_or_roughness: unity/Assets/Rts/Art/Textures/Units/medium_tank/medium_tank_orm.png
    emission: unity/Assets/Rts/Art/Textures/Units/medium_tank/medium_tank_emission.png
    team_mask: unity/Assets/Rts/Art/Textures/Units/medium_tank/medium_tank_team_mask.png
    projectile_albedo: unity/Assets/Rts/Art/Textures/Units/medium_tank/medium_tank_shell_projectile.png
    muzzle_flash_sheet: unity/Assets/Rts/Art/Textures/Units/medium_tank/medium_tank_muzzle_flash_sheet.png
    icon: unity/Assets/Rts/Art/Icons/medium_tank_icon.png

  reference_images:
    concept_front: build/screenshots/units/medium_tank_front.png
    concept_side: build/screenshots/units/medium_tank_side.png
    concept_top: build/screenshots/units/medium_tank_top.png
    concept_three_quarter: build/screenshots/units/medium_tank_three_quarter.png
```

Additional source model part files:

```yaml
model_parts:
  body_hull: unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank_hull.obj
  turret: unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank_turret.obj
  barrel: unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank_barrel.obj
  track_left: unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank_track_left.obj
  track_right: unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank_track_right.obj
  team_color_body: unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank_team_body.obj
  team_color_turret: unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank_team_turret.obj
  emissive_body: unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank_emissive_body.obj
  emissive_turret: unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank_emissive_turret.obj
  glass_body: unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank_glass_body.obj
  bolts: unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank_bolts.obj
  lod1: unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank_lod1.obj
  lod2: unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank_lod2.obj
  projectile_model: unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank_shell_projectile.obj
  review_glb: unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank_articulated.glb
```

### Image Dimensions

- Albedo: `2048x2048`.
- Normal: `2048x2048`.
- ORM: `2048x2048`.
- Emission: `2048x2048`.
- Team mask: `2048x2048`.
- Icon: `512x512`, transparent PNG.
- Muzzle flash sheet: `512x512`, transparent 2x2 sprite sheet.
- Projectile albedo: `512x512`, transparent PNG.
- Reference renders: top/front/side/three-quarter/team-color readability `1400x1000` or larger; socket review `1600x1100`.

### Transparency

- Icon: transparent background with clean alpha edge.
- Muzzle flash: transparent background with clean alpha edge.
- Projectile texture: transparent background.
- Team mask: opaque black/white mask.
- Main albedo: opaque.
- Glass material: material alpha may be used for optics, but main tank albedo remains opaque.

### Scale, Pivot, And Orientation

```yaml
scale_and_pivots:
  footprint_width_cells: 1
  footprint_height_cells: 1
  visual_length_meters: 7.35
  visual_width_meters: 3.58
  visual_height_meters: 2.70
  root_origin: ground_center
  forward_axis: +Z
  turret_default_yaw_degrees: 0
  selection_radius: 1.96
  selection_height: 2.78
  prefab_height_offset: 0
```

### Required Sockets

Every required socket must be a child transform with `ActorPrefabSocket` set to the matching `ActorPrefabSocketKind`.

Common sockets filled:

- `Root`
- `VisualRoot`
- `BodyRoot`
- `SelectionAnchor`
- `HealthBarAnchor`
- `UiAnchor`
- `VfxSmoke`
- `VfxExplosion`

Vehicle/tank sockets filled:

- `TurretRoot`
- `BarrelRoot`
- `MuzzlePrimary`
- `TrackLeft`
- `TrackRight`

Optional tank sockets intentionally not created:

- `MuzzleSecondary` is not created because this Medium Tank has one primary cannon.
- `WheelLeft` and `WheelRight` are not created because this is a tracked vehicle, not a wheeled vehicle.

### Socket Coordinate Table

| Socket | Required | Local Position XYZ | Local Rotation XYZ | Notes |
| --- | --- | --- | --- | --- |
| Root | yes | 0.00,0.00,0.00 | 0,0,0 | Ground center / actor root |
| VisualRoot | yes | 0.00,0.00,0.00 | 0,0,0 | Parent for model art |
| BodyRoot | yes | 0.00,0.00,0.00 | 0,0,0 | Main hull/body root |
| TurretRoot | yes | 0.00,1.52,0.12 | 0,0,0 | Yaw pivot; child of VisualRoot |
| BarrelRoot | yes | 0.00,0.12,1.02 | 0,0,0 | Pitch/recoil child; child of TurretRoot |
| MuzzlePrimary | yes | 0.00,0.00,3.88 | 0,0,0 | Barrel exit; child of BarrelRoot; default world position approximately 0.00,1.64,5.02 |
| SelectionAnchor | yes | 0.00,0.05,0.00 | 0,0,0 | Center of selection ring |
| HealthBarAnchor | yes | 0.00,3.00,0.00 | 0,0,0 | Above tallest visible point |
| UiAnchor | yes | 0.00,2.78,-0.15 | 0,0,0 | Floating UI anchor |
| VfxSmoke | yes | 1.05,1.62,-2.20 | 0,0,0 | Rear deck damage smoke/exhaust anchor |
| VfxExplosion | yes | 0.00,1.18,0.00 | 0,0,0 | Death explosion center |
| TrackLeft | yes | -1.34,0.58,0.00 | 0,0,0 | Left track animation root |
| TrackRight | yes | 1.34,0.58,0.00 | 0,0,0 | Right track animation root |

### Import Settings

- Model scale factor: `1`.
- Real-world units: meters.
- Forward axis: `+Z`.
- Up axis: `+Y`.
- Root pivot: ground center.
- Import normals and tangents when supplied.
- Generate lightmap UVs: disabled for mobile runtime unless static light baking is added.
- Materials: Unity Lit/URP-compatible material, not flat unlit placeholders.
- Albedo/icon/emission textures: `sRGB = true`.
- Normal map: mark as Normal Map.
- ORM/roughness/metallic/AO: `sRGB = false`.
- Mipmaps: enabled for runtime textures.
- Compression: ASTC or platform default high-quality compression for Quest-safe runtime use.
- Icons/VFX with alpha: keep alpha channel and use transparent-compatible material/shader.

## 3. Movement Requirements

### Current Movement System

- Deterministic movement is in `Rts.Core`.
- Pathfinding is `src/Rts.Core/Pathfinding/GridPathfinder.cs`.
- Pathfinding uses 8-way movement with diagonal step cost and prevents diagonal corner-cutting.
- Position is fixed-point grid/subcell data in core snapshots.
- Unity visuals interpolate/smooth actor movement through `ActorVisualMotionController` and `VisualMotionProfile`.

### Required Movement Fields

```yaml
movement:
  movement_class: Tracked
  speed_per_tick: 14
  turn_rate_degrees_per_tick: 4
  visual_motion_profile_id: tracked_medium
  acceleration_smoothing: 0.22
  braking_smoothing: 0.18
  turn_smoothing: 0.20
  facing_lag: 0.06
  visual_arrival_distance: 0.05
  track_or_wheel_animation_scale: 1.05
  infantry_step_rate: null
  infantry_stride_length: null
  aircraft_altitude_offset: null
  aircraft_bank_amount: null
  formation_spacing_cells: 1.25
  pathing_footprint_cells: 1
  stopping_distance_cells: 0
  slope_limit_degrees: null
  collision_radius_meters: 1.70
```

### Movement Testing

Required tests or manual checks:

- Unit accepts move order to an open destination.
- Unit rejects unreachable or impassable destinations.
- Unit can follow diagonal paths when both adjacent cardinal cells are passable.
- Unit does not cut diagonally through blocked corners.
- Unit visual faces travel direction without excessive sliding.
- Tracked motion profile visually matches a medium tank.
- Left and right track renderers can be animated independently for turn-in-place presentation.

## 4. Combat Requirements

### Current Combat System

- Weapon definitions are `WeaponDefinition` in `src/Rts.Core/Data/Definitions.cs`.
- Attack orders validate target ownership, destroyed state, target type, min/max range, and weapon targeting flags.
- Auto-targeting is used by attack-move, guard, and patrol orders.
- `WeaponFireMode.InstantHit` applies damage immediately.
- `WeaponFireMode.Projectile` creates a deterministic `ProjectileState`, rendered in Unity by `ProjectileRenderSystem`.
- Turret visual tracking is presentation-side through `TurretVisualAimController`; gameplay facing is deterministic in core snapshots.

### Required Weapon Fields

```yaml
combat:
  has_weapon: true
  weapon_id: medium_tank_90mm_cannon
  display_name: 90mm Cannon
  damage: 72
  damage_kind: Kinetic
  range_cells: 5
  min_range_cells: 0
  cooldown_ticks: 78
  fire_mode: Projectile
  projectile_kind: Shell
  projectile_speed_subcells_per_tick: 560
  projectile_lifetime_ticks: 70
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
  projectile_visual_id: medium_tank_90mm_cannon_projectile
  impact_visual_id: tank_shell_impact
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
  recoil_distance_meters: 0.10

projectile_visual:
  prefab_required: true
  projectile_mesh_or_sprite: unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank_shell_projectile.obj
  tracer_color: warm_orange
  tracer_length: 0.55
  muzzle_flash_texture: unity/Assets/Rts/Art/Textures/Units/medium_tank/medium_tank_muzzle_flash_sheet.png
  impact_effect: tank_shell_impact
```

### Damage And Armor Categories

```yaml
armor:
  armor_class: vehicle_medium
  intended_counters: [vehicle_light, vehicle_medium, structure_light]
  vulnerable_to: [vehicle_heavy, rocket_infantry, aircraft_strike]
```

### Combat Testing

Required tests or manual checks:

- Valid attack order starts combat.
- Invalid target type is rejected.
- Cooldown prevents continuous firing.
- Projectile weapons spawn a projectile snapshot.
- Projectile impact applies damage.
- Destroyed target stops accepting orders.
- Turret/muzzle visually points toward target and recoil/muzzle flash is visible.
- Unit can auto-acquire targets during attack-move/guard/patrol if applicable.

## 5. Unity Implementation Requirements

### Exact Folder Paths

```text
unity/Assets/Rts/Art/Source/Units/medium_tank/
unity/Assets/Rts/Art/Textures/Units/medium_tank/
unity/Assets/Rts/Art/Materials/Units/medium_tank/
unity/Assets/Rts/Art/Icons/medium_tank_icon.png
unity/Assets/Rts/Art/Prefabs/Actors/Production/Units/medium_tank/medium_tank.prefab
unity/Assets/Rts/ScriptableObjects/Art/ActorVisualDefinitions/medium_tank_visual.asset
unity/Assets/Rts/ScriptableObjects/Rendering/Motion/tracked_medium.asset
unity/Assets/Rts/ScriptableObjects/Rendering/Combat/medium_tank_90mm_cannon_profile.asset
unity/Assets/Rts/Scenes/medium_tank_UnitReview.unity
docs/units/medium_tank_UNIT_PACKET.md
```

### Required Prefab Hierarchy

```text
medium_tank
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
      TrackLeft [ActorPrefabSocket: TrackLeft]
      TrackRight [ActorPrefabSocket: TrackRight]
      VfxSmoke [ActorPrefabSocket: VfxSmoke]
      VfxExplosion [ActorPrefabSocket: VfxExplosion]
    SelectionAnchor [ActorPrefabSocket: SelectionAnchor]
    HealthBarAnchor [ActorPrefabSocket: HealthBarAnchor]
    UiAnchor [ActorPrefabSocket: UiAnchor]
```

### Required Components

- `ActorPrefabDescriptor`
- `ActorPrefabSocket` on every required socket child
- `LODGroup`
- `MeshFilter`/`MeshRenderer` on renderable children
- `ProjectAegisMediumTankVisualRig` for turret yaw, barrel recoil, track material phase, and team-color application
- Do not add UnityEngine references to `src/Rts.Core`.
- Do not put gameplay authority in Unity MonoBehaviours.

### ActorVisualDefinition Fields

```yaml
actor_visual_definition:
  actorTypeId: medium_tank
  displayName: Medium Tank
  safeDisplayName: Medium Tank
  category: Vehicle
  productionStatus: production_ready_art_packet
  icon: unity/Assets/Rts/Art/Icons/medium_tank_icon.png
  generatedBlockoutPrefab: unity/Assets/Rts/Art/Prefabs/Actors/GeneratedBlockouts/medium_tank_generated_blockout.prefab
  productionPrefab: unity/Assets/Rts/Art/Prefabs/Actors/Production/Units/medium_tank/medium_tank.prefab
  fallbackPrefab: unity/Assets/Rts/Art/Prefabs/Actors/GeneratedBlockouts/medium_tank_generated_blockout.prefab
  preferredPrefabMode: production
  motionProfileId: tracked_medium
  selectionRadius: 1.96
  selectionHeight: 2.78
  footprintWidth: 1
  footprintHeight: 1
  visualScale: 1
  prefabHeightOffset: 0
  useVehicleMotionController: true
  useInfantryMotionController: false
  useAircraftMotionController: false
  useTurretVisualController: true
  requiredSockets: [Root, VisualRoot, BodyRoot, TurretRoot, BarrelRoot, MuzzlePrimary, TrackLeft, TrackRight, VfxSmoke, VfxExplosion, SelectionAnchor, HealthBarAnchor, UiAnchor]
```

### Naming Conventions

- Actor id: `medium_tank`.
- Weapon id: `medium_tank_90mm_cannon`.
- Motion profile id: `tracked_medium`.
- Unity asset names: start with `medium_tank`.
- Prefab root: `medium_tank`.
- Sockets: exactly match `ActorPrefabSocketKind` names.
- Materials: `mat_medium_tank_body`, `mat_medium_tank_tracks`, `mat_medium_tank_weapon`, `mat_medium_tank_team_color`, `mat_medium_tank_emissive`, `mat_medium_tank_glass`, `mat_medium_tank_bolts`.
- Textures: `medium_tank_albedo.png`, `medium_tank_normal.png`, `medium_tank_orm.png`, `medium_tank_emission.png`, `medium_tank_team_mask.png`.

### Editor Tools Or Importers To Check

Before implementation, inspect current tools instead of inventing new ones:

- Actor visual generation/import tools under `unity/Assets/Rts/Editor/`
- Tank/building/terrain generator scripts under `unity/Assets/Rts/Scripts/Art/Production/`
- Existing validation tools under `tools/`
- Current highest medium validation script under `tools/`

## 6. Validation Requirements

### Compile And Core Tests

```powershell
cd "E:\OpenRA Mod\ProjectAegisRTS"
dotnet run --no-restore --project src/Rts.Core.Tests
.	oolsuild-rts-core-for-unity.ps1
```

If restore has never been run on the machine, run `dotnet restore` once first, then use `--no-restore` for repeated runs.

### Unity And Regression Checks

Use the current highest medium check available in `tools/`, plus targeted checks for the unit:

```powershell
.	oolsudit-medium-validation-recursion.ps1
if (Test-Path .	oolsudit-full-validation-recursion.ps1) { .	oolsudit-full-validation-recursion.ps1 }
.	oolsun-stage4-checks.ps1
.	oolsun-stage5-checks.ps1
.	oolsun-stage32-8-medium-checks.ps1
git diff --check
```

Scan for forbidden Unity references in core:

```powershell
if (Get-Command rg -ErrorAction SilentlyContinue) {
    rg "UnityEngine" .\src\Rts.Core
} else {
    Get-ChildItem ".\src\Rts.Core" -Recurse -Include *.cs | Select-String -Pattern "UnityEngine"
}
```

Any match in `src/Rts.Core` is a failure.

### Required Screenshots Or Captures

```yaml
screenshots:
  unit_review_scene: build/screenshots/units/medium_tank_socket_review.png
  player_facing_scene: build/screenshots/units/medium_tank_three_quarter.png
  optional_turntable_gif: null
```

The review screenshot must show:

- Final production prefab mesh, not primitive fallback.
- Top/side/front/three-quarter inspection renders.
- Socket markers or labels for muzzle, turret, selection, tracks, VFX, health bar, and UI anchors.
- Projectile/muzzle effect texture and projectile mesh supplied for armed combat.
- Team-color readability render showing hull-attached team panels recolored for inspection.

### Required Validation Scene

```text
unity/Assets/Rts/Scenes/medium_tank_UnitReview.unity
```

The scene must include:

- The final prefab.
- A similar existing unit for scale comparison.
- Lighting that shows surface texture and silhouette.
- Camera framing suitable for screenshot.
- Optional movement/combat sandbox if the unit is armed or animated.

### Done Means

- Core rules compile and deterministic tests pass.
- New unit is defined in `DemoRules.CreateDefaultRules()` if gameplay implementation is requested.
- Producing building list includes the new unit if it should be buildable.
- Unit has an `ActorVisualDefinition`.
- Unit resolves to a production prefab, not a generated primitive fallback.
- Prefab has required sockets and metadata.
- Textures/materials are assigned and visible.
- Movement and combat behavior match the packet.
- Rts.Core remains UnityEngine-free.
- PCDesktop sidebar remains intact.
- QuestXR Stage4/Stage5 checks remain intact.
- Stage27.1 placement HUD separation remains intact.
- Validation scene and screenshots exist.
- `git diff --check` passes.

## 7. Completed Unit Packet

```yaml
packet_version: 1
project: ProjectAegisRTS
unit:
  type_id: medium_tank
  display_name: Medium Tank
  category: Vehicle
  role: mid-tier main battle tank with heavier armor and one primary cannon
  design_summary: Medium Tank production mesh with separated hull, turret, barrel, tracks, strong top-down silhouette, visible armor plates, vents, bolts, hatches, exhaust, panel seams, orange/cyan emissive accents, and hull-attached team-color panels on front, sides, upper deck, rear, and turret.
  visual_style_reference: ProjectAegis military-industrial RTS vehicle style inspired by classic Red Alert 2 proportions, modernized for Quest 3S readable 3D mesh presentation.
  comparable_existing_unit: light_tank

core_definition:
  max_health: 520
  production:
    kind: Unit
    cost: 700
    build_time_ticks: 540
    factory_type_id: war_factory
    prerequisite_type_ids: []
    exempt_from_low_power_pause: false
  sight:
    radius_cells: 6
  movement:
    movement_class: Tracked
    speed_per_tick: 14
    turn_rate_degrees_per_tick: 4
    visual_motion_profile_id: tracked_medium
  weapon:
    has_weapon: true
    weapon_id: medium_tank_90mm_cannon
    display_name: 90mm Cannon
    damage: 72
    damage_kind: Kinetic
    range_cells: 5
    min_range_cells: 0
    cooldown_ticks: 78
    fire_mode: Projectile
    projectile_kind: Shell
    projectile_speed_subcells_per_tick: 560
    projectile_lifetime_ticks: 70
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
    projectile_visual_id: medium_tank_90mm_cannon_projectile
    impact_visual_id: tank_shell_impact
  special:
    capture: null
    transport: null
    aircraft: null

visuals:
  runtime_format: mesh_prefab
  model_files:
    - unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank.obj
    - unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank_articulated.glb
    - unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank_hull.obj
    - unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank_turret.obj
    - unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank_barrel.obj
    - unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank_track_left.obj
    - unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank_track_right.obj
    - unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank_team_body.obj
    - unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank_team_turret.obj
    - unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank_emissive_body.obj
    - unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank_emissive_turret.obj
    - unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank_glass_body.obj
    - unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank_bolts.obj
    - unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank_lod1.obj
    - unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank_lod2.obj
    - unity/Assets/Rts/Art/Source/Units/medium_tank/medium_tank_shell_projectile.obj
  texture_files:
    - unity/Assets/Rts/Art/Textures/Units/medium_tank/medium_tank_albedo.png
    - unity/Assets/Rts/Art/Textures/Units/medium_tank/medium_tank_normal.png
    - unity/Assets/Rts/Art/Textures/Units/medium_tank/medium_tank_orm.png
    - unity/Assets/Rts/Art/Textures/Units/medium_tank/medium_tank_emission.png
    - unity/Assets/Rts/Art/Textures/Units/medium_tank/medium_tank_team_mask.png
    - unity/Assets/Rts/Art/Textures/Units/medium_tank/medium_tank_muzzle_flash_sheet.png
    - unity/Assets/Rts/Art/Textures/Units/medium_tank/medium_tank_shell_projectile.png
  icon_file: unity/Assets/Rts/Art/Icons/medium_tank_icon.png
  material_plan: mat_medium_tank_body, mat_medium_tank_tracks, mat_medium_tank_weapon, mat_medium_tank_team_color, mat_medium_tank_emissive, mat_medium_tank_glass, mat_medium_tank_bolts using shared 2048 texture set plus separate team mask and emission map.
  dimensions:
    footprint_width_cells: 1
    footprint_height_cells: 1
    visual_length_meters: 7.35
    visual_width_meters: 3.58
    visual_height_meters: 2.70
    selection_radius: 1.96
    selection_height: 2.78
  sockets:
    - socket: Root
      local_position: [0.00, 0.00, 0.00]
      local_rotation: [0, 0, 0]
    - socket: VisualRoot
      local_position: [0.00, 0.00, 0.00]
      local_rotation: [0, 0, 0]
    - socket: BodyRoot
      local_position: [0.00, 0.00, 0.00]
      local_rotation: [0, 0, 0]
    - socket: TurretRoot
      local_position: [0.00, 1.52, 0.12]
      local_rotation: [0, 0, 0]
    - socket: BarrelRoot
      local_position: [0.00, 0.12, 1.02]
      local_rotation: [0, 0, 0]
    - socket: MuzzlePrimary
      local_position: [0.00, 0.00, 3.88]
      local_rotation: [0, 0, 0]
    - socket: TrackLeft
      local_position: [-1.34, 0.58, 0.00]
      local_rotation: [0, 0, 0]
    - socket: TrackRight
      local_position: [1.34, 0.58, 0.00]
      local_rotation: [0, 0, 0]
    - socket: SelectionAnchor
      local_position: [0.00, 0.05, 0.00]
      local_rotation: [0, 0, 0]
    - socket: HealthBarAnchor
      local_position: [0.00, 3.00, 0.00]
      local_rotation: [0, 0, 0]
    - socket: UiAnchor
      local_position: [0.00, 2.78, -0.15]
      local_rotation: [0, 0, 0]
    - socket: VfxSmoke
      local_position: [1.05, 1.62, -2.20]
      local_rotation: [0, 0, 0]
    - socket: VfxExplosion
      local_position: [0.00, 1.18, 0.00]
      local_rotation: [0, 0, 0]
  required_animation_hooks:
    turret_yaw: TurretRoot local_y yaw driven by TurretVisualAimController or ProjectAegisMediumTankVisualRig.SetTurretYaw
    barrel_recoil: BarrelRoot local -Z recoil using ProjectAegisMediumTankVisualRig.PlayRecoil
    tracks_or_wheels: TrackLeft and TrackRight material property _TrackPhase and optional mesh scrolling; independent left/right track animation supported
    infantry_walk: null
    aircraft_rotor_or_hover: null
    death_vfx: VfxExplosion socket plus VfxSmoke damage anchor

unity_paths:
  source_folder: unity/Assets/Rts/Art/Source/Units/medium_tank/
  textures_folder: unity/Assets/Rts/Art/Textures/Units/medium_tank/
  materials_folder: unity/Assets/Rts/Art/Materials/Units/medium_tank/
  prefab_path: unity/Assets/Rts/Art/Prefabs/Actors/Production/Units/medium_tank/medium_tank.prefab
  visual_definition_path: unity/Assets/Rts/ScriptableObjects/Art/ActorVisualDefinitions/medium_tank_visual.asset
  review_scene_path: unity/Assets/Rts/Scenes/medium_tank_UnitReview.unity

acceptance:
  must_be_buildable: true
  must_appear_in_player_facing_scene: true
  required_core_tests:
    - dotnet run --no-restore --project src/Rts.Core.Tests
    - .\tools\build-rts-core-for-unity.ps1
    - verify medium_tank remains or is added to DemoRules.CreateDefaultRules with matching values if gameplay implementation is requested
  required_unity_validations:
    - run prefab auto-builder or Codex importer for medium_tank
    - verify ActorPrefabDescriptor exists
    - verify ActorPrefabSocket components exist for Root, VisualRoot, BodyRoot, TurretRoot, BarrelRoot, MuzzlePrimary, TrackLeft, TrackRight, VfxSmoke, VfxExplosion, SelectionAnchor, HealthBarAnchor, UiAnchor
    - verify LODGroup has LOD0, LOD1, LOD2 renderers
    - verify team-color renderers are hull-attached and not only turret-attached
    - verify productionPrefab is assigned in medium_tank_visual.asset and fallback is not used in player-facing scene
    - verify Rts.Core contains no UnityEngine references
  required_screenshots:
    - build/screenshots/units/medium_tank_top.png
    - build/screenshots/units/medium_tank_front.png
    - build/screenshots/units/medium_tank_side.png
    - build/screenshots/units/medium_tank_three_quarter.png
    - build/screenshots/units/medium_tank_socket_review.png
    - build/screenshots/units/medium_tank_team_color_readability.png
```
