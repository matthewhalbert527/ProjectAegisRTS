# ProjectAegisRTS Unit Packet Template

Use this packet when asking ChatGPT Pro or an art-generation workflow to design a new ProjectAegisRTS unit. Fill every field. If a value is intentionally unknown, write `UNKNOWN` and explain why. Codex should not have to infer scale, sockets, gameplay role, weapon behavior, or implementation paths from a single concept image.

This project is currently mesh/prefab-first for runtime unit visuals. A concept render alone is not a complete implementation packet.

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
- Runtime fallback: if a visual definition or prefab is missing, Unity creates primitive fallback visuals. A finished unit must avoid that path.

### Existing Examples To Copy

- Core unit definitions: `src/Rts.Core/Demo/DemoRules.cs`
  - `rifle_infantry`
  - `light_tank`
  - `medium_tank`
  - `heavy_tank`
  - `harvester`
  - `scout_rover`
  - `apc`
  - `attack_aircraft`
- Visual definition example: `unity/Assets/Rts/ScriptableObjects/Art/ActorVisualDefinitions/light_tank_visual.asset`
- Tank prefab examples:
  - `unity/Assets/Rts/Art/UnityAITankSlate/Prefabs/light_tank_unity_ai_tank.prefab`
  - `unity/Assets/Rts/Art/Prefabs/Actors/Production/MVP/Tanks/light_tank_tank_source.prefab`
- Generic fallback examples: `unity/Assets/Rts/Art/Prefabs/Actors/GeneratedBlockouts/`

### Required Core Unit Fields

Fill these exactly:

```yaml
unit:
  type_id: TODO_snake_case_id
  display_name: TODO
  actor_kind: Unit
  category: TODO # Infantry | Vehicle | Aircraft | Support | Resource | Unknown
  role: TODO # scout, main battle tank, anti-air, artillery, engineer, harvester, transport, etc.
  max_health: TODO_INT

production:
  kind: Unit
  cost: TODO_INT
  build_time_ticks: TODO_INT
  factory_type_id: TODO # barracks | war_factory | dual_helipad | other existing producer
  prerequisite_type_ids: [] # e.g. [comm_center, tech_center]
  exempt_from_low_power_pause: false

movement:
  speed_per_tick: TODO_INT
  turn_rate_degrees_per_tick: TODO_INT
  visual_motion_profile_id: TODO # e.g. tracked_light, tracked_medium, tracked_heavy, wheeled_scout, default_infantry, default_aircraft
  movement_class: TODO # Infantry | Wheeled | Tracked | Harvester | Aircraft

combat:
  has_weapon: TODO_BOOL
  weapon: TODO_OR_NULL

sight:
  radius_cells: TODO_INT

special:
  capture: null
  transport: null
  aircraft: null
  harvester_role: false
```

## 2. Visual Asset Requirements

### Runtime Visual Type

ProjectAegisRTS expects Unity-ready 3D prefabs for player-facing unit visuals.

- Preferred deliverable: `.fbx`, `.glb`, or `.obj` model files plus texture maps.
- Acceptable reference-only deliverable: orthographic top/front/side/3-quarter renders plus exact dimensions and socket positions. This is not as implementation-ready as real mesh files.
- Not sufficient: a single concept image, a sprite sheet without pivots, or unlabelled art.

### Sprite Versus Mesh

- Required runtime format today: mesh prefab.
- Sprite facing count: not applicable for current runtime units.
- Optional review renders: 8, 16, or 32 facing turntable images may be included for art review, but Codex should not treat them as runtime unit data unless a sprite importer is explicitly added later.

### Required Files

