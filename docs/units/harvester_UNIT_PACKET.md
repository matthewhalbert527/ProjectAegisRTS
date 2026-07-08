# Art Deliverables Description - harvester

Preferred deliverable status: **OPTION A supplied**. This clean revision provides a Unity-ready ProjectAegisRTS `harvester` mesh-prefab asset with all visible team-color, emissive, panel, bolt, pipe, intake, and cargo details attached to the vehicle body. Detached decorative dots and floating inspection-looking fragments have been removed from player-facing geometry.

Design intent:

- Build a production RTS resource harvester that reads clearly from top/isometric camera.
- Keep `type_id: harvester` so Codex can replace visual art while preserving existing Rts.Core gameplay authority.
- Use a slow tracked industrial silhouette: armored hull, front rotating ore cutter, armored cab, top conveyor, rear ore storage drum, rear refinery dock hardware, exhaust pipes, and cargo marker.
- Make team ownership readable from RTS distance through white team-color geometry on hull sides, front glacis, upper deck, rear panel, cab roof, and storage tank panels.
- Ensure all team-color and detail geometry is attached to the hull/tank surfaces; no random pieces float around the asset.

Delivered files:

- `unity/Assets/Rts/Art/Source/Units/harvester/harvester.obj`
- `unity/Assets/Rts/Art/Source/Units/harvester/harvester_articulated.glb`
- `unity/Assets/Rts/Art/Source/Units/harvester/harvester_hull.obj`
- `unity/Assets/Rts/Art/Source/Units/harvester/harvester_cab.obj`
- `unity/Assets/Rts/Art/Source/Units/harvester/harvester_track_left.obj`
- `unity/Assets/Rts/Art/Source/Units/harvester/harvester_track_right.obj`
- `unity/Assets/Rts/Art/Source/Units/harvester/harvester_intake_cutter.obj`
- `unity/Assets/Rts/Art/Source/Units/harvester/harvester_conveyor.obj`
- `unity/Assets/Rts/Art/Source/Units/harvester/harvester_ore_tank.obj`
- `unity/Assets/Rts/Art/Source/Units/harvester/harvester_pipes.obj`
- `unity/Assets/Rts/Art/Source/Units/harvester/harvester_team_body.obj`
- `unity/Assets/Rts/Art/Source/Units/harvester/harvester_team_tank.obj`
- `unity/Assets/Rts/Art/Source/Units/harvester/harvester_emissive.obj`
- `unity/Assets/Rts/Art/Source/Units/harvester/harvester_glass.obj`
- `unity/Assets/Rts/Art/Source/Units/harvester/harvester_bolts.obj`
- `unity/Assets/Rts/Art/Source/Units/harvester/harvester_ore_cargo.obj`
- `unity/Assets/Rts/Art/Source/Units/harvester/harvester_machine_gun.obj`
- `unity/Assets/Rts/Art/Source/Units/harvester/harvester_lod1.obj`
- `unity/Assets/Rts/Art/Source/Units/harvester/harvester_lod2.obj`
- `unity/Assets/Rts/Art/Textures/Units/harvester/harvester_albedo.png`
- `unity/Assets/Rts/Art/Textures/Units/harvester/harvester_normal.png`
- `unity/Assets/Rts/Art/Textures/Units/harvester/harvester_orm.png`
- `unity/Assets/Rts/Art/Textures/Units/harvester/harvester_emission.png`
- `unity/Assets/Rts/Art/Textures/Units/harvester/harvester_team_mask.png`
- `unity/Assets/Rts/Art/Textures/Units/harvester/harvester_resource_dust_sheet.png`
- `unity/Assets/Rts/Art/Icons/harvester_icon.png`
- `unity/Assets/Rts/Editor/ProjectAegisHarvesterPrefabBuilder.cs`
- `unity/Assets/Rts/Scripts/Art/Production/ProjectAegisHarvesterVisualRig.cs`
- `build/screenshots/units/harvester_top.png`
- `build/screenshots/units/harvester_front.png`
- `build/screenshots/units/harvester_side.png`
- `build/screenshots/units/harvester_three_quarter.png`
- `build/screenshots/units/harvester_socket_review.png`
- `build/screenshots/units/harvester_team_color_readability.png`

Exact dimensions in meters:

- Total visual length including ore cutter: **8.46 m**.
- Visual width across tracks and side armor: **4.66 m**.
- Visual height to storage tank/exhaust details: **3.13 m**.
- Root pivot: **0,0,0 ground center**.
- Forward axis: **+Z**.
- Up axis: **+Y**.
- Intended prefab scale: **1,1,1**.

