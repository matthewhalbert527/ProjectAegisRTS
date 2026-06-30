# Stage 10 Economy Design

## Scope

Stage 10 adds the first deterministic resource harvesting and refinery loop. It is an MVP economy slice, not final RTS balance or final harvester behavior.

## Core Authority

`Rts.Core` owns:

- ore resource cells and remaining amounts,
- harvester cargo, target resource cell, target refinery, and work state,
- refinery docking/unloading state,
- credit awards from unloading,
- harvest/return command validation,
- deterministic economy snapshots and bounded economy events.

Unity owns only presentation and input routing. It renders resources, cargo markers, refinery dock markers, unload markers, and debug HUD data from snapshots and events.

## Runtime Loop

1. A harvester receives a harvest order for a resource cell.
2. The core moves it to the resource cell using existing deterministic movement.
3. When it reaches the cell, it harvests ore into cargo and reduces the resource amount.
4. At capacity, or when the resource depletes, it moves to an owned refinery dock cell.
5. The refinery unloads cargo over ticks and awards credits.
6. If the original resource still has ore, the harvester loops back to it.

## Stage 10 Commands

- `IssueHarvestOrderCommand`
- `ReturnToRefineryCommand`
- `AssignHarvesterToResourceCellCommand`
- `AssignHarvesterToRefineryCommand`

`StopCommand`, direct move orders, attack orders, damage/destruction, and invalid ownership/actor cases clear or reject harvesting where appropriate.

## Snapshot Surface

`WorldSnapshot.Economy` exposes:

- `ResourceSnapshot`
- `HarvesterSnapshot`
- `RefinerySnapshot`
- `EconomyEventSnapshot`

Actor snapshots also expose `HasHarvestOrder` for HUDs and selection feedback.

## Known Limits

- Ore is represented as deterministic resource cells, not final terrain art.
- Docking is a simple cell target, not final path reservation or animated queueing.
- Harvester route selection uses nearest owned refinery and the issued resource cell.
- There is no full depletion search, fog interaction, worker AI, collision crowding, or audio.
- Unity visuals are placeholder primitives intended to be replaced by Stage 8/production assets later.
