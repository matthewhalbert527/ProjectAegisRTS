# Stage 23 Base Management Design

Stage 23 turns the reserved base-management command slots into deterministic player-facing actions while preserving the Stage 22 command matrix and Stage 0-21.5 behavior.

## Player Commands

- Repair starts deterministic building repair on one selected owned damaged building.
- Sell removes one selected owned building and grants a 50 percent cost refund.
- Power toggles one selected owned building off or on.
- Rally enters a click-to-cell mode for one selected production building and records the spawn rally point.

The PCDesktop command bar exposes Repair, Sell, Power, and Rally in the right sidebar. Rally uses the same left-click board flow as Patrol. Quest left-hand routing exposes equivalent repair, sell, power, and rally methods without changing the existing Stage 4 menu layout.

## Core Rules

- Repair restores 10 hit points per tick and spends 5 credits per tick.
- Repair stops when the building reaches max health, is destroyed, or the player runs out of credits.
- Sell removes the building, clears its map occupancy, removes dependent production queue entries, clears targeting state against the sold actor, and refunds 50 percent of the building cost.
- Powered-off buildings are excluded from power generation and consumption.
- Powered-off producers pause active production until powered back on.
- Rally points are accepted only for production buildings and affect newly spawned units.

## Snapshot Contract

`ActorSnapshot` now exposes:

- `RallyPoint`
- `IsRepairing`
- `RepairProgressTicks`
- `RepairSpentCredits`
- `IsManuallyPoweredOff`

Unity uses those fields for validation and player-facing status without moving gameplay authority out of `Rts.Core`.

## Validation

Use `.\tools\run-stage23-fast-checks.ps1` while iterating on base-management behavior. Use `.\tools\run-stage23-medium-checks.ps1` before committing. Use `.\tools\run-stage23-checks.ps1` for the slow full acceptance gate.
