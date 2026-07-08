# Stage 7 Building Animation Plan

## Implemented Foundation

Stage 7 implements the building visualization foundation as Unity presentation code:

- Profile-driven building visual data through `BuildingVisualProfile` assets.
- Runtime lookup through `BuildingVisualProfileLibrary`.
- Snapshot-driven state derivation in `BuildingVisualStateController`.
- Generated placeholder child parts through `BuildingPlaceholderPartFactory`.
- Separate controllers for lights, machinery, production, doors, damage, and type-specific loops.
- A Stage 7 scene and smoke validator that prove the visual layer initializes and responds to power, production, and damage states.

The system is intentionally prepared for final assets but does not require them.

## Final 3D Model Integration

Future building models should replace generated placeholder parts behind the same controller references. The likely integration path is:

- Keep `BuildingVisualProfile` as the per-building tuning source.
- Add named sockets or child transforms for lights, doors, machinery, damage markers, and type-specific moving parts.
- Teach `BuildingPlaceholderPartFactory` or a sibling binding component to prefer authored child transforms over generated primitives.
- Keep `BuildingVisualStateController` as the only component that derives visual state from snapshots.
- Keep all production, power, damage, and animation presentation out of `Rts.Core`.

## Per-Building Animation Targets

- `fabrication_hub`: crane or construction arm, powered lights, production bay door/activity.
- `power_plant`: turbine/core spin, powered lights, low-power slowdown.
- `advanced_power_plant`: larger or dual core, stronger lighting, faster powered loop.
- `barracks`: door opening activity, production pulse, low-power dimming.
- `war_factory`: bay door, conveyor placeholder, production light.
- `refinery`: dock pump and unload arm loop.
- `gun_tower`: barrel or turret idle sweep.
- `cannon_turret`: heavier barrel idle sweep.
- `advanced_gun_tower`: stronger turret/barrel idle loop and warning light.
- `comm_center`: radar dish rotation.
- `repair_bay`: repair arm loops and service door.
- `tech_center`: scanner or antenna loop.
- `field_hospital`: beacon and medical lights.
- `dual_helipad`: pad lights and platform status markers.

## Future Animator Controller Plan

When final models exist, add optional Animator integration beside the transform-loop controllers:

- Keep Stage 7 enum states as the source for Animator parameters.
- Map power state to bool/int parameters such as `Powered`, `LowPower`, and `Offline`.
- Map production state to `Producing`, `ProductionProgress01`, and optional trigger hooks.
- Map damage state to `Damaged` and `DestroyedPlaceholder`.
- Avoid root motion or any Animator output that feeds back into authoritative simulation.

## Future VFX Plan

VFX should remain presentation-only:

- Replace damage smoke placeholders with authored particle or VFX Graph effects.
- Add low-power flicker and offline shutdown effects.
- Add production sparks, loading pulses, and repair glows.
- Add destruction/debris VFX only after combat and destruction rules exist in `Rts.Core`.
- Gate expensive VFX behind quality settings for Quest/MR performance.

## Future Sound Event Hooks

Sound should be driven from visual state transitions, not deterministic gameplay mutation:

- Power loop on/off.
- Low-power warning chirp.
- Production start/progress/complete accents.
- Door open/close.
- Turbine/radar/machinery loop intensity.
- Damage warning and destroyed placeholder stingers.

Sound events should be client-local and safe to disable.

## Future Damage And Destruction Plan

Stage 7 only shows health-derived presentation. A later combat stage should define:

- Authoritative damage commands/events in `Rts.Core`.
- Snapshot fields needed for hit, fire, repair, and destruction presentation.
- Destroyed building replacement visuals and cleanup rules.
- Rebuild/sell/repair interactions.
- Replay-safe combat event recording if transient VFX need deterministic alignment.
