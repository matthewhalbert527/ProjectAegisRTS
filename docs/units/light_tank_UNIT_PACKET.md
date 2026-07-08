# Art Deliverables Description - light_tank

Preferred deliverable status: **OPTION A supplied**. This packet includes Unity-ready mesh source files and texture/VFX deliverables for a ProjectAegisRTS Light Tank. The runtime design is a compact, fast tracked tank with a cast-style rounded turret, long single cannon, five-wheel track silhouette, readable white team-color plates, cyan optics, orange marker lights, visible tread blocks, exhaust/smoke anchor, turret yaw, barrel recoil, and animatable left/right tracks.

Delivered files:

- `unity/Assets/Rts/Art/Source/Units/light_tank/light_tank.obj` - static combined OBJ, meters, +Z forward, +Y up, root pivot at ground center.
- `unity/Assets/Rts/Art/Source/Units/light_tank/light_tank_articulated.glb` - review GLB with named geometry nodes.
- `unity/Assets/Rts/Art/Source/Units/light_tank/light_tank_hull.obj`
- `unity/Assets/Rts/Art/Source/Units/light_tank/light_tank_turret.obj`
- `unity/Assets/Rts/Art/Source/Units/light_tank/light_tank_barrel.obj`
- `unity/Assets/Rts/Art/Source/Units/light_tank/light_tank_track_left.obj`
- `unity/Assets/Rts/Art/Source/Units/light_tank/light_tank_track_right.obj`
- `unity/Assets/Rts/Art/Source/Units/light_tank/light_tank_team_body.obj`
- `unity/Assets/Rts/Art/Source/Units/light_tank/light_tank_team_turret.obj`
- `unity/Assets/Rts/Art/Source/Units/light_tank/light_tank_emissive_body.obj`
- `unity/Assets/Rts/Art/Source/Units/light_tank/light_tank_emissive_turret.obj`
- `unity/Assets/Rts/Art/Source/Units/light_tank/light_tank_glass_body.obj`
- `unity/Assets/Rts/Art/Source/Units/light_tank/light_tank_bolts.obj`
- `unity/Assets/Rts/Art/Source/Units/light_tank/light_tank_lod1.obj`
- `unity/Assets/Rts/Art/Source/Units/light_tank/light_tank_lod2.obj`
- `unity/Assets/Rts/Art/Textures/Units/light_tank/light_tank_albedo.png` - 2048x2048.
- `unity/Assets/Rts/Art/Textures/Units/light_tank/light_tank_normal.png` - 2048x2048.
- `unity/Assets/Rts/Art/Textures/Units/light_tank/light_tank_orm.png` - 2048x2048; R=AO, G=roughness, B=metallic, A=unused/opaque.
- `unity/Assets/Rts/Art/Textures/Units/light_tank/light_tank_team_mask.png` - 2048x2048.
- `unity/Assets/Rts/Art/Textures/Units/light_tank/light_tank_emission.png` - 2048x2048.
- `unity/Assets/Rts/Art/Textures/Units/light_tank/light_tank_muzzle_flash_sheet.png` - 512x512 transparent 2x2 muzzle flash sheet.
- `unity/Assets/Rts/Art/Textures/Units/light_tank/light_tank_shell_projectile.png` - 512x512 optional projectile texture.
- `unity/Assets/Rts/Art/Source/Units/light_tank/light_tank_shell_projectile.obj` - optional shell projectile mesh.
- `unity/Assets/Rts/Art/Icons/light_tank_icon.png` - 512x512 transparent icon rendered from the generated mesh.
- `build/screenshots/units/light_tank_top.png`
- `build/screenshots/units/light_tank_front.png`
- `build/screenshots/units/light_tank_side.png`
- `build/screenshots/units/light_tank_three_quarter.png`
- `build/screenshots/units/light_tank_socket_review.png`

Exact dimensions in meters:

- Total visual length including barrel: **5.98 m**.
- Main hull/track footprint length: **4.05 m**.
- Visual width across tracks: **3.10 m**.
- Visual height including turret antenna: **2.27 m**.
- Root pivot: **0,0,0 ground center**.
- Forward axis: **+Z**.
- Up axis: **+Y**.
- Intended prefab scale: **1,1,1**.

Polygon / triangle budget:

- LOD0: approximately **7,776 triangles** including hull, turret, barrel, separate tracks, bolts, team panels, emissive lenses, and glass.
- LOD1: approximately **568 triangles** static simplified tank silhouette.
- LOD2: approximately **~160 triangles** static extreme-distance silhouette.

Socket summary:

| Socket | Parent | Local Position XYZ | Local Rotation XYZ | Purpose |
| --- | --- | --- | --- | --- |
| Root | prefab | 0,0,0 | 0,0,0 | Ground center / actor root |
| VisualRoot | Root | 0,0,0 | 0,0,0 | Parent for visible model |
| BodyRoot | VisualRoot | 0,0,0 | 0,0,0 | Main hull/body root |
| TurretRoot | VisualRoot | 0,1.36,-0.08 | 0,0,0 | Turret yaw pivot |
| BarrelRoot | TurretRoot | 0,0.10,0.66 | 0,0,0 | Barrel pitch/recoil pivot |
| MuzzlePrimary | BarrelRoot | 0,0,3.40 | 0,0,0 | Actual barrel tip |
| TrackLeft | VisualRoot | -1.14,0.52,0 | 0,0,0 | Left track animation root |
| TrackRight | VisualRoot | 1.14,0.52,0 | 0,0,0 | Right track animation root |
| SelectionAnchor | Root | 0,0.05,0 | 0,0,0 | Selection ring center |
| HealthBarAnchor | Root | 0,2.65,0 | 0,0,0 | Health bar above antenna/turret |
| UiAnchor | Root | 0,2.45,-0.15 | 0,0,0 | Floating UI anchor |
| VfxSmoke | VisualRoot | 0.65,1.70,-1.45 | 0,0,0 | Damage smoke/exhaust origin |
| VfxExplosion | VisualRoot | 0,1.10,0 | 0,0,0 | Destruction explosion center |

Texture-to-material assignment table:

| Material | Renderers / Parts | Albedo | Normal | ORM | Emission | Team Mask |
| --- | --- | --- | --- | --- | --- | --- |
| `mat_light_tank_body` | HullArmor, TurretArmor, LODs | `light_tank_albedo.png` | `light_tank_normal.png` | `light_tank_orm.png` | none | none |
| `mat_light_tank_tracks` | TrackLeftMesh, TrackRightMesh | `light_tank_albedo.png` | `light_tank_normal.png` | `light_tank_orm.png` | none | none |
| `mat_light_tank_weapon` | BarrelMesh, projectile mesh | `light_tank_albedo.png` | `light_tank_normal.png` | `light_tank_orm.png` | none | none |
| `mat_light_tank_team_color` | TeamColorPanels_Body, TeamColorPanels_Turret | `light_tank_albedo.png` | `light_tank_normal.png` | `light_tank_orm.png` | none | `light_tank_team_mask.png` |
| `mat_light_tank_emissive` | Emissive_Body, Emissive_Turret | `light_tank_albedo.png` | `light_tank_normal.png` | `light_tank_orm.png` | `light_tank_emission.png` | none |
| `mat_light_tank_glass` | Glass_Body optics | `light_tank_albedo.png` | `light_tank_normal.png` | `light_tank_orm.png` | `light_tank_emission.png` | none |
| `mat_light_tank_bolts` | BoltsAndFasteners | `light_tank_albedo.png` | `light_tank_normal.png` | `light_tank_orm.png` | none | none |

---

# ProjectAegisRTS Unit Packet: Light Tank

## 1. Current Unit Architecture

### Source Of Truth

- Core gameplay definitions live in `src/Rts.Core/Demo/DemoRules.cs`.
- Core data contracts live in `src/Rts.Core/Data/Definitions.cs`.
- Runtime actor state lives in `src/Rts.Core/Actors/ActorTypes.cs`.
- Unity visual definitions live in `unity/Assets/Rts/ScriptableObjects/Art/ActorVisualDefinitions/`.
- Unity actor visual prefabs live under `unity/Assets/Rts/Art/Prefabs/Actors/`.
- Generated blockout prefabs are fallback/debug art, not final player-facing art.
- Light Tank implementation packet path: `docs/units/light_tank_UNIT_PACKET.md`.

### Format Used Today

- Gameplay: C# rules in `DemoRules.CreateDefaultRules()`, not JSON.
- Visuals: Unity `ActorVisualDefinition` ScriptableObjects plus Unity prefabs.
- Prefab validation: `ActorPrefabDescriptor` and child `ActorPrefabSocket` components.
- Motion presentation: Unity `VisualMotionProfile` ScriptableObjects.
- Combat presentation: Unity `CombatVisualProfile` ScriptableObjects.
- Runtime fallback: if a visual definition or prefab is missing, Unity creates primitive fallback visuals. This finished Light Tank must resolve to the production prefab and avoid the fallback path.

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
  type_id: light_tank
  display_name: Light Tank
  actor_kind: Unit
  category: Vehicle
  role: fast tracked skirmisher and early armored assault unit
  max_health: 320

production:
  kind: Unit
  cost: 450
  build_time_ticks: 360
  factory_type_id: war_factory
  prerequisite_type_ids: []
  exempt_from_low_power_pause: false

movement:
  speed_per_tick: 18
  turn_rate_degrees_per_tick: 5
  visual_motion_profile_id: tracked_light
  movement_class: Tracked

