# Stage 16 Vertical Slice Design

## Purpose

Stage 16 turns the staged systems into one playable match slice. It keeps `Rts.Core` deterministic and Unity-free while adding a small match/scenario layer for start, reset, objective state, victory, defeat, and snapshots.

## Core Model

New deterministic concepts live under:

- `src/Rts.Core/Match`
- `src/Rts.Core/Scenarios`
- `src/Rts.Core/Victory`

`MatchState` owns the current `MatchPhase`, local `PlayerOutcome`, elapsed ticks, winning player, and objective state. `ScenarioDefinition` describes the local player, enemy player, objective list, `VictoryCondition`, and `DefeatCondition`.

The Stage 16 vertical slice uses:

- victory: enemy `fabrication_hub` destroyed, with a fallback that all enemy combat/building actors are destroyed,
- defeat: player `fabrication_hub` destroyed,
- objectives: destroy the enemy base and protect the player base.

`WorldSnapshot` now includes `MatchSnapshot` and `ScenarioSnapshot` so Unity UI and validation read match state from the same deterministic snapshot stream as actors, economy, fog, AI, and map data.

## Demo World

`DemoWorldFactory.CreateVerticalSliceWorld()` creates a 32x32 scenario with:

- player base: fabrication hub, power, barracks, war factory, refinery, comm center, gun tower, harvester, scout, tank, and infantry,
- enemy AI base: fabrication hub, power, barracks, war factory, refinery, gun tower, harvester, scout, tank, and infantry,
- ore fields for player and enemy economy,
- fog/minimap visibility through player-perspective snapshots,
- terrain variety for pathing and map overlays,
- deterministic AI configured for player 2.

## Unity Scene

`Assets/Rts/Scenes/Stage16_PlayableVerticalSlice.unity` is generated from Stage 15 and adds:

- `VerticalSliceScenarioController`
- `VerticalSliceDebugActions`
- `MatchObjectiveHud`
- `IntegratedSystemsStatusHud`

The scene keeps the earlier rendering stacks: board, actors, art prefab resolver, combat, economy, fog/minimap, AI, terrain/pathing, feedback, performance HUD, simulated dual-hand controls, and a regenerated PC desktop HUD/sidebar.

## Debug Actions

Unity debug actions route through safe `Rts.Core` methods:

- start/reset match,
- grant credits,
- reveal map,
- select harvester/combat unit,
- issue harvest and attack smoke commands,
- destroy enemy base for victory validation,
- destroy player base for defeat validation.

Unity does not mutate actor internals directly.

## Validation Expectations

Stage 16 Play Mode smoke verifies the integrated loop:

- board and actors exist,
- match starts running,
- player and hidden enemy bases exist,
- resources, harvesters, refineries, fog, minimap, AI, terrain, and objectives are present,
- production, harvest, and attack commands route,
- destroying the enemy base triggers victory,
- destroying the player base triggers defeat,
- no red console errors repeat.
