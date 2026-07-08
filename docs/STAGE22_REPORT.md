# Stage 22 Report

Stage 22 implements the first classic RTS command-matrix pass on top of the completed Stage 21.5 player build.

## Implemented

- Added deterministic `Rts.Core` commands for attack-move, guard, patrol, scatter, and deploy placeholder.
- Added new order states while preserving existing move, attack, harvest, stop, rally, and power behavior.
- Added opportunistic deterministic auto-fire for attack-move, guard, and patrol.
- Added PCDesktop command routing and command bar buttons for Stop, Move, Attack, Attack Move, Guard, Patrol, Scatter, Deploy, Repair, Sell, and Power.
- Added double-click same-type selection, box selection, and client-local control groups.
- Added Stage 22 Unity scene validation and Play Mode smoke validation.
- Added fast, medium, player-facing, and full Stage 22 validation scripts.
- Extended the medium-recursion audit to include Stage 22.

## Validation Commands

```powershell
dotnet run --no-restore --project src/Rts.Core.Tests
.\tools\build-rts-core-for-unity.ps1
.\tools\audit-medium-validation-recursion.ps1
.\tools\run-stage22-fast-checks.ps1
.\tools\run-stage22-medium-checks.ps1
.\tools\run-stage22-player-facing-checks.ps1 -SkipPlayerBuild
git diff --check
```

Use `.\tools\run-stage22-checks.ps1` for the slow full acceptance gate.

## Known Limits

- Patrol does not yet loop back and forth.
- Guard does not yet follow or protect another target actor.
- Deploy is a deterministic placeholder only.
- Repair and Sell remain reserved for a later base-management checkpoint.
- Control groups, box selection, and double-click selection are client-local Unity behaviors, as intended.

## Next Recommendation

Continue the overnight pass with the next highest-impact base-management checkpoint: repair/sell/power/rally/prerequisite polish, while keeping Stage 22 command controls covered by the new validation tier.