combat:
  has_weapon: true
  weapon: light_tank_76mm_cannon

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
- Optional review renders: included as top, front, side, three-quarter, and socket-review PNG files for validation and art review.

### Required Files

```yaml
visual_assets:
  source_model:
    path: unity/Assets/Rts/Art/Source/Units/light_tank/light_tank.obj
    format: obj
    real_world_units: meters
    forward_axis: +Z
    up_axis: +Y
    root_pivot: ground_center_of_footprint

  textures:
    albedo: unity/Assets/Rts/Art/Textures/Units/light_tank/light_tank_albedo.png
    normal: unity/Assets/Rts/Art/Textures/Units/light_tank/light_tank_normal.png
    orm_or_roughness: unity/Assets/Rts/Art/Textures/Units/light_tank/light_tank_orm.png
    emission: unity/Assets/Rts/Art/Textures/Units/light_tank/light_tank_emission.png
    team_mask: unity/Assets/Rts/Art/Textures/Units/light_tank/light_tank_team_mask.png
    projectile_albedo: unity/Assets/Rts/Art/Textures/Units/light_tank/light_tank_shell_projectile.png
    muzzle_flash_sheet: unity/Assets/Rts/Art/Textures/Units/light_tank/light_tank_muzzle_flash_sheet.png
    icon: unity/Assets/Rts/Art/Icons/light_tank_icon.png

  reference_images:
    concept_front: build/screenshots/units/light_tank_front.png
    concept_side: build/screenshots/units/light_tank_side.png
    concept_top: build/screenshots/units/light_tank_top.png
    concept_three_quarter: build/screenshots/units/light_tank_three_quarter.png
```

Additional source model part files:

