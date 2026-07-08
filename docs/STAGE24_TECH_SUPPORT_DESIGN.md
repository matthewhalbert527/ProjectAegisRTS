# Stage 24 Tech / Support Design

Stage 24 adds the first real tech tree layer and support-power foundation without moving gameplay authority out of `Rts.Core`.

## Core Model

- `ProductionDefinition` now carries prerequisite type IDs and normalized `PrerequisiteDefinition` entries.
- `TechTreeSystem` resolves the first missing prerequisite through deterministic owned-actor checks.
- `RtsWorld.BeginProduction` rejects locked items with `MissingPrerequisite`.
- Support powers are defined by `SupportPowerDefinition` and tracked per player by `SupportPowerState`.
- `SupportPowerSnapshot` exposes unlock, missing prerequisite, cooldown, readiness, effect, target kind, and activation count.

## Unlocks

The starter build path remains intact: power plant, barracks, war factory, refinery, gun tower, rifle infantry, light tank, and harvester stay available through their existing producers.

Advanced progression now uses these gates:

- `refinery` unlocks `comm_center`.
- `comm_center` unlocks `tech_center`, cannon turret, medium/scout/rocket progression, and Reveal Scan.
- `war_factory` unlocks `repair_bay`.
- `repair_bay` unlocks Emergency Repair Pulse.
- `tech_center` unlocks heavy/advanced/aircraft progression and placeholder powers.
- `advanced_power_plant` unlocks the Power Surge placeholder.

## Support Powers

- Reveal Scan: real targeted cell power that temporarily reveals a radius and starts cooldown.
- Emergency Repair Pulse: real targeted cell power that repairs owned buildings in radius.
- Precision Strike: placeholder command, prerequisite-gated and cooldown-backed.
- Production Boost: placeholder command, prerequisite-gated and cooldown-backed.
- Power Surge: placeholder command, prerequisite-gated and cooldown-backed.

## UI Contract

Desktop production cards show missing factory and missing prerequisite reasons separately. The right sidebar includes a compact support-power strip backed by support snapshots. Buttons are disabled while locked or cooling down. Quest left-hand routing has a support-power passthrough for later hand UI.

Debug panels remain hidden by default, and `Rts.Core` remains UnityEngine-free.
