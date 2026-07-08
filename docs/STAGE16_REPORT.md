# Stage 16 Report: Integrated Playable Vertical Slice

## Summary

Stage 16 adds the first integrated playable vertical slice. It combines the deterministic core systems from Stages 0-15 into a single match loop with scenario state, objectives, victory/defeat detection, player-perspective fog/minimap, economy, combat, AI, terrain/pathing, feedback, performance HUDs, PC sidebar controls, simulated dual-hand controls, and prefab resolution.

`Rts.Core` remains deterministic and UnityEngine-free.

## Core Additions

- `MatchState`, `MatchPhase`, `PlayerOutcome`, and `MatchSnapshot`.
- `ScenarioDefinition`, `ScenarioObjective`, `ScenarioObjectiveState`, and `ScenarioSnapshot`.
- `VictoryCondition` and `DefeatCondition`.
- `DemoWorldFactory.CreateVerticalSliceWorld()`.
- Safe scenario debug APIs for damage, credit grants, and map reveal.
- `WorldSnapshot` now carries match and scenario snapshots.
- 10 new Stage 16 core tests, bringing the console suite to 65/65.

## Unity Additions

- `Assets/Rts/Scenes/Stage16_PlayableVerticalSlice.unity`
- `VerticalSliceScenarioController`
- `VerticalSliceDebugActions`
- `MatchObjectiveHud`
- `IntegratedSystemsStatusHud`
- `Stage16SceneCreator`
- `Stage16SceneValidator`
- `Stage16PlayModeSmokeValidator`

Stage 16 is generated from Stage 15, then restores the Stage 2 desktop HUD/sidebar layer that the later XR-derived scene chain no longer carried.

## Validation

Use:

```powershell
.\tools\run-stage16-fast-checks.ps1
.\tools\run-stage16-medium-checks.ps1
.\tools\run-stage16-checks.ps1
```

Fast is for Stage 16 iteration. Medium is the pre-commit gate: it runs core tests, one Unity DLL build, direct Stage 15 Unity validation, Stage 16 Unity validation and smoke, the medium recursion audit, UnityEngine scan, and `git diff --check`.

Full is the final acceptance gate and validates Stage 0 through Stage 16 with the flattened full-chain runner.

`tools\audit-medium-validation-recursion.ps1` now scans Stage 9 through Stage 16 medium scripts and fails if a medium tier calls a prior medium script.

## Limitations

- The match loop is intentionally small: one local player, one deterministic AI opponent, and simple base-destroy objectives.
- Art remains blockout/prefab-resolution driven; final 3D assets are still later-stage work.
- Multiplayer, replays, checksums, save/load, campaign scripting, and release packaging remain later-stage work.
