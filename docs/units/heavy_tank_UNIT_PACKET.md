# Art Deliverables Description - heavy_tank Blocky Mammoth Rebuild

Preferred deliverable status: **OPTION A supplied**. This packet rebuilds the existing `heavy_tank` as a larger, blockier twin-cannon heavy assault tank with an eight-tube rear missile rack. It is designed as a production RTS asset for ProjectAegisRTS with real 3D volume, a broad chassis, separated turret/barrel/track/launcher parts, visible armor seams, bolts, vents, hatches, side skirts, optics, orange/cyan emissive accents, and readable white team-color panels attached to hull and turret surfaces.

Design intent:

- Replace the previous smoother heavy tank visual with a more blocky Mammoth-style silhouette.
- Keep `type_id: heavy_tank` so Codex can replace the visual asset while preserving existing Rts.Core gameplay authority.
- Provide double main barrels through `MuzzlePrimary` and `MuzzleSecondary`.
- Provide missile rack geometry plus `MuzzleMissile01` through `MuzzleMissile08` sockets for future presentation or weapon-profile expansion.
- Keep the main armor military olive/industrial, while using separate white team-color geometry on hull sides, upper hull, glacis, rear hull, turret, and missile pod.

Delivered files:

- `unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank.obj`
- `unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_articulated.glb`
- `unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_hull.obj`
- `unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_turret.obj`
- `unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_barrel.obj`
- `unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_track_left.obj`
- `unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_track_right.obj`
- `unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_team_body.obj`
- `unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_team_turret.obj`
- `unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_team_missile_pod.obj`
- `unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_missile_pod.obj`
- `unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_missile_tubes.obj`
- `unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_emissive_body.obj`
- `unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_emissive_turret.obj`
- `unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_emissive_missile_pod.obj`
- `unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_orange_accents_body.obj`
- `unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_orange_accents_turret.obj`
- `unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_glass_body.obj`
- `unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_glass_turret.obj`
- `unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_bolts.obj`
- `unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_turret_bolts.obj`
- `unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_missile_pod_bolts.obj`
- `unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_lod1.obj`
- `unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_lod2.obj`
- `unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_shell_projectile.obj`
- `unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_missile_projectile.obj`
- `unity/Assets/Rts/Art/Textures/Units/heavy_tank/heavy_tank_albedo.png`
- `unity/Assets/Rts/Art/Textures/Units/heavy_tank/heavy_tank_normal.png`
- `unity/Assets/Rts/Art/Textures/Units/heavy_tank/heavy_tank_orm.png`
- `unity/Assets/Rts/Art/Textures/Units/heavy_tank/heavy_tank_emission.png`
- `unity/Assets/Rts/Art/Textures/Units/heavy_tank/heavy_tank_team_mask.png`
- `unity/Assets/Rts/Art/Textures/Units/heavy_tank/heavy_tank_muzzle_flash_sheet.png`
- `unity/Assets/Rts/Art/Textures/Units/heavy_tank/heavy_tank_shell_projectile.png`
- `unity/Assets/Rts/Art/Textures/Units/heavy_tank/heavy_tank_missile_projectile.png`
- `unity/Assets/Rts/Art/Icons/heavy_tank_icon.png`
- `unity/Assets/Rts/Editor/ProjectAegisHeavyTankBlockyMammothPrefabBuilder.cs`
- `unity/Assets/Rts/Scripts/Art/Production/ProjectAegisHeavyTankBlockyMammothVisualRig.cs`
- `build/screenshots/units/heavy_tank_top.png`
- `build/screenshots/units/heavy_tank_front.png`
- `build/screenshots/units/heavy_tank_side.png`
- `build/screenshots/units/heavy_tank_three_quarter.png`
- `build/screenshots/units/heavy_tank_socket_review.png`
- `build/screenshots/units/heavy_tank_team_color_readability.png`

Exact dimensions in meters:

- Total visual length including twin cannon barrels: **9.20 m**.
- Main hull/track footprint length: **6.92 m**.
- Visual width across tracks and side armor: **4.72 m**.
- Visual height to missile pod top: **3.38 m**.
- Visual height including antennas: **4.15 m**.
- Root pivot: **0,0,0 ground center**.
- Forward axis: **+Z**.
- Up axis: **+Y**.
- Intended prefab scale: **1,1,1**.

Polygon / triangle budget:

- LOD0: approximately **11,436 triangles** including separated tracks, twin barrels, missile rack, team panels, bolts, optics, vents, and emissive details.
- LOD1: approximately **316 triangles** static simplified tank silhouette.
- LOD2: approximately **92 triangles** static extreme-distance silhouette.
- Target range for a few visible Quest 3S heavy tanks: **12,000-18,000 LOD0 triangles** with aggressive LOD transitions.

Team-color implementation:

- Team color is represented by separate white geometry in `heavy_tank_team_body.obj`, `heavy_tank_team_turret.obj`, and `heavy_tank_team_missile_pod.obj`, plus `heavy_tank_team_mask.png`.
- Hull team areas include left hull side, right hull side, upper deck strips, front glacis panel, lower front markers, rear hull panel, and side/rear skirt panels.
- Turret team areas include left turret side, right turret side, turret top support panels, and commander cupola top.
- Missile-rack team areas include top paneling and side vertical plates.
- Team color is intentionally not applied to the whole vehicle.

