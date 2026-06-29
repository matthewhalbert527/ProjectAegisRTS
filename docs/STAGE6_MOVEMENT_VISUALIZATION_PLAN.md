# Stage 6 Movement Visualization Plan

## Goal

Stage 6 should add high-quality visual movement on top of deterministic `Rts.Core` snapshots. The simulation remains authoritative; Unity may smooth, animate, and embellish presentation, but it must not change pathfinding, actor positions, facing, command validation, or tick outcomes.

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

## Recommended Unity Classes

- `VisualMotionProfile`: ScriptableObject or serializable data for per-actor visual tuning.
- `VisualMotionProfileRegistry`: maps `visual_motion_profile_id` and actor category to profiles.
- `ActorVisualMotionController`: per-actor visual smoothing, acceleration, braking, and turn interpolation.
- `VehicleVisualAnimator`: wheels, tracks, suspension, body lean, and turret lag hooks.
- `InfantryVisualAnimator`: locomotion blend and aim/recoil placeholders.
- `AircraftVisualAnimator`: altitude, banking, hover, and VTOL presentation.
- `VisualMotionDebugHud`: optional debug readout for snapshot speed, visual speed, facing error, and profile id.
- `Stage6VisualMotionSmokeValidator`: automated validator for deterministic snapshot preservation and non-authoritative visual updates.

## Validation Strategy

- Run all Stage 5 checks before Stage 6 work begins.
- Add smoke validation that spawns the demo world, selects a mobile actor, issues a move command, and confirms:
  - `Rts.Core` checksum stays unchanged by visual components.
  - actor views move visually without creating authoritative commands.
  - visual facing converges toward snapshot facing.
  - wheel/track/turret/aircraft placeholder components can be absent without errors.
  - no repeating red console errors are logged.
- Keep acceptance automated through a future `tools/run-stage6-checks.ps1`.

## Non-Goals

- No final art.
- No combat implementation.
- No Unity physics authority.
- No multiplayer, AI, fog, or economy expansion.
- No Quest-only package requirement.