```yaml
model_parts:
  body_hull: unity/Assets/Rts/Art/Source/Units/light_tank/light_tank_hull.obj
  turret: unity/Assets/Rts/Art/Source/Units/light_tank/light_tank_turret.obj
  barrel: unity/Assets/Rts/Art/Source/Units/light_tank/light_tank_barrel.obj
  track_left: unity/Assets/Rts/Art/Source/Units/light_tank/light_tank_track_left.obj
  track_right: unity/Assets/Rts/Art/Source/Units/light_tank/light_tank_track_right.obj
  team_color_body: unity/Assets/Rts/Art/Source/Units/light_tank/light_tank_team_body.obj
  team_color_turret: unity/Assets/Rts/Art/Source/Units/light_tank/light_tank_team_turret.obj
  emissive_body: unity/Assets/Rts/Art/Source/Units/light_tank/light_tank_emissive_body.obj
  emissive_turret: unity/Assets/Rts/Art/Source/Units/light_tank/light_tank_emissive_turret.obj
  glass_body: unity/Assets/Rts/Art/Source/Units/light_tank/light_tank_glass_body.obj
  bolts: unity/Assets/Rts/Art/Source/Units/light_tank/light_tank_bolts.obj
  lod1: unity/Assets/Rts/Art/Source/Units/light_tank/light_tank_lod1.obj
  lod2: unity/Assets/Rts/Art/Source/Units/light_tank/light_tank_lod2.obj
  projectile_model: unity/Assets/Rts/Art/Source/Units/light_tank/light_tank_shell_projectile.obj
  review_glb: unity/Assets/Rts/Art/Source/Units/light_tank/light_tank_articulated.glb
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
- Reference renders: `1400x1000` for top/front/side/three-quarter and `1600x1100` for socket review.

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
  visual_length_meters: 5.98
  visual_width_meters: 3.10
  visual_height_meters: 2.27
  root_origin: ground_center
  forward_axis: +Z
  turret_default_yaw_degrees: 0
  selection_radius: 1.70
  selection_height: 2.40
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

- `MuzzleSecondary` is not created because the Light Tank has a single cannon.
- `WheelLeft` and `WheelRight` are not created because this is a tracked vehicle, not a wheeled vehicle.

### Socket Coordinate Table

| Socket | Required | Local Position XYZ | Local Rotation XYZ | Notes |
| --- | --- | --- | --- | --- |
| Root | yes | 0,0,0 | 0,0,0 | Ground center |
| VisualRoot | yes | 0,0,0 | 0,0,0 | Parent for model art |
| BodyRoot | yes | 0,0,0 | 0,0,0 | Main hull/body |
| TurretRoot | yes | 0,1.36,-0.08 | 0,0,0 | Yaw pivot; child of VisualRoot |
| BarrelRoot | yes | 0,0.10,0.66 | 0,0,0 | Pitch/recoil child; child of TurretRoot |
| MuzzlePrimary | yes | 0,0,3.40 | 0,0,0 | Barrel exit; child of BarrelRoot; world position at default yaw/pitch is approximately 0,1.46,3.98 |
| SelectionAnchor | yes | 0,0.05,0 | 0,0,0 | Center of selection ring |
| HealthBarAnchor | yes | 0,2.65,0 | 0,0,0 | Above tallest visible point |
| UiAnchor | yes | 0,2.45,-0.15 | 0,0,0 | Floating UI anchor |
| VfxSmoke | yes | 0.65,1.70,-1.45 | 0,0,0 | Engine/damage smoke anchor on rear deck |
| VfxExplosion | yes | 0,1.10,0 | 0,0,0 | Death explosion center |
| TrackLeft | yes | -1.14,0.52,0 | 0,0,0 | Left track animation root |
| TrackRight | yes | 1.14,0.52,0 | 0,0,0 | Right track animation root |

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
- Mipmaps: enabled for runtime albedo/normal/ORM/emission; disabled or optional for icon/VFX depending on UI/VFX importer.
- Compression: ASTC or platform-default high-quality mobile compression for Quest; keep texture count fixed and use material instancing.
- Icons/VFX with alpha: keep alpha channel and use transparent-compatible material/shader.

Texture-to-material assignment table:

| Material | Parts | Albedo | Normal | ORM | Emission | Team Mask |
| --- | --- | --- | --- | --- | --- | --- |
| `mat_light_tank_body` | HullArmor, TurretArmor, LOD1_Static, LOD2_Static | `light_tank_albedo.png` | `light_tank_normal.png` | `light_tank_orm.png` | none | none |
| `mat_light_tank_tracks` | TrackLeftMesh, TrackRightMesh | `light_tank_albedo.png` | `light_tank_normal.png` | `light_tank_orm.png` | none | none |
| `mat_light_tank_weapon` | BarrelMesh, shell projectile | `light_tank_albedo.png` | `light_tank_normal.png` | `light_tank_orm.png` | none | none |
| `mat_light_tank_team_color` | TeamColorPanels_Body, TeamColorPanels_Turret | `light_tank_albedo.png` | `light_tank_normal.png` | `light_tank_orm.png` | none | `light_tank_team_mask.png` |
| `mat_light_tank_emissive` | Emissive_Body, Emissive_Turret | `light_tank_albedo.png` | `light_tank_normal.png` | `light_tank_orm.png` | `light_tank_emission.png` | none |
| `mat_light_tank_glass` | Glass_Body | `light_tank_albedo.png` | `light_tank_normal.png` | `light_tank_orm.png` | `light_tank_emission.png` | none |
| `mat_light_tank_bolts` | BoltsAndFasteners | `light_tank_albedo.png` | `light_tank_normal.png` | `light_tank_orm.png` | none | none |

LOD plan:

| LOD | Approximate Triangles | Use |
| --- | ---: | --- |
| LOD0 | 7776 | Close RTS camera and unit review; separate turret/barrel/tracks/team/emissive pieces |
| LOD1 | 568 | Mid-distance static simplified silhouette |
| LOD2 | 160 | Far-distance silhouette |
| Culled | 0 | Extreme distance/off-screen |

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
  speed_per_tick: 18
  turn_rate_degrees_per_tick: 5
  visual_motion_profile_id: tracked_light
  acceleration_smoothing: 10.0
  braking_smoothing: 14.0
  turn_smoothing: 12.0
  facing_lag: 0.08
  visual_arrival_distance: 0.03
  track_or_wheel_animation_scale: 2.4
  infantry_step_rate: null
  infantry_stride_length: null
  aircraft_altitude_offset: null
  aircraft_bank_amount: null
  formation_spacing_cells: 1.15
  pathing_footprint_cells: 1
  stopping_distance_cells: 0
  slope_limit_degrees: null
  collision_radius_meters: 1.55
```

### Movement Testing

Required tests or manual checks:

- Unit accepts move order to an open destination.
- Unit rejects unreachable or impassable destinations.
- Unit can follow diagonal paths when both adjacent cardinal cells are passable.
- Unit does not cut diagonally through blocked corners.
- Unit visual faces travel direction without excessive sliding.
- Tracked visual motion profile scrolls or pulses `TrackLeft` and `TrackRight` consistently with movement speed.
- Track animation reverses cleanly when the unit turns in place or reverses if that behavior is supported.
- Unit formation spacing remains readable at 1.15 cells.

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
  weapon_id: light_tank_76mm_cannon
  display_name: 76mm Light Cannon
  damage: 36
  damage_kind: Kinetic
  range_cells: 5
  min_range_cells: 0
  cooldown_ticks: 50
  fire_mode: Projectile
  projectile_kind: Shell
  projectile_speed_subcells_per_tick: 72
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
  projectile_visual_id: light_tank_76mm_shell
  impact_visual_id: tank_shell_impact_small
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
  recoil_distance_meters: 0.08