Polygon / triangle budget:

- LOD0: approximately **7,452 triangles** including separated tracks, enlarged intake cutter, conveyor, ore tank, team panels, pipes, attached detail plates, optics, emissive details, and a single defensive machine gun.
- LOD1: approximately **160 triangles** static simplified harvester silhouette.
- LOD2: approximately **108 triangles** static extreme-distance silhouette.

Team-color implementation:

- Team color is represented by separate white geometry in `harvester_team_body.obj` and `harvester_team_tank.obj`, plus `harvester_team_mask.png`.
- Hull team areas include left hull side, right hull side, upper deck strips, front glacis panels, lower scoop/ramp markings, cab roof, and rear dock panel.
- Storage team areas include side tank plates, top hatch panel, rear tank strip, and rear pod panels.
- Team color is intentionally not applied to the whole vehicle.

Socket summary:

| Socket | Required | Local Position XYZ | Local Rotation XYZ | Notes |
| --- | --- | --- | --- | --- |
| Root | yes | 0.00,0.00,0.00 | 0,0,0 | Ground center |
| VisualRoot | yes | 0.00,0.00,0.00 | 0,0,0 | Parent for model art |
| BodyRoot | yes | 0.00,0.00,0.00 | 0,0,0 | Main hull/body root |
| TrackLeft | yes | -1.86,0.42,0.02 | 0,0,0 | Left track animation root |
| TrackRight | yes | 1.86,0.42,0.02 | 0,0,0 | Right track animation root |
| ResourceIntake | yes | 0.00,0.64,3.78 | 0,0,0 | Front ore cutter/intake VFX and harvesting contact |
| CutterDrumRoot | yes | 0.00,0.64,3.78 | 0,0,0 | Rotating intake cutter drum root |
| ConveyorRoot | yes | 0.00,1.18,1.25 | -7,0,0 | Scrolling conveyor visual root |
| HarvesterDock | yes | 0.00,0.90,-3.58 | 0,180,0 | Rear refinery docking and unload point |
| SelectionAnchor | yes | 0.00,0.05,0.00 | 0,0,0 | Center of selection ring |
| HealthBarAnchor | yes | 0.00,3.30,-0.35 | 0,0,0 | Above storage tank readable height |
| UiAnchor | yes | 0.00,3.50,-0.35 | 0,0,0 | Floating UI anchor |
| VfxSmoke | yes | 0.62,3.08,-3.05 | 0,0,0 | Exhaust and damage-smoke anchor |
| VfxExplosion | yes | 0.00,1.25,0.00 | 0,0,0 | Death explosion center |
| OreCargoAnchor | yes | 0.00,2.92,-1.82 | 0,0,0 | Visible cargo/ore fullness marker |
| RefineryUnloadVfx | yes | 0.00,0.92,-3.72 | 0,180,0 | Particle stream when unloading |
| LightRoot | yes | 0.00,1.05,3.38 | 0,0,0 | Front light cluster |
| MachineGunRoot | yes | 0.00,2.92,0.80 | 0,0,0 | Single defensive machine gun pivot/root |
| MuzzleDefense | yes | 0.00,3.03,1.64 | 0,0,0 | Defensive machine gun muzzle |

Texture-to-material assignment table:

| Material | Renderers / Parts | Albedo | Normal | ORM | Emission | Team Mask |
| --- | --- | --- | --- | --- | --- |
| `mat_harvester_body` | HullArmor, CabArmor, OreStorageAndProcessing, LODs | `harvester_albedo.png` | `harvester_normal.png` | `harvester_orm.png` | none | none |
| `mat_harvester_tracks` | TrackLeftMesh, TrackRightMesh, ConveyorAssembly | `harvester_albedo.png` | `harvester_normal.png` | `harvester_orm.png` | none | none |
| `mat_harvester_metal` | OreIntakeAndCutter, PipesAndDockHardware, AttachedBoltsAndSurfaceDetails, MachineGunDefense | `harvester_albedo.png` | `harvester_normal.png` | `harvester_orm.png` | none | none |
| `mat_harvester_team_color` | TeamColorPanels_Body, TeamColorPanels_Tank | `harvester_albedo.png` | `harvester_normal.png` | `harvester_orm.png` | none | `harvester_team_mask.png` |
| `mat_harvester_emissive_orange` | Emissive orange strips, cyan work lights, hazard beacons | `harvester_albedo.png` | `harvester_normal.png` | `harvester_orm.png` | `harvester_emission.png` | none |
| `mat_harvester_glass` | GlassAndOptics | `harvester_albedo.png` | `harvester_normal.png` | `harvester_orm.png` | `harvester_emission.png` | none |
| `mat_harvester_ore` | VisibleOreCargo and optional ore-chunk VFX | `harvester_albedo.png` | `harvester_normal.png` | `harvester_orm.png` | none | none |

