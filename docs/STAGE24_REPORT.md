# Stage 24 Report

Stage 24 implements tech prerequisites, advanced unlocks, and a support-power foundation on top of Stage 23 base management.

## Implemented

- Added deterministic production prerequisites and `TechTreeSystem` checks in `Rts.Core`.
- Added support-power definitions, player support-power state, activation command, cooldowns, and snapshots.
- Added real Reveal Scan and Emergency Repair Pulse behavior.
- Added prerequisite gates for advanced buildings, defenses, infantry, vehicles, aircraft, and support powers.
- Added right-sidebar support-power buttons and production-card missing-prerequisite reasons.
- Added Quest left-hand support-power routing placeholder.
- Added Stage 24 Unity validation, Play Mode smoke validation, player-facing checks, and fast/medium/full validation tiers.
- Extended the medium-recursion audit to include Stage 24.

## Validation Commands

```powershell
dotnet run --no-restore --project src/Rts.Core.Tests
.\tools\build-rts-core-for-unity.ps1
.\tools\audit-medium-validation-recursion.ps1
.\tools\run-stage24-fast-checks.ps1
.\tools\run-stage24-medium-checks.ps1
.\tools\run-stage24-player-facing-checks.ps1 -SkipPlayerBuild
git diff --check
```

Use `.\tools\run-stage24-checks.ps1` for the slow full acceptance gate.

## Known Limits

- Precision Strike, Production Boost, and Power Surge are prerequisite/cooldown-backed placeholders only.
- Reveal Scan uses a deterministic timed reveal window; it does not yet include bespoke VFX/audio.
- Support powers use a compact sidebar strip rather than a full superweapon panel.

## Next Recommendation

Continue with Stage 25 engineer/capture/repair/transport mechanics using the Stage 24 prerequisite and support-power boundary as the unlock surface.