projectile_visual:
  prefab_required: true
  projectile_mesh_or_sprite: unity/Assets/Rts/Art/Source/Units/light_tank/light_tank_shell_projectile.obj
  tracer_color: '#F2C36A'
  tracer_length: 0.45
  muzzle_flash_texture: unity/Assets/Rts/Art/Textures/Units/light_tank/light_tank_muzzle_flash_sheet.png
  impact_effect: tank_shell_impact_small
```

### Damage And Armor Categories

```yaml
armor:
  armor_class: vehicle_light
  intended_counters:
    - infantry_light
    - resource_harvesters
    - scout_rover
    - light_vehicles
  vulnerable_to:
    - rocket_soldier
    - heavy_tank
    - turret
    - advanced_gun_tower
    - attack_aircraft
```

### Combat Testing

Required tests or manual checks:

- Valid attack order starts combat against enemy ground unit.
- Valid attack order starts combat against enemy building.
- Invalid air target is rejected because `can_target_air` is false.
- Cooldown prevents continuous firing faster than `cooldown_ticks: 50`.
- Projectile weapons spawn a deterministic projectile snapshot.
- Projectile impact applies 36 kinetic damage.
- Destroyed target stops accepting orders.
- Turret root visually yaws toward target.
- Barrel root recoils by up to 0.08 meters on fire.
- Muzzle flash appears at `MuzzlePrimary`.
- Unit can auto-acquire valid ground targets during attack-move/guard/patrol.

## 5. Unity Implementation Requirements

### Exact Folder Paths

```text
unity/Assets/Rts/Art/Source/Units/light_tank/
unity/Assets/Rts/Art/Textures/Units/light_tank/
unity/Assets/Rts/Art/Materials/Units/light_tank/
unity/Assets/Rts/Art/Icons/light_tank_icon.png
unity/Assets/Rts/Art/Prefabs/Actors/Production/Units/light_tank/light_tank.prefab
unity/Assets/Rts/ScriptableObjects/Art/ActorVisualDefinitions/light_tank_visual.asset
unity/Assets/Rts/ScriptableObjects/Rendering/Motion/tracked_light.asset
unity/Assets/Rts/ScriptableObjects/Rendering/Combat/light_tank_76mm_cannon_profile.asset
unity/Assets/Rts/Scenes/light_tank_UnitReview.unity
docs/units/light_tank_UNIT_PACKET.md
```

### Required Prefab Hierarchy

```text
light_tank
  ActorPrefabDescriptor
  LODGroup
  Root [ActorPrefabSocket: Root]
    VisualRoot [ActorPrefabSocket: VisualRoot]
      BodyRoot [ActorPrefabSocket: BodyRoot]
        LOD0_Meshes
          HullArmor
          TeamColorPanels_Body
          Emissive_Body
          Glass_Body
          BoltsAndFasteners
        LOD1_Meshes
          LOD1_Static
        LOD2_Meshes
          LOD2_Static
      TurretRoot [ActorPrefabSocket: TurretRoot]
        TurretArmor
        TeamColorPanels_Turret
        Emissive_Turret
        BarrelRoot [ActorPrefabSocket: BarrelRoot]
          BarrelMesh
          MuzzlePrimary [ActorPrefabSocket: MuzzlePrimary]
      TrackLeft [ActorPrefabSocket: TrackLeft]
        TrackLeftMesh
      TrackRight [ActorPrefabSocket: TrackRight]
        TrackRightMesh
      VfxSmoke [ActorPrefabSocket: VfxSmoke]
      VfxExplosion [ActorPrefabSocket: VfxExplosion]
    SelectionAnchor [ActorPrefabSocket: SelectionAnchor]
    HealthBarAnchor [ActorPrefabSocket: HealthBarAnchor]
    UiAnchor [ActorPrefabSocket: UiAnchor]
```

### Required Components

- `ActorPrefabDescriptor` on prefab root `light_tank`.
- `ActorPrefabSocket` on every required socket child.
- `LODGroup` on prefab root.
- `BoxCollider` on prefab root with center `0,1.08,0.05` and size `3.10,2.20,4.25`.
- `MeshFilter`/`MeshRenderer` on imported mesh children.
- `ProjectAegisLightTankVisualRig` or equivalent Unity-side rig controller.
- `TankVisualRigController` may be substituted if it already supports `TurretRoot`, `BarrelRoot`, `MuzzlePrimary`, `TrackLeft`, and `TrackRight` without changing gameplay authority.
- Do not add UnityEngine references to `src/Rts.Core`.
- Do not put gameplay authority in Unity MonoBehaviours.

### ActorVisualDefinition Fields

```yaml
actor_visual_definition:
  actorTypeId: light_tank
  displayName: Light Tank
  safeDisplayName: Light Tank
  category: Vehicle
  productionStatus: production
  icon: unity/Assets/Rts/Art/Icons/light_tank_icon.png
  generatedBlockoutPrefab: unity/Assets/Rts/Art/Prefabs/Actors/GeneratedBlockouts/light_tank_generated_blockout.prefab
  productionPrefab: unity/Assets/Rts/Art/Prefabs/Actors/Production/Units/light_tank/light_tank.prefab
  fallbackPrefab: unity/Assets/Rts/Art/Prefabs/Actors/GeneratedBlockouts/light_tank_generated_blockout.prefab
  preferredPrefabMode: production
  motionProfileId: tracked_light
  selectionRadius: 1.70
  selectionHeight: 2.40
  footprintWidth: 1.0
  footprintHeight: 1.0
  visualScale: 1
  prefabHeightOffset: 0
  useVehicleMotionController: true
  useInfantryMotionController: false
  useAircraftMotionController: false
  useTurretVisualController: true
  requiredSockets:
    - Root
    - VisualRoot
    - BodyRoot
    - SelectionAnchor
    - HealthBarAnchor
    - UiAnchor
    - VfxSmoke
    - VfxExplosion
    - TurretRoot
    - BarrelRoot
    - MuzzlePrimary
    - TrackLeft
    - TrackRight
