# Stage 10 Report

## Summary

Stage 10 adds deterministic ore harvesting, harvester cargo, refinery unloading, economy snapshots/events, and Unity placeholder economy presentation. The playable scene is `Assets/Rts/Scenes/Stage10_EconomyHarvesting.unity`.

## Branch And Commit

- Branch: `codex/overnight-stage10-stage15`
- Base Stage 9 commit: `5d2ee28188d84fb7661d3f7d7bfe812e8c2396ed`
- Stage 10 implementation commit: this commit

## Systems Created

- Core economy state for resource cells, harvesters, refinery dock/unload state, and economy events.
- Core economy commands for harvesting, refinery return, resource assignment, and refinery assignment.
- Deterministic harvesting loop: travel to ore, collect cargo, deplete resource cells, return to refinery, unload cargo, award credits, and loop while ore remains.
- Economy snapshots surfaced through `WorldSnapshot.Economy` plus actor harvest-order flags.
- Unity bridge methods for harvest and refinery return commands.
- Unity rendering systems for resource cells, harvester cargo markers, refinery dock state, economy event markers, and the F8 economy debug HUD.
- Stage 10 scene generation, scene validation, Play Mode smoke validation, and full Stage 0-through-Stage 10 acceptance gate.

## Architecture Boundary

`Rts.Core` remains deterministic and UnityEngine-free. Unity submits harvest commands and renders economy snapshot/event data; it does not own resource amounts, cargo, refinery unload timing, credit awards, or harvester work states.

## Manual Play Mode Checklist

Open `Assets/Rts/Scenes/Stage10_EconomyHarvesting.unity`, press Play, and verify:

- Board, grid, and actors are visible.
- Ore/resource cells are visible.
- F8 toggles the economy debug HUD.
- A selected harvester can be ordered to harvest an ore cell.
- Harvester cargo increases while harvesting.
- Ore amount decreases.
- The harvester returns to the refinery and unloads.
- Player credits increase after unload.
- Refinery dock/unload markers and economy event markers appear.
- Tick count, pause/resume, single-step, movement, and combat presentation still work.
- No repeating red console errors.

## Known Limits

- Resource art, docking animation, audio, queueing, depletion search, fog, and worker AI remain future stages.
- Stage 10 validates through the slow full gate for this pass. Faster Stage 10 iteration tiers can be added in the next tooling pass without weakening the full acceptance gate.

## Commands

Acceptance commands:

```powershell
dotnet run --no-restore --project src/Rts.Core.Tests
.\tools\build-rts-core-for-unity.ps1
.\tools\run-unity-stage10-validation.ps1
.\tools\run-stage10-checks.ps1
git diff --check
```