---

# ProjectAegisRTS Unit Packet: Harvester

## 1. Current Unit Architecture

### Source Of Truth

- Core gameplay definitions live in `src/Rts.Core/Demo/DemoRules.cs`.
- Core data contracts live in `src/Rts.Core/Data/Definitions.cs`.
- Runtime actor state lives in `src/Rts.Core/Actors/ActorTypes.cs`.
- Unity visual definitions live in `unity/Assets/Rts/ScriptableObjects/Art/ActorVisualDefinitions/`.
- Unity actor visual prefabs live under `unity/Assets/Rts/Art/Prefabs/Actors/`.
- Generated blockout prefabs are fallback/debug art, not final player-facing art.
- Harvester packet path: `docs/units/harvester_UNIT_PACKET.md`.

### Format Used Today

- Gameplay: C# rules in `DemoRules.CreateDefaultRules()`, not JSON.
- Visuals: Unity `ActorVisualDefinition` ScriptableObjects plus Unity prefabs.
- Prefab validation: `ActorPrefabDescriptor` and child `ActorPrefabSocket` components.
- Motion presentation: Unity `VisualMotionProfile` ScriptableObjects.
- Combat presentation: Unity `CombatVisualProfile` ScriptableObjects.
- Runtime fallback: if a visual definition or prefab is missing, Unity creates primitive fallback visuals. This finished Harvester must resolve to the production prefab and avoid the fallback path.

### Existing Examples To Copy

- Core unit definitions: `src/Rts.Core/Demo/DemoRules.cs`
  - `harvester`
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
  type_id: harvester
  display_name: Harvester
  actor_kind: Unit
  category: Resource
  role: tracked resource collector and refinery-docking ore carrier with light defensive machine gun
  max_health: 750

production:
  kind: Unit
  cost: 1400
  build_time_ticks: 900
  factory_type_id: war_factory
  prerequisite_type_ids: [refinery]
  exempt_from_low_power_pause: false

movement:
  speed_per_tick: 8
  turn_rate_degrees_per_tick: 3
  visual_motion_profile_id: harvester_tracked
  movement_class: Harvester

combat:
  has_weapon: true
  weapon: defensive_machine_gun

sight:
  radius_cells: 5

special:
  capture: null
  transport: null
  aircraft: null
  harvester_role: true
  self_defense_weapon: true
```

## 2. Visual Asset Requirements

### Runtime Visual Type

ProjectAegisRTS expects Unity-ready 3D prefabs for player-facing unit visuals.

- Preferred deliverable supplied: `.obj` and `.glb` model files plus texture maps, icon, review screenshots, prefab auto-builder, visual rig script, and completed unit packet.
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
    path: unity/Assets/Rts/Art/Source/Units/harvester/harvester.obj
    format: obj
    real_world_units: meters
    forward_axis: +Z
    up_axis: +Y
    root_pivot: ground_center_of_footprint

  textures:
    albedo: unity/Assets/Rts/Art/Textures/Units/harvester/harvester_albedo.png
    normal: unity/Assets/Rts/Art/Textures/Units/harvester/harvester_normal.png
    orm_or_roughness: unity/Assets/Rts/Art/Textures/Units/harvester/harvester_orm.png
    emission: unity/Assets/Rts/Art/Textures/Units/harvester/harvester_emission.png
    team_mask: unity/Assets/Rts/Art/Textures/Units/harvester/harvester_team_mask.png
    projectile_albedo: null
    muzzle_flash_sheet: null
    icon: unity/Assets/Rts/Art/Icons/harvester_icon.png

  reference_images:
    concept_front: build/screenshots/units/harvester_front.png
    concept_side: build/screenshots/units/harvester_side.png
    concept_top: build/screenshots/units/harvester_top.png
    concept_three_quarter: build/screenshots/units/harvester_three_quarter.png
```

### Image Dimensions

