# Stage 20 360-Degree Visual Standards

Stage 20 moves the MVP set from plain blockouts to first-pass production proxies. The visual goal is not final art yet. The goal is a reliable standard for 3D tabletop miniatures that read well in PC top-down play and remain credible when a Quest player walks around the board.

## Why 360-Degree Standards Matter

Classic fixed-camera RTS art can hide missing backs, flat sides, and billboard-like shapes. ProjectAegisRTS cannot rely on that trick. In Quest/MR, players can lean in, rotate the board, and inspect buildings from every side. Stage 20 proxies must therefore avoid single-sided "movie set" construction.

## Required Building Rules

- Grid footprint must remain exact.
- Foundation/base must align to the gameplay footprint.
- Main volume should be inset, raised, stepped, tiered, or beveled.
- Top-down silhouette must identify the actor quickly.
- Front, side, rear, and roof detail must all exist.
- Roof machinery, vents, tanks, cranes, turbines, or gantries should reinforce identity.
- Production buildings must include doors, ramps, exits, and production VFX sockets.
- Powered buildings must expose light and machinery sockets so Stage 7 power-state controllers still work.
- Minor decorative overhangs are allowed only when they do not confuse placement readability.

## Required Unit Rules

- Infantry must be more readable than a capsule: body, head, weapon, and aim pivot are required.
- Vehicles must expose chassis, drive/track markers, top detail, and VFX sockets.
- Combat actors must keep turret, barrel, muzzle, and aim hooks where applicable.
- Harvesters must show cargo/collector identity and dock alignment.

## Materials And Performance

- Use a small shared material set.
- Prefer primitive composition with clear silhouettes over dense geometry.
- MVP proxies should include `LODGroup`.
- Quest-safe proxies should stay under the validator's recommended mesh/material counts.
- Validation tags must describe view coverage and grid/base compliance.

## Platform UI Split

- `PCDesktop`: right-side sidebar, minimap above production, mouse/keyboard, Esc pause, Quest left-hand UI hidden by default.
- `QuestXR`: left-hand build/selection, right-hand tactical commands and board controls, PC sidebar hidden by default.
- `DebugHybrid`: explicit editor-only test mode.

## MVP Actor Briefs

- `fabrication_hub`: large foundation, construction tower, crane/arm, production door, exit/rally sockets, powered lights.
- `power_plant`: compact reactor body, turbine/core marker, exhaust stacks, low-power-readable light/core details.
- `refinery`: dock pad, storage tanks, pipes, pump arm, harvester dock, unload/production exit.
- `barracks`: compact entry-focused building, roof intake, side lockers/vents, rear service detail.
- `war_factory`: large bay, rollup door, vehicle ramp, roof gantry/crane, side vents.
- `gun_tower`: pedestal, rotating turret housing, barrel, muzzle marker, rear ammo/detail.
- `rifle_infantry`: body, head, simple weapon, aim pivot, backpack/detail.
- `light_tank`: chassis, turret, barrel, tracks, engine deck, muzzle and smoke/explosion sockets.
- `harvester`: larger chassis, cargo hopper, collector/intake, dock marker, tracks, smoke/explosion sockets.

## Stage 21 QA Addendum

Stage 21 tightens these standards with structured MVP visual QA:

- Every MVP proxy must carry replacement metadata, fallback references, and socket scaffolding.
- Bases and pivots must sit on the gameplay footprint without hidden geometry dipping below the root plane.
- Top-down, side, rear, and Quest-style walkaround views must remain readable.
- Optional artist-authored model candidates must be staged through the Stage 21 import scan before being assigned as active production prefabs.
- Player-facing validation must still confirm the PCDesktop sidebar, QuestXR hand-control split, hidden debug panels, hidden placement controls outside placement mode, clean Player.log, and UnityEngine-free `Rts.Core`.
