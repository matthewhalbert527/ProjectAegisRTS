# Stage 29 Realistic Battlefield Visual Direction

Stage 29 moves the prototype from readable proxy art toward a more grounded battlefield look while staying Quest-safe and easy to validate.

## Direction

- Keep the gameboard readable from PC top-down and Quest walkaround views.
- Use restrained military-industrial materials: worn painted metal, concrete, compacted soil, dark rubber/track surfaces, warm utility lights, muted fog, and mineral resource highlights.
- Preserve the fine placement grid: texture and detail should support placement decisions, not hide valid/invalid footprints.
- Keep proxy silhouettes distinct by actor type. Buildings need roof/side/rear service detail; vehicles need top plates, running gear, turrets or hoppers; infantry need head/body/weapon/backpack separation.
- Prefer simple modular geometry and shared materials over high-poly decoration.

## MVP Actor Focus

- `fabrication_hub`: construction scaffold, crane counterweight, visible build bay grounding.
- `power_plant`: turbine glow, cable bundles, exhaust/utility read.
- `refinery`: ore bin glow, pipes, dock pump identity.
- `barracks`: door/muster light, roof ridge, side service panels.
- `war_factory`: gantry, roll-up bay, floor track, broad vehicle exit.
- `gun_tower`: shielded turret, sensor lens, strong pedestal.
- `rifle_infantry`: readable head/shoulders, weapon, pack, muzzle cue.
- `light_tank`: turret optic, side stowage, tracks, hull plates.
- `harvester`: hopper resource glint, collector tooth row, dock alignment.

## Non-Goals

- No final artist-authored FBX/GLB replacement is required in Stage 29.
- No gameplay balance, movement, pathing, targeting, or production logic changes.
- No shader-heavy, platform-specific rendering dependency.

## Stage 32 Follow-Up

Stage 32 expands this direction with a generated terrain-piece and battlefield set-dressing library. The same rules still apply: terrain detail must support top-down readability, preserve the fine placement grid, stay Quest-safe, and remain visual-only rather than gameplay terrain authority.