Socket summary:

| Socket | Required | Local Position XYZ | Local Rotation XYZ | Notes |
| --- | --- | --- | --- | --- |
| Root | yes | 0.00,0.00,0.00 | 0,0,0 | Ground center |
| VisualRoot | yes | 0.00,0.00,0.00 | 0,0,0 | Parent for model art |
| BodyRoot | yes | 0.00,0.00,0.00 | 0,0,0 | Main hull/body root |
| TurretRoot | yes | 0.00,1.74,0.78 | 0,0,0 | Main turret yaw pivot |
| BarrelRoot | yes | 0.00,0.12,1.12 | 0,0,0 | Child of TurretRoot; twin cannon recoil/pitch root |
| MuzzlePrimary | yes | -0.42,0.00,4.92 | 0,0,0 | Left cannon barrel exit; child of BarrelRoot |
| MuzzleSecondary | yes | 0.42,0.00,4.92 | 0,0,0 | Right cannon barrel exit; child of BarrelRoot |
| MissileLauncherRoot | yes | 1.10,1.86,-1.72 | 0,0,0 | Rear-deck missile rack yaw pivot |
| MuzzleMissile01 | yes | -0.63,0.72,0.83 | 0,0,0 | Missile tube 01; child of MissileLauncherRoot |
| MuzzleMissile02 | yes | -0.21,0.72,0.83 | 0,0,0 | Missile tube 02; child of MissileLauncherRoot |
| MuzzleMissile03 | yes | 0.21,0.72,0.83 | 0,0,0 | Missile tube 03; child of MissileLauncherRoot |
| MuzzleMissile04 | yes | 0.63,0.72,0.83 | 0,0,0 | Missile tube 04; child of MissileLauncherRoot |
| MuzzleMissile05 | yes | -0.63,1.08,0.83 | 0,0,0 | Missile tube 05; child of MissileLauncherRoot |
| MuzzleMissile06 | yes | -0.21,1.08,0.83 | 0,0,0 | Missile tube 06; child of MissileLauncherRoot |
| MuzzleMissile07 | yes | 0.21,1.08,0.83 | 0,0,0 | Missile tube 07; child of MissileLauncherRoot |
| MuzzleMissile08 | yes | 0.63,1.08,0.83 | 0,0,0 | Missile tube 08; child of MissileLauncherRoot |
| TrackLeft | yes | -1.83,0.55,0.10 | 0,0,0 | Left track animation root |
| TrackRight | yes | 1.83,0.55,0.10 | 0,0,0 | Right track animation root |
| SelectionAnchor | yes | 0.00,0.05,0.10 | 0,0,0 | Center of selection ring |
| HealthBarAnchor | yes | 0.00,4.20,0.10 | 0,0,0 | Above missile pod/antenna readable height |
| UiAnchor | yes | 0.00,3.95,-0.20 | 0,0,0 | Floating UI anchor |
| VfxSmoke | yes | 1.75,1.92,-2.75 | 0,0,0 | Rear deck smoke/exhaust anchor |
| VfxExplosion | yes | 0.00,1.35,0.10 | 0,0,0 | Death explosion center |

Texture-to-material assignment table:

| Material | Renderers / Parts | Albedo | Normal | ORM | Emission | Team Mask |
| --- | --- | --- | --- | --- | --- | --- |
| `mat_heavy_tank_body` | HullArmor, TurretArmor, MissilePod, LODs | `heavy_tank_albedo.png` | `heavy_tank_normal.png` | `heavy_tank_orm.png` | none | none |
| `mat_heavy_tank_tracks` | TrackLeftMesh, TrackRightMesh | `heavy_tank_albedo.png` | `heavy_tank_normal.png` | `heavy_tank_orm.png` | none | none |
| `mat_heavy_tank_weapon` | TwinBarrelMesh, MissileTubes, projectile meshes | `heavy_tank_albedo.png` | `heavy_tank_normal.png` | `heavy_tank_orm.png` | none | none |
| `mat_heavy_tank_team_color` | TeamColorPanels_Body, TeamColorPanels_Turret, TeamColorPanels_MissilePod | `heavy_tank_albedo.png` | `heavy_tank_normal.png` | `heavy_tank_orm.png` | none | `heavy_tank_team_mask.png` |
| `mat_heavy_tank_emissive_cyan` | Emissive_Body_Cyan, Emissive_Turret_Cyan | `heavy_tank_albedo.png` | `heavy_tank_normal.png` | `heavy_tank_orm.png` | `heavy_tank_emission.png` | none |
| `mat_heavy_tank_emissive_orange` | OrangeAccents_Body, OrangeAccents_Turret, Emissive_MissilePod_Orange | `heavy_tank_albedo.png` | `heavy_tank_normal.png` | `heavy_tank_orm.png` | `heavy_tank_emission.png` | none |
| `mat_heavy_tank_glass` | Glass_Body, Glass_Turret | `heavy_tank_albedo.png` | `heavy_tank_normal.png` | `heavy_tank_orm.png` | `heavy_tank_emission.png` | none |
| `mat_heavy_tank_bolts` | BoltsAndBodyDetails, TurretBolts, MissilePodBolts | `heavy_tank_albedo.png` | `heavy_tank_normal.png` | `heavy_tank_orm.png` | none | none |

