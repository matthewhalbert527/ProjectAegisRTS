# Stage 31 Per-Actor Production Checklist

Use this checklist before replacing any MVP production proxy with real art. Each actor must keep the Stage 20/21 metadata contract, Stage 29 grounded battlefield direction, and Stage 30 readability cues.

## Shared Checklist

- Root pivot centered on gameplay footprint at ground height.
- Foundation/base matches the fine-grid footprint.
- `ActorPrefabDescriptor.actorTypeId` uses the safe actor ID.
- `ProductionVisualValidationTag` stays present and truthful.
- Required `ActorPrefabSocket` children stay present.
- `LODGroup` exists and follows the Stage 31 Quest budget.
- Fallback proxy remains assigned until the real model passes all validation.
- Materials follow `docs/STAGE31_ARTIST_HANDOFF_PACKAGE.md` naming rules.
- Top, front, side, rear, and roof views are readable.
- PCDesktop 1600x900 top-down view and QuestXR walkaround view both preserve actor identity.

## fabrication_hub

- Keep large 3x3 foundation, construction tower, crane/arm, production door, exit/rally sockets, and powered lights.
- Top-down read should say "build hub" through crane/gantry and broad work pad, not a generic warehouse.

## power_plant

- Keep compact 2x2 reactor body, turbine/core marker, exhaust/utility details, and low-power-readable emissive or machinery cues.
- Status lights must remain readable without overpowering the fine placement grid.

## refinery

- Keep 3x3 dock pad, storage tanks, pipes, pump arm, harvester dock, unload cue, and production exit.
- Resource relationship should be obvious through hopper, tank, pipe, or ore-bin details.

## barracks

- Keep 2x2 compact entry-focused footprint, doorway/muster cue, roof intake, side lockers or vents, and rear service detail.
- Silhouette should remain smaller and personnel-focused beside factories.

## war_factory

- Keep 3x2 large bay, roll-up door, vehicle ramp, floor track, roof gantry/crane, side vents, and exit sockets.
- It must read as the primary vehicle production building from above.

## gun_tower

- Keep 1x1 pedestal, rotating turret housing, barrel, muzzle marker, rear ammo/detail, and clear facing cue.
- Weapon silhouette must read before surface material detail.

## rifle_infantry

- Keep 1x1 body/head/weapon/backpack separation, aim socket, muzzle cue, and selection readability.
- Avoid over-detail that disappears at the PC camera distance.

## light_tank

- Keep 1x1 chassis, turret, barrel, tracks, engine deck, muzzle, smoke, and explosion sockets.
- Turret and hull orientation must be legible from the top-down camera.

## harvester

- Keep 1x1 larger utility chassis, cargo hopper, collector/intake, dock marker, tracks, smoke/explosion sockets, and resource pop cue.
- It should be visually related to the refinery without sharing an identical silhouette.