- Albedo: `2048x2048`.
- Normal: `2048x2048`.
- ORM: `2048x2048`.
- Emission: `2048x2048`.
- Team mask: `2048x2048`.
- Icon: `512x512`, transparent PNG.
- Resource dust sheet: `512x512`, transparent PNG.
- Reference renders: `1500x1000` except transparent icon.

### Transparency

- Icon and resource dust sheet use alpha.
- Main albedo is opaque.
- Glass/optic parts are separate geometry/material assignment targets and may use alpha or transparent-compatible shader in Unity.
- Team mask uses a clean white-on-black mask for team-color areas.

### Scale, Pivot, And Orientation

```yaml
scale_and_pivots:
  footprint_width_cells: 1
  footprint_height_cells: 1
  visual_length_meters: 8.46
  visual_width_meters: 4.66
  visual_height_meters: 3.13
  root_origin: ground_center
  forward_axis: +Z
  turret_default_yaw_degrees: 0
  selection_radius: 2.55
  selection_height: 3.50
  prefab_height_offset: 0
```

### Required Sockets

Every socket should be a child transform with `ActorPrefabSocket` set to the matching `ActorPrefabSocketKind`.

Common sockets supplied: `Root`, `VisualRoot`, `BodyRoot`, `SelectionAnchor`, `HealthBarAnchor`, `UiAnchor`, `VfxSmoke`, and `VfxExplosion`.

Harvester special-purpose sockets supplied: `TrackLeft`, `TrackRight`, `ResourceIntake`, `CutterDrumRoot`, `ConveyorRoot`, `HarvesterDock`, `OreCargoAnchor`, `RefineryUnloadVfx`, and `LightRoot`.

### Socket Coordinate Table

| Socket | Required | Local Position XYZ | Local Rotation XYZ | Notes |
| --- | --- | --- | --- | --- |
| Root | yes | 0.00,0.00,0.00 | 0,0,0 | Ground center |
| VisualRoot | yes | 0.00,0.00,0.00 | 0,0,0 | Parent for model art |
| BodyRoot | yes | 0.00,0.00,0.00 | 0,0,0 | Main hull/body root |
| TrackLeft | yes | -1.86,0.42,0.02 | 0,0,0 | Left track animation root |
| TrackRight | yes | 1.86,0.42,0.02 | 0,0,0 | Right track animation root |
| ResourceIntake | yes | 0.00,0.64,3.78 | 0,0,0 | Front ore cutter/intake VFX and harvesting contact |
| CutterDrumRoot | yes | 0.00,0.64,3.78 | 0,0,0 | Rotating intake cutter drum root |
| ConveyorRoot | yes | 0.00,1.18,1.25 | -7,0,0 | Scrolling conveyor visual root |
| HarvesterDock | yes | 0.00,0.90,-3.58 | 0,180,0 | Rear refinery docking and unload point |
| SelectionAnchor | yes | 0.00,0.05,0.00 | 0,0,0 | Center of selection ring |
| HealthBarAnchor | yes | 0.00,3.30,-0.35 | 0,0,0 | Above storage tank readable height |
| UiAnchor | yes | 0.00,3.50,-0.35 | 0,0,0 | Floating UI anchor |
| VfxSmoke | yes | 0.62,3.08,-3.05 | 0,0,0 | Exhaust and damage-smoke anchor |
| VfxExplosion | yes | 0.00,1.25,0.00 | 0,0,0 | Death explosion center |
| OreCargoAnchor | yes | 0.00,2.92,-1.82 | 0,0,0 | Visible cargo/ore fullness marker |
| RefineryUnloadVfx | yes | 0.00,0.92,-3.72 | 0,180,0 | Particle stream when unloading |
| LightRoot | yes | 0.00,1.05,3.38 | 0,0,0 | Front light cluster |
| MachineGunRoot | yes | 0.00,2.92,0.80 | 0,0,0 | Single defensive machine gun pivot/root |
| MuzzleDefense | yes | 0.00,3.03,1.64 | 0,0,0 | Defensive machine gun muzzle |

### Import Settings

