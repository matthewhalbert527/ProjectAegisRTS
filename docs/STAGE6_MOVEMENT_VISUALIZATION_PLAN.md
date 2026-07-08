# Stage 6 Movement Visualization Plan

## Goal

Stage 6 should add high-quality visual movement on top of deterministic `Rts.Core` snapshots. The simulation remains authoritative; Unity may smooth, animate, and embellish presentation, but it must not change pathfinding, actor positions, facing, command validation, or tick outcomes.

## Implementation Status

Implemented in `Assets/Rts/Scenes/Stage6_MovementVisualization.unity` with profile-driven Unity presentation controllers. `Rts.Core` remains UnityEngine-free and authoritative; Stage 6 only consumes snapshot fields such as cell position, fixed world position, facing, normalized speed, movement phase, and `VisualMotionProfileId`.

## Visual Vehicle Layer

- Acceleration and braking: interpolate visual speed toward snapshot speed, with per-profile easing and clamp values.
- Turning arcs: steer visual body orientation toward authoritative facing with turn-rate limits and overshoot damping.
- Wheel and track animation: derive wheel rotation or track scroll from visual distance traveled, not from Unity physics.
- Suspension: add small local offsets from terrain/sample probes or deterministic-safe visual noise; never feed this back into core state.
- Turret lag: let turret visuals follow target or facing snapshots with configurable lag, dead zone, and return-to-forward behavior.
- Formation spacing: use snapshot positions as anchors and add only small presentation offsets for readability.

## Infantry Layer

- Locomotion blend states: idle, walk, run, stop, turn-in-place, aim, recoil placeholder, and death placeholder.
- Foot phase: derive from normalized visual speed and distance, not from authoritative simulation state.
- Aim offsets: optional visual-only upper-body aiming once combat snapshots exist.

## Aircraft Layer

- Banking: derive roll from visual turn rate and clamp for readability.
- Altitude: maintain visual altitude offsets by motion profile while grid cell and fixed position remain authoritative.
- VTOL/takeoff/landing: add visual state transitions later around spawn, unload, or landing-pad commands.
- Hover: use subtle visual-only idle motion.

## Separation From Simulation

`Rts.Core` already exposes cell position, fixed world position, facing, normalized speed, movement phase, and `visual_motion_profile_id` in snapshots. Stage 6 should consume those fields from Unity render systems and add presentation components only. Do not add Unity physics, `Rigidbody`, `NavMeshAgent`, or floating-point Unity transforms as gameplay authority.

## Unity Classes

- `VisualMotionProfile`: ScriptableObject data for per-actor visual tuning.
- `VisualMotionProfileLibrary`: maps `VisualMotionProfileId`, actor type, and actor category to profiles.
- `ActorVisualMotionController`: per-actor visual smoothing, acceleration, braking, arrival, and turn interpolation.
- `VehicleVisualMotionController`: track/wheel phase, braking/turning flags, and suspension placeholders.
- `InfantryVisualMotionController`: idle/walk/run locomotion placeholder and aim/fire blend placeholders.
- `AircraftVisualMotionController`: banking, altitude offset, and hover presentation.
- `TurretVisualAimController`: turret lag and recoil placeholder.
- `MovementPathPreview`: visual-only movement path line and endpoint markers.
- `MovementDebugHud`: debug readout for controller counts, profile id, visual speed, state, facing, target, and category details.
- `Stage6PlayModeSmokeValidator`: automated validator for scene wiring, runtime actor views, move command preview, showcase controllers, pause/resume, single-step, low-power state, and red console errors.

## Validation Strategy

- Run all Stage 5 checks before Stage 6 work begins.
- Run `tools/run-unity-stage6-validation.ps1` to create the Stage 6 scene, validate required objects/components, and run play-mode smoke.
- Run `tools/run-stage6-checks.ps1` for the full Stage 0-6 gate, UnityEngine scan, and whitespace check.
- Smoke validation confirms generated actor visuals, selected vehicle motion, path preview, showcase vehicle/infantry/aircraft/turret controllers, pause/resume, single-step, low-power visual state, and no red console errors.

## Non-Goals

- No final art.
- No combat implementation.
- No Unity physics authority.
- No multiplayer, AI, fog, or economy expansion.
- No Quest-only package requirement.