---

# ProjectAegisRTS Unit Packet: Heavy Tank Blocky Mammoth Rebuild

## 1. Current Unit Architecture

### Source Of Truth

- Core gameplay definitions live in `src/Rts.Core/Demo/DemoRules.cs`.
- Core data contracts live in `src/Rts.Core/Data/Definitions.cs`.
- Runtime actor state lives in `src/Rts.Core/Actors/ActorTypes.cs`.
- Unity visual definitions live in `unity/Assets/Rts/ScriptableObjects/Art/ActorVisualDefinitions/`.
- Unity actor visual prefabs live under `unity/Assets/Rts/Art/Prefabs/Actors/`.
- Generated blockout prefabs are fallback/debug art, not final player-facing art.
- Heavy Tank rebuild packet path: `docs/units/heavy_tank_UNIT_PACKET.md`.

### Format Used Today

- Gameplay: C# rules in `DemoRules.CreateDefaultRules()`, not JSON.
- Visuals: Unity `ActorVisualDefinition` ScriptableObjects plus Unity prefabs.
- Prefab validation: `ActorPrefabDescriptor` and child `ActorPrefabSocket` components.
- Motion presentation: Unity `VisualMotionProfile` ScriptableObjects.
- Combat presentation: Unity `CombatVisualProfile` ScriptableObjects.
- Runtime fallback: if a visual definition or prefab is missing, Unity creates primitive fallback visuals. This finished Heavy Tank must resolve to the production prefab and avoid the fallback path.

### Existing Examples To Copy

- Core unit definitions: `src/Rts.Core/Demo/DemoRules.cs`
  - `light_tank`
  - `medium_tank`
  - `heavy_tank`
  - `scout_rover`
  - `apc`
- Visual definition example: `unity/Assets/Rts/ScriptableObjects/Art/ActorVisualDefinitions/light_tank_visual.asset`.
- Tank prefab examples:
  - `unity/Assets/Rts/Art/UnityAITankSlate/Prefabs/light_tank_unity_ai_tank.prefab`
  - `unity/Assets/Rts/Art/Prefabs/Actors/Production/MVP/Tanks/light_tank_tank_source.prefab`
- Generic fallback examples: `unity/Assets/Rts/Art/Prefabs/Actors/GeneratedBlockouts/`.

### Required Core Unit Fields

```yaml
unit:
  type_id: heavy_tank
  display_name: Heavy Tank
  actor_kind: Unit
  category: Vehicle
  role: late-tier blocky heavy assault tank with twin main cannon visuals and rear missile-rack presentation sockets
  max_health: 900

production:
  kind: Unit
  cost: 1150
  build_time_ticks: 850
  factory_type_id: war_factory
  prerequisite_type_ids: []
  exempt_from_low_power_pause: false

movement:
  speed_per_tick: 10
  turn_rate_degrees_per_tick: 3
  visual_motion_profile_id: tracked_heavy
  movement_class: Tracked

combat:
  has_weapon: true
  weapon: heavy_tank_125mm_cannon

sight:
  radius_cells: 7

special:
  capture: null
  transport: null
  aircraft: null
  harvester_role: false
```

## 2. Visual Asset Requirements

### Runtime Visual Type

ProjectAegisRTS expects Unity-ready 3D prefabs for player-facing unit visuals.

- Preferred deliverable supplied: `.obj` and `.glb` model files plus texture maps, icon, muzzle flash sprite sheet, projectile texture/model, screenshots, prefab auto-builder, and completed unit packet.
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
    path: unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank.obj
    format: obj
    real_world_units: meters
    forward_axis: +Z
    up_axis: +Y
    root_pivot: ground_center_of_footprint

  textures:
    albedo: unity/Assets/Rts/Art/Textures/Units/heavy_tank/heavy_tank_albedo.png
    normal: unity/Assets/Rts/Art/Textures/Units/heavy_tank/heavy_tank_normal.png
    orm_or_roughness: unity/Assets/Rts/Art/Textures/Units/heavy_tank/heavy_tank_orm.png
    emission: unity/Assets/Rts/Art/Textures/Units/heavy_tank/heavy_tank_emission.png
    team_mask: unity/Assets/Rts/Art/Textures/Units/heavy_tank/heavy_tank_team_mask.png
    projectile_albedo: unity/Assets/Rts/Art/Textures/Units/heavy_tank/heavy_tank_shell_projectile.png
    muzzle_flash_sheet: unity/Assets/Rts/Art/Textures/Units/heavy_tank/heavy_tank_muzzle_flash_sheet.png
    icon: unity/Assets/Rts/Art/Icons/heavy_tank_icon.png

  reference_images:
    concept_front: build/screenshots/units/heavy_tank_front.png
    concept_side: build/screenshots/units/heavy_tank_side.png
    concept_top: build/screenshots/units/heavy_tank_top.png
    concept_three_quarter: build/screenshots/units/heavy_tank_three_quarter.png