```yaml
visual_assets:
  source_model:
    path: unity/Assets/Rts/Art/Source/Units/TODO_type_id/TODO_type_id.fbx
    format: TODO # fbx | glb | obj
    real_world_units: meters
    forward_axis: +Z
    up_axis: +Y
    root_pivot: ground_center_of_footprint

  textures:
    albedo: unity/Assets/Rts/Art/Textures/Units/TODO_type_id/TODO_type_id_albedo.png
    normal: unity/Assets/Rts/Art/Textures/Units/TODO_type_id/TODO_type_id_normal.png
    orm_or_roughness: unity/Assets/Rts/Art/Textures/Units/TODO_type_id/TODO_type_id_orm.png
    emission: unity/Assets/Rts/Art/Textures/Units/TODO_type_id/TODO_type_id_emission.png
    team_mask: unity/Assets/Rts/Art/Textures/Units/TODO_type_id/TODO_type_id_team_mask.png
    projectile_albedo: TODO_OR_NULL
    muzzle_flash_sheet: TODO_OR_NULL
    icon: unity/Assets/Rts/Art/Icons/TODO_type_id_icon.png

  reference_images:
    concept_front: TODO_OR_NULL
    concept_side: TODO_OR_NULL
    concept_top: TODO_OR_NULL
    concept_three_quarter: TODO_OR_NULL
```

### Image Dimensions

- Albedo, normal, ORM/roughness, emission, team mask: prefer `2048x2048`; `1024x1024` minimum for small/simple units.
- Icon: square PNG, prefer `512x512`, transparent background.
- Muzzle flash or impact sheet: power-of-two PNG, `512x512` or `1024x1024`, transparent background.
- Reference renders: `1024x1024` minimum, `2048x2048` preferred.

### Transparency

- Icons, muzzle flashes, impact effects, smoke, glass masks, and team masks may use alpha.
- Main albedo should usually be opaque unless the unit intentionally has transparent glass/energy parts.
- Transparent textures must use clean alpha edges, no baked background color.

### Scale, Pivot, And Orientation

The board mapper uses Unity meters through `BoardCoordinateMapper.CellSizeMeters`; actors are placed at grid-cell centers or fixed-world positions. Unit prefabs should be authored in meters and left at prefab scale `1,1,1`.

```yaml
scale_and_pivots:
  footprint_width_cells: TODO_INT_OR_FLOAT
  footprint_height_cells: TODO_INT_OR_FLOAT
  visual_length_meters: TODO_FLOAT
  visual_width_meters: TODO_FLOAT
  visual_height_meters: TODO_FLOAT
  root_origin: ground_center
  forward_axis: +Z
  turret_default_yaw_degrees: TODO # use 0 for +Z forward unless art calls for a posed review angle
  selection_radius: TODO_FLOAT
  selection_height: TODO_FLOAT
  prefab_height_offset: 0
```

### Required Sockets

Every socket should be a child transform with `ActorPrefabSocket` set to the matching `ActorPrefabSocketKind`.

Common sockets:

- `Root`
- `VisualRoot`
- `BodyRoot`
- `SelectionAnchor`
- `HealthBarAnchor`
- `UiAnchor`
- `VfxSmoke`
- `VfxExplosion`

Vehicle/tank sockets:

- `TurretRoot`
- `BarrelRoot`
- `MuzzlePrimary`
- `MuzzleSecondary` if needed
- `TrackLeft` and `TrackRight` for tracked vehicles
- `WheelLeft` and `WheelRight` for wheeled vehicles

Infantry sockets:

- `Head`
- `WeaponSocket`
- `MuzzlePrimary` if armed

Aircraft sockets:

- `AircraftRotor` if applicable
- `LandingPadAnchor`
- `MuzzlePrimary` if armed

Special-purpose sockets:

- `HarvesterDock`
- `RepairArmRoot`
- `CraneRoot`
- `RadarDishRoot`
- `TurbineRoot`
- `LightRoot`
- `AimPivot`

### Socket Coordinate Table

Fill all required sockets with local coordinates in meters:

