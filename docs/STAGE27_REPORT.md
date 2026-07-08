# Stage 27 Report

Stage 27 is complete after local validation.

Implemented:

- Easy, Normal, and Hard AI difficulty profiles in `Rts.Core`.
- Deterministic timed attack-wave state in AI snapshots.
- AI production targets for infantry, vehicles, and harvesters by difficulty.
- Reachable attack-wave staging so enemy pressure avoids invalid building-cell attack-move orders.
- Hard AI damaged-building repair through the existing deterministic repair command.
- Boot Options skirmish difficulty selection with saved player preference.
- Stage16 simulation driver difficulty restart support.
- Player-facing objective HUD enemy-pressure summary.
- Stage 27 Unity validators and fast/medium/player-facing/full validation scripts.

Validation target:

- `Rts.Core` tests: 107/107.
- Stage 27 Unity validation verifies difficulty preferences, right sidebar preservation, hidden debug defaults, AI attack pressure, restart/difficulty controls, and non-debug victory.
- `Rts.Core` remains UnityEngine-free.
- Medium validation remains non-recursive through Stage 27.

Known limitations:

- AI remains a deterministic pressure prototype, not a full strategic opponent.
- Difficulty profiles tune timing and targets but do not yet include faction personality, map analysis, retreats, or advanced threat scoring.
- Economy integration assigns idle harvesters and queues target production, but does not yet implement a full build-order planner.