```

### Image Dimensions

- Albedo: `2048x2048`.
- Normal: `2048x2048`.
- ORM: `2048x2048`.
- Emission: `2048x2048`.
- Team mask: `2048x2048`.
- Icon: `512x512`, transparent PNG.
- Muzzle flash sheet: `512x512`, transparent PNG.
- Projectile textures: `512x512`, transparent PNG.
- Reference renders: at least `1200x850`; three-quarter and socket-review are larger.

### Transparency

- Icon, muzzle flash, projectile textures, glass masks, and team masks use alpha where appropriate.
- Main albedo is opaque.
- Transparent textures use clean alpha edges and no baked background color.

### Scale, Pivot, And Orientation

```yaml
scale_and_pivots:
  footprint_width_cells: 1
  footprint_height_cells: 1
  visual_length_meters: 9.20
  visual_width_meters: 4.72
  visual_height_meters: 4.15
  root_origin: ground_center
  forward_axis: +Z
  turret_default_yaw_degrees: 0
  selection_radius: 2.55
  selection_height: 3.40
  prefab_height_offset: 0
```

### Required Sockets

Every socket should be a child transform with `ActorPrefabSocket` set to the matching `ActorPrefabSocketKind` where the project enum supports it. The `MissileLauncherRoot` and `MuzzleMissile01`-`MuzzleMissile08` sockets are extra art sockets; if the existing enum does not contain those names, the auto-builder stores them as transform names for Codex to wire or extend.

### Socket Coordinate Table

| Socket | Required | Local Position XYZ | Local Rotation XYZ | Notes |
| --- | --- | --- | --- | --- |
| Root | yes | 0.00,0.00,0.00 | 0,0,0 | Ground center |
| VisualRoot | yes | 0.00,0.00,0.00 | 0,0,0 | Parent for model art |
| BodyRoot | yes | 0.00,0.00,0.00 | 0,0,0 | Main hull/body root |
| TurretRoot | yes | 0.00,1.74,0.78 | 0,0,0 | Main turret yaw pivot |
| BarrelRoot | yes | 0.00,0.12,1.12 | 0,0,0 | Child of TurretRoot; twin cannon recoil/pitch root |
| MuzzlePrimary | yes | -0.42,0.00,4.92 | 0,0,0 | Left cannon barrel exit; child of BarrelRoot |
| MuzzleSecondary | yes | 0.42,0.00,4.92 | 0,0,0 | Right cannon barrel exit; child of BarrelRoot |
| MissileLauncherRoot | yes | 1.10,1.86,-1.72 | 0,0,0 | Rear-deck missile rack yaw pivot |
| MuzzleMissile01 | yes | -0.63,0.72,0.83 | 0,0,0 | Missile tube 01; child of MissileLauncherRoot |
| MuzzleMissile02 | yes | -0.21,0.72,0.83 | 0,0,0 | Missile tube 02; child of MissileLauncherRoot |
| MuzzleMissile03 | yes | 0.21,0.72,0.83 | 0,0,0 | Missile tube 03; child of MissileLauncherRoot |
| MuzzleMissile04 | yes | 0.63,0.72,0.83 | 0,0,0 | Missile tube 04; child of MissileLauncherRoot |
| MuzzleMissile05 | yes | -0.63,1.08,0.83 | 0,0,0 | Missile tube 05; child of MissileLauncherRoot |
| MuzzleMissile06 | yes | -0.21,1.08,0.83 | 0,0,0 | Missile tube 06; child of MissileLauncherRoot |
| MuzzleMissile07 | yes | 0.21,1.08,0.83 | 0,0,0 | Missile tube 07; child of MissileLauncherRoot |
| MuzzleMissile08 | yes | 0.63,1.08,0.83 | 0,0,0 | Missile tube 08; child of MissileLauncherRoot |
| TrackLeft | yes | -1.83,0.55,0.10 | 0,0,0 | Left track animation root |
| TrackRight | yes | 1.83,0.55,0.10 | 0,0,0 | Right track animation root |
| SelectionAnchor | yes | 0.00,0.05,0.10 | 0,0,0 | Center of selection ring |
| HealthBarAnchor | yes | 0.00,4.20,0.10 | 0,0,0 | Above missile pod/antenna readable height |
| UiAnchor | yes | 0.00,3.95,-0.20 | 0,0,0 | Floating UI anchor |
| VfxSmoke | yes | 1.75,1.92,-2.75 | 0,0,0 | Rear deck smoke/exhaust anchor |
| VfxExplosion | yes | 0.00,1.35,0.10 | 0,0,0 | Death explosion center |

### Import Settings

- Model scale factor: `1`.
- Import normals and tangents when supplied.
- Generate lightmap UVs: disabled unless the project needs baked lighting.
- Materials: Unity Lit/URP-compatible materials.
- Albedo/icon/emission textures: `sRGB = true`.
- Normal map: mark as Normal Map.
- ORM texture: `sRGB = false`.
- Mipmaps: enabled for runtime textures.
- Compression: normal/high quality; use Quest-safe compression when building Android.
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
  speed_per_tick: 10
  turn_rate_degrees_per_tick: 3
  visual_motion_profile_id: tracked_heavy
  acceleration_smoothing: 0.16
  braking_smoothing: 0.18
  turn_smoothing: 0.22
  facing_lag: 0.08
  visual_arrival_distance: 0.08
  track_or_wheel_animation_scale: 0.75
  infantry_step_rate: null
  infantry_stride_length: null
  aircraft_altitude_offset: null
  aircraft_bank_amount: null
  formation_spacing_cells: 1.40
  pathing_footprint_cells: 1
  stopping_distance_cells: 0
  slope_limit_degrees: null
  collision_radius_meters: 2.30
```

