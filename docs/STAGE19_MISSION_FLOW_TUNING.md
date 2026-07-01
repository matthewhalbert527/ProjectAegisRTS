# Stage 19 Mission Flow Tuning

## Goal

Stage 19 makes the current vertical slice feel like a complete prototype mission instead of a systems sandbox. It keeps `Rts.Core` authoritative and uses Unity only for guidance, rendering, local selection, and command routing.

## Mission Flow Controller

`VerticalSliceMissionFlowController` reads `VerticalSliceProgressTracker` and exposes:

- current mission beat id,
- current instruction text,
- completed beat labels,
- next recommended action,
- recommended production type id.

It does not mutate gameplay state. The tracker derives beat state from snapshots, local selection, production queues, economy events, combat events, scenario objectives, and match outcome.

## Build Order And Pacing

The Stage 19 path is:

```text
Power Plant -> Refinery -> harvester loop -> Barracks -> infantry -> War Factory -> light tank -> scout -> attack -> victory
```

The world keeps a prebuilt base for prototype readability, but mission beats ask the player to build additional key items so production, placement, queue states, and sidebar highlights are exercised.

## Layout Changes

- The player ore field is closer and larger so harvesting is visible sooner.
- Early enemy contact moved farther east/northeast so the opening is less noisy.
- The player starts with a slightly stronger combat group so a non-debug attack can win.
- The enemy base remains reachable by normal movement and attack commands.

## Fine Placement

Stage 18.5 introduced the 2x fine placement grid. Stage 19 tunes the player-facing guidance around it:

- coarse lines are emphasized,
- fine lines are lighter,
- placement panel shows coarse/fine footprint sizes,
- invalid placement explains occupied, blocked, out-of-bounds, out-of-radius, and pending-production cases,
- prompts explain green/red placement preview states.

## Non-Debug Victory Path

Stage 19 adds a core test and Unity smoke path that use normal commands:

1. Move combat units toward the enemy base.
2. Select in-range combat units that can target buildings.
3. Attack the enemy Fabrication Hub.
4. Advance deterministic ticks until victory.
5. Verify match outcome and scenario objective agree.

No scenario damage or debug victory action is used for this validation path.

## Validation Tiers

- Fast: `.\tools\run-stage19-fast-checks.ps1`
- Medium: `.\tools\run-stage19-medium-checks.ps1`
- Player-facing: `.\tools\run-stage19-player-facing-checks.ps1 -SkipPlayerBuild`
- Full: `.\tools\run-stage19-checks.ps1`

Stage 19.5 builds on this mission flow by reorganizing the Windows player UI into a right-side PC sidebar and adding an Esc pause menu. See `docs/STAGE19_5_PC_SIDEBAR_PAUSE_MENU.md` and `docs/STAGE19_5_UI_REWORK_REPORT.md`.

Medium validation remains non-recursive and is guarded by:

```powershell
.\tools\audit-medium-validation-recursion.ps1
```