```

### Naming Conventions

- Actor id: `light_tank`.
- Weapon id: `light_tank_76mm_cannon`.
- Projectile visual id: `light_tank_76mm_shell`.
- Motion profile id: `tracked_light`.
- Unity asset names start with `light_tank`.
- Prefab root: `light_tank`.
- Sockets exactly match `ActorPrefabSocketKind` names.
- Materials:
  - `mat_light_tank_body`
  - `mat_light_tank_tracks`
  - `mat_light_tank_weapon`
  - `mat_light_tank_team_color`
  - `mat_light_tank_emissive`
  - `mat_light_tank_glass`
  - `mat_light_tank_bolts`
- Textures:
  - `light_tank_albedo.png`
  - `light_tank_normal.png`
  - `light_tank_orm.png`
  - `light_tank_emission.png`
  - `light_tank_team_mask.png`
  - `light_tank_muzzle_flash_sheet.png`
  - `light_tank_shell_projectile.png`
  - `light_tank_icon.png`

### Editor Tools Or Importers To Check

Before implementation, inspect current tools instead of inventing new ones:

- Actor visual generation/import tools under `unity/Assets/Rts/Editor/`.
- Tank/building/terrain generator scripts under `unity/Assets/Rts/Scripts/Art/Production/`.
- Existing validation tools under `tools/`.
- Current highest medium validation script under `tools/`.
- Supplied helper script: `unity/Assets/Rts/Editor/ProjectAegisLightTankPrefabBuilder.cs`.
- Supplied runtime helper script: `unity/Assets/Rts/Scripts/Art/Production/ProjectAegisLightTankVisualRig.cs`.

## 6. Validation Requirements

### Compile And Core Tests

```powershell
cd "E:\OpenRA Mod\ProjectAegisRTS"
dotnet run --no-restore --project src/Rts.Core.Tests
.\tools\build-rts-core-for-unity.ps1
```

If restore has never been run on the machine, run `dotnet restore` once first, then use `--no-restore` for repeated runs.

### Unity And Regression Checks

Use the current highest medium check available in `tools/`, plus targeted checks for the unit:

```powershell
.\tools\audit-medium-validation-recursion.ps1
if (Test-Path .\tools\audit-full-validation-recursion.ps1) { .\tools\audit-full-validation-recursion.ps1 }
.\tools\run-stage4-checks.ps1
.\tools\run-stage5-checks.ps1
.\tools\run-stage32-8-medium-checks.ps1
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
  unit_review_scene: build/screenshots/units/light_tank_review.png
  player_facing_scene: build/screenshots/units/light_tank_player_facing.png
  optional_turntable_gif: build/screenshots/units/light_tank_turntable.gif
```

Supplied review screenshots:

```yaml
supplied_screenshots:
  top: build/screenshots/units/light_tank_top.png
  front: build/screenshots/units/light_tank_front.png
  side: build/screenshots/units/light_tank_side.png
  three_quarter: build/screenshots/units/light_tank_three_quarter.png
  socket_review: build/screenshots/units/light_tank_socket_review.png