### Movement Testing

Required tests or manual checks:

- Unit accepts move order to an open destination.
- Unit rejects unreachable or impassable destinations.
- Unit can follow diagonal paths when both adjacent cardinal cells are passable.
- Unit does not cut diagonally through blocked corners.
- Unit visual faces travel direction without excessive sliding.
- Tracked motion profile visually matches a slow, heavy vehicle.
- Left and right track roots are present for future tread material scrolling or track animation.

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
  weapon_id: heavy_tank_125mm_cannon
  display_name: Heavy 125mm Cannon
  damage: 110
  damage_kind: Explosive
  range_cells: 7
  min_range_cells: 0
  cooldown_ticks: 65
  fire_mode: Projectile
  projectile_kind: Shell
  projectile_speed_subcells_per_tick: 400
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
  projectile_visual_id: heavy_tank_125mm_shell_projectile
  impact_visual_id: heavy_shell_impact
```

### Turret And Projectile Visual Contract

```yaml
turret:
  has_turret: true
  turret_root_socket: TurretRoot
  barrel_root_socket: BarrelRoot
  muzzle_socket: MuzzlePrimary
  secondary_muzzle_socket: MuzzleSecondary
  missile_launcher_root_socket: MissileLauncherRoot
  missile_muzzle_sockets: [MuzzleMissile01, MuzzleMissile02, MuzzleMissile03, MuzzleMissile04, MuzzleMissile05, MuzzleMissile06, MuzzleMissile07, MuzzleMissile08]
  yaw_axis: local_y
  default_forward: +Z
  recoil_distance_meters: 0.18

projectile_visual:
  prefab_required: true
  projectile_mesh_or_sprite: unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_shell_projectile.obj
  tracer_color: orange_white
  tracer_length: 0.70
  muzzle_flash_texture: unity/Assets/Rts/Art/Textures/Units/heavy_tank/heavy_tank_muzzle_flash_sheet.png
  impact_effect: heavy_shell_impact
```

### Damage And Armor Categories

```yaml
armor:
  armor_class: vehicle_heavy
  intended_counters: [structures, vehicles_medium, vehicles_heavy]
  vulnerable_to: [rocket_soldier, heavy_tank, attack_aircraft, advanced_gun_tower]
```

### Combat Testing

Required tests or manual checks:

- Valid attack order starts combat.
- Invalid target type is rejected.
- Cooldown prevents continuous firing.
- Projectile weapons spawn a projectile snapshot.
- Projectile impact applies damage.
- Destroyed target stops accepting orders.
- Main turret and `MuzzlePrimary` point toward target.
- `MuzzleSecondary` is available for visual alternation even if current core fires one gameplay projectile.
- Rear missile pod and eight missile muzzle sockets are available for future visual salvos or a later multi-weapon extension.
- Unit can auto-acquire ground targets during attack-move, guard, or patrol when current rules support it.

## 5. Unity Implementation Requirements

### Exact Folder Paths

```text
unity/Assets/Rts/Art/Source/Units/heavy_tank/
unity/Assets/Rts/Art/Textures/Units/heavy_tank/
unity/Assets/Rts/Art/Materials/Units/heavy_tank/
unity/Assets/Rts/Art/Icons/heavy_tank_icon.png
unity/Assets/Rts/Art/Prefabs/Actors/Production/Units/heavy_tank/heavy_tank.prefab
unity/Assets/Rts/ScriptableObjects/Art/ActorVisualDefinitions/heavy_tank_visual.asset
unity/Assets/Rts/ScriptableObjects/Rendering/Motion/tracked_heavy.asset
unity/Assets/Rts/ScriptableObjects/Rendering/Combat/heavy_tank_125mm_cannon.asset
unity/Assets/Rts/Scenes/heavy_tank_UnitReview.unity
docs/units/heavy_tank_UNIT_PACKET.md
```

### Required Prefab Hierarchy

```text
heavy_tank
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
          MuzzleSecondary [ActorPrefabSocket: MuzzleSecondary]
      MissileLauncherRoot [transform socket]
        MuzzleMissile01
        MuzzleMissile02
        MuzzleMissile03
        MuzzleMissile04
        MuzzleMissile05
        MuzzleMissile06
        MuzzleMissile07
        MuzzleMissile08
      TrackLeft [ActorPrefabSocket: TrackLeft]
      TrackRight [ActorPrefabSocket: TrackRight]
      VfxSmoke [ActorPrefabSocket: VfxSmoke]
      VfxExplosion [ActorPrefabSocket: VfxExplosion]
    SelectionAnchor [ActorPrefabSocket: SelectionAnchor]
    HealthBarAnchor [ActorPrefabSocket: HealthBarAnchor]
    UiAnchor [ActorPrefabSocket: UiAnchor]
