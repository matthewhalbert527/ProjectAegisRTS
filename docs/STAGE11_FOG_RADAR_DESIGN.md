# Stage 11 Fog Radar Design

## Scope

Stage 11 adds the deterministic visibility foundation for fog of war, radar status, and minimap data. It is a foundation pass, not final stealth, cloak, radar art, terrain occlusion, or AI behavior.

## Core Authority

`Rts.Core` owns:

- per-player visibility state,
- unexplored, explored, and visible cell states,
- actor sight radius metadata,
- radar provider metadata,
- deterministic visibility updates,
- player-perspective actor filtering,
- fog, radar, and minimap snapshots.

Unity owns only presentation. It renders fog overlays, minimap dots, radar readouts, and debug HUD data from snapshots.

## Visibility Rules

- Each non-destroyed actor reveals cells around its current cell using deterministic integer radius checks.
- Visible cells become explored when no longer currently visible.
- Explored cells remain explored.
- Enemy actors are hidden from player-perspective snapshots unless their cell is visible.
- The default `CreateSnapshot()` remains unfiltered for older scenes; Stage 11 uses `CreateSnapshot(playerId)`.

## Radar Rules

- The current placeholder radar is active when an owned radar provider is powered and not low-power.
- `comm_center` is the first demo radar provider.
- Radar does not yet grant map-wide visibility, stealth detection, or minimap sweep behavior.

## Known Limits

- No terrain occlusion or line-of-sight blockers yet.
- No stealth, cloak, jammer, sensor tower, or radar sweep behavior.
- Projectiles and events are not fog-filtered yet.
- Minimap art is placeholder UI/primitive dots.