```markdown
| Socket | Required | Local Position XYZ | Local Rotation XYZ | Notes |
| --- | --- | --- | --- | --- |
| Root | yes | 0,0,0 | 0,0,0 | Ground center |
| VisualRoot | yes | TODO | TODO | Parent for model art |
| BodyRoot | yes | TODO | TODO | Main hull/body |
| TurretRoot | TODO | TODO | TODO | Yaw pivot, if turreted |
| BarrelRoot | TODO | TODO | TODO | Pitch/recoil child, if used |
| MuzzlePrimary | TODO | TODO | TODO | Barrel exit |
| SelectionAnchor | yes | TODO | TODO | Center of selection ring |
| HealthBarAnchor | yes | TODO | TODO | Above tallest visible point |
| UiAnchor | yes | TODO | TODO | Floating UI anchor |
| VfxSmoke | yes | TODO | TODO | Damage smoke |
| VfxExplosion | yes | TODO | TODO | Death explosion |
```

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
  movement_class: TODO # Infantry | Wheeled | Tracked | Harvester | Aircraft
  speed_per_tick: TODO_INT
  turn_rate_degrees_per_tick: TODO_INT
  visual_motion_profile_id: TODO
  acceleration_smoothing: TODO_FLOAT
  braking_smoothing: TODO_FLOAT
  turn_smoothing: TODO_FLOAT
  facing_lag: TODO_FLOAT
  visual_arrival_distance: TODO_FLOAT
  track_or_wheel_animation_scale: TODO_FLOAT
  infantry_step_rate: TODO_FLOAT_OR_NULL
  infantry_stride_length: TODO_FLOAT_OR_NULL
  aircraft_altitude_offset: TODO_FLOAT_OR_NULL
  aircraft_bank_amount: TODO_FLOAT_OR_NULL
  formation_spacing_cells: TODO_FLOAT # design intent; not a core field yet
  pathing_footprint_cells: 1 # current units occupy one core pathing cell unless code changes
  stopping_distance_cells: 0 # current movement consumes path to the destination cell
  slope_limit_degrees: null # current grid map does not model slopes
  collision_radius_meters: TODO_FLOAT # visual/selection intent; core collision is grid passability
```

### Movement Testing

Required tests or manual checks:

- Unit accepts move order to an open destination.
- Unit rejects unreachable or impassable destinations.
- Unit can follow diagonal paths when both adjacent cardinal cells are passable.
- Unit does not cut diagonally through blocked corners.
- Unit visual faces travel direction without excessive sliding.
- Tracked/wheeled/infantry/aircraft motion profile visually matches the unit role.
- If the unit is a harvester, it can path to a resource cell and refinery dock.

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
  has_weapon: TODO_BOOL
  weapon_id: TODO_OR_NULL
  display_name: TODO_OR_NULL
  damage: TODO_INT
  damage_kind: TODO # None | Kinetic | Explosive | Fire | Energy
  range_cells: TODO_INT
  min_range_cells: 0
  cooldown_ticks: TODO_INT
  fire_mode: TODO # InstantHit | Projectile
  projectile_kind: TODO # None | Bullet | Shell | Rocket | Beam
  projectile_speed_subcells_per_tick: TODO_INT
  projectile_lifetime_ticks: TODO_INT
  projectile_homes_to_target: TODO_BOOL
  can_target_ground: TODO_BOOL
  can_target_air: TODO_BOOL
  can_target_buildings: TODO_BOOL
  can_target_units: TODO_BOOL
  requires_line_of_sight: TODO_BOOL
  burst_count: 1
  burst_delay_ticks: 0
  area_radius_cells: 0
  muzzle_socket_id: MuzzlePrimary
  projectile_visual_id: TODO # usually weapon_id or weapon_id_projectile
  impact_visual_id: TODO # e.g. impact_placeholder, tank_shell, rocket_placeholder
```

### Turret And Projectile Visual Contract

```yaml
turret:
  has_turret: TODO_BOOL
  turret_root_socket: TurretRoot
  barrel_root_socket: BarrelRoot
  muzzle_socket: MuzzlePrimary
  yaw_axis: local_y
  default_forward: +Z
  recoil_distance_meters: TODO_FLOAT

projectile_visual:
  prefab_required: TODO_BOOL
  projectile_mesh_or_sprite: TODO
  tracer_color: TODO
  tracer_length: TODO_FLOAT
  muzzle_flash_texture: TODO_OR_NULL
  impact_effect: TODO_OR_NULL
```

### Damage And Armor Categories

Core currently stores damage kind but does not have a full armor-vs-damage table. Use these design tags for future balancing:

```yaml
armor:
  armor_class: TODO # infantry_light, vehicle_light, vehicle_medium, vehicle_heavy, aircraft_light, structure_light, etc.
  intended_counters: []
  vulnerable_to: []
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

Use these paths unless a stage-specific implementation prompt overrides them:

```text
unity/Assets/Rts/Art/Source/Units/TODO_type_id/
unity/Assets/Rts/Art/Textures/Units/TODO_type_id/
unity/Assets/Rts/Art/Materials/Units/TODO_type_id/
unity/Assets/Rts/Art/Icons/TODO_type_id_icon.png
unity/Assets/Rts/Art/Prefabs/Actors/Production/Units/TODO_type_id/TODO_type_id.prefab
unity/Assets/Rts/ScriptableObjects/Art/ActorVisualDefinitions/TODO_type_id_visual.asset
unity/Assets/Rts/ScriptableObjects/Rendering/Motion/TODO_motion_profile.asset
unity/Assets/Rts/ScriptableObjects/Rendering/Combat/TODO_weapon_or_projectile_profile.asset
unity/Assets/Rts/Scenes/TODO_type_id_UnitReview.unity
docs/units/TODO_type_id_UNIT_PACKET.md
```

### Required Prefab Hierarchy

```text
TODO_type_id
  ActorPrefabDescriptor
  LODGroup
  Root [ActorPrefabSocket: Root]
    VisualRoot [ActorPrefabSocket: VisualRoot]
      BodyRoot [ActorPrefabSocket: BodyRoot]
        LOD0_Meshes
        LOD1_Meshes
        LOD2_Meshes
      TurretRoot [ActorPrefabSocket: TurretRoot, if turreted]
        BarrelRoot [ActorPrefabSocket: BarrelRoot, if turreted]
          MuzzlePrimary [ActorPrefabSocket: MuzzlePrimary, if armed]
      TrackLeft or WheelLeft [ActorPrefabSocket]
      TrackRight or WheelRight [ActorPrefabSocket]
      VfxSmoke [ActorPrefabSocket: VfxSmoke]
      VfxExplosion [ActorPrefabSocket: VfxExplosion]
    SelectionAnchor [ActorPrefabSocket: SelectionAnchor]
    HealthBarAnchor [ActorPrefabSocket: HealthBarAnchor]
    UiAnchor [ActorPrefabSocket: UiAnchor]
```

### Required Components

- `ActorPrefabDescriptor`
- `ActorPrefabSocket` on every required socket child
- `LODGroup` for final production art when practical
- `MeshFilter`/`MeshRenderer` or `SkinnedMeshRenderer` on renderable children
- `TankVisualRigController` or equivalent Unity-side rig controller only if already compatible with the unit type
- Do not add UnityEngine references to `src/Rts.Core`.
- Do not put gameplay authority in Unity MonoBehaviours.

### ActorVisualDefinition Fields

```yaml
actor_visual_definition:
  actorTypeId: TODO_type_id
  displayName: TODO
  safeDisplayName: TODO
  category: TODO # Building | Defense | Infantry | Vehicle | Aircraft | Support | Resource | Unknown
  productionStatus: TODO
  icon: TODO_icon_asset
  generatedBlockoutPrefab: TODO_OR_EXISTING_FALLBACK
  productionPrefab: TODO_final_prefab
  fallbackPrefab: TODO_existing_safe_fallback
  preferredPrefabMode: production
  motionProfileId: TODO
  selectionRadius: TODO_FLOAT
  selectionHeight: TODO_FLOAT
  footprintWidth: TODO_FLOAT
  footprintHeight: TODO_FLOAT
  visualScale: 1
  prefabHeightOffset: 0
  useVehicleMotionController: TODO_BOOL
  useInfantryMotionController: TODO_BOOL
  useAircraftMotionController: TODO_BOOL
  useTurretVisualController: TODO_BOOL
  requiredSockets: TODO_SOCKET_LIST
```

### Naming Conventions

- Actor id: `snake_case`.
- Weapon id: `snake_case`, usually prefixed by actor or weapon role.
- Motion profile id: `snake_case`.
- Unity asset names: start with the actor id.
- Prefab root: `TODO_type_id`.
- Sockets: exactly match `ActorPrefabSocketKind` names.
- Materials: `mat_TODO_type_id_body`, `mat_TODO_type_id_tracks`, etc.
- Textures: `TODO_type_id_albedo.png`, `TODO_type_id_normal.png`, etc.

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
  unit_review_scene: build/screenshots/units/TODO_type_id_review.png
  player_facing_scene: build/screenshots/units/TODO_type_id_player_facing.png
  optional_turntable_gif: build/screenshots/units/TODO_type_id_turntable.gif
```

The review screenshot must show:

- Final production prefab, not primitive fallback.
- Top/side/front or turntable inspection if practical.
- Socket markers or labels for muzzle, turret, selection, and UI anchors.
- Projectile/muzzle effect if the unit is armed.
- Scale comparison against existing similar unit.

### Required Validation Scene

```text
unity/Assets/Rts/Scenes/TODO_type_id_UnitReview.unity
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

Copy this section for each new unit and fill it completely.

```yaml
packet_version: 1
project: ProjectAegisRTS
unit:
  type_id: TODO
  display_name: TODO
  category: TODO
  role: TODO
  design_summary: TODO
  visual_style_reference: TODO
  comparable_existing_unit: TODO

core_definition:
  max_health: TODO
  production:
    kind: Unit
    cost: TODO
    build_time_ticks: TODO
    factory_type_id: TODO
    prerequisite_type_ids: []
    exempt_from_low_power_pause: false
  sight:
    radius_cells: TODO
  movement:
    movement_class: TODO
    speed_per_tick: TODO
    turn_rate_degrees_per_tick: TODO
    visual_motion_profile_id: TODO
  weapon:
    has_weapon: TODO
    weapon_id: TODO
    display_name: TODO
    damage: TODO
    damage_kind: TODO
    range_cells: TODO
    min_range_cells: 0
    cooldown_ticks: TODO
    fire_mode: TODO
    projectile_kind: TODO
    projectile_speed_subcells_per_tick: TODO
    projectile_lifetime_ticks: TODO
    projectile_homes_to_target: TODO
    can_target_ground: TODO
    can_target_air: TODO
    can_target_buildings: TODO
    can_target_units: TODO
    requires_line_of_sight: TODO
    burst_count: 1
    burst_delay_ticks: 0
    area_radius_cells: 0
    muzzle_socket_id: MuzzlePrimary
    projectile_visual_id: TODO
    impact_visual_id: TODO
  special:
    capture: null
    transport: null
    aircraft: null

visuals:
  runtime_format: mesh_prefab
  model_files: []
  texture_files: []
  icon_file: TODO
  material_plan: TODO
  dimensions:
    footprint_width_cells: TODO
    footprint_height_cells: TODO
    visual_length_meters: TODO
    visual_width_meters: TODO
    visual_height_meters: TODO
    selection_radius: TODO
    selection_height: TODO
  sockets:
    - socket: Root
      local_position: [0, 0, 0]
      local_rotation: [0, 0, 0]
    - socket: VisualRoot
      local_position: TODO
      local_rotation: TODO
    - socket: BodyRoot
      local_position: TODO
      local_rotation: TODO
    - socket: SelectionAnchor
      local_position: TODO
      local_rotation: TODO
    - socket: HealthBarAnchor
      local_position: TODO
      local_rotation: TODO
    - socket: UiAnchor
      local_position: TODO
      local_rotation: TODO
    - socket: VfxSmoke
      local_position: TODO
      local_rotation: TODO
    - socket: VfxExplosion
      local_position: TODO
      local_rotation: TODO
  required_animation_hooks:
    turret_yaw: TODO_OR_NULL
    barrel_recoil: TODO_OR_NULL
    tracks_or_wheels: TODO_OR_NULL
    infantry_walk: TODO_OR_NULL
    aircraft_rotor_or_hover: TODO_OR_NULL
    death_vfx: TODO

unity_paths:
  source_folder: unity/Assets/Rts/Art/Source/Units/TODO_type_id/
  textures_folder: unity/Assets/Rts/Art/Textures/Units/TODO_type_id/
  materials_folder: unity/Assets/Rts/Art/Materials/Units/TODO_type_id/
  prefab_path: unity/Assets/Rts/Art/Prefabs/Actors/Production/Units/TODO_type_id/TODO_type_id.prefab
  visual_definition_path: unity/Assets/Rts/ScriptableObjects/Art/ActorVisualDefinitions/TODO_type_id_visual.asset
  review_scene_path: unity/Assets/Rts/Scenes/TODO_type_id_UnitReview.unity

acceptance:
  must_be_buildable: TODO_BOOL
  must_appear_in_player_facing_scene: TODO_BOOL
  required_core_tests: []
  required_unity_validations: []
  required_screenshots: []
```
