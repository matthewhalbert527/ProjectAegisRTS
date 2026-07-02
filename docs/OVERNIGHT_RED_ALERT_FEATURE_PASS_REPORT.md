# Overnight Red Alert-Style Feature Pass Report

## Active Branch

`codex/overnight-red-alert-feature-pass`

## Baseline

Stage 21.5 was validated before feature work. The baseline included `Rts.Core` tests, Stage 21.5 medium checks, Stage 21.5 player-facing checks with player build skipped, Player.log inspection, the medium-recursion audit, UnityEngine-free scan, and `git diff --check`.

## Checkpoint 1: Stage 22 Classic RTS Command Matrix

Stage 22 adds deterministic command foundations and PCDesktop interaction polish:

- Attack Move
- Guard
- Patrol
- Scatter
- Deploy placeholder
- Stop polish through the existing stop command
- Client-local control groups
- Double-click same-type selection
- Box selection
- Compact right-sidebar command matrix
- Stage 22 fast, medium, player-facing, and full validation scripts

The medium tier remains non-recursive and is guarded by `tools/audit-medium-validation-recursion.ps1`.

## Checkpoint 2: Stage 23 Base Management Commands

Stage 23 turns the reserved base-management controls into deterministic commands:

- Building repair with credit spend over time
- Sell building with deterministic removal and partial refund
- Manual power toggle with production pause for powered-off producers
- Rally points for production buildings and spawned units
- PCDesktop Repair, Sell, Power, and Rally command routing
- Quest left-hand compatible command routing methods
- Stage 23 fast, medium, player-facing, and full validation scripts

The medium tier remains non-recursive and is guarded through Stage 23 by `tools/audit-medium-validation-recursion.ps1`.