- Model scale factor: `1`.
- Import normals and tangents when supplied.
- Generate lightmap UVs only if needed.
- Materials: Unity Lit/URP-compatible material, not flat unlit placeholders.
- Albedo/icon/emission textures: `sRGB = true`.
- Normal map: mark as Normal Map.
- ORM/roughness/metallic/AO: `sRGB = false`.
- Mipmaps: enabled for runtime textures.
- Compression: normal quality or high quality for hero units; keep Quest-safe texture counts.
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
  movement_class: Harvester
  speed_per_tick: 8
  turn_rate_degrees_per_tick: 3
  visual_motion_profile_id: harvester_tracked
  acceleration_smoothing: 0.42
  braking_smoothing: 0.50
  turn_smoothing: 0.55
  facing_lag: 0.20
  visual_arrival_distance: 0.08
  track_or_wheel_animation_scale: 1.0
  infantry_step_rate: null
  infantry_stride_length: null
  aircraft_altitude_offset: null
  aircraft_bank_amount: null
  formation_spacing_cells: 1.35
  pathing_footprint_cells: 1
  stopping_distance_cells: 0
  slope_limit_degrees: null
  collision_radius_meters: 2.32
```

### Movement Testing

Required tests or manual checks:

- Unit accepts move order to an open destination.
- Unit rejects unreachable or impassable destinations.
- Unit can follow diagonal paths when both adjacent cardinal cells are passable.
- Unit does not cut diagonally through blocked corners.
- Unit visual faces travel direction without excessive sliding.
- Tracked/harvester motion profile visually matches the unit role.
- Harvester can path to a resource cell and refinery dock.

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
  has_weapon: false
  weapon_id: null
  display_name: null
  damage: 0
  damage_kind: None
  range_cells: 0
  min_range_cells: 0
  cooldown_ticks: 0
  fire_mode: InstantHit
  projectile_kind: None
  projectile_speed_subcells_per_tick: 0
  projectile_lifetime_ticks: 0
  projectile_homes_to_target: false
  can_target_ground: false
  can_target_air: false
  can_target_buildings: false
  can_target_units: false
  requires_line_of_sight: false
  burst_count: 1
  burst_delay_ticks: 0
  area_radius_cells: 0
  muzzle_socket_id: null
  projectile_visual_id: null
  impact_visual_id: harvester_impact_dust
```

### Turret And Projectile Visual Contract

```yaml
turret:
  has_turret: false
  turret_root_socket: null
  barrel_root_socket: null
  muzzle_socket: null
  yaw_axis: local_y
  default_forward: +Z
  recoil_distance_meters: 0.0

projectile_visual:
  prefab_required: false
  projectile_mesh_or_sprite: null
  tracer_color: null
  tracer_length: 0.0
  muzzle_flash_texture: null
  impact_effect: harvester_impact_dust
```

### Damage And Armor Categories

```yaml
armor:
  armor_class: vehicle_heavy_resource
  intended_counters: [resource_collection, refinery_supply, economy_expansion]
  vulnerable_to: [rocket_soldier, heavy_tank, flame_trooper, aircraft_attack]
```

### Combat Testing

Required tests or manual checks:

- Unit does not expose an attack order or weapon behavior.
- Unit can be targeted and damaged by enemy units.
- Destroyed harvester stops accepting orders.
- Destruction spawns `VfxExplosion` and optional ore/dust debris.

## 5. Unity Implementation Requirements

### Exact Folder Paths

```text
unity/Assets/Rts/Art/Source/Units/harvester/
unity/Assets/Rts/Art/Textures/Units/harvester/
unity/Assets/Rts/Art/Materials/Units/harvester/
unity/Assets/Rts/Art/Icons/harvester_icon.png
unity/Assets/Rts/Art/Prefabs/Actors/Production/Units/harvester/harvester.prefab
unity/Assets/Rts/ScriptableObjects/Art/ActorVisualDefinitions/harvester_visual.asset
unity/Assets/Rts/ScriptableObjects/Rendering/Motion/harvester_tracked.asset
unity/Assets/Rts/ScriptableObjects/Rendering/Combat/harvester_impact_dust.asset
unity/Assets/Rts/Scenes/harvester_UnitReview.unity
docs/units/harvester_UNIT_PACKET.md
```

### Required Prefab Hierarchy

