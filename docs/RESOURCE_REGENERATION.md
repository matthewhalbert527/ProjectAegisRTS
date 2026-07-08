# Resource Regeneration

## Runtime Model

`AegisResourcePlacement` now carries optional regeneration fields:

- `fieldId`
- `amount`
- `maxAmount`
- `regenerates`
- `regenerationRatePerTick`
- `regenerationDelayTicks`

`RtsWorld` stores matching deterministic state on `ResourceCellState`. Harvesting reduces `Amount` and records the harvest tick. When regeneration is enabled, the world restores resources after the configured delay and clamps at `MaxAmount`.

## Standalone Simulation

`AegisResourceSimulation` in `src/Rts.Core/Maps/Generation` provides a small deterministic test harness for resource fields. It covers depletion, hidden/depleted state, delay, rate, max cap, per-field regeneration overrides, owner/neutral metadata, ticks since harvest, and repeatability.

The simulation supports:

- `ore`
- `crystal`
- `salvage`
- `energy`

The core world factory maps these identifiers into deterministic `ResourceKind` values. Unknown resource IDs fall back to ore for compatibility.

## Generator Defaults

- Scarce maps use fewer fields and slower effective recovery.
- Balanced maps use moderate regeneration.
- Resource-rich maps add more fields and faster regeneration.

Unity can hide, scale, or swap visuals when a field reports depleted state. Gameplay authority remains in `Rts.Core`.

## Validation

The test harness verifies harvest reduction, depletion, over-harvest prevention, delay behavior, regeneration increments, max caps, deterministic replay, and profile-specific regeneration differences.