```

### Required Components

- `ActorPrefabDescriptor`.
- `ActorPrefabSocket` on every required socket child where supported by the current enum.
- `LODGroup`.
- `MeshFilter`/`MeshRenderer` on renderable children after Unity imports OBJ files.
- `ProjectAegisHeavyTankBlockyMammothVisualRig` for team color, turret, missile launcher idle scan, barrel recoil, and socket references.
- Do not add UnityEngine references to `src/Rts.Core`.
- Do not put gameplay authority in Unity MonoBehaviours.

### ActorVisualDefinition Fields

```yaml
actor_visual_definition:
  actorTypeId: heavy_tank
  displayName: Heavy Tank
  safeDisplayName: Heavy Tank
  category: Vehicle
  productionStatus: production_ready_after_unity_import
  icon: unity/Assets/Rts/Art/Icons/heavy_tank_icon.png
  generatedBlockoutPrefab: unity/Assets/Rts/Art/Prefabs/Actors/GeneratedBlockouts/heavy_tank_generated_blockout.prefab
  productionPrefab: unity/Assets/Rts/Art/Prefabs/Actors/Production/Units/heavy_tank/heavy_tank.prefab
  fallbackPrefab: unity/Assets/Rts/Art/Prefabs/Actors/GeneratedBlockouts/heavy_tank_generated_blockout.prefab
  preferredPrefabMode: production
  motionProfileId: tracked_heavy
  selectionRadius: 2.55
  selectionHeight: 3.40
  footprintWidth: 1
  footprintHeight: 1
  visualScale: 1
  prefabHeightOffset: 0
  useVehicleMotionController: true
  useInfantryMotionController: false
  useAircraftMotionController: false
  useTurretVisualController: true
  requiredSockets: [Root, VisualRoot, BodyRoot, TurretRoot, BarrelRoot, MuzzlePrimary, MuzzleSecondary, TrackLeft, TrackRight, VfxSmoke, VfxExplosion, SelectionAnchor, HealthBarAnchor, UiAnchor]
```

### Naming Conventions

- Actor id: `heavy_tank`.
- Weapon id: `heavy_tank_125mm_cannon`.
- Motion profile id: `tracked_heavy`.
- Unity asset names start with `heavy_tank`.
- Prefab root: `heavy_tank`.
- Sockets: names match `ActorPrefabSocketKind` where supported; additional missile sockets use explicit transform names.
- Materials: `mat_heavy_tank_body`, `mat_heavy_tank_tracks`, `mat_heavy_tank_weapon`, `mat_heavy_tank_team_color`, `mat_heavy_tank_emissive_cyan`, `mat_heavy_tank_emissive_orange`, `mat_heavy_tank_glass`, `mat_heavy_tank_bolts`.
- Textures: `heavy_tank_albedo.png`, `heavy_tank_normal.png`, `heavy_tank_orm.png`, `heavy_tank_emission.png`, `heavy_tank_team_mask.png`.

### Editor Tools Or Importers To Check

Before implementation, inspect current tools instead of inventing new ones:

- Actor visual generation/import tools under `unity/Assets/Rts/Editor/`.
- Tank/building/terrain generator scripts under `unity/Assets/Rts/Scripts/Art/Production/`.
- Existing validation tools under `tools/`.
- Current highest medium validation script under `tools/`.

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
.	ools
un-stage4-checks.ps1
.	ools
un-stage5-checks.ps1
.	ools
un-stage32-8-medium-checks.ps1
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
  unit_review_scene: build/screenshots/units/heavy_tank_socket_review.png
  player_facing_scene: build/screenshots/units/heavy_tank_three_quarter.png
  optional_turntable_gif: build/screenshots/units/heavy_tank_turntable.gif
  top: build/screenshots/units/heavy_tank_top.png
  front: build/screenshots/units/heavy_tank_front.png
  side: build/screenshots/units/heavy_tank_side.png
  team_color_readability: build/screenshots/units/heavy_tank_team_color_readability.png
```

The review screenshot must show:

- Final production prefab, not primitive fallback.
- Top/side/front or turntable inspection if practical.
- Socket markers or labels for muzzle, turret, selection, and UI anchors.
- Projectile/muzzle effect if the unit is armed.
- Scale comparison against existing similar unit when the Unity review scene is created.

### Required Validation Scene

```text
unity/Assets/Rts/Scenes/heavy_tank_UnitReview.unity
```

The scene must include:

- The final prefab.
- A similar existing unit for scale comparison.
- Lighting that shows surface texture and silhouette.
- Camera framing suitable for screenshots.
- Optional movement/combat sandbox because the unit is armed and animated.

### Done Means

- Core rules compile and deterministic tests pass.
- `heavy_tank` remains defined in `DemoRules.CreateDefaultRules()`.
- Producing building list preserves the existing heavy tank buildability settings.
- Unit has an `ActorVisualDefinition` pointing to the production prefab.
- Unit resolves to a production prefab, not a generated primitive fallback.
- Prefab has required sockets and metadata.
- Textures/materials are assigned and visible.
- Movement and combat behavior match this packet.
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
  type_id: heavy_tank
  display_name: Heavy Tank
  category: Vehicle
  role: late-tier blocky heavy assault tank
  design_summary: Larger blockier heavy tank rebuild with broad chassis, twin cannon barrels, eight-tube rear missile rack, reinforced tracks, layered armor blocks, readable hull-attached team-color panels, cyan optics, orange emissive accents, vents, bolts, hatches, side skirts, exhausts, and separate animatable turret/track/launcher parts.
  visual_style_reference: ProjectAegis RTS heavy-tank menu artwork with blocky dual-barrel Mammoth-inspired silhouette and rear missile pod, adapted as original production mesh geometry.
  comparable_existing_unit: medium_tank and previous heavy_tank