```

The review screenshot must show:

- Final production prefab, not primitive fallback.
- Top/side/front or turntable inspection if practical.
- Socket markers or labels for muzzle, turret, selection, and UI anchors.
- Projectile/muzzle effect if the unit is armed.
- Scale comparison against existing similar unit.

### Required Validation Scene

```text
unity/Assets/Rts/Scenes/light_tank_UnitReview.unity
```

The scene must include:

- The final prefab.
- A similar existing unit for scale comparison, preferably `medium_tank` or the previous light tank prefab.
- Lighting that shows surface texture and silhouette.
- Camera framing suitable for screenshot.
- Movement/combat sandbox because the unit moves and fires.
- Socket marker overlays for `TurretRoot`, `BarrelRoot`, `MuzzlePrimary`, `VfxSmoke`, `VfxExplosion`, `SelectionAnchor`, `HealthBarAnchor`, and `UiAnchor`.

### Done Means

- Core rules compile and deterministic tests pass.
- Unit is defined in `DemoRules.CreateDefaultRules()` as `light_tank` if gameplay implementation is requested.
- `war_factory` producing building list includes `light_tank`.
- Unit has an `ActorVisualDefinition` at `unity/Assets/Rts/ScriptableObjects/Art/ActorVisualDefinitions/light_tank_visual.asset`.
- Unit resolves to production prefab `unity/Assets/Rts/Art/Prefabs/Actors/Production/Units/light_tank/light_tank.prefab`, not a generated primitive fallback.
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
  type_id: light_tank
  display_name: Light Tank
  category: Vehicle
  role: fast tracked skirmisher and early armored assault unit
  design_summary: Compact tracked light tank with cast-style rounded turret, long single cannon, five-wheel tread silhouette, white team-color armor panels, cyan optics, orange marker lights, visible treads, vents, bolts, hatches, exhaust, panel seams, subtle weathering, and a readable RTS camera silhouette.
  visual_style_reference: Production RTS asset inspired by Red Alert 2-era military readability without copying specific Red Alert unit art; classic olive armor, white team-color panels, orange/cyan tech accents, exaggerated silhouette for zoomed-out play.
  comparable_existing_unit: light_tank

core_definition:
  max_health: 320
  production:
    kind: Unit
    cost: 450
    build_time_ticks: 360
    factory_type_id: war_factory
    prerequisite_type_ids: []
    exempt_from_low_power_pause: false
  sight:
    radius_cells: 6
  movement:
    movement_class: Tracked
    speed_per_tick: 18
    turn_rate_degrees_per_tick: 5
    visual_motion_profile_id: tracked_light
  weapon:
    has_weapon: true
    weapon_id: light_tank_76mm_cannon
    display_name: 76mm Light Cannon
    damage: 36
    damage_kind: Kinetic
    range_cells: 5
    min_range_cells: 0
    cooldown_ticks: 50
    fire_mode: Projectile
    projectile_kind: Shell
    projectile_speed_subcells_per_tick: 72
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
    projectile_visual_id: light_tank_76mm_shell
    impact_visual_id: tank_shell_impact_small
  special:
    capture: null
    transport: null
    aircraft: null

visuals:
  runtime_format: mesh_prefab
  model_files:
    - unity/Assets/Rts/Art/Source/Units/light_tank/light_tank.obj
    - unity/Assets/Rts/Art/Source/Units/light_tank/light_tank_articulated.glb
    - unity/Assets/Rts/Art/Source/Units/light_tank/light_tank_hull.obj
    - unity/Assets/Rts/Art/Source/Units/light_tank/light_tank_turret.obj
    - unity/Assets/Rts/Art/Source/Units/light_tank/light_tank_barrel.obj
    - unity/Assets/Rts/Art/Source/Units/light_tank/light_tank_track_left.obj
    - unity/Assets/Rts/Art/Source/Units/light_tank/light_tank_track_right.obj
    - unity/Assets/Rts/Art/Source/Units/light_tank/light_tank_team_body.obj
    - unity/Assets/Rts/Art/Source/Units/light_tank/light_tank_team_turret.obj
    - unity/Assets/Rts/Art/Source/Units/light_tank/light_tank_emissive_body.obj
    - unity/Assets/Rts/Art/Source/Units/light_tank/light_tank_emissive_turret.obj
    - unity/Assets/Rts/Art/Source/Units/light_tank/light_tank_glass_body.obj
    - unity/Assets/Rts/Art/Source/Units/light_tank/light_tank_bolts.obj
    - unity/Assets/Rts/Art/Source/Units/light_tank/light_tank_lod1.obj
    - unity/Assets/Rts/Art/Source/Units/light_tank/light_tank_lod2.obj
    - unity/Assets/Rts/Art/Source/Units/light_tank/light_tank_shell_projectile.obj
  texture_files:
    - unity/Assets/Rts/Art/Textures/Units/light_tank/light_tank_albedo.png
    - unity/Assets/Rts/Art/Textures/Units/light_tank/light_tank_normal.png
    - unity/Assets/Rts/Art/Textures/Units/light_tank/light_tank_orm.png
    - unity/Assets/Rts/Art/Textures/Units/light_tank/light_tank_emission.png
    - unity/Assets/Rts/Art/Textures/Units/light_tank/light_tank_team_mask.png
    - unity/Assets/Rts/Art/Textures/Units/light_tank/light_tank_muzzle_flash_sheet.png
    - unity/Assets/Rts/Art/Textures/Units/light_tank/light_tank_shell_projectile.png
  icon_file: unity/Assets/Rts/Art/Icons/light_tank_icon.png
  material_plan: mat_light_tank_body for hull/turret/LODs, mat_light_tank_tracks for TrackLeft/TrackRight, mat_light_tank_weapon for barrel/projectile, mat_light_tank_team_color for white recolorable panels only, mat_light_tank_emissive for cyan/orange lights, mat_light_tank_glass for optic glass, mat_light_tank_bolts for small fasteners.
  dimensions:
    footprint_width_cells: 1
    footprint_height_cells: 1
    visual_length_meters: 5.98
    visual_width_meters: 3.10
    visual_height_meters: 2.27
    selection_radius: 1.70
    selection_height: 2.40
  sockets:
    - socket: Root
      local_position: [0, 0, 0]
      local_rotation: [0, 0, 0]
    - socket: VisualRoot
      local_position: [0, 0, 0]
      local_rotation: [0, 0, 0]
    - socket: BodyRoot
      local_position: [0, 0, 0]
      local_rotation: [0, 0, 0]
    - socket: TurretRoot
      local_position: [0, 1.36, -0.08]
      local_rotation: [0, 0, 0]
    - socket: BarrelRoot
      local_position: [0, 0.10, 0.66]
      local_rotation: [0, 0, 0]
    - socket: MuzzlePrimary
      local_position: [0, 0, 3.40]
      local_rotation: [0, 0, 0]
    - socket: TrackLeft
      local_position: [-1.14, 0.52, 0]
      local_rotation: [0, 0, 0]
    - socket: TrackRight
      local_position: [1.14, 0.52, 0]
      local_rotation: [0, 0, 0]
    - socket: SelectionAnchor
      local_position: [0, 0.05, 0]
      local_rotation: [0, 0, 0]
    - socket: HealthBarAnchor
      local_position: [0, 2.65, 0]
      local_rotation: [0, 0, 0]
    - socket: UiAnchor
      local_position: [0, 2.45, -0.15]
      local_rotation: [0, 0, 0]
    - socket: VfxSmoke
      local_position: [0.65, 1.70, -1.45]
      local_rotation: [0, 0, 0]
    - socket: VfxExplosion
      local_position: [0, 1.10, 0]
      local_rotation: [0, 0, 0]
  required_animation_hooks:
    turret_yaw: TurretRoot rotates on local Y, default forward +Z, supports continuous yaw tracking.
    barrel_recoil: BarrelRoot recoils backward along local -Z by 0.08 meters and returns to rest.
    tracks_or_wheels: TrackLeft and TrackRight are separate animation roots; use material property or tread UV scroll via tracked_light motion profile.
    infantry_walk: null
    aircraft_rotor_or_hover: null
    death_vfx: VfxExplosion spawns tank_shell_impact_small or vehicle_death_explosion; VfxSmoke spawns damage smoke while low health.

unity_paths:
  source_folder: unity/Assets/Rts/Art/Source/Units/light_tank/
  textures_folder: unity/Assets/Rts/Art/Textures/Units/light_tank/
  materials_folder: unity/Assets/Rts/Art/Materials/Units/light_tank/
  prefab_path: unity/Assets/Rts/Art/Prefabs/Actors/Production/Units/light_tank/light_tank.prefab
  visual_definition_path: unity/Assets/Rts/ScriptableObjects/Art/ActorVisualDefinitions/light_tank_visual.asset
  review_scene_path: unity/Assets/Rts/Scenes/light_tank_UnitReview.unity

acceptance:
  must_be_buildable: true
  must_appear_in_player_facing_scene: true
  required_core_tests:
    - Rts.Core.Tests passes after light_tank definition remains valid.
    - war_factory production list contains light_tank.
    - light_tank weapon can attack ground units and buildings.
    - light_tank weapon rejects aircraft targets.
    - projectile snapshot is created for light_tank_76mm_cannon.
  required_unity_validations:
    - light_tank_visual.asset resolves to production prefab, not fallback.
    - ActorPrefabDescriptor exists on root.
    - ActorPrefabSocket exists on all required sockets.
    - LODGroup has LOD0, LOD1, LOD2, and cull stage.
    - Materials assign albedo, normal, ORM, emission, and team color renderers correctly.
    - ApplyTeamColor recolors only mat_light_tank_team_color renderers.
    - TurretRoot rotates independently from BodyRoot.
    - BarrelRoot is child of TurretRoot and MuzzlePrimary is child of BarrelRoot.
    - TrackLeft and TrackRight are separate objects.
  required_screenshots:
    - build/screenshots/units/light_tank_top.png
    - build/screenshots/units/light_tank_front.png
    - build/screenshots/units/light_tank_side.png
    - build/screenshots/units/light_tank_three_quarter.png
    - build/screenshots/units/light_tank_socket_review.png
    - build/screenshots/units/light_tank_review.png
    - build/screenshots/units/light_tank_player_facing.png
```
