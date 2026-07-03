# Movement And Animation Targets

## Stage 0

Stage 0 implements simple deterministic grid pathfinding and fixed-point per-tick movement. It exposes visual extension fields such as desired speed, normalized speed, facing, turn rate, movement phase, and visual motion profile ID. This is gameplay-safe scaffolding, not final visual motion.

Stage 28.1 preserves that deterministic core model while making movement ticks advance along existing eight-way path steps. Diagonal path cells now move both fixed axes with integer vector stepping, so units can visibly move diagonally without gaining extra diagonal speed.

## Future Vehicle Movement

- Acceleration and braking curves.
- Steering arcs and turn radius.
- Track, wheel, and suspension animation.
- Turret lag and stabilization.
- Formation spacing and avoidance presentation.
- Dust, ground contact, recoil, and damage motion.

## Future Infantry Movement

- Locomotion blend tree.
- Aim offsets and upper-body firing.
- Recoil, reload, flinch, and suppression reactions.
- Ragdoll or authored death states later.

## Future Aircraft Movement

- Banking, altitude, climb/descent, and turning arcs.
- Takeoff and landing sequences.
- Hover/VTOL behavior for lifter units.
- Rotor, exhaust, landing gear, and payload animation.

## Future Building Animation

- Powered idle: lights, fans, turbines, cranes, radar dishes, doors, service arms.
- Production active: building-specific machinery sequences.
- Low power/offline: lights dark, machinery stopped or slowed, warning states.
- Damaged and destroyed states.

## Production Event Examples

- War factory door opens and vehicle rolls out.
- Barracks door opens and infantry exits.
- Refinery harvester docks and unloads.
- Power plant turbines or cores animate.
- Comm center dish rotates.
- Repair bay arms animate around a serviced vehicle.
