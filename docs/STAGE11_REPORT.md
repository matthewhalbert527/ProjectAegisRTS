# Stage 11 Report

## Summary

Stage 11 adds deterministic fog of war, radar status, minimap snapshot data, Unity fog overlay presentation, and improved minimap rendering. The playable scene is `Assets/Rts/Scenes/Stage11_FogRadarMinimap.unity`.

## Branch And Commit

- Branch: `codex/overnight-stage10-stage15`
- Base Stage 10 validation-tier commit: `d058f3e`
- Stage 11 implementation commit: this commit

## Systems Created

- Core visibility definitions: `CellVisibility`, `SightDefinition`, `RadarDefinition`, and `PlayerVisibilityState`.
- Per-player visibility state and deterministic visibility update in `RtsWorld`.
- Player-perspective snapshots through `CreateSnapshot(playerId)` that hide unseen enemy actors.
- Fog, radar, minimap actor dot, and minimap resource dot snapshots.
- Stage 11 demo world with own scout/comm center, visible enemy, hidden enemy, and resource cells.
- Unity fog overlay renderer, visibility debug renderer, radar adapter, minimap render system, minimap dot view, and F7 fog debug HUD.
- Stage 2 minimap fallback upgraded to consume minimap snapshots when available.
- Stage 11 scene generation, scene validation, Play Mode smoke validation, and fast/medium/full validation tiers.

## Architecture Boundary

`Rts.Core` remains deterministic and UnityEngine-free. Unity reads fog/radar/minimap snapshot data and renders placeholder overlays/dots; it does not own visibility rules, explored state, radar activation, or actor hiding.

## Validation Tiers

- `tools/run-stage11-fast-checks.ps1`: current Stage 11 Unity validation, Play Mode smoke or live fallback, UnityEngine-free scan, and `git diff --check`.
- `tools/run-stage11-medium-checks.ps1`: `Rts.Core` tests, Unity DLL build, Stage 10 immediate dependency validation, Stage 11 validation, UnityEngine-free scan, and `git diff --check`.
- `tools/run-stage11-checks.ps1`: slow full Stage 0-through-Stage 11 acceptance gate.

## Manual Play Mode Checklist

Open `Assets/Rts/Scenes/Stage11_FogRadarMinimap.unity`, press Play, and verify:

- Board, grid, and owned actors are visible.
- Fog overlay covers unexplored cells and dims explored cells.
- The visible enemy near the scout appears.
- The distant enemy remains hidden in the player-perspective view.
- F7 toggles the fog/radar debug HUD.
- Radar reports active while the comm center is powered.
- Minimap dots appear for owned and visible enemy actors.
- Moving the scout leaves explored cells behind.
- Tick count, pause/resume, single-step, movement, combat, and economy presentation still work.
- No repeating red console errors.

## Commands

Acceptance commands:

```powershell
dotnet run --no-restore --project src/Rts.Core.Tests
.\tools\build-rts-core-for-unity.ps1
.\tools\run-stage11-fast-checks.ps1
.\tools\run-stage11-medium-checks.ps1
.\tools\run-stage11-checks.ps1
git diff --check
```