```text
harvester
  ActorPrefabDescriptor
  LODGroup
  Root [ActorPrefabSocket: Root]
    VisualRoot [ActorPrefabSocket: VisualRoot]
      BodyRoot [ActorPrefabSocket: BodyRoot]
        LOD0_Meshes
        LOD1_Meshes
        LOD2_Meshes
      TrackLeft [ActorPrefabSocket: TrackLeft]
      TrackRight [ActorPrefabSocket: TrackRight]
      ResourceIntake [ActorPrefabSocket: ResourceIntake]
      CutterDrumRoot [ActorPrefabSocket: CutterDrumRoot]
      ConveyorRoot [ActorPrefabSocket: ConveyorRoot]
      HarvesterDock [ActorPrefabSocket: HarvesterDock]
      OreCargoAnchor [ActorPrefabSocket: OreCargoAnchor]
      RefineryUnloadVfx [ActorPrefabSocket: RefineryUnloadVfx]
      LightRoot [ActorPrefabSocket: LightRoot]
      VfxSmoke [ActorPrefabSocket: VfxSmoke]
      VfxExplosion [ActorPrefabSocket: VfxExplosion]
    SelectionAnchor [ActorPrefabSocket: SelectionAnchor]
    HealthBarAnchor [ActorPrefabSocket: HealthBarAnchor]
    UiAnchor [ActorPrefabSocket: UiAnchor]
```

### Required Components

- `ActorPrefabDescriptor`.
- `ActorPrefabSocket` on every required socket child.
- `LODGroup` for final production art.
- `MeshFilter`/`MeshRenderer` children for hull, cab, tracks, cutter, conveyor, ore tank, pipes, team panels, emissive parts, glass, details, and LOD meshes.
- `ProjectAegisHarvesterVisualRig` or equivalent Unity-side rig controller for cutter/conveyor/track/cargo animation hooks.
- Do not add UnityEngine references to `src/Rts.Core`.
- Do not put gameplay authority in Unity MonoBehaviours.

### ActorVisualDefinition Fields

```yaml
actor_visual_definition:
  actorTypeId: harvester
  displayName: Harvester
  safeDisplayName: Harvester
  category: Resource
  productionStatus: production_candidate_clean_v2
  icon: unity/Assets/Rts/Art/Icons/harvester_icon.png
  generatedBlockoutPrefab: unity/Assets/Rts/Art/Prefabs/Actors/GeneratedBlockouts/harvester_generated_blockout.prefab
  productionPrefab: unity/Assets/Rts/Art/Prefabs/Actors/Production/Units/harvester/harvester.prefab
  fallbackPrefab: unity/Assets/Rts/Art/Prefabs/Actors/GeneratedBlockouts/harvester_generated_blockout.prefab
  preferredPrefabMode: production
  motionProfileId: harvester_tracked
  selectionRadius: 2.55
  selectionHeight: 3.50
  footprintWidth: 1
  footprintHeight: 1
  visualScale: 1
  prefabHeightOffset: 0
  useVehicleMotionController: true
  useInfantryMotionController: false
  useAircraftMotionController: false
  useTurretVisualController: false
  requiredSockets: [Root, VisualRoot, BodyRoot, TrackLeft, TrackRight, ResourceIntake, CutterDrumRoot, ConveyorRoot, HarvesterDock, OreCargoAnchor, RefineryUnloadVfx, LightRoot, VfxSmoke, VfxExplosion, SelectionAnchor, HealthBarAnchor, UiAnchor]
```

### Naming Conventions

- Actor id: `harvester`.
- Motion profile id: `harvester_tracked`.
- Unity asset names start with `harvester`.
- Prefab root: `harvester`.
- Sockets exactly match the listed socket names.
- Materials: `mat_harvester_body`, `mat_harvester_tracks`, `mat_harvester_metal`, `mat_harvester_team_color`, `mat_harvester_emissive_orange`, `mat_harvester_glass`, `mat_harvester_ore`.
- Textures: `harvester_albedo.png`, `harvester_normal.png`, `harvester_orm.png`, `harvester_emission.png`, `harvester_team_mask.png`.

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
  unit_review_scene: build/screenshots/units/harvester_socket_review.png
  player_facing_scene: build/screenshots/units/harvester_three_quarter.png
  optional_turntable_gif: null