core_definition:
  max_health: 900
  production:
    kind: Unit
    cost: 1150
    build_time_ticks: 850
    factory_type_id: war_factory
    prerequisite_type_ids: []
    exempt_from_low_power_pause: false
  sight:
    radius_cells: 7
  movement:
    movement_class: Tracked
    speed_per_tick: 10
    turn_rate_degrees_per_tick: 3
    visual_motion_profile_id: tracked_heavy
  weapon:
    has_weapon: true
    weapon_id: heavy_tank_125mm_cannon
    display_name: Heavy 125mm Cannon
    damage: 110
    damage_kind: Explosive
    range_cells: 7
    min_range_cells: 0
    cooldown_ticks: 65
    fire_mode: Projectile
    projectile_kind: Shell
    projectile_speed_subcells_per_tick: 400
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
    projectile_visual_id: heavy_tank_125mm_shell_projectile
    impact_visual_id: heavy_shell_impact
  special:
    capture: null
    transport: null
    aircraft: null

visuals:
  runtime_format: mesh_prefab
  model_files:
    - unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank.obj
    - unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_articulated.glb
    - unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_hull.obj
    - unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_turret.obj
    - unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_barrel.obj
    - unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_track_left.obj
    - unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_track_right.obj
    - unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_team_body.obj
    - unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_team_turret.obj
    - unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_team_missile_pod.obj
    - unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_missile_pod.obj
    - unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_missile_tubes.obj
    - unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_emissive_body.obj
    - unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_emissive_turret.obj
    - unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_emissive_missile_pod.obj
    - unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_orange_accents_body.obj
    - unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_orange_accents_turret.obj
    - unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_glass_body.obj
    - unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_glass_turret.obj
    - unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_bolts.obj
    - unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_turret_bolts.obj
    - unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_missile_pod_bolts.obj
    - unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_lod1.obj
    - unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_lod2.obj
    - unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_shell_projectile.obj
    - unity/Assets/Rts/Art/Source/Units/heavy_tank/heavy_tank_missile_projectile.obj
  texture_files:
    - unity/Assets/Rts/Art/Textures/Units/heavy_tank/heavy_tank_albedo.png
    - unity/Assets/Rts/Art/Textures/Units/heavy_tank/heavy_tank_normal.png
    - unity/Assets/Rts/Art/Textures/Units/heavy_tank/heavy_tank_orm.png
    - unity/Assets/Rts/Art/Textures/Units/heavy_tank/heavy_tank_emission.png
    - unity/Assets/Rts/Art/Textures/Units/heavy_tank/heavy_tank_team_mask.png
    - unity/Assets/Rts/Art/Textures/Units/heavy_tank/heavy_tank_muzzle_flash_sheet.png
    - unity/Assets/Rts/Art/Textures/Units/heavy_tank/heavy_tank_shell_projectile.png
    - unity/Assets/Rts/Art/Textures/Units/heavy_tank/heavy_tank_missile_projectile.png
  icon_file: unity/Assets/Rts/Art/Icons/heavy_tank_icon.png
  material_plan: mat_heavy_tank_body for armor/turret/launcher/LODs; mat_heavy_tank_tracks for tracks; mat_heavy_tank_weapon for twin barrels and missile tubes; mat_heavy_tank_team_color for white recolorable body/turret/launcher panels; mat_heavy_tank_emissive_cyan for cyan optics/lights; mat_heavy_tank_emissive_orange for orange accents and missile tube caps; mat_heavy_tank_glass for optics; mat_heavy_tank_bolts for bolts, vents, seams, and grilles.
  dimensions:
    footprint_width_cells: 1
    footprint_height_cells: 1
    visual_length_meters: 9.20
    visual_width_meters: 4.72
    visual_height_meters: 4.15
    selection_radius: 2.55
    selection_height: 3.40
  sockets:
    - socket: Root
      local_position: [0.00, 0.00, 0.00]
      local_rotation: [0, 0, 0]
      notes: Ground center
    - socket: VisualRoot
      local_position: [0.00, 0.00, 0.00]
      local_rotation: [0, 0, 0]
      notes: Parent for model art
    - socket: BodyRoot
      local_position: [0.00, 0.00, 0.00]
      local_rotation: [0, 0, 0]
      notes: Main hull/body root
    - socket: TurretRoot
      local_position: [0.00, 1.74, 0.78]
      local_rotation: [0, 0, 0]
      notes: Main turret yaw pivot
    - socket: BarrelRoot
      local_position: [0.00, 0.12, 1.12]
      local_rotation: [0, 0, 0]
      notes: Child of TurretRoot; twin cannon recoil/pitch root
    - socket: MuzzlePrimary
      local_position: [-0.42, 0.00, 4.92]
      local_rotation: [0, 0, 0]
      notes: Left cannon barrel exit; child of BarrelRoot
    - socket: MuzzleSecondary
      local_position: [0.42, 0.00, 4.92]
      local_rotation: [0, 0, 0]
      notes: Right cannon barrel exit; child of BarrelRoot
    - socket: MissileLauncherRoot
      local_position: [1.10, 1.86, -1.72]
      local_rotation: [0, 0, 0]
      notes: Rear-deck missile rack yaw pivot
    - socket: MuzzleMissile01
      local_position: [-0.63, 0.72, 0.83]
      local_rotation: [0, 0, 0]
      notes: Missile tube 01; child of MissileLauncherRoot
    - socket: MuzzleMissile02
      local_position: [-0.21, 0.72, 0.83]
      local_rotation: [0, 0, 0]
      notes: Missile tube 02; child of MissileLauncherRoot
    - socket: MuzzleMissile03
      local_position: [0.21, 0.72, 0.83]
      local_rotation: [0, 0, 0]
      notes: Missile tube 03; child of MissileLauncherRoot
    - socket: MuzzleMissile04
      local_position: [0.63, 0.72, 0.83]
      local_rotation: [0, 0, 0]
      notes: Missile tube 04; child of MissileLauncherRoot
    - socket: MuzzleMissile05
      local_position: [-0.63, 1.08, 0.83]
      local_rotation: [0, 0, 0]
      notes: Missile tube 05; child of MissileLauncherRoot
    - socket: MuzzleMissile06
      local_position: [-0.21, 1.08, 0.83]
      local_rotation: [0, 0, 0]
      notes: Missile tube 06; child of MissileLauncherRoot
    - socket: MuzzleMissile07
      local_position: [0.21, 1.08, 0.83]
      local_rotation: [0, 0, 0]
      notes: Missile tube 07; child of MissileLauncherRoot
    - socket: MuzzleMissile08
      local_position: [0.63, 1.08, 0.83]
      local_rotation: [0, 0, 0]
      notes: Missile tube 08; child of MissileLauncherRoot
    - socket: TrackLeft
      local_position: [-1.83, 0.55, 0.10]
      local_rotation: [0, 0, 0]
      notes: Left track animation root
    - socket: TrackRight
      local_position: [1.83, 0.55, 0.10]
      local_rotation: [0, 0, 0]
      notes: Right track animation root
    - socket: SelectionAnchor
      local_position: [0.00, 0.05, 0.10]
      local_rotation: [0, 0, 0]
      notes: Center of selection ring
    - socket: HealthBarAnchor
      local_position: [0.00, 4.20, 0.10]
      local_rotation: [0, 0, 0]
      notes: Above missile pod/antenna readable height
    - socket: UiAnchor
      local_position: [0.00, 3.95, -0.20]
      local_rotation: [0, 0, 0]
      notes: Floating UI anchor
    - socket: VfxSmoke
      local_position: [1.75, 1.92, -2.75]
      local_rotation: [0, 0, 0]
      notes: Rear deck smoke/exhaust anchor
    - socket: VfxExplosion
      local_position: [0.00, 1.35, 0.10]
      local_rotation: [0, 0, 0]
      notes: Death explosion center
  required_animation_hooks:
    turret_yaw: TurretRoot local_y yaw
    barrel_recoil: BarrelRoot local_z recoil distance 0.18 meters
    tracks_or_wheels: TrackLeft and TrackRight material-scroll or tread animation hooks
    infantry_walk: null
    aircraft_rotor_or_hover: null
    missile_launcher_yaw: MissileLauncherRoot local_y yaw or idle scan
    missile_salvo_sockets: [MuzzleMissile01, MuzzleMissile02, MuzzleMissile03, MuzzleMissile04, MuzzleMissile05, MuzzleMissile06, MuzzleMissile07, MuzzleMissile08]
    death_vfx: VfxExplosion plus VfxSmoke

