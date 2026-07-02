# Stage 23 Report

Stage 23 implements base management commands on top of the Stage 22 classic RTS command matrix.

## Implemented

- Added deterministic `Rts.Core` commands for building repair, repair cancel, building sell, power toggle behavior hardening, and rally point validation.
- Added repair state, repair accounting, manual power-off state, and rally point fields to actor snapshots.
- Added deterministic sell removal, 50 percent refunds, map occupancy cleanup, producer queue cleanup, and target cleanup.
- Added production pausing for manually powered-off producers.
- Added PCDesktop command routing and command bar actions for Repair, Sell, Power, and Rally.
- Added left-hand Quest-compatible routing methods for repair, sell, power, and rally.
- Added Stage 23 Unity scene validation and Play Mode smoke validation.
- Added fast, medium, player-facing, and full Stage 23 validation scripts.
- Extended the medium-recursion audit to include Stage 23.

## Validation Commands

```powershell
dotnet run --no-restore --project src/Rts.Core.Tests
.\tools\build-rts-core-for-unity.ps1
.\tools\audit-medium-validation-recursion.ps1
.\tools\run-stage23-fast-checks.ps1
.\tools\run-stage23-medium-checks.ps1
.\tools\run-stage23-player-facing-checks.ps1 -SkipPlayerBuild
git diff --check
```

Use `.\tools\run-stage23-checks.ps1` for the slow full acceptance gate.

## Known Limits

- Repair is a deterministic command loop, not a full repair cursor/economy UI treatment.
- Sell uses a fixed 50 percent refund and removes the building immediately.
- Rally points route spawned units to a cell but do not yet draw persistent rally-line visuals.
- Power toggle affects production/power state but does not yet expose per-building hotkeys beyond the command bar route.

## Next Recommendation

Continue the overnight pass with tech prerequisites, support powers, walls/defenses, and higher-tier unlocks while keeping Stage 23 validation in the medium tier as a direct dependency for the next checkpoint.