```

The review screenshot shows the production prefab geometry, socket markers/labels, and clear separation between player-facing geometry and debug socket markers.

### Required Validation Scene

```text
unity/Assets/Rts/Scenes/harvester_UnitReview.unity
```

The scene must include:

- The final prefab.
- A similar existing unit for scale comparison.
- Lighting that shows surface texture and silhouette.
- Camera framing suitable for screenshot.
- Optional movement/resource sandbox for harvesting, docking, cutter spin, conveyor motion, and unload VFX.

### Done Means

- Core rules compile and deterministic tests pass.
- `harvester` gameplay definition remains preserved in `DemoRules.CreateDefaultRules()`.
- War factory/refinery production flow remains preserved as currently implemented.
- Unit has an `ActorVisualDefinition`.
- Unit resolves to a production prefab, not a generated primitive fallback.
- Prefab has required sockets and metadata.
- Textures/materials are assigned and visible.
- Movement/resource behavior match the packet.
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
  type_id: harvester
  display_name: Harvester
  category: Resource
  role: tracked resource collector and refinery-docking ore carrier with light defensive machine gun
  design_summary: Clean tracked industrial harvester with front cutter drum, attached scoop teeth, armored cab, visible conveyor, rear cylindrical ore storage, dock hardware, side pipes, exhaust details, readable team panels, and no floating decorative fragments.
  visual_style_reference: C&C / Red Alert 2-inspired industrial economy vehicle, adapted to ProjectAegisRTS military-green/cyan/orange palette and Quest-safe low-poly geometry.
  comparable_existing_unit: harvester

core_definition:
  max_health: 750
  production:
    kind: Unit
    cost: 1400
    build_time_ticks: 900
    factory_type_id: war_factory
    prerequisite_type_ids: [refinery]
    exempt_from_low_power_pause: false
  sight:
    radius_cells: 5
  movement:
    movement_class: Harvester
    speed_per_tick: 8
    turn_rate_degrees_per_tick: 3
    visual_motion_profile_id: harvester_tracked
  weapon:
    has_weapon: false
    weapon_id: null
    display_name: null
    damage: 0
    damage_kind: None
    range_cells: 0
    min_range_cells: 0
    cooldown_ticks: 0
    fire_mode: InstantHit
    projectile_kind: None
    projectile_speed_subcells_per_tick: 0
    projectile_lifetime_ticks: 0
    projectile_homes_to_target: false
    can_target_ground: false
    can_target_air: false
    can_target_buildings: false
    can_target_units: false
    requires_line_of_sight: false
    burst_count: 1
    burst_delay_ticks: 0
    area_radius_cells: 0
    muzzle_socket_id: null
    projectile_visual_id: null
    impact_visual_id: harvester_impact_dust
  special:
    capture: null
    transport: null
    aircraft: null

visuals:
  runtime_format: mesh_prefab
  model_files:
    - unity/Assets/Rts/Art/Source/Units/harvester/harvester.obj
    - unity/Assets/Rts/Art/Source/Units/harvester/harvester_articulated.glb
    - unity/Assets/Rts/Art/Source/Units/harvester/harvester_hull.obj
    - unity/Assets/Rts/Art/Source/Units/harvester/harvester_cab.obj
    - unity/Assets/Rts/Art/Source/Units/harvester/harvester_track_left.obj
    - unity/Assets/Rts/Art/Source/Units/harvester/harvester_track_right.obj
    - unity/Assets/Rts/Art/Source/Units/harvester/harvester_intake_cutter.obj
    - unity/Assets/Rts/Art/Source/Units/harvester/harvester_conveyor.obj
    - unity/Assets/Rts/Art/Source/Units/harvester/harvester_ore_tank.obj
    - unity/Assets/Rts/Art/Source/Units/harvester/harvester_pipes.obj
    - unity/Assets/Rts/Art/Source/Units/harvester/harvester_team_body.obj
    - unity/Assets/Rts/Art/Source/Units/harvester/harvester_team_tank.obj
    - unity/Assets/Rts/Art/Source/Units/harvester/harvester_emissive.obj
    - unity/Assets/Rts/Art/Source/Units/harvester/harvester_glass.obj
    - unity/Assets/Rts/Art/Source/Units/harvester/harvester_bolts.obj
    - unity/Assets/Rts/Art/Source/Units/harvester/harvester_ore_cargo.obj
    - unity/Assets/Rts/Art/Source/Units/harvester/harvester_lod1.obj
    - unity/Assets/Rts/Art/Source/Units/harvester/harvester_lod2.obj
  texture_files:
    - unity/Assets/Rts/Art/Textures/Units/harvester/harvester_albedo.png
    - unity/Assets/Rts/Art/Textures/Units/harvester/harvester_normal.png
    - unity/Assets/Rts/Art/Textures/Units/harvester/harvester_orm.png
    - unity/Assets/Rts/Art/Textures/Units/harvester/harvester_emission.png
    - unity/Assets/Rts/Art/Textures/Units/harvester/harvester_team_mask.png
    - unity/Assets/Rts/Art/Textures/Units/harvester/harvester_resource_dust_sheet.png
  icon_file: unity/Assets/Rts/Art/Icons/harvester_icon.png
  material_plan: mat_harvester_body for hull/cab/tank, mat_harvester_tracks for tracks/conveyor, mat_harvester_metal for cutter/pipes/attached details, mat_harvester_team_color for white recolorable panels, mat_harvester_emissive_orange for orange/cyan light strips, mat_harvester_glass for cab/optic glass, mat_harvester_ore for cargo.
  dimensions:
    footprint_width_cells: 1
    footprint_height_cells: 1
    visual_length_meters: 8.46
    visual_width_meters: 4.66
    visual_height_meters: 3.13
    selection_radius: 2.55
    selection_height: 3.50
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
    - socket: TrackLeft
      local_position: [-1.86, 0.42, 0.02]
      local_rotation: [0, 0, 0]
    - socket: TrackRight
      local_position: [1.86, 0.42, 0.02]
      local_rotation: [0, 0, 0]
    - socket: ResourceIntake
      local_position: [0.00, 0.54, 3.66]
      local_rotation: [0, 0, 0]
    - socket: CutterDrumRoot
      local_position: [0.00, 0.54, 3.66]
      local_rotation: [0, 0, 0]
    - socket: ConveyorRoot
      local_position: [0.00, 1.18, 1.25]
      local_rotation: [-7, 0, 0]
    - socket: HarvesterDock
      local_position: [0.00, 0.90, -3.58]
      local_rotation: [0, 180, 0]
    - socket: SelectionAnchor
      local_position: [0, 0.05, 0]
      local_rotation: [0, 0, 0]
    - socket: HealthBarAnchor
      local_position: [0, 3.30, -0.35]
      local_rotation: [0, 0, 0]
    - socket: UiAnchor
      local_position: [0, 3.50, -0.35]
      local_rotation: [0, 0, 0]
    - socket: VfxSmoke
      local_position: [0.62, 3.08, -3.05]
      local_rotation: [0, 0, 0]
    - socket: VfxExplosion
      local_position: [0, 1.25, 0]
      local_rotation: [0, 0, 0]
    - socket: OreCargoAnchor
      local_position: [0, 2.92, -1.82]
      local_rotation: [0, 0, 0]
    - socket: RefineryUnloadVfx
      local_position: [0, 0.92, -3.72]
      local_rotation: [0, 180, 0]
    - socket: LightRoot
      local_position: [0, 1.05, 3.38]
      local_rotation: [0, 0, 0]
  required_animation_hooks:
    turret_yaw: null
    barrel_recoil: null
    tracks_or_wheels: TrackLeft and TrackRight scroll/UV or tread animation hooks
    infantry_walk: null
    aircraft_rotor_or_hover: null
    cutter_spin: CutterDrumRoot rotates around local X
    conveyor_motion: ConveyorRoot scrolls backward during harvesting/unloading
    cargo_fill: OreCargoAnchor scales/appears based on cargo amount
    refinery_unload_vfx: RefineryUnloadVfx
    death_vfx: VfxExplosion

unity_paths:
  source_folder: unity/Assets/Rts/Art/Source/Units/harvester/
  textures_folder: unity/Assets/Rts/Art/Textures/Units/harvester/
  materials_folder: unity/Assets/Rts/Art/Materials/Units/harvester/
  prefab_path: unity/Assets/Rts/Art/Prefabs/Actors/Production/Units/harvester/harvester.prefab
  visual_definition_path: unity/Assets/Rts/ScriptableObjects/Art/ActorVisualDefinitions/harvester_visual.asset
  review_scene_path: unity/Assets/Rts/Scenes/harvester_UnitReview.unity

acceptance:
  must_be_buildable: true
  must_appear_in_player_facing_scene: true
  required_core_tests:
    - dotnet run --no-restore --project src/Rts.Core.Tests
    - tools/build-rts-core-for-unity.ps1
  required_unity_validations:
    - prefab resolves to production visual, not generated fallback
    - required sockets exist and match packet coordinates
    - Harvester pathing and refinery dock behavior remain controlled by Rts.Core
    - team-color material changes only white team panels
    - cutter, conveyor, track, cargo, smoke, unload hooks, and defensive machine gun sockets are present
  required_screenshots:
    - build/screenshots/units/harvester_three_quarter.png
    - build/screenshots/units/harvester_top.png
    - build/screenshots/units/harvester_front.png
    - build/screenshots/units/harvester_side.png
    - build/screenshots/units/harvester_socket_review.png
    - build/screenshots/units/harvester_team_color_readability.png
```