unity_paths:
  source_folder: unity/Assets/Rts/Art/Source/Units/heavy_tank/
  textures_folder: unity/Assets/Rts/Art/Textures/Units/heavy_tank/
  materials_folder: unity/Assets/Rts/Art/Materials/Units/heavy_tank/
  prefab_path: unity/Assets/Rts/Art/Prefabs/Actors/Production/Units/heavy_tank/heavy_tank.prefab
  visual_definition_path: unity/Assets/Rts/ScriptableObjects/Art/ActorVisualDefinitions/heavy_tank_visual.asset
  review_scene_path: unity/Assets/Rts/Scenes/heavy_tank_UnitReview.unity

acceptance:
  must_be_buildable: true
  must_appear_in_player_facing_scene: true
  required_core_tests:
    - dotnet run --no-restore --project src/Rts.Core.Tests
    - tools/build-rts-core-for-unity.ps1
  required_unity_validations:
    - import OBJ/GLB and textures
    - run ProjectAegisRTS/Art/Build Heavy Tank Blocky Mammoth Prefab
    - confirm production prefab replaces generated fallback for heavy_tank
    - verify ActorPrefabDescriptor exists
    - verify ActorPrefabSocket or equivalent transform exists for Root, VisualRoot, BodyRoot, TurretRoot, BarrelRoot, MuzzlePrimary, MuzzleSecondary, TrackLeft, TrackRight, VfxSmoke, VfxExplosion, SelectionAnchor, HealthBarAnchor, UiAnchor
    - verify team color changes only white team panel renderers
    - verify twin barrels, missile pod, and heavy blocky silhouette are readable from RTS camera
  required_screenshots:
    - build/screenshots/units/heavy_tank_top.png
    - build/screenshots/units/heavy_tank_front.png
    - build/screenshots/units/heavy_tank_side.png
    - build/screenshots/units/heavy_tank_three_quarter.png
    - build/screenshots/units/heavy_tank_socket_review.png
    - build/screenshots/units/heavy_tank_team_color_readability.png
```
