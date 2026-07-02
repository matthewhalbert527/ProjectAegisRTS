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

## Checkpoint 3: Stage 24 Tech Tree / Support Powers

Stage 24 gives production progression a deterministic tech-tree boundary and starts the support-power layer:

- Production prerequisites and first-missing-prerequisite rejection in `Rts.Core`
- Advanced unlock gates for comm center, tech center, repair bay, advanced defenses, heavy vehicles, and aircraft
- Support-power definitions, per-player state, cooldowns, commands, and snapshots
- Real Reveal Scan and Emergency Repair Pulse powers
- Placeholder Precision Strike, Production Boost, and Power Surge powers with prerequisites/cooldowns
- PCDesktop right-sidebar support-power strip and production-card availability reasons
- Quest left-hand compatible support-power routing method
- Stage 24 fast, medium, player-facing, and full validation scripts

The medium tier remains non-recursive and is guarded through Stage 24 by `tools/audit-medium-validation-recursion.ps1`.

## Checkpoint 4: Stage 25 Engineers / Capture / Transports

Stage 25 adds the first deterministic utility-unit and passenger mechanics:

- Engineer building capture with generic captureable building metadata
- Engineer one-shot field repair for owned damaged buildings
- APC infantry transport capacity
- Load/unload commands with passenger state hidden from board snapshots
- Transport snapshots for passenger HUD/readout support
- Deterministic passenger destruction when a transport dies
- PCDesktop Capture, Eng Repair, Load, and Unload command modes
- Quest left-hand compatible engineer/transport routing methods
- Stage 25 fast, medium, player-facing, and full validation scripts

The medium tier remains non-recursive and is guarded through Stage 25 by `tools/audit-medium-validation-recursion.ps1`.

## Checkpoint 5: Stage 26 Airfield / Aircraft / Naval Foundation

Stage 26 adds deterministic air and water movement foundations without beginning Stage 27:

- Aircraft metadata for attack and heavy-lifter aircraft
- Dual-helipad airfield pad state and snapshots
- Produced aircraft docking at helipad pads
- Placeholder fuel/rearm state and airborne altitude snapshots
- Aircraft movement over water/buildings through the existing pathing system
- Naval movement class and water passability for future naval units
- Player vertical-slice tech/helipad/aircraft assets while keeping the base normally powered
- Unity aircraft visual altitude linked to core aircraft snapshots
- Stage 26 fast, medium, player-facing, and full validation scripts

The medium tier remains non-recursive and is guarded through Stage 26 by `tools/audit-medium-validation-recursion.ps1`.

## Checkpoint 6: Stage 27 Skirmish Playability / AI Pressure

Stage 27 makes the vertical slice feel more like a playable skirmish prototype:

- Easy, Normal, and Hard deterministic AI profiles
- Timed attack-wave state and snapshot fields
- Difficulty-based infantry, vehicle, and harvester production targets
- Reachable attack-wave staging and conservative defense/economy/base target priority
- Hard AI building repair through the existing repair command
- Boot Options skirmish difficulty selection saved to player preferences
- Stage16 restart support for changing difficulty during testing
- Player-facing enemy-pressure status in the objective HUD
- Stage 27 fast, medium, player-facing, and full validation scripts

The medium tier remains non-recursive and is guarded through Stage 27 by `tools/audit-medium-validation-recursion.ps1`.
